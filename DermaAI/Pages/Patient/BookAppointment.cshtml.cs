using DermaAI.Data;
using DermaAI.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace DermaAI.Pages.Patient
{
    public class BookAppointmentModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public BookAppointmentModel(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        // ✅ FIX: This belongs here, NOT inside InputModel
        public List<Doctor> Doctors { get; set; } = new();

        public class InputModel
        {
            [Required]
            public int DoctorId { get; set; }

            [Required]
            public DateTime ScheduledDate { get; set; }

            [Required]
            public string TimeSlot { get; set; } = "";

            public string? Notes { get; set; }
        }

        public async Task OnGetAsync()
        {
            Input = new InputModel
            {
                ScheduledDate = DateTime.Today
            };

            Doctors = await _context.Doctors
                .OrderBy(d => d.FullName)
                .ToListAsync();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                // reload doctors if page reloads with error
                Doctors = await _context.Doctors.ToListAsync();
                return Page();
            }

            var user = await _userManager.GetUserAsync(User);

            var patient = await _context.Patients
                .FirstOrDefaultAsync(p => p.FullName == user!.FullName);

            if (patient == null)
            {
                ModelState.AddModelError("", "Patient record not found. Please complete your profile first.");

                Doctors = await _context.Doctors.ToListAsync();
                return Page();
            }

            var appointment = new Appointment
            {
                Patient = patient,
                DoctorId = Input.DoctorId,
                ScheduledDate = Input.ScheduledDate,
                Notes = $"[{Input.TimeSlot}] {Input.Notes}",
                Status = "Pending",
                CreatedAt = DateTime.Now
            };

            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Appointment booked successfully!";
            return RedirectToPage();
        }
    }
}