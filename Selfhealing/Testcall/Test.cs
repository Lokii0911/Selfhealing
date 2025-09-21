using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotnetSelenium.TestCase;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using Selfhealing.Setup;    

namespace Selfhealing.Testcall
{
    public class Test:Base
    {
        [Test]
        public void TestLogin()
        {
           Login login = new Login(driver,wait);
           login.DoLogin("Admin", "admin123");
        }   

    }
}
