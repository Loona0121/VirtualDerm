using DermaAI.Data;
using DermaAI.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace DermaAI.Pages.Patient
{
    public class ProfileModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public ProfileModel(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public bool HasPatientRecord { get; set; }

        public class InputModel
        {
            [Required]
            public string FullName { get; set; } = "";
            public string Email { get; set; } = "";
            [Required]
            public int Age { get; set; }
            [Required]
            public string Sex { get; set; } = "";
            [Required]
            public string Address { get; set; } = "";
            [Required]
            public string ContactNumber { get; set; } = "";
        }

        public async Task OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            var patient = await _context.Patients
                .FirstOrDefaultAsync(p => p.FullName == user!.FullName);

            HasPatientRecord = patient != null;

            Input.FullName = user?.FullName ?? "";
            Input.Email = user?.Email ?? "";

            if (patient != null)
            {
                Input.Age = patient.Age;
                Input.Sex = patient.Sex;
                Input.Address = patient.Address;
                Input.ContactNumber = patient.ContactNumber;
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            var user = await _userManager.GetUserAsync(User);
            var patient = await _context.Patients
                .FirstOrDefaultAsync(p => p.FullName == user!.FullName);

            if (patient == null)
            {
                patient = new DermaAI.Models.Patient
                {
                    FullName = Input.FullName,
                    Age = Input.Age,
                    Sex = Input.Sex,
                    Address = Input.Address,
                    ContactNumber = Input.ContactNumber
                };
                _context.Patients.Add(patient);
            }
            else
            {
                patient.FullName = Input.FullName;
                patient.Age = Input.Age;
                patient.Sex = Input.Sex;
                patient.Address = Input.Address;
                patient.ContactNumber = Input.ContactNumber;
            }

            await _context.SaveChangesAsync();

            user!.FullName = Input.FullName;
            await _userManager.UpdateAsync(user);

            TempData["Success"] = "Profile saved successfully!";
            return RedirectToPage();
        }
    }
}