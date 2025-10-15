using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace VideoCallApp.Controllers
{
    public class TurnController : Controller
    {
        private readonly IConfiguration _configuration;

        public TurnController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet("credentials")]
        public IActionResult GetTurnCredentials()
        {
            var turnSecret = _configuration["TurnServer:Secret"];
            var turnServer = _configuration["TurnServer:Host"];

            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds() + 3600;
            var username = $"{timestamp}:webrtc";
            var credential = GenerateCredential(username, turnSecret);

            return Ok(new
            {
                username = username,
                credential = credential,
                urls = new[]
                {
                $"stun:{turnServer}:3478",
                $"turn:{turnServer}:3478",
                $"turns:{turnServer}:5349"
            },
                ttl = 3600 
            });
        }

        private string GenerateCredential(string username, string secret)
        {
            using (var hmac = new System.Security.Cryptography.HMACSHA1(Encoding.UTF8.GetBytes(secret)))
            {
                var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(username));
                return Convert.ToBase64String(hash);
            }
        }
    }
}
