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
                return wait.Until(ExpectedConditions.ElementExists(by));
            }
            catch (NoSuchElementException)
            {
                return TryWithLlmFallback(hint);
            }
            catch (WebDriverTimeoutException)
            {
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
                .Where(n => n.Name == "script" || n.Name == "style")
                .ToList()
                .ForEach(n => n.Remove());
            string cleanedHtml = doc.DocumentNode.OuterHtml;
            File.WriteAllText("HtmlForAI.html", cleanedHtml);
            Console.WriteLine("Cleaned HTML saved to HtmlForAI.html");
            string xpath = LlmHelper.GetXPath(cleanedHtml, hint).Result;

            if (!string.IsNullOrEmpty(xpath))
            {
                Console.WriteLine($"👉 Raw XPath from LLM: {xpath}");
                xpath = xpath.Replace("```", "").Trim();
                var lines = xpath.Split('\n')
                                 .Select(l => l.Trim())
                                 .Where(l => l.StartsWith("/") || l.StartsWith("./"))
                                 .ToList();

                if (lines.Count > 0)
                    xpath = lines[0];

                Console.WriteLine($"👉 Cleaned XPath used: {xpath}");
                return driver.FindElement(By.XPath(xpath));
            }

            throw new Exception($"Element not found and no XPath from LLM for hint: {hint}");
        }

    }
}
