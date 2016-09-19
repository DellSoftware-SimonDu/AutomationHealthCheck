using OpenQA.Selenium;
using System;


namespace Dell.WebTests
{
    public class LogIn
    {
        public static void logIn(IWebDriver driver, String username, String password)
        {
            driver.FindElement(By.Id("Email")).Clear();
            driver.FindElement(By.Id("Email")).SendKeys(username);// enter username
            driver.FindElement(By.Id("Password")).Clear();
            driver.FindElement(By.Id("Password")).SendKeys(password);// enter passord
            driver.FindElement(By.Id("btnSignIn")).Submit();// log in
        }
    }
}
