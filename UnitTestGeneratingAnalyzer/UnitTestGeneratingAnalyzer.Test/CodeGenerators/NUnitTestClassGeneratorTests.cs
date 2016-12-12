using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using NUnit.Framework;
using UnitTestGeneratingAnalyzer.CodeGenerators;

namespace UnitTestGeneratingAnalyzer.Test.CodeGenerators
{
    [TestFixture]
    public class NUnitTestClassGeneratorTests
    {
        private NUnitTestClassGenerator _target;

        [SetUp]
        public void Setup()
        {
            _target = new NUnitTestClassGenerator();
        }

        [Test]
        public void Generate()
        {
            var code = @"
                        namespace ConsoleApplication1
                        {
                            public class SomeClass
                            {
                                IDependency _dependency;

                                public SomeClass(IDependency dependency)
                                {
                                    _dependency=dependency;
                                }
                            }
                        }";
            var codeIDependency = @"
                        namespace ConsoleApplication1
                        {
                            public interface IDependency
                            {
                                string Name;
                            }
                        }";

            var workspace1 = new AdhocWorkspace();
            var projectInfo = ProjectInfo.Create(ProjectId.CreateNewId(), VersionStamp.Create(), "NewProject", "NewProject", LanguageNames.CSharp);
            var newProject = workspace1.AddProject(projectInfo);
            var newDocumentIDependency = newProject.AddDocument("IDependency.cs", SourceText.From(codeIDependency));
            //var newDocument = newProject.AddDocument("NewFile.cs", SourceText.From(code));
           
            newProject.AddMetadataReference(MetadataReference.CreateFromFile(typeof(System.Object).Assembly.Location));
            
           // var testClassRoot = _target.TestClassRootAsync(newDocumentIDependency, "SomeClass");
        }
            }
}
