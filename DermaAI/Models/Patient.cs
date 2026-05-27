using System.ComponentModel.DataAnnotations;

namespace DermaAI.Models
{
    public class Patient
    {
        public int Id { get; set; }

        [Required, StringLength(100)]
        public string FullName { get; set; }

        [Required]
        public int Age { get; set; }

        [Required]
        public string Sex { get; set; }

        [Required, StringLength(200)]
        public string Address { get; set; }

        [Required, StringLength(20)]
        public string ContactNumber { get; set; }

        public string? BloodType { get; set; }
        public string? Allergies { get; set; }
        public string? CurrentMedications { get; set; }
        public string? MedicalHistory { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
        public ICollection<ChatSession> ChatSessions { get; set; } = new List<ChatSession>();
    }
}