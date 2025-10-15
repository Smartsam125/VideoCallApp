using Microsoft.VisualBasic;
using VideoCallApp.Enums;

namespace VideoCallApp.Models
{
    public class CallLog
    {
        public int Id { get; set; }

        public DateTime StartTime { get; set; } = DateTime.UtcNow;

        public DateTime? EndTime { get; set; }

        public CallStatus Status { get; set; } = CallStatus.Initiated;

        public Enums.CallType Type { get; set; } = Enums.CallType.Video;

        public int Duration { get; set; } = 0;

       
        public int CallerId { get; set; }
        public int CalleeId { get; set; }

       
        public virtual User Caller { get; set; } = null!;
        public virtual User Callee { get; set; } = null!;
    }
}
