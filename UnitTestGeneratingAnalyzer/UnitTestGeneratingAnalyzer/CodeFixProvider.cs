using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Rename;
using Microsoft.CodeAnalysis.Editing;
using UnitTestGeneratingAnalyzer.CodeGenerators;

namespace UnitTestGeneratingAnalyzer
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(UnitTestGeneratingAnalyzerCodeFixProvider)), Shared]
    public class UnitTestGeneratingAnalyzerCodeFixProvider : CodeFixProvider
    {
        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get { return ImmutableArray.Create(UnitTestGeneratingAnalyzerAnalyzer.DiagnosticId); }
        }

        public sealed override FixAllProvider GetFixAllProvider()
        {
            return WellKnownFixAllProviders.BatchFixer;
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            // TODO: Replace the following code with your own analysis, generating a CodeAction for each fix to suggest
            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;

            // Find the type declaration identified by the diagnostic.
            var declaration = root.FindToken(diagnosticSpan.Start).Parent.AncestorsAndSelf().OfType<ClassDeclarationSyntax>().First();

            // Register a code action that will invoke the fix.
            context.RegisterCodeFix(
                CodeAction.Create("Create Test class", c => AddTestClassAsync(context.Document, declaration, c)),
                diagnostic);
        }

        internal async Task<Document> AddTestClassAsync(Document document, ClassDeclarationSyntax declaration, CancellationToken cancellationToken)
        {
            //var testClassGenerator = new NUnitTestClassGenerator();

            //var rootTestClassDocument = testClassGenerator.TestClassRootAsync(document, declaration.Identifier.Text).Result;

            var rootTestClassDocument = TestClassRootAsync(document, declaration.Identifier.Text).Result;

            var syntaxRoot = await document.GetSyntaxRootAsync();
            var newSyntaxRoot = syntaxRoot.InsertNodesAfter(declaration, new[] { rootTestClassDocument });

            return document.WithSyntaxRoot(newSyntaxRoot);
        }

        public async Task<SyntaxNode> TestClassRootAsync(Document document, string className)
        {
            var docSyntaxRoot = await document.GetSyntaxRootAsync();
            var semanticModel = await document.GetSemanticModelAsync();
            var classDec = docSyntaxRoot.DescendantNodes().OfType<ClassDeclarationSyntax>().Where(x => x.Identifier.Text == className).First();

            var generator = SyntaxGenerator.GetGenerator(document);

            var usingDirectives = docSyntaxRoot.ChildNodes().OfType<UsingDirectiveSyntax>().ToList();
            usingDirectives.Add(generator.NamespaceImportDeclaration("Rhino.Mocks") as UsingDirectiveSyntax);


            var fieldDeclarations = new List<SyntaxNode>();
            var expressionStatements = new List<SyntaxNode>();
            var constructorWithParameters =
                classDec.DescendantNodes()
                    .OfType<ConstructorDeclarationSyntax>()
                    .FirstOrDefault(
                        x => x.ParameterList.Parameters.Any());
            if (constructorWithParameters != null)
            {
                var constructorParam = constructorWithParameters.ParameterList.Parameters;
                foreach (var parameter in constructorParam)
                {
                    var parameterType = parameter.Type;
                    var fieldName = string.Format("_{0}", parameterType.ToString().ToLowerInvariant());
                    var fieldDec = generator.FieldDeclaration(fieldName
                                                            , parameterType
                                                            , Accessibility.Private);
                    fieldDeclarations.Add(fieldDec);

                    var fieldIdentifier = generator.IdentifierName(fieldName);
                    var mocksRepositoryIdentifier = generator.IdentifierName("MocksRepository");
                    var parameterTypeIdentifier = generator.IdentifierName(parameterType.ToString());

                    var memberAccessExpression = generator.MemberAccessExpression(mocksRepositoryIdentifier, generator.GenericName("GenerateStub", parameterTypeIdentifier));
                    var invocationExpression = generator.InvocationExpression(memberAccessExpression);

                    var expressionStatementSettingField = generator.AssignmentStatement(fieldIdentifier, invocationExpression);
                    expressionStatements.Add(expressionStatementSettingField);
                }
            }

            var constructorParameters = fieldDeclarations.SelectMany(x => x.DescendantNodes().OfType<VariableDeclaratorSyntax>().Select(y => y.Identifier.Text));

            var setupBody = new List<SyntaxNode>();
            setupBody.AddRange(expressionStatements);

            var targetObjectCreationExpression = generator.ObjectCreationExpression(generator.IdentifierName(className), constructorParameters.Select(x => generator.IdentifierName(x)));

            var expressionStatementTargetInstantiation = generator.AssignmentStatement(generator.IdentifierName("_target"), targetObjectCreationExpression);
            setupBody.Add(expressionStatementTargetInstantiation);

            // Generate the Clone method declaration
            var setupMethodDeclaration = generator.MethodDeclaration("Setup", null,
              null, null,
              Accessibility.Public,
              DeclarationModifiers.None,
              setupBody);
            var setupAttribute = generator.Attribute("SetUp");

            var setupMethodWithSetupAttribute = generator.InsertAttributes(setupMethodDeclaration, 0, setupAttribute);

            var members = new List<SyntaxNode>();
            members.AddRange(fieldDeclarations);
            members.Add(setupMethodWithSetupAttribute);

            var classDefinition = generator.ClassDeclaration(
             classDec.Identifier.Text + "Tests",
             typeParameters: null,
             accessibility: Accessibility.Public,
             modifiers: DeclarationModifiers.None,
             baseType: null,
             interfaceTypes: null,
             members: members);

            var testFixtureAttribute = generator.Attribute("TestFixture");

            var classWithTestFixtureAttribute = generator.InsertAttributes(classDefinition, 0, testFixtureAttribute);

            var namespaceDeclarationSyntax = docSyntaxRoot.DescendantNodes().OfType<NamespaceDeclarationSyntax>().FirstOrDefault();
            string namespaceText = namespaceDeclarationSyntax != null ? namespaceDeclarationSyntax.Name.ToString() : "MyTests";
            var namespaceDeclaration = generator.NamespaceDeclaration(namespaceText, classWithTestFixtureAttribute);

            var allNodes = new List<SyntaxNode>();
            allNodes.AddRange(usingDirectives);
            allNodes.Add(namespaceDeclaration);

            // Get a CompilationUnit (code file) for the generated code
            var newNode = generator.CompilationUnit(allNodes).NormalizeWhitespace();

            return newNode;
        }
    }
}