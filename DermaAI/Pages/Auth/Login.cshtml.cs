using System.ComponentModel.DataAnnotations;
using DermaAI.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DermaAI.Pages.Auth
{
    public class LoginModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;

        public LoginModel(
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public class InputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; } = "";

            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; } = "";
        }

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            var result = await _signInManager.PasswordSignInAsync(
                Input.Email, Input.Password,
                isPersistent: false,
                lockoutOnFailure: false);

            if (result.Succeeded)
            {
                var user = await _userManager.FindByEmailAsync(Input.Email);
                var roles = await _userManager.GetRolesAsync(user!);
                var role = roles.FirstOrDefault();

                return role switch
                {
                    "Admin" => RedirectToPage("/Admin/Dashboard"),
                    "Staff" => RedirectToPage("/Staff/Dashboard"),
                    "Patient" => RedirectToPage("/Patient/Dashboard"),
                    _ => RedirectToPage("/Index")
                };
            }

            ModelState.AddModelError(string.Empty, "Invalid email or password.");
            return Page();
        }

        public async Task<IActionResult> OnPostLogoutAsync()
        {
            await _signInManager.SignOutAsync();
            return RedirectToPage("/Auth/Login");
        }
    }
}