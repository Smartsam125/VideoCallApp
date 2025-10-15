using System.ComponentModel.DataAnnotations;

namespace VideoCallApp.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Username { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public bool IsOnline { get; set; } = false;

        public string? ConnectionId { get; set; }
        public virtual ICollection<Message> SentMessages { get; set; } = new List<Message>();
        public virtual ICollection<Message> ReceivedMessages { get; set; } = new List<Message>();
        public virtual ICollection<CallLog> InitiatedCalls { get; set; } = new List<CallLog>();
        public virtual ICollection<CallLog> ReceivedCalls { get; set; } = new List<CallLog>();
    }
}