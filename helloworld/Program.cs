using NUnit.Framework;
using NUnit.Framework.Interfaces;
using System;

namespace helloworld
{
    [TestFixture]
    public class Program
    {
        [SetUp]
        public void SetUp()
        {

        }

        [Test]
        public void IsTrue()
        {
        //    Console.WriteLine("Hello, World!");
            Assert.IsTrue(2 + 2 == 5);

        }
    }
}
