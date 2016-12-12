using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;
using System.Threading.Tasks;
using System;

namespace UnitTestGeneratingAnalyzer.Test.Helpers
{
    public class RoslynSolutionProvider
    {
        public Project GetSolutionAsync()
        {
            var msWorkspace = MSBuildWorkspace.Create();
            //string currentDir = Environment.CurrentDirectory;
            //var relativePath = @"..\..\..\..\UnderTest\TestClassGeneratorSamples\TestClassGeneratorSamples.sln";
            //string solutionPath = currentDir + relativePath;

            string solutionPath = @"C:\Users\Tuba\Desktop\Roslyn presentation\Projects\UnitTestGeneratingAnalyzer\UnderTest\TestClassGeneratorSamples\TestClassGeneratorSamples.csproj";

            //You must install the MSBuild Tools or this line will throw an exception:
            return msWorkspace.OpenProjectAsync(solutionPath).Result;
        }
    }
}
