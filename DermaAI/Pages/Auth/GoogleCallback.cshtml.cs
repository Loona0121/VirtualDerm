using DermaAI.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DermaAI.Pages.Auth
{
    public class GoogleCallbackModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;

        public GoogleCallbackModel(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null) return RedirectToPage("/Auth/Login");

            var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, false);

            if (result.Succeeded)
            {
                var user = await _userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);
                var roles = await _userManager.GetRolesAsync(user!);
                var role = roles.FirstOrDefault();

                return role switch
                {
                    "Admin" => Redirect("/Home/Index"),
                    "Staff" => Redirect("/Staff/Dashboard"),
                    "Patient" => RedirectToPage("/Patient/Dashboard"),
                    _ => Redirect("/")
                };
            }

            // New user — create account
            var email = info.Principal.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value ?? "";
            var name = info.Principal.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value ?? "";

            var newUser = new ApplicationUser
            {
                UserName = email,
                Email = email,
                FullName = name,
                RoleName = "Patient"  // default role for Google sign-in
            };

            var createResult = await _userManager.CreateAsync(newUser);
            if (createResult.Succeeded)
            {
                await _userManager.AddLoginAsync(newUser, info);
                await _userManager.AddToRoleAsync(newUser, "Patient");
                await _signInManager.SignInAsync(newUser, false);
                return RedirectToPage("/Patient/Dashboard");
            }

            return RedirectToPage("/Auth/Login");
        }
    }
}