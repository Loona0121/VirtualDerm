using Microsoft.AspNetCore.Identity;

namespace DermaAI.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; } = "";
        public string RoleName { get; set; } = "";
        public string? ProfilePhoto { get; set; }
        public string? Specialization { get; set; }

        public int YearsOfExperience { get; set; }

        public string? Bio { get; set; }

        public string? ClinicName { get; set; }

        public bool IsVerified { get; set; } = true;
    }
}