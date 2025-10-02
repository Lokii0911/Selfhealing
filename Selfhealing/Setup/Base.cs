using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Selfhealing.Setup
{
    public class Base
    {
        protected static IWebDriver driver;
        protected static WebDriverWait wait;
        [OneTimeSetUp]
        public void StartBrowser()
        {

            driver = new ChromeDriver();
            driver.Manage().Window.Maximize();
            wait = new WebDriverWait(driver, TimeSpan.FromSeconds(50));
        }
        [OneTimeTearDown]
        public void CloseBrowser()
        {

            driver.Quit();
            driver.Dispose();
        }

    }
    
}