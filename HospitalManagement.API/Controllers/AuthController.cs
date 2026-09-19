using System.Security.Claims;
using HospitalManagement.API.DTOs;
using HospitalManagement.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HospitalManagement.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _service;

    public AuthController(IAuthService service)
    {
        _service = service;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var response = await _service.LoginAsync(request);

        return response == null
            ? Unauthorized(new { message = "Invalid username or password." })
            : Ok(response);
    }

    [HttpGet("profile")]
    [Authorize]
    public async Task<IActionResult> Profile()
    {
        var userId = GetUserId();
        var user = await _service.GetUserAsync(userId);

        return user == null
            ? NotFound(new { message = "User not found." })
            : Ok(new
            {
                user.Username,
                user.FullName,
                user.Role
            });
    }

    [HttpPut("password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword(ChangePasswordRequest request)
    {
        var updated = await _service.ChangePasswordAsync(GetUserId(), request);

        return updated
            ? Ok(new { message = "Password changed successfully." })
            : BadRequest(new { message = "Current password is incorrect." });
    }

    [HttpPut("profile")]
    [Authorize]
    public async Task<IActionResult> UpdateProfile(ProfileRequest request)
    {
        var user = await _service.UpdateProfileAsync(GetUserId(), request);

        return user == null
            ? NotFound(new { message = "User not found." })
            : Ok(new
            {
                user.Username,
                user.FullName,
                user.Role
            });
    }

    private int GetUserId()
    {
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(claim, out var userId) ? userId : 0;
    }
}
