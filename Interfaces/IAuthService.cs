using VideoCallApp.DTOs;
using VideoCallApp.Models;

namespace VideoCallApp.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponseDto> RegisterAsync(RegisterDto request);
        Task<AuthResponseDto> LoginAsync(LoginDto request);
        Task<User?> GetUserByIdAsync(int userId);
    }
}
