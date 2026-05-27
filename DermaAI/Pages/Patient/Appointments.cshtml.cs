using DermaAI.Data;
using DermaAI.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace DermaAI.Pages.Patient
{
    public class AppointmentsModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public AppointmentsModel(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public List<Appointment> UpcomingAppointments { get; set; } = new();
        public List<Appointment> PastAppointments { get; set; } = new();

        public async Task OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            var patient = await _context.Patients
                .FirstOrDefaultAsync(p => p.FullName == user!.FullName);

            if (patient != null)
            {
                var today = DateTime.Today;

                UpcomingAppointments = await _context.Appointments
                    .Where(a => a.PatientId == patient.Id && a.ScheduledDate >= today)
                    .OrderBy(a => a.ScheduledDate)
                    .ToListAsync();

                PastAppointments = await _context.Appointments
                    .Where(a => a.PatientId == patient.Id && a.ScheduledDate < today)
                    .OrderByDescending(a => a.ScheduledDate)
                    .ToListAsync();
            }
        }

        public async Task<IActionResult> OnPostCancelAsync(int appointmentId)
        {
            var appointment = await _context.Appointments.FindAsync(appointmentId);
            if (appointment != null)
            {
                appointment.Status = "Cancelled";
                await _context.SaveChangesAsync();
                TempData["Success"] = "Appointment cancelled successfully.";
            }
            return RedirectToPage();
        }
    }
}