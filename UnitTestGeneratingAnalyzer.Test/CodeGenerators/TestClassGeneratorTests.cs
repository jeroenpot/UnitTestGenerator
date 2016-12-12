using NUnit.Framework;
using System.Linq;
using UnitTestGeneratingAnalyzer.CodeGenerators;
using UnitTestGeneratingAnalyzer.Test.Helpers;

namespace UnitTestGeneratingAnalyzer.Test.CodeGenerators
{
    [TestFixture]
    public class TestClassGeneratorTests
    {
        private NUnitTestClassGenerator _target;

        [SetUp]
        public void Setup()
        {
            _target = new NUnitTestClassGenerator();
        }

        [Test]
        public void GeneratesTestClass_For_ClassWithoutDependencies()
        {
            var solution = new RoslynSolutionProvider().GetSolutionAsync();
            var document = solution.Documents.Where(x => x.Name == "ClassWithoutDependencies.cs").First();

            var testClassRoot = _target.TestClassRootAsync(document, "ClassWithoutDependencies");
        }

        [Test]
        public void GeneratesTestClass_For_ClassWithOneDependencies()
        {
            var solution = new RoslynSolutionProvider().GetSolutionAsync();
            var document = solution.Documents.Where(x => x.Name == "ClassWithOneDependency.cs").First();

            var testClassRoot = _target.TestClassRootAsync(document, "ClassWithOneDependency");
        }
    }
}
