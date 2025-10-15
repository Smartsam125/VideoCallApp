using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using VideoCallApp.DTOs;
using VideoCallApp.Interfaces;
using VideoCallApp.Services;

namespace VideoCallApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ChatController : ControllerBase
    {
        private readonly IChatService _chatService;

        public ChatController(IChatService chatService)
        {
            _chatService = chatService;
        }

        [HttpGet("history/{userId}")]
        public async Task<ActionResult<List<MessageDto>>> GetChatHistory(int userId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var messages = await _chatService.GetChatHistoryAsync(currentUserId, userId, page, pageSize);
            return Ok(messages);
        }

        [HttpGet("online-users")]
        public async Task<ActionResult<List<UserDto>>> GetOnlineUsers()
        {
            var users = await _chatService.GetOnlineUsersAsync();
            return Ok(users);
        }

        [HttpPost("initiate-call")]
        public async Task<ActionResult<CallLogDto>> InitiateCall([FromBody] InitiateCallDto request)
        {
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var callLog = await _chatService.SaveCallLogAsync(currentUserId, request.CalleeId, request.Type);
            return Ok(callLog);
        }
    }
}