using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace UnitTestGeneratingAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class UnitTestGeneratingAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "D1";

        internal static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.AnalyzerTitle), Resources.ResourceManager, typeof(Resources));
        internal static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.AnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
        internal static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.AnalyzerDescription), Resources.ResourceManager, typeof(Resources));
        internal const string Category = "CodeGeneration";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Info, isEnabledByDefault: true, description: Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeSyntaxNode, SyntaxKind.ClassDeclaration);
        }

        private void AnalyzeSyntaxNode(SyntaxNodeAnalysisContext context)
        {
            var classDec = context.Node as ClassDeclarationSyntax;

            if (!classDec.Modifiers.Any(SyntaxKind.AbstractKeyword))
            {
                var diagnostic = Diagnostic.Create(Rule, classDec.Identifier.GetLocation(), classDec.Identifier.Text + "Tests");

                context.ReportDiagnostic(diagnostic);
            }
        }
    }
}
