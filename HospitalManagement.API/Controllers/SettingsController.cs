using HospitalManagement.API.DTOs;
using HospitalManagement.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HospitalManagement.API.Controllers;

[ApiController]
[Authorize]
[Route("api/settings")]
public class SettingsController : ControllerBase
{
    private readonly ISettingsService _service;
    private readonly IAuthService _authService;

    public SettingsController(ISettingsService service, IAuthService authService)
    {
        _service = service;
        _authService = authService;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var settings = await _service.GetAsync();
        return Ok(settings);
    }

    [HttpPut]
    public async Task<IActionResult> Update(AppSettingRequest request)
    {
        return Ok(await _service.UpdateAsync(request));
    }

    [HttpGet("profile")]
    public async Task<IActionResult> Profile()
    {
        var user = await _authService.GetUserAsync(GetUserId());

        return user == null
            ? NotFound(new { message = "User not found." })
            : Ok(new
            {
                user.Username,
                user.FullName,
                user.Role
            });
    }

    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile(ProfileRequest request)
    {
        var user = await _authService.UpdateProfileAsync(GetUserId(), request);

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
    public async Task<IActionResult> ChangePassword(ChangePasswordRequest request)
    {
        var changed = await _authService.ChangePasswordAsync(GetUserId(), request);

        return changed
            ? Ok(new { message = "Password changed successfully." })
            : BadRequest(new { message = "Current password is incorrect." });
    }

    private int GetUserId()
    {
        var claim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(claim, out var userId) ? userId : 0;
    }
}
