using System;
using OpenQA.Selenium;
using OpenQA.Selenium.Remote;

namespace Dell.WebTests
{
    // RemoteWebDriver class has SessionId property as protected.
    // This class is a workaround to get access to it.
    public class MyRemoteWebDriver : RemoteWebDriver
    {
        public MyRemoteWebDriver(Uri remoteAddress, ICapabilities desiredCapabilities)
          : base(remoteAddress, desiredCapabilities, RemoteWebDriver.DefaultCommandTimeout)
        {
        }

        public string GetSessionID()
        {
            return this.SessionId.ToString();
        }
    }
}
