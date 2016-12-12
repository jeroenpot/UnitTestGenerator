using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CSharp.Syntax;
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

        internal async Task<Solution> AddTestClassAsync(Document document, ClassDeclarationSyntax declaration, CancellationToken cancellationToken)
        {
            var testClassGenerator = new NUnitTestClassGenerator();

            var rootTestClassDocument = await testClassGenerator.TestClassRootAsync(document, declaration.Identifier.Text);

            var newSolution = document.Project.Solution;
            var newProject = newSolution.Projects.Where(x => x.Id == document.Project.Id).First();
            var documentName = declaration.Identifier.Text + "Tests.cs";
            newProject.AddDocument(documentName, rootTestClassDocument);

            return newSolution;
        }
    }
}