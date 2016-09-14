using NUnit.Framework;
using NUnit.Framework.Interfaces;
using OpenQA.Selenium;
using OpenQA.Selenium.Remote;
using System;

namespace Dell.WebTests
{
    [TestFixture]
    public class Init  // other class should extend this class, beacuse I use this class as a father class.
    {
        public MyRemoteWebDriver driver;

        [SetUp]
        public void SetUp()
        {

            var commandExecutorUri = new Uri("http://ondemand.saucelabs.com/wd/hub");

            // set up the desired capabilities
            DesiredCapabilities desiredCapabilites = new DesiredCapabilities(); // set the desired browser
            desiredCapabilites.SetCapability(CapabilityType.BrowserName, "firefox");
            desiredCapabilites.SetCapability(CapabilityType.Version, "45");
            desiredCapabilites.SetCapability("platform", "Windows 7"); // operating system to use
            desiredCapabilites.SetCapability("username", Environment.GetEnvironmentVariable("SAUCE_USER_NAME"));
            desiredCapabilites.SetCapability("accessKey", Environment.GetEnvironmentVariable("SAUCE_API_KEY"));
            desiredCapabilites.SetCapability("screenResolution", "1920x1200");

            String jobName = TestContext.CurrentContext.Test.Name + "_firefox_45_Windows 7";
            desiredCapabilites.SetCapability("name", jobName);// give the test a name

            String buildname = Environment.GetEnvironmentVariable("JOB_NAME") + "_" + jobName + "_#" + Environment.GetEnvironmentVariable("BUILD_NUMBER");
            desiredCapabilites.SetCapability("build", buildname);

            // start a new remote web driver session on Sauce Labs
            driver = new MyRemoteWebDriver(commandExecutorUri, desiredCapabilites);
            driver.Manage().Timeouts().ImplicitlyWait(TimeSpan.FromSeconds(30));

            // navigate to the page under test
            string url = GetSiteUrl();
            Console.WriteLine("Site URL: {0}/", url);
            driver.Navigate().GoToUrl(url + "/home/signIn");

            Assert.AreEqual("Sign In | Spotlight", driver.Title);
            Console.WriteLine("Title is correct.");
            LogIn.logIn(driver, Constant.USER_NAME, Constant.PASSWORD);

            // verify the browser was navigated to the correct page
            Console.WriteLine("URL: {0}", driver.Url);

            //update sample date
            //ExecuteCommands.ExecuteCommand(Constant.UPDATE_SAMPLE_DATE);
            //String SessionId = ((MyRemoteWebDriver)driver).GetSessionID().ToString();
            //Console.WriteLine("Link On Saucelabs: " + "https://saucelabs.com/beta/tests/" + SessionId + "/commands");
        }

        [TearDown]
        public void Cleanup()
        {
            // get the status of the current test
            bool passed = TestContext.CurrentContext.Result.Outcome.Status == TestStatus.Passed;
            try
            {
                // log the result to sauce labs
                ((IJavaScriptExecutor)driver).ExecuteScript("sauce:job-result=" + (passed ? "passed" : "failed"));
            }
            finally
            {
                //Delete DS let the original data remains the same
                ExecuteCommands.ExecuteCommand(Constant.DELETE_DS);
                // terminate the remote webdriver sessionS
                driver.Quit();
            }
        }

        private static string GetSiteUrl()
        {
            return Environment.GetEnvironmentVariable("SITE_URL") ??
                "https://test.spotlightessentials.com";
        }
    }
}
