using DermaAI.Models;
using DermaAI.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DermaAI.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Summary counts
            ViewBag.TotalPatients = await _context.Patients.CountAsync();
            ViewBag.TotalAppointments = await _context.Appointments.CountAsync();
            ViewBag.TotalConsultations = await _context.Consultations.CountAsync();
            ViewBag.TotalChatSessions = await _context.ChatSessions.CountAsync();

            // Upcoming appointments (next 5, status Pending or Confirmed)
            ViewBag.UpcomingAppointments = await _context.Appointments
                .Include(a => a.Patient)
                .Where(a => a.ScheduledDate >= DateTime.Now)
                .OrderBy(a => a.ScheduledDate)
                .Take(5)
                .ToListAsync();

            // Recent consultations (last 5)
            ViewBag.RecentConsultations = await _context.Consultations
                .Include(c => c.Appointment)
                .ThenInclude(a => a.Patient)
                .OrderByDescending(c => c.CreatedAt)
                .Take(5)
                .ToListAsync();

            // Chat sessions per day for last 7 days (for chart)
            var last7Days = Enumerable.Range(0, 7)
                .Select(i => DateTime.Today.AddDays(-6 + i))
                .ToList();

            var chatData = new List<int>();
            var appointmentData = new List<int>();
            var chartLabels = new List<string>();

            foreach (var day in last7Days)
            {
                chartLabels.Add(day.ToString("MMM d"));
                chatData.Add(await _context.ChatSessions
                    .CountAsync(c => c.CreatedAt.Date == day.Date));
                appointmentData.Add(await _context.Appointments
                    .CountAsync(a => a.ScheduledDate.Date == day.Date));
            }

            ViewBag.ChartLabels = System.Text.Json.JsonSerializer.Serialize(chartLabels);
            ViewBag.ChartChatData = System.Text.Json.JsonSerializer.Serialize(chatData);
            ViewBag.ChartAppointmentData = System.Text.Json.JsonSerializer.Serialize(appointmentData);

            return View();
        }
        public IActionResult Landing()
        {
            return View();
        }
    }
}