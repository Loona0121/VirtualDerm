using System.ComponentModel.DataAnnotations;

namespace DermaAI.Models
{
    public class ChatMessage
    {
        public int Id { get; set; }

        [Required]
        public int ChatSessionId { get; set; }
        public ChatSession? ChatSession { get; set; }

        [Required]
        public string Role { get; set; } = "";

        [Required]
        public string Content { get; set; } = "";

        public DateTime SentAt { get; set; } = DateTime.Now;
    }
}