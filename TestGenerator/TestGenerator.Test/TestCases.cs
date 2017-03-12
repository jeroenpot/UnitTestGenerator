using System.Collections;
using NUnit.Framework;

namespace TestGenerator.Test
{
    class TestCases : IEnumerable
    {
        public IEnumerator GetEnumerator()
        {
            yield return new TestCaseData(ClassWithoutSetup, ClassWithSetup);
            yield return new TestCaseData(ClassWithSetup, ClassWithSetup);
            yield return new TestCaseData(ClassWithFilledInSetup, ClassWithSetup);
        }

        const string ClassWithoutSetup = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;
    using NUnit.Framework;

    namespace ConsoleApplication1
    {
        class TypeNameTests
        {   
        }
    }";

        const string ClassWithSetup = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;
    using NUnit.Framework;

    namespace ConsoleApplication1
    {
        class TypeNameTests
        {   
            [SetUp]
            public void Setup()
            {
            }
        }
    }";

        const string ClassWithFilledInSetup = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;
    using NUnit.Framework;

    namespace ConsoleApplication1
    {
        class TypeNameTests
        {   
            [SetUp]
            public void Setup()
            {
                var x=1;
            }
        }
    }";
    }
}
