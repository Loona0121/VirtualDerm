using System.ComponentModel.DataAnnotations;

namespace DermaAI.Models
{
    public class Consultation
    {
        public int Id { get; set; }

        [Required]
        public int AppointmentId { get; set; }
        public Appointment? Appointment { get; set; }

        [Required]
        public string DoctorNotes { get; set; } = "";

        public string? Diagnosis { get; set; }

        public string? Prescription { get; set; }

        public DateTime? FollowUpDate { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}