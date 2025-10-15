using VideoCallApp.Enums;

namespace VideoCallApp.DTOs
{
    public class CallLogDto
    {
        public int Id { get; set; }
        public int CallerId { get; set; }
        public int CalleeId { get; set; }
        public CallType Type { get; set; }
        public CallStatus Status { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public int Duration { get; set; }
    }
}
