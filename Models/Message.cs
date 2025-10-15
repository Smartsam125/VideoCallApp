using System.ComponentModel.DataAnnotations;

namespace VideoCallApp.Models
{
    public class Message
    {
        public int Id { get; set; }

        [Required]
        public string Content { get; set; } = string.Empty;

        public DateTime SentAt { get; set; } = DateTime.UtcNow;

        public bool IsRead { get; set; } = false;

     
        public int SenderId { get; set; }
        public int ReceiverId { get; set; }

      
        public virtual User Sender { get; set; } = null!;
        public virtual User Receiver { get; set; } = null!;
    }
}