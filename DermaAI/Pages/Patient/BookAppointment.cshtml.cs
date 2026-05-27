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

        public class InputModel
        {
            [Required]
            public string DoctorName { get; set; } = "";

            [Required]
            public DateTime ScheduledDate { get; set; }

            [Required]
            public string TimeSlot { get; set; } = "";

            public string? Notes { get; set; }
        }

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            var user = await _userManager.GetUserAsync(User);
            var patient = await _context.Patients
                .FirstOrDefaultAsync(p => p.FullName == user!.FullName);

            if (patient == null)
            {
                ModelState.AddModelError("", "Patient record not found. Please complete your profile first.");
                return Page();
            }

            var appointment = new Appointment
            {
                PatientId = patient.Id,
                DoctorName = Input.DoctorName,
                ScheduledDate = Input.ScheduledDate,
                Notes = $"[{Input.TimeSlot}] {Input.Notes}",
                Status = "Pending"
            };

            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Appointment booked successfully!";
            return RedirectToPage("/Patient/BookAppointment");
        }
    }
}