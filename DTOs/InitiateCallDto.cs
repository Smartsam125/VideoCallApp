using System.ComponentModel.DataAnnotations;
using VideoCallApp.Enums;

namespace VideoCallApp.DTOs
{
    public class InitiateCallDto
    {
        [Required]
        public int CalleeId { get; set; }

        [Required]
        public CallType Type { get; set; }
    }
}
