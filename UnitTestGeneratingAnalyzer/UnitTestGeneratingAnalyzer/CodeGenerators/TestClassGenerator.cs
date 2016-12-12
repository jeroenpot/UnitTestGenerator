using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using System.Threading.Tasks;

namespace UnitTestGeneratingAnalyzer.CodeGenerators
{
    public interface ITestClassGenerator
    {
        Task<SyntaxNode> TestClassRootAsync(Document document, string className);
    }

    public class NUnitTestClassGenerator : ITestClassGenerator
    {
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
