using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Serilog;
using System.Security.Claims;
using VideoCallApp.DTOs;
using VideoCallApp.Enums;
using VideoCallApp.Interfaces;
using VideoCallApp.Models;
using VideoCallApp.Services;

namespace VideoCallApp.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly IChatService _chatService;
        private readonly IAuthService _authService;

        public ChatHub(IChatService chatService, IAuthService authService)
        {
            _chatService = chatService;
            _authService = authService;
        }

        //public override async Task OnConnectedAsync()
        //{
        //    var userId = int.Parse(Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        //    await _chatService.SetUserOnlineAsync(userId, Context.ConnectionId);

        //    // Notify all clients about user coming online
        //    var user = await _authService.GetUserByIdAsync(userId);
        //    if (user != null)
        //    {
        //        await Clients.All.SendAsync("UserOnline", new UserDto
        //        {
        //            Id = user.Id,
        //            Username = user.Username,
        //            Email = user.Email,
        //            IsOnline = true
        //        });
        //    }

        //    await base.OnConnectedAsync();
        //}
        public override async Task OnConnectedAsync()
        {
            var userId = int.Parse(Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            Log.Information($"========== USER CONNECTED ==========");
            Log.Information($"UserId from claim: {userId}");
            Log.Information($"Context.UserIdentifier: {Context.UserIdentifier}");
            Log.Information($"ConnectionId: {Context.ConnectionId}");

            await _chatService.SetUserOnlineAsync(userId, Context.ConnectionId);
            var user = await _authService.GetUserByIdAsync(userId);
            if (user != null)
            {
                await Clients.All.SendAsync("UserOnline", new UserDto
                {
                    Id = user.Id,
                    Username = user.Username,
                    Email = user.Email,
                    IsOnline = true
                });
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = int.Parse(Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            await _chatService.SetUserOfflineAsync(userId);
            var user = await _authService.GetUserByIdAsync(userId);
            if (user != null)
            {
                await Clients.All.SendAsync("UserOffline", new UserDto
                {
                    Id = user.Id,
                    Username = user.Username,
                    Email = user.Email,
                    IsOnline = false
                });
            }

            await base.OnDisconnectedAsync(exception);
        }

    
        public async Task SendMessage(SendMessageDto messageDto)
        {
            var senderId = int.Parse(Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var message = await _chatService.SaveMessageAsync(senderId, messageDto.ReceiverId, messageDto.Content);
            await Clients.User(messageDto.ReceiverId.ToString()).SendAsync("ReceiveMessage", message);

           
            await Clients.Caller.SendAsync("MessageSent", message);
        }

        public async Task<int> InitiateCall(int calleeId, string type)
        {
            var callerId = int.Parse(Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            Log.Information($"InitiateCall - Caller: {callerId}, Callee: {calleeId}, Type: {type}");
            Log.Information($"Context.UserIdentifier: {Context.UserIdentifier}");
            var caller = await _authService.GetUserByIdAsync(callerId);
            CallType callType = type.ToLower() == "video" ? CallType.Video : CallType.Audio;
            var callLog = await _chatService.SaveCallLogAsync(callerId, calleeId, callType);

            var callOffer = new CallOfferDto
            {
                CallerId = callerId,
                CallerUsername = caller?.Username ?? "Unknown",
                Type = callType,
                CallLogId = callLog.Id
            };
            Log.Information($"Sending IncomingCall to user {calleeId}");
            await Clients.User(calleeId.ToString()).SendAsync("IncomingCall", callOffer);
            Log.Information($"IncomingCall sent successfully");
            return callLog.Id;
        }

        public async Task SendOffer(int calleeId, object offer, int callLogId)
        {
            await _chatService.UpdateCallLogAsync(callLogId, CallStatus.Ringing);

            var callOffer = new CallOfferDto
            {
                CallerId = int.Parse(Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0"),
                CallerUsername = Context.User?.Identity?.Name ?? "Unknown",
                Offer = offer,
                CallLogId = callLogId
            };

            await Clients.User(calleeId.ToString()).SendAsync("ReceiveOffer", callOffer);
        }

        public async Task SendAnswer(int callerId, object answer, int callLogId)
        {
            await _chatService.UpdateCallLogAsync(callLogId, CallStatus.Answered);

            var callAnswer = new CallAnswerDto
            {
                Answer = answer,
                CallLogId = callLogId
            };

            await Clients.User(callerId.ToString()).SendAsync("ReceiveAnswer", callAnswer);
        }

        public async Task SendIceCandidate(int targetUserId, object candidate)
        {
            var iceCandidate = new IceCandidateDto
            {
                Candidate = candidate,
                TargetUserId = targetUserId
            };

            await Clients.User(targetUserId.ToString()).SendAsync("ReceiveIceCandidate", iceCandidate);
        }

        public async Task AcceptCall(int callLogId)
        {
            await _chatService.UpdateCallLogAsync(callLogId, CallStatus.Answered);
            await Clients.Caller.SendAsync("CallAccepted", callLogId);
        }

        public async Task DeclineCall(int callLogId, int callerId)
        {
            await _chatService.UpdateCallLogAsync(callLogId, CallStatus.Declined, DateTime.UtcNow);
            await Clients.User(callerId.ToString()).SendAsync("CallDeclined", callLogId);
        }

        public async Task EndCall(int callLogId, int otherUserId)
        {
            await _chatService.UpdateCallLogAsync(callLogId, CallStatus.Ended, DateTime.UtcNow);
            await Clients.User(otherUserId.ToString()).SendAsync("CallEnded", callLogId);
        }

        public async Task Typing(int receiverId)
        {
            var senderId = int.Parse(Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            await Clients.User(receiverId.ToString()).SendAsync("UserTyping", senderId);
        }

        public async Task StopTyping(int receiverId)
        {
            var senderId = int.Parse(Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            await Clients.User(receiverId.ToString()).SendAsync("UserStoppedTyping", senderId);
        }
    }
}