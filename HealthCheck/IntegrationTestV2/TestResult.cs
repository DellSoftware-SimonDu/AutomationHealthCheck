using System;
using NUnit.Framework;
using System.Linq;
using System.Text;
using System.Collections.Generic;


namespace IntegrationTestV2
{
    [TestFixture]
    public class TestResult 
    {
        long timeStamp = Convert.ToInt64((DateTime.Now - DateTime.Parse("1/1/1970 0:0:0")).TotalMilliseconds);
        string scriptPath = @"C:\Program Files (x86)\Jenkins\jobs\healthcheckworker-systemhealthchecks2-NunitTest\workspace\HealthCheck\IntegrationTestV2\PSScripts\";
        [SetUp]
        public void UpdateSampleData()
        {
            ExecutePowerShell.ExecutePoweShell(scriptPath + "UploadSampleData.ps1", timeStamp);
        }
        [TearDown]
        public void CleanUp()
        {
            ExecutePowerShell.ExecutePoweShell(scriptPath + "CleanUp.ps1", timeStamp);
        }

        [Test]
        public void TestResultJson()
        {
            Assert.AreEqual(1, 1);
            
        }
    }
}
