using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Framework;
using System;
using TestHelper;

namespace UnitTestGeneratingAnalyzer.Test
{
    [TestFixture]
    public class DiagnosticAnalyzerTests : CodeFixVerifier
    {
        [TestCase("using System; public interface ITypeName{}")]
        [TestCase("using System; public enum TypeName{}")]
        [TestCase("using System; public abstract class TypeNameBase{}")]
        [TestCase("using System; public struct TypeNameBase{}")]
        public void When_NoClass_DoesNotDisplayDiagnostic(string code)
        {
            VerifyCSharpDiagnostic(code);
        }

        [Test]
        public void When_ThereIsANonAbstractClass_Diagnostic_displayed_on_correct_span()
        {
            var test = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;

    namespace ConsoleApplication1
    {
        class TypeName
        {   
        }
    }";
            var expected = new DiagnosticResult
            {
                Id = UnitTestGeneratingAnalyzerAnalyzer.DiagnosticId,
                Message = String.Format("Generate '{0}' in the same file", "TypeNameTests"),
                Severity = DiagnosticSeverity.Info,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 11, 15)
                        }
            };

            VerifyCSharpDiagnostic(test, expected);
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new UnitTestGeneratingAnalyzerAnalyzer();
        }
    }
}