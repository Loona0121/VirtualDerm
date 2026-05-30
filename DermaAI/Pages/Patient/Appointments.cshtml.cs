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
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AppointmentsModel(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public List<Appointment> UpcomingAppointments { get; set; } = new();
        public List<Appointment> PastAppointments { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return RedirectToPage("/Auth/Login");

            var patient = await _context.Patients
                .FirstOrDefaultAsync(p => p.UserId == user.Id);

            if (patient == null)
                return Page();

            var allAppointments = await _context.Appointments
               .AsNoTracking()
               .Include(a => a.Doctor)       
               .Where(a => a.PatientId == patient.Id)
               .OrderByDescending(a => a.ScheduledDate)
               .ToListAsync();

            UpcomingAppointments = allAppointments
                .Where(a => a.ScheduledDate >= DateTime.Now)
                .ToList();

            PastAppointments = allAppointments
                .Where(a => a.ScheduledDate < DateTime.Now)
                .ToList();

            return Page();
        }
    }
}