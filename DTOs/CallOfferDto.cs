using VideoCallApp.Enums;

namespace VideoCallApp.DTOs
{
    public class CallOfferDto
    {
        public int CallerId { get; set; }
        public string CallerUsername { get; set; } = string.Empty;
        public CallType Type { get; set; }
        public object Offer { get; set; } = new object();
        public int CallLogId { get; set; }
    }
}
