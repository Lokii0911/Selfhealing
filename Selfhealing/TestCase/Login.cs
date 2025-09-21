using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using NUnit.Framework;
using SeleniumExtras.WaitHelpers;
using Selfhealing.Setup;

namespace DotnetSelenium.TestCase
{
    public class Login:Basefunc
    {
        private readonly IWebDriver driver;
        private readonly WebDriverWait wait;

        private static readonly By username = By.Name("user");// wrong locator
        private static readonly By password = By.Name("pass");// wrong locator
        private static readonly By button = By.CssSelector("button[type='subit']");// wrong locator
        private static readonly By dashboardHeader = By.XPath("//h6[text()='Dashboard']");

        public Login(IWebDriver driver, WebDriverWait wait):base(driver,wait)
        {
            this.driver = driver;
            this.wait = wait;
        }

        public Login DoLogin(string usernameValue, string passwordValue)
        {
            driver.Navigate().GoToUrl("https://opensource-demo.orangehrmlive.com/");
            SafeFind(username, "Username field in the login page").SendKeys(usernameValue);
            SafeFind(password, "Password field in the login page").SendKeys(passwordValue);
            SafeFind(button, "Login button on the login page").Click();
            return this;
        }



    }
}