using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace DermaAI.Pages.Patient
{
    [IgnoreAntiforgeryToken]
    public class AIAssistantModel : PageModel
    {
        private readonly IConfiguration _configuration;

        public AIAssistantModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void OnGet() { }

        public async Task<IActionResult> OnPostChatAsync([FromBody] ChatRequest request)
        {
            var apiKey = _configuration["GroqApiKey"];

            if (string.IsNullOrEmpty(apiKey))
            {
                return new JsonResult(new { reply = "Error: API Key is missing in configuration." });
            }

            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

            var payload = new
            {
                model = "llama-3.3-70b-versatile",
                messages = new[]
                {
                    new { role = "system", content = "You are a helpful AI skin assistant for VirtualDerm. Use markdown for lists and bold text. Keep responses concise." },
                    new { role = "user", content = request.Message }
                },
                max_tokens = 800
            };

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                var response = await httpClient.PostAsync("https://api.groq.com/openai/v1/chat/completions", content);
                var responseBody = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    return new JsonResult(new { reply = $"API Error: {response.StatusCode}" });
                }

                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var data = JsonSerializer.Deserialize<JsonElement>(responseBody, options);

                string reply = "No response from AI.";

                if (data.TryGetProperty("choices", out var choices) && choices.GetArrayLength() > 0)
                {
                    reply = choices[0].GetProperty("message").GetProperty("content").GetString() ?? reply;
                }

                return new JsonResult(new { reply });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { reply = "System Error: " + ex.Message });
            }
        }

        public class ChatRequest
        {
            public string Message { get; set; } = "";
        }
    }
}