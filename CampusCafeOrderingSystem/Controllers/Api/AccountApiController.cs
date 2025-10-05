using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using CampusCafeOrderingSystem.Models;

namespace CampusCafeOrderingSystem.Controllers.Api
{
    [Route("api/[controller]")]
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
        public async Task<ActionResult> GetCurrentUser()
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return Unauthorized(new { error = "User not logged in" });
                }

                var roles = await _userManager.GetRolesAsync(user);

                return Ok(new
                {
                    id = user.Id,
                    email = user.Email,
                    userName = user.UserName,
                    roles = roles
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to get user information", details = ex.Message });
            }
        }
    }
}