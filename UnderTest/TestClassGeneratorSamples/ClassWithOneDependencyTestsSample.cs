using NUnit.Framework;
using Rhino.Mocks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestClassGeneratorSamples
{
    [TestFixture]
    class ClassWithOneDependencyTestsSample
    {
        IDependency _dependency;
        ClassWithOneDependency _target;

        [SetUp]
        public void Setup()
        {
            _dependency = MockRepository.GenerateStub<IDependency>();
            _target = new ClassWithOneDependency(_dependency);
        }
    }
}
