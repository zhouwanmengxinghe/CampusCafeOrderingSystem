using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CampusCafeOrderingSystem.Controllers
{
    [Route("api/account")]
    [ApiController]
    [Authorize]
    public class AccountApiController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;

        public AccountApiController(UserManager<IdentityUser> userManager)
        {
            _userManager = userManager;
        }

        [HttpGet("current-user")]
        public async Task<IActionResult> GetCurrentUser()
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return Unauthorized(new { error = "User not found" });
                }

                return Ok(new
                {
                    id = user.Id,
                    email = user.Email,
                    userName = user.UserName
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Internal server error", details = ex.Message });
            }
        }
    }
}