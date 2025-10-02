using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection.Emit;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using static System.Net.Mime.MediaTypeNames;

public static class LlmHelper
{
    private static readonly HttpClient client;

    static LlmHelper()
    {
        var apiKey = GetApiKey();

        client = new HttpClient
        {
            BaseAddress = new Uri("https://openrouter.ai/api/v1/")
        };

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", apiKey);

        client.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));
    }

    private static string GetApiKey()
    {
        var config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        return config["OpenAI:ApiKey"]; 
    }

    public static async Task<string> GetXPath(string html, string hint)
    {
        Console.WriteLine("Requesting XPath from LLM...");

        var payload = new
        {
            model = "openai/gpt-oss-20b:free",
            messages = new[]
            {
                new { role = "system", content = "You must output ONLY a valid XPath expression. No comments, no explanation, no extra text,No Extra add one before and after the Xpath's" },
                new { role = "system", content = "You are an assistant that generates only accurate relative XPath expressions for Selenium. " + "Never include ``` markers. " +"Always ensure buttons with text are matched using `//button[.//span[normalize-space()='TEXT']]`. " + "For links, use `//a[.//span[normalize-space()='TEXT']]`. " +"Return ONLY the XPath string, nothing else." },
                new { role = "system", content ="You must output ONLY one valid relative XPath. Always prefer unique attributes: id, name, data - *, placeholder, type, aria - label.If targeting buttons / links with visible text, use contains(normalize-space(.), 'TEXT'). Never return overly generic XPaths like //a[.//span] or //* without filters. Output ONLY the XPath, no explanations." },
                new { role = "user", content = $"Given this HTML:\n{html}\nAnd the hint: {hint}\nReturn ONLY the best accurate relative XPath. No explanation." }
            }
        };

        var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

        var response = await client.PostAsync("chat/completions", content);
        var result = await response.Content.ReadAsStringAsync();

        try
        {
            using var doc = JsonDocument.Parse(result);

            if (doc.RootElement.TryGetProperty("choices", out var choices) &&
                choices.GetArrayLength() > 0)
            {
                var firstChoice = choices[0];

                if (firstChoice.TryGetProperty("message", out var message) &&
                    message.TryGetProperty("content", out var contentProp))
                {
                    return contentProp.GetString().Trim();
                }
            }

            if (doc.RootElement.TryGetProperty("error", out var error))
            {
                var errMsg = error.GetProperty("message").GetString();
                throw new Exception($"OpenRouter API error: {errMsg}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠️ Failed to parse OpenRouter response: {ex.Message}");
            Console.WriteLine(result);
        }

        return string.Empty;
    }
}
