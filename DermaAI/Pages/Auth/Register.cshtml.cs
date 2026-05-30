using System.ComponentModel.DataAnnotations;
using DermaAI.Data;
using DermaAI.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DermaAI.Pages.Auth
{
    public class RegisterModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ApplicationDbContext _context;  // ✅ added

        public RegisterModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ApplicationDbContext context)  // ✅ added
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;  // ✅ added
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public class InputModel
        {
            [Required]
            public string FullName { get; set; } = "";

            [Required]
            [EmailAddress]
            public string Email { get; set; } = "";

            [Required]
            public string Role { get; set; } = "";

            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; } = "";

            [Required]
            [Compare("Password")]
            [DataType(DataType.Password)]
            public string ConfirmPassword { get; set; } = "";
        }

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync()
        {
            if (string.IsNullOrWhiteSpace(Input.Role))
            {
                ModelState.AddModelError("Input.Role", "Please select a role.");
                return Page();
            }

            if (!ModelState.IsValid)
                return Page();

            var assignedRole = Input.Role;

            var user = new ApplicationUser
            {
                UserName = Input.Email,
                Email = Input.Email,
                FullName = Input.FullName,
                RoleName = assignedRole
            };

            var result = await _userManager.CreateAsync(user, Input.Password);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);
                return Page();
            }

            var roleResult = await _userManager.AddToRoleAsync(user, assignedRole);

            if (!roleResult.Succeeded)
            {
                foreach (var error in roleResult.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);

                await _userManager.DeleteAsync(user);
                return Page();
            }

            
            if (assignedRole == "Patient")
            {
               
                var patient = new DermaAI.Models.Patient
                {
                    UserId = user.Id,                    
                    Name = Input.FullName,               
                    FullName = Input.FullName,           
                    Age = 0,                             
                    Sex = "",                            
                    Address = "",                        
                    ContactNumber = "",                  
                    CreatedAt = DateTime.Now
                };

                _context.Patients.Add(patient);
                await _context.SaveChangesAsync();
            }

            await _signInManager.SignInAsync(user, isPersistent: false);

            return RedirectToPage("/Index");
        }
    }
}