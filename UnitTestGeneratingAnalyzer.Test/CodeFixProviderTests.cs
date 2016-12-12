using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Framework;
using System;
using System.Linq;
using TestHelper;

namespace UnitTestGeneratingAnalyzer.Test
{
    [TestFixture]
    public class CodeFixProviderTests : CodeFixVerifier
    {
        //[TestCase("public class A{public A(){}}")]
        //[TestCase("public sealed class A{IB _b; public A(IB b){_b=b;}}")]
        //[TestCase("public class A{public C{get;set;}}}")]
        public void GeneratesTestClass_For_Class(string classDec)
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
        {0}
    }";

            var fixtest = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;

    namespace ConsoleApplication1
    {
        {0}
        {1}
    }";
            var testCase = string.Format(test, classDec);
            var expected = string.Format(fixtest, classDec, "hereComesTestClass");

            var actualRootNode = GetRootNodeAfterFix(testCase);
            var classDeclarations= actualRootNode.DescendantNodes().OfType<ClassDeclarationSyntax>().ToList();

            Assert.AreEqual(2, classDeclarations.Count);

            VerifyCSharpFix(testCase, expected);
        }

        protected override CodeFixProvider GetCSharpCodeFixProvider()
        {
            return new UnitTestGeneratingAnalyzerCodeFixProvider();
        }
    }
}