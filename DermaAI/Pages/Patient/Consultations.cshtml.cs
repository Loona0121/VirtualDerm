using DermaAI.Data;
using DermaAI.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace DermaAI.Pages.Patient
{
    public class ConsultationsModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public ConsultationsModel(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public List<Consultation> Consultations { get; set; } = new();

        public async Task OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            var patient = await _context.Patients
                .FirstOrDefaultAsync(p => p.FullName == user!.FullName);

            if (patient != null)
            {
                var appointmentIds = await _context.Appointments
                    .Where(a => a.PatientId == patient.Id)
                    .Select(a => a.Id)
                    .ToListAsync();

                Consultations = await _context.Consultations
                    .Include(c => c.Appointment)
                    .Where(c => appointmentIds.Contains(c.AppointmentId))
                    .OrderByDescending(c => c.CreatedAt)
                    .ToListAsync();
            }
        }
    }
}