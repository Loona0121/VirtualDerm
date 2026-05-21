using Microsoft.AspNetCore.Identity;

namespace DermaAI.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; } = "";
        public string RoleName { get; set; } = "";
    }
}