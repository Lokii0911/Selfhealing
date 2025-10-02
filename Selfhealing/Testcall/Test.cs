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
using Selfhealing.TestCase;

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
        [Test]
        public void TestAddEmp()
        {
            Login log = new Login(driver, wait);
            AddEmployee add = new AddEmployee(driver, wait);
            log
                .DoLogin("Admin","admin123");
            add
                .Emp()
                .AddButton("Lokesh", "Kumar", "S", "1001")
                .logincredetials("lokii#23", "Lokii@123", "Lokii@123");
               
        }   

    }
}
