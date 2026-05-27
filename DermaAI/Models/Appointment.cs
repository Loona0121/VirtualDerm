using System.ComponentModel.DataAnnotations;

namespace DermaAI.Models
{
    public class Appointment
    {
        public int Id { get; set; }

        [Required]
        public int PatientId { get; set; }
        public Patient Patient { get; set; }
        public string DoctorId { get; set; }
        public DateTime ScheduledDate { get; set; }

        public string Status { get; set; } = "Pending";

        [StringLength(500)]
        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public Consultation Consultation { get; set; }
    }
}