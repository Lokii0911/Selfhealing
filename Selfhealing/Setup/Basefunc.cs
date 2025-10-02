using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Selfhealing.Setup
{
     public class Basefunc
    {
        protected IWebDriver driver;
        protected WebDriverWait wait;

        public Basefunc(IWebDriver driver, WebDriverWait wait)
        {
            this.driver = driver;
            this.wait = wait;
        }

        public IWebElement SafeFind(By by, string hint = "")
        {
            try
            {
                return wait.Until(ExpectedConditions.ElementIsVisible(by));
            }
            catch (Exception ex) when (ex is NoSuchElementException || ex is WebDriverTimeoutException || ex is InvalidSelectorException)
            {
                Console.WriteLine($"SafeFind fallback triggered for {by}: {ex.Message}");
                return TryWithLlmFallback(hint);
            }
            catch (Exception ex)
            {
                
                Console.WriteLine($"Unexpected error in SafeFind: {ex.Message}");
                throw;
            }
        }

        private IWebElement TryWithLlmFallback(string hint)
        {
            string rawHtml = driver.PageSource;

            var doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(rawHtml);
            doc.DocumentNode.Descendants()
                .Where(n => n.Name == "script" || n.Name == "style" || n.Name == "svg" || n.Name == "path")
                .ToList()
                .ForEach(n => n.Remove());
            foreach (var node in doc.DocumentNode.Descendants())
            {
                node.Attributes
                    .Where(a =>
                        a.Name == "style" ||                     
                        a.Name.StartsWith("aria-") ||            
                        a.Name.StartsWith("onclick") ||           
                        (a.Name == "class" && a.Value.Length > 150) 
                    )
                    .ToList()
                    .ForEach(a => a.Remove());
            }

            string cleanedHtml = doc.DocumentNode.OuterHtml;
            File.WriteAllText("HtmlForAI.html", cleanedHtml);
            Console.WriteLine("Cleaned HTML saved to HtmlForAI.html");
            string xpath = LlmHelper.GetXPath(cleanedHtml, hint).Result;
            if (!string.IsNullOrEmpty(xpath))
            {
                Console.WriteLine($"👉 Raw XPath from LLM: {xpath}");
                xpath = xpath.Replace("```", "")
                             .Replace("<!--xpath-->", "")
                             .Replace("<![CDATA[", "")
                             .Replace("]]>", "")
                             .Replace("**", "")
                             .Trim();
                xpath = System.Text.RegularExpressions.Regex.Replace(xpath, @"<!--.*?-->", "");
                if (xpath.StartsWith("xpath=", StringComparison.OrdinalIgnoreCase))
                    xpath = xpath.Substring("xpath=".Length).Trim();

                xpath = System.Text.RegularExpressions.Regex.Replace(xpath, @"^/+", "//");

                var lines = xpath.Split('\n')
                                 .Select(l => l.Trim())  
                                 .Where(l => l.Length > 0
                                             && !l.StartsWith("//<!--")
                                             && !l.StartsWith("<!--")
                                             && (l.StartsWith("/") || l.StartsWith(".")))
                                 .ToList();

                if (lines.Count > 0)
                    xpath = lines[0];

                if (xpath.StartsWith("(") && xpath.EndsWith(")"))
                {
                    
                    if (!System.Text.RegularExpressions.Regex.IsMatch(xpath, @"\)\[\d+\]$"))
                    {
                        xpath = xpath.Substring(1, xpath.Length - 2);
                    }
                }

                
                if (xpath.StartsWith("(") && xpath.Contains(")[") && !xpath.Contains(")[1"))
                {
                    xpath = xpath.Trim('(', ')');
                }

                if (xpath.StartsWith("/") && !xpath.StartsWith("//"))
                    xpath = "/" + xpath;

                if (xpath.StartsWith(".//"))
                    xpath = xpath.Substring(1);

                if (!xpath.StartsWith("/") && !xpath.StartsWith("."))
                    xpath = "//" + xpath;

                Console.WriteLine($"👉 Cleaned XPath used: {xpath}");
                return driver.FindElement(By.XPath(xpath));
            }

            throw new Exception($"Element not found and no XPath from LLM for hint: {hint}");
        }

        public void SafeClick(By locator)
        {
            try
            {
                var element = wait.Until(ExpectedConditions.ElementToBeClickable(locator));
                element.Click();
            }
            catch (WebDriverTimeoutException)
            {
                Console.WriteLine($"Element not clickable : {locator}");
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error during SafeClick: {e.Message}");
            }
        }

        public void Sendvalues(By locator, string value)
        {
            try
            {
                wait.Until(ExpectedConditions.ElementToBeClickable(locator)).SendKeys(value);

            }
            catch (WebDriverTimeoutException)
            {
                Console.WriteLine($"Element not clickable : {locator}");
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error during SafeClick: {e.Message}");
            }
        }

    }
}
