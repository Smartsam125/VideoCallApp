using System.ComponentModel.DataAnnotations;

namespace VideoCallApp.DTOs
{
    public class SendMessageDto
    {
        [Required]
        public int ReceiverId { get; set; }

        [Required]
        public string Content { get; set; } = string.Empty;
    }
}
