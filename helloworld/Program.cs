using NUnit.Framework;
using NUnit.Framework.Interfaces;
using System;

namespace helloworld
{
    [TestFixture]
    public class Program
    {
        //  static void Main(string[] args)
        //  {
        //      System.Console.WriteLine("Hello, World!");
        //       Console.ReadKey();
        //   }

        [SetUp]
        public void SetUp()
        {

        }

        [Test]
        public void HelloWorld()
        {
            Console.WriteLine("Hello, World!");
                 
        }
    }
}
