using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using BlazorApp.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding;

public class GeminiWeatherSummaryService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;

    public GeminiWeatherSummaryService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
    }

    public async Task<string> GenerateSummaryAsync(string weatherData)
    {
        var apiKey = _configuration["QroqAI:ApiKey"]; // Store it in appsettings.json under QroqAI section
        var endpoint = "https://api.gorq.cloud/v1/completion"; // Replace with the actual Qroq endpoint if different

        var requestBody = new
        {
            model = "gorq-1", // Adjust to correct model name if needed
            prompt = $"Summarize this weather data in a friendly format: {weatherData}",
            max_tokens = 200,
            temperature = 0.7
        };

        var json = JsonSerializer.Serialize(requestBody);
        var client = _httpClientFactory.CreateClient();

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

        var request = new HttpRequestMessage(HttpMethod.Post, endpoint)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };

        var response = await client.SendAsync(request);

        if (response.IsSuccessStatusCode)
        {
            var responseContent = await response.Content.ReadAsStringAsync();

            using var doc = JsonDocument.Parse(responseContent);
            var summary = doc.RootElement
                             .GetProperty("choices")[0]
                             .GetProperty("text")
                             .GetString();

            return summary?.Trim() ?? "No summary returned from Qroq AI.";
        }
        else
        {
            var error = await response.Content.ReadAsStringAsync();
            return $"Failed to generate summary from Qroq AI: {response.StatusCode} - {error}";
        }
    }

    // ✅ New: Chatbot method using message history
    public async Task<string> GenerateChatResponseAsync(List<GeminiChatMessage> messages)
    {
        var apiKey = _configuration["Gemini:ApiKey"];
        var endpoint = $"https://generativelanguage.googleapis.com/v1/models/gemini-1.5-pro:generateContent?key={apiKey}";

        var contents = messages.Select(m => new
        {
            role = m.Role,
            parts = new[] { new { text = m.Content } }
        });

        var requestBody = new { contents = contents };

        var json = JsonSerializer.Serialize(requestBody);
        var client = _httpClientFactory.CreateClient();

        var request = new HttpRequestMessage(HttpMethod.Post, endpoint)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };

        var response = await client.SendAsync(request);

        if (response.IsSuccessStatusCode)
        {
            var responseContent = await response.Content.ReadAsStringAsync();

            using var doc = JsonDocument.Parse(responseContent);
            var reply = doc.RootElement
                           .GetProperty("candidates")[0]
                           .GetProperty("content")
                           .GetProperty("parts")[0]
                           .GetProperty("text")
                           .GetString();

            return reply ?? "No reply from Gemini.";
        }
        else
        {
            var error = await response.Content.ReadAsStringAsync();
            return $"Gemini error: {response.StatusCode} - {error}";
        }
    }
    public async Task<string> GenerateForecastSummaryAsync(string forecastPrompt)
    {
        var apiKey = _configuration["Gemini:ApiKey"];
        var model = "models/gemini-pro";
        var url = $"https://generativelanguage.googleapis.com/v1/models/gemini-1.5-pro:generateContent?key={apiKey}";

        var body = new
        {
            contents = new[]
            {
            new
            {
                parts = new[]
                {
                    new { text = $"Generate a 5-day weather summary in simple language to user easly understand for users based on this data:\n\n{forecastPrompt}" }
                }
            }
        }
        };

        var client = _httpClientFactory.CreateClient();
        var request = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");

        var response = await client.PostAsync(url, request);

        if (response.IsSuccessStatusCode)
        {
            var json = await response.Content.ReadAsStringAsync();

            try
            {
                using var doc = JsonDocument.Parse(json);
                return doc.RootElement
                          .GetProperty("candidates")[0]
                          .GetProperty("content")
                          .GetProperty("parts")[0]
                          .GetProperty("text")
                          .GetString() ?? "Gemini returned no summary.";
            }
            catch (Exception ex)
            {
                return $"Failed to parse Gemini response: {ex.Message}";
            }
        }
        else
        {
            var error = await response.Content.ReadAsStringAsync();
            return $"Failed to generate summary from Gemini: {response.StatusCode} - {error}";
        }
    }

    public async Task<string> GenerateAnalysisSummaryAsync(List<WeatherData> weatherDataList, string city, int days)
    {
        var apiKey = _configuration["Gemini:ApiKey"];
        var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-pro:generateContent?key={apiKey}";

        var summaryPrompt = $"Summarize the historical weather data for {city} over the past {days} days in a user-friendly format. " +
                            $"Here are the details:\n\n" +
                            string.Join("\n", weatherDataList.Select(d =>
                                $"{d.Date:MMM dd}: Temp {d.Temperature}°C, Humidity {d.Humidity}%"));

        var requestBody = new
        {
            contents = new[]
            {
            new
            {
                parts = new[]
                {
                    new { text = summaryPrompt }
                }
            }
        }
        };

        var client = _httpClientFactory.CreateClient();
        var request = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

        var response = await client.PostAsync(url, request);
        var json = await response.Content.ReadAsStringAsync();

        if (response.IsSuccessStatusCode)
        {
            using var doc = JsonDocument.Parse(json);
            return doc.RootElement
                      .GetProperty("candidates")[0]
                      .GetProperty("content")
                      .GetProperty("parts")[0]
                      .GetProperty("text")
                      .GetString() ?? "Gemini returned no summary.";
        }
        else
        {
            return $"Failed to generate analysis summary: {response.StatusCode} - {json}";
        }
    }

        
}
