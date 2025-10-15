using VideoCallApp.DTOs;
using VideoCallApp.Enums;

namespace VideoCallApp.Interfaces
{
    public interface IChatService
    {
        Task<MessageDto> SaveMessageAsync(int senderId, int receiverId, string content);
        Task<List<MessageDto>> GetChatHistoryAsync(int userId1, int userId2, int page = 1, int pageSize = 20);
        Task<List<UserDto>> GetOnlineUsersAsync();
        Task SetUserOnlineAsync(int userId, string connectionId);
        Task SetUserOfflineAsync(int userId);
        Task<CallLogDto> SaveCallLogAsync(int callerId, int calleeId, CallType type);
        Task UpdateCallLogAsync(int callLogId, CallStatus status, DateTime? endTime = null);
    }
}
