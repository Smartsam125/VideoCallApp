using Microsoft.EntityFrameworkCore;
using VideoCallApp.Data;
using VideoCallApp.DTOs;
using VideoCallApp.Enums;
using VideoCallApp.Interfaces;
using VideoCallApp.Models;

namespace VideoCallApp.Services
{
    public class ChatService : IChatService
    {
        private readonly ApplicationDbContext _context;

        public ChatService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<MessageDto> SaveMessageAsync(int senderId, int receiverId, string content)
        {
            var message = new Message
            {
                SenderId = senderId,
                ReceiverId = receiverId,
                Content = content,
                SentAt = DateTime.UtcNow
            };

            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

         
            var sender = await _context.Users.FindAsync(senderId);

            return new MessageDto
            {
                Id = message.Id,
                Content = message.Content,
                SentAt = message.SentAt,
                SenderId = message.SenderId,
                ReceiverId = message.ReceiverId,
                SenderUsername = sender?.Username ?? "Unknown"
            };
        }

        public async Task<List<MessageDto>> GetChatHistoryAsync(int userId1, int userId2, int page = 1, int pageSize = 20)
        {
            var messages = await _context.Messages
                .Include(m => m.Sender)
                .Where(m => (m.SenderId == userId1 && m.ReceiverId == userId2) ||
                           (m.SenderId == userId2 && m.ReceiverId == userId1))
                .OrderByDescending(m => m.SentAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(m => new MessageDto
                {
                    Id = m.Id,
                    Content = m.Content,
                    SentAt = m.SentAt,
                    SenderId = m.SenderId,
                    ReceiverId = m.ReceiverId,
                    SenderUsername = m.Sender.Username
                })
                .ToListAsync();

            return messages.OrderBy(m => m.SentAt).ToList();
        }

        public async Task<List<UserDto>> GetOnlineUsersAsync()
        {
            var users = await _context.Users
                .Where(u => u.IsOnline)
                .Select(u => new UserDto
                {
                    Id = u.Id,
                    Username = u.Username,
                    Email = u.Email,
                    IsOnline = u.IsOnline
                })
                .ToListAsync();

            return users;
        }

        public async Task SetUserOnlineAsync(int userId, string connectionId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user != null)
            {
                user.IsOnline = true;
                user.ConnectionId = connectionId;
                await _context.SaveChangesAsync();
            }
        }

        public async Task SetUserOfflineAsync(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user != null)
            {
                user.IsOnline = false;
                user.ConnectionId = null;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<CallLogDto> SaveCallLogAsync(int callerId, int calleeId, CallType type)
        {
            var callLog = new CallLog
            {
                CallerId = callerId,
                CalleeId = calleeId,
                Type = type,
                Status = CallStatus.Initiated,
                StartTime = DateTime.UtcNow
            };

            _context.CallLogs.Add(callLog);
            await _context.SaveChangesAsync();

            return new CallLogDto
            {
                Id = callLog.Id,
                CallerId = callLog.CallerId,
                CalleeId = callLog.CalleeId,
                Type = callLog.Type,
                Status = callLog.Status,
                StartTime = callLog.StartTime
            };
        }

        public async Task UpdateCallLogAsync(int callLogId, CallStatus status, DateTime? endTime = null)
        {
            var callLog = await _context.CallLogs.FindAsync(callLogId);
            if (callLog != null)
            {
                callLog.Status = status;
                if (endTime.HasValue)
                {
                    callLog.EndTime = endTime;
                    callLog.Duration = (int)(endTime.Value - callLog.StartTime).TotalSeconds;
                }
                await _context.SaveChangesAsync();
            }
        }
    }
}
