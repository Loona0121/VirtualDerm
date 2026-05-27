using System.ComponentModel.DataAnnotations;
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

        public RegisterModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
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
            public string Role { get; set; } = ""; // Dermatologist or Patient

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
            // 1. Validate role selection
            if (string.IsNullOrWhiteSpace(Input.Role))
            {
                ModelState.AddModelError("Input.Role", "Please select a role.");
                return Page();
            }

            // 2. Validate form
            if (!ModelState.IsValid)
            {
                return Page();
            }

            // 3. Use EXACT role name from DB
            var assignedRole = Input.Role;

            // 4. Create user
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
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return Page();
            }

            // 5. Assign role (THIS IS WHAT FIXES YOUR DROPDOWN ISSUE)
            var roleResult = await _userManager.AddToRoleAsync(user, assignedRole);

            if (!roleResult.Succeeded)
            {
                foreach (var error in roleResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }

                // optional cleanup if role fails
                await _userManager.DeleteAsync(user);

                return Page();
            }

            // 6. Auto login
            await _signInManager.SignInAsync(user, isPersistent: false);

            // 7. Redirect safely
            return RedirectToPage("/Index");
        }
    }
}