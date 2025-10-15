namespace VideoCallApp.DTOs
{
    
}
public class WebRTCSignalDto
{
    public string Type { get; set; } = string.Empty; // offer, answer, ice-candidate
    public object Data { get; set; } = new object();
    public int TargetUserId { get; set; }
}