using System.ComponentModel.DataAnnotations;

namespace DermaAI.Models
{
    public class ChatSession
    {
        public int Id { get; set; }

        [Required]
        public int PatientId { get; set; }
        public Patient Patient { get; set; }

        [Required, StringLength(1000)]
        public string InitialSymptoms { get; set; } = "";

        public string AISummary { get; set; } = "";

        public string Status { get; set; } = "Active";

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public ICollection<ChatMessage> ChatMessages { get; set; }
    }
}