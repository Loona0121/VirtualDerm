using System.ComponentModel.DataAnnotations;

namespace DermaAI.Models
{
    public class Doctor
    {
        public int Id { get; set; }

        [Required]
        public string FullName { get; set; } = string.Empty;

        [Required]
        public string Specialty { get; set; } = string.Empty;

        public string? UserId { get; set; }  

        public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    }
}