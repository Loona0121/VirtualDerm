using DermaAI.Data;
using DermaAI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace DermaAI.Pages.Patient
{
    [Authorize]
    public class DashboardModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public DashboardModel(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public string PatientName { get; set; } = "";
        public int UpcomingAppointments { get; set; }
        public int PastConsultations { get; set; }
        public int TotalAppointments { get; set; }

        public async Task OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            PatientName = user?.FullName ?? "Patient";

            var today = DateTime.Today;

            var patient = await _context.Patients
                .FirstOrDefaultAsync(p => p.FullName == user!.FullName);

            if (patient != null)
            {
                UpcomingAppointments = await _context.Appointments
                    .Where(a => a.PatientId == patient.Id && a.ScheduledDate >= today)
                    .CountAsync();

                TotalAppointments = await _context.Appointments
                    .Where(a => a.PatientId == patient.Id)
                    .CountAsync();

                var patientAppointmentIds = await _context.Appointments
                    .Where(a => a.PatientId == patient.Id)
                    .Select(a => a.Id)
                    .ToListAsync();

                PastConsultations = await _context.Consultations
                    .Where(c => patientAppointmentIds.Contains(c.AppointmentId))
                    .CountAsync();
            }
        }
    }
}