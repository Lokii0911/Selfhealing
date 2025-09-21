using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

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

        return config["OpenAi:ApiKey"];  // 👈 store under "OpenRouter"
    }

    public static async Task<string> GetXPath(string html, string hint)
    {
        Console.WriteLine("Requesting XPath from LLM...");

        var payload = new
        {
            model = "openai/gpt-oss-20b:free",
            messages = new[]
            {
                new { role = "system", content = "You must output ONLY a valid XPath expression. No comments, no explanation, no extra text." },
                new { role = "user", content = $"Given this HTML:\n{html}\nAnd the hint: {hint}\nReturn ONLY the best relative XPath. No explanation." }
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
