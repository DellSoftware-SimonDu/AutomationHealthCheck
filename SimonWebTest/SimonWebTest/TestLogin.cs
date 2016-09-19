using System;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Remote;
using NUnit.Framework.Interfaces;

namespace Dell.WebTests
{
    [TestFixture]
    public class TestLogin
    {
        private MyRemoteWebDriver _driver;
        [SetUp]
        public void Init()
        {
            var commandExecutorUri = new Uri("http://ondemand.saucelabs.com/wd/hub");

            // set up the desired capabilities
            var desiredCapabilites = new DesiredCapabilities("internet explorer", "11", Platform.CurrentPlatform); // set the desired browser
            desiredCapabilites.SetCapability("platform", "Windows 7"); // operating system to use
            desiredCapabilites.SetCapability("username", "simondu123");
            desiredCapabilites.SetCapability("accessKey", "e97b0dde-bd4e-4f09-8261-6a344ff8556c");
            desiredCapabilites.SetCapability("name", TestContext.CurrentContext.Test.Name); // give the test a name

            // start a new remote web driver session on Sauce Labs
            _driver = new MyRemoteWebDriver(commandExecutorUri, desiredCapabilites);
            Console.WriteLine("SauceOnDemandSessionID={0} job-name={1}",
                _driver.GetSessionID(), "TestLogin");
            Console.WriteLine("https://saucelabs.com/tests/{0}",
                _driver.GetSessionID());
            _driver.Manage().Timeouts().ImplicitlyWait(TimeSpan.FromSeconds(30));

            // navigate to the page under test
            string url = GetSiteUrl();
            Console.WriteLine("Site URL: {0}/", url);
            _driver.Navigate().GoToUrl(url + "/home/signIn");
        }

        [TearDown]
        public void Cleanup()
        {
            // get the status of the current test
            bool passed = TestContext.CurrentContext.Result.Outcome.Status == TestStatus.Passed;
            try
            {
                // log the result to sauce labs
                ((IJavaScriptExecutor)_driver).ExecuteScript("sauce:job-result=" + (passed ? "passed" : "failed"));
            }
            finally
            {
                // terminate the remote webdriver session
                _driver.Quit();
            }
        }

        [Test]
        public void UserShouldBeAbleToSignIn()
        {
            Console.WriteLine("Hello");
            Assert.AreEqual("Sign In | Spotlight", _driver.Title);
            Console.WriteLine("Title is correct.");

            _driver.FindElement(By.Id("Email")).SendKeys("test5@example.com");
            _driver.FindElement(By.Id("Password")).SendKeys("Quest123");
            _driver.FindElement(By.Id("Email")).Submit();
            Console.WriteLine("Submitted form.");

            // verify the browser was navigated to the correct page
            Console.WriteLine("URL: {0}", _driver.Url);
        }

        private static string GetSiteUrl()
        {
            return Environment.GetEnvironmentVariable("SITE_URL") ??
                "https://test.spotlightessentials.com";
        }
    }
}
