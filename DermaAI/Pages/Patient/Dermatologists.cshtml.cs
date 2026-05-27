using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using DermaAI.Models;

namespace DermaAI.Pages.Patient
{
    public class DermatologistsModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public DermatologistsModel(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        // ✅ THIS MUST EXIST EXACTLY LIKE THIS
        public List<ApplicationUser> Dermatologists { get; set; } = new();

        public async Task OnGetAsync()
        {
            Dermatologists = (await _userManager.GetUsersInRoleAsync("Dermatologist")).ToList();
        }
    }
}