using NUnit.Framework;
using NUnit.Framework.Interfaces;
using System;

namespace helloworld
{
    class TestClass
    {
        [TestFixture]
        public class TestClasss
        {
            [TestCase]
            public void AddTest()
            {
                MathsHelper helper = new MathsHelper();
                int result = helper.Add(20, 10);
                Assert.AreEqual(30, result);
            }

            [TestCase]
            public void SubtractTest()
            {
                MathsHelper helper = new MathsHelper();
                int result = helper.Subtract(20, 10);
                Assert.AreEqual(10, result);
            }
        }
    }
}
