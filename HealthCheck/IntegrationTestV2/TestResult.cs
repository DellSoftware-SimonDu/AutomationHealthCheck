using System;
using System.IO;
using NUnit.Framework;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.Net;
using System.Management.Automation;
using System.Collections.ObjectModel;

namespace IntegrationTestV2
{
    [TestFixture]
    public class TestResult 
    {
        long timeStamp = Convert.ToInt64((DateTime.Now - DateTime.Parse("1/1/1970 0:0:0")).TotalMilliseconds);

        [SetUp]
        public void UpdateSampleData()
        {
            ExecutePowerShell.ExecutePoweShell(@"C:\Scripts\UploadSampleData1.ps1", timeStamp);
        }
        [TearDown]
        public void CleanUp()
        {
            ExecutePowerShell.ExecutePoweShell(@"C:\Scripts\CleanUp.ps1", timeStamp);
        }

        [Test]
        public void TestResultJson()
        {
            Assert.AreEqual(1, 1);
        }
    }
}
