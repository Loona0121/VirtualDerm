using DermaAI.Data;
using DermaAI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DermaAI.Controllers
{
    [Authorize(Roles = "Dermatologist")]
    public class StaffController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;

        public StaffController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IConfiguration configuration)
        {
            _context = context;
            _userManager = userManager;
            _configuration = configuration;
        }

        public async Task<IActionResult> Dashboard()
        {
            ViewBag.TotalPatients = await _context.Patients.CountAsync();
            ViewBag.TotalAppointments = await _context.Appointments.CountAsync();
            ViewBag.TotalConsultations = await _context.Consultations.CountAsync();

            ViewBag.UpcomingAppointments = await _context.Appointments
                .Include(a => a.Patient)
                .Where(a => a.ScheduledDate >= DateTime.Now)
                .OrderBy(a => a.ScheduledDate)
                .Take(5)
                .ToListAsync();

            ViewBag.RecentConsultations = await _context.Consultations
                .Include(c => c.Appointment)
                .ThenInclude(a => a.Patient)
                .OrderByDescending(c => c.CreatedAt)
                .Take(5)
                .ToListAsync();

            return View();
        }

        public async Task<IActionResult> Appointments()
        {
            var appointments = await _context.Appointments
                .Include(a => a.Patient)
                .OrderByDescending(a => a.ScheduledDate)
                .ToListAsync();
            return View(appointments);
        }

        public async Task<IActionResult> Consultations()
        {
            var consultations = await _context.Consultations
                .Include(c => c.Appointment)
                .ThenInclude(a => a.Patient)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
            return View(consultations);
        }

        public async Task<IActionResult> Patients()
        {
            var patients = await _context.Patients.ToListAsync();
            return View(patients);
        }

        public IActionResult AIAssistant()
        {
            return View();
        }

        // ← GET Profile
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            return View(user);
        }

        // ← POST Profile (only one, with photo upload)
        [HttpPost]
        public async Task<IActionResult> Profile(string FullName, string PhoneNumber, IFormFile? ProfilePhoto)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            user.FullName = FullName;
            user.PhoneNumber = PhoneNumber;

            if (ProfilePhoto != null && ProfilePhoto.Length > 0)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "profiles");
                Directory.CreateDirectory(uploadsFolder);

                var fileName = user.Id + Path.GetExtension(ProfilePhoto.FileName);
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                    await ProfilePhoto.CopyToAsync(stream);

                user.ProfilePhoto = "/uploads/profiles/" + fileName;
            }

            await _userManager.UpdateAsync(user);
            TempData["Success"] = "Profile updated successfully.";
            return RedirectToAction("Profile");
        }

        public IActionResult Settings()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Settings(string CurrentPassword, string NewPassword, string ConfirmPassword)
        {
            if (NewPassword != ConfirmPassword)
            {
                TempData["Error"] = "New passwords do not match.";
                return RedirectToAction("Settings");
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            var result = await _userManager.ChangePasswordAsync(user, CurrentPassword, NewPassword);

            if (result.Succeeded)
                TempData["Success"] = "Password changed successfully.";
            else
                TempData["Error"] = string.Join(" ", result.Errors.Select(e => e.Description));

            return RedirectToAction("Settings");
        }

        [HttpPost]
        public async Task<IActionResult> UpdateAppointmentStatus(int appointmentId, string status)
        {
            var appointment = await _context.Appointments.FindAsync(appointmentId);
            if (appointment == null) return NotFound();

            appointment.Status = status;
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Appointment marked as {status}.";
            return RedirectToAction("Appointments");
        }

        public async Task<IActionResult> CreateConsultation()
        {
            ViewBag.Appointments = await _context.Appointments
                .Include(a => a.Patient)
                .Where(a => a.Status == "Confirmed")
                .ToListAsync();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SaveConsultation(int AppointmentId, string Diagnosis, string DoctorNotes, string Prescription, DateTime? FollowUpDate)
        {
            if (AppointmentId == 0)
            {
                TempData["Error"] = "Please select an appointment.";
                return RedirectToAction("CreateConsultation");
            }

            var consultation = new Consultation
            {
                AppointmentId = AppointmentId,
                DoctorNotes = DoctorNotes,
                Prescription = Prescription,
                FollowUpDate = FollowUpDate,
                CreatedAt = DateTime.Now
            };

            _context.Consultations.Add(consultation);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Consultation saved successfully.";
            return RedirectToAction("Consultations");
        }

        public async Task<IActionResult> ConsultationDetails(int id)
        {
            var consultation = await _context.Consultations
                .Include(c => c.Appointment)
                .ThenInclude(a => a.Patient)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (consultation == null) return NotFound();
            return View(consultation);
        }

        public async Task<IActionResult> PatientDetails(int id)
        {
            var patient = await _context.Patients
                .Include(p => p.Appointments)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (patient == null) return NotFound();
            return View(patient);
        }

        [HttpPost]
        public async Task<IActionResult> AIChat([FromBody] AIChatRequest request)
        {
            try
            {
                var historyJson = HttpContext.Session.GetString("StaffChatHistory");
                var history = string.IsNullOrEmpty(historyJson)
                    ? new List<object>()
                    : System.Text.Json.JsonSerializer.Deserialize<List<System.Text.Json.JsonElement>>(historyJson)!
                        .Select(e => (object)new { role = e.GetProperty("role").GetString(), content = e.GetProperty("content").GetString() })
                        .ToList();

                history.Add(new { role = "user", content = request.Message });

                var apiKey = _configuration["GroqApiKey"];
                var client = new HttpClient();
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

                var messages = new List<object>
                {
                    new {
                        role = "system",
                        content = "You are DermaAI, an AI-powered dermatology assistant for medical staff on VirtualDerm, a virtual dermatology platform. You are assisting licensed dermatologists and healthcare professionals. When staff describes a patient's skin symptoms, provide: 1) Top 2-3 possible differential diagnoses with brief clinical reasoning, 2) Severity assessment, 3) Recommended clinical workup or tests, 4) Treatment options and medications commonly used. For follow-up questions, answer them directly and contextually based on the conversation. Use proper medical terminology. Be concise and clinically focused."
                    }
                };

                messages.AddRange(history.TakeLast(10));

                var requestBody = new
                {
                    model = "llama-3.1-8b-instant",
                    messages = messages,
                    max_tokens = 1000
                };

                var json = System.Text.Json.JsonSerializer.Serialize(requestBody);
                var content = new System.Net.Http.StringContent(json, System.Text.Encoding.UTF8, "application/json");

                var response = await client.PostAsync("https://api.groq.com/openai/v1/chat/completions", content);
                var responseString = await response.Content.ReadAsStringAsync();

                var jsonDoc = System.Text.Json.JsonDocument.Parse(responseString);

                if (jsonDoc.RootElement.TryGetProperty("error", out var errorProp))
                {
                    var errorMsg = errorProp.TryGetProperty("message", out var msg) ? msg.GetString() : "API error occurred.";
                    return Json(new { reply = $"API Error: {errorMsg}" });
                }

                var aiText = jsonDoc.RootElement
                    .GetProperty("choices")[0]
                    .GetProperty("message")
                    .GetProperty("content")
                    .GetString();

                history.Add(new { role = "assistant", content = aiText });
                HttpContext.Session.SetString("StaffChatHistory", System.Text.Json.JsonSerializer.Serialize(history));

                return Json(new { reply = aiText });
            }
            catch (Exception ex)
            {
                return Json(new { reply = $"Error: {ex.Message}" });
            }
        }

        [HttpPost]
        public IActionResult ClearChat()
        {
            HttpContext.Session.Remove("StaffChatHistory");
            return Json(new { success = true });
        }

        public IActionResult ChatHistory()
        {
            var historyJson = HttpContext.Session.GetString("StaffChatHistory");
            if (string.IsNullOrEmpty(historyJson))
                return Json(new { messages = new List<object>() });

            var history = System.Text.Json.JsonSerializer.Deserialize<List<System.Text.Json.JsonElement>>(historyJson);
            var messages = history?.Select(e => new {
                role = e.GetProperty("role").GetString(),
                content = e.GetProperty("content").GetString()
            }).ToList();

            return Json(new { messages });
        }

        public class AIChatRequest
        {
            public string Message { get; set; } = "";
        }
    }
}