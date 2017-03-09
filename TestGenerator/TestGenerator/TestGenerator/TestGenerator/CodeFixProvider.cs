using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;

namespace TestGenerator
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(TestGeneratorCodeFixProvider)), Shared]
    public class TestGeneratorCodeFixProvider : CodeFixProvider
    {
        private const string title = "(Re-)Generate SetUp";

        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get { return ImmutableArray.Create(TestGeneratorAnalyzer.DiagnosticId); }
        }

        public sealed override FixAllProvider GetFixAllProvider()
        {
            // See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/FixAllProvider.md for more information on Fix All Providers
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

            if (declaration.Identifier.Text.EndsWith("Tests"))
            {
                // Register a code action that will invoke the fix.
                context.RegisterCodeFix(
                    CodeAction.Create(
                        title: title,
                        createChangedDocument: c => CreateSetupMethod(context.Document, declaration, c),
                        equivalenceKey: title),
                    diagnostic);
            }
        }

        private async Task<Document> CreateSetupMethod(Document document, ClassDeclarationSyntax classDecl, CancellationToken cancellationToken)
        {
            var generator = SyntaxGenerator.GetGenerator(document);
            var setupMethodDeclaration = SetupMethod(generator);

            var root = await document.GetSyntaxRootAsync(cancellationToken);
            var existingSetupMethod =root.DescendantNodes().OfType<MethodDeclarationSyntax>()
                                                            .FirstOrDefault(_ => _.Identifier.Text == "Setup");

            SyntaxNode newRoot;
            if (existingSetupMethod != null)
            {
                newRoot = root.ReplaceNode(existingSetupMethod, setupMethodDeclaration);
            }
            else
            {
                var newClassDecl= classDecl.AddMembers(setupMethodDeclaration as MemberDeclarationSyntax);
                newRoot = root.ReplaceNode(classDecl, newClassDecl);
            }

            var newDocument = document.WithSyntaxRoot(newRoot);
            return newDocument;
        }

        private SyntaxNode SetupMethod(SyntaxGenerator generator)
        {
            var setupMethodDeclaration = generator.MethodDeclaration("Setup", null,
            null, null,
            Accessibility.Public,
            DeclarationModifiers.None,
            null);
            var setupAttribute = generator.Attribute("SetUp");

            var setupMethodWithSetupAttribute = generator.InsertAttributes(setupMethodDeclaration, 0, setupAttribute);
            return setupMethodWithSetupAttribute;
        }
    }
}