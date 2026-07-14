using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OmniRAG.Application.Abstractions;
using System.Security.Claims;

namespace OmniRAG.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet("profile")]
        public async Task<IActionResult> GetOrCreateUserProfile()
        {
            // 1. Extract unique claims out of the validated Entra ID token
            var azureObjectId = User.FindFirst("oid")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var email = User.FindFirst("preferred_username")?.Value ?? User.FindFirst(ClaimTypes.Email)?.Value;
            var name = User.FindFirst("name")?.Value ?? "New User";

            // Extract roles from the token array mapping
            var roles = User.FindAll("roles").Select(c => c.Value).ToList();

            if (string.IsNullOrEmpty(azureObjectId))
            {
                return BadRequest("Invalid token metadata: Object ID missing.");
            }

            var cancellationToken = HttpContext.RequestAborted;
            var (profile, isFirstTime) = await _userService.GetOrCreateByAzureObjectIdAsync(azureObjectId, name, email, cancellationToken);

            return Ok(new
            {
                Profile = profile,
                Roles = roles,
                RequiresInitialSetup = isFirstTime
            });
        }

        [HttpPost("login")]
        public async Task<IActionResult> LoginAndSyncProfile()
        {
            // 1. Extract the unique security claims out of the validated Bearer token
            var azureObjectId = User.FindFirst("oid")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var email = User.FindFirst("preferred_username")?.Value ?? User.FindFirst(ClaimTypes.Email)?.Value;
            var name = User.FindFirst("name")?.Value ?? "New User";

            // Extract the user's Microsoft Entra assigned roles
            var roles = User.FindAll("roles").Select(c => c.Value).ToList();

            if (string.IsNullOrEmpty(azureObjectId))
            {
                return BadRequest(new { Message = "Authentication failed: Unique Azure User Object ID (oid) is missing." });
            }

            var cancellationToken = HttpContext.RequestAborted;
            var (profile, isFirstTime) = await _userService.GetOrCreateByAzureObjectIdAsync(azureObjectId, name, email, cancellationToken);

            return Ok(new
            {
                Message = isFirstTime ? "First-time login profile created." : "Welcome back!",
                Profile = profile,
                AssignedRoles = roles,
                RequiresInitialSetup = isFirstTime || !profile.IsProfileCompleted
            });
        }
    }
}
