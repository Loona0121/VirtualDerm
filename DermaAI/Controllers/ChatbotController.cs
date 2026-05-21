using DermaAI.Models;
using DermaAI.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Text.Json;

namespace DermaAI.Controllers
{
    public class ChatbotController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public ChatbotController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // GET: Chatbot
        public IActionResult Index()
        {
            return View();
        }

        // GET: Chatbot/Chat/5
        public async Task<IActionResult> Chat(int sessionId)
        {
            var session = await _context.ChatSessions
                .Include(s => s.ChatMessages)
                .Include(s => s.Patient)
                .FirstOrDefaultAsync(s => s.Id == sessionId);

            if (session == null) return NotFound();

            return View(session);
        }

        // POST: Chatbot/StartSession
        [HttpPost]
        public async Task<IActionResult> StartSession(int patientId, string initialSymptoms)
        {
            // Verify patient exists
            var patient = await _context.Patients.FindAsync(patientId);
            if (patient == null)
            {
                return Content("Patient not found. Please go to /Patients and add a patient first.");
            }

            var session = new ChatSession
            {
                PatientId = patientId,
                InitialSymptoms = initialSymptoms,
                Status = "Active",
                CreatedAt = DateTime.Now,
                AISummary = ""
            };

            _context.ChatSessions.Add(session);
            await _context.SaveChangesAsync();

            // Add first user message
            var userMessage = new ChatMessage
            {
                ChatSessionId = session.Id,
                Role = "user",
                Content = initialSymptoms,
                SentAt = DateTime.Now
            };
            _context.ChatMessages.Add(userMessage);
            await _context.SaveChangesAsync();

            // Get AI response
            var aiResponse = await GetGroqResponse(initialSymptoms);

            // Save AI message
            var aiMessage = new ChatMessage
            {
                ChatSessionId = session.Id,
                Role = "assistant",
                Content = aiResponse,
                SentAt = DateTime.Now
            };
            _context.ChatMessages.Add(aiMessage);

            // Update session summary
            session.AISummary = aiResponse.Length > 200 ? aiResponse.Substring(0, 200) + "..." : aiResponse;
            await _context.SaveChangesAsync();

            return RedirectToAction("Chat", new { sessionId = session.Id });
        }

        // POST: Chatbot/SendMessage
        [HttpPost]
        public async Task<IActionResult> SendMessage(int sessionId, string message)
        {
            // Save user message
            var userMessage = new ChatMessage
            {
                ChatSessionId = sessionId,
                Role = "user",
                Content = message,
                SentAt = DateTime.Now
            };
            _context.ChatMessages.Add(userMessage);
            await _context.SaveChangesAsync();

            // Get conversation history
            var history = await _context.ChatMessages
                .Where(m => m.ChatSessionId == sessionId)
                .OrderBy(m => m.SentAt)
                .ToListAsync();

            // Get AI response
            var aiResponse = await GetGroqResponse(message, history);

            // Save AI message
            var aiMessage = new ChatMessage
            {
                ChatSessionId = sessionId,
                Role = "assistant",
                Content = aiResponse,
                SentAt = DateTime.Now
            };
            _context.ChatMessages.Add(aiMessage);
            await _context.SaveChangesAsync();

            return RedirectToAction("Chat", new { sessionId });
        }

        private async Task<string> GetGroqResponse(string userMessage, List<ChatMessage>? history = null)
        {
            var apiKey = _configuration["GroqApiKey"];
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

            var messages = new List<object>
            {
                new {
                    role = "system",
                    content = "You are DermaAI, a helpful dermatology assistant for rural barangay health centers in the Philippines. When a patient describes their skin symptoms, provide: 1) Top 2-3 possible conditions, 2) Severity level (mild/moderate/severe), 3) Basic home care advice, 4) Whether they should see a doctor urgently. Always remind them this is not a substitute for a licensed physician. Keep responses clear and easy to understand."
                }
            };

            if (history != null)
            {
                foreach (var msg in history.TakeLast(10))
                {
                    messages.Add(new { role = msg.Role, content = msg.Content });
                }
            }
            else
            {
                messages.Add(new { role = "user", content = userMessage });
            }

            var requestBody = new
            {
                model = "llama-3.1-8b-instant",
                messages = messages,
                max_tokens = 1000
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync("https://api.groq.com/openai/v1/chat/completions", content);
            var responseString = await response.Content.ReadAsStringAsync();

            var jsonDoc = JsonDocument.Parse(responseString);
            var aiText = jsonDoc.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();

            return aiText ?? "Sorry, I could not process your request. Please try again.";
        }
    }
}