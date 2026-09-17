using System.Security.Claims; using HospitalManagement.API.DTOs; using HospitalManagement.API.Services; using Microsoft.AspNetCore.Authorization; using Microsoft.AspNetCore.Mvc;
namespace HospitalManagement.API.Controllers;
[ApiController,Authorize,Route("api/settings")]
public class SettingsController:ControllerBase{private readonly ISettingsService settings;private readonly IAuthService auth;public SettingsController(ISettingsService settings,IAuthService auth){this.settings=settings;this.auth=auth;}
[HttpGet]public async Task<IActionResult>Get(){var s=await settings.GetAsync();return Ok(s);}
[HttpPut]public async Task<IActionResult>Update(AppSettingRequest request)=>Ok(await settings.UpdateAsync(request));
[HttpGet("profile")]public async Task<IActionResult>Profile(){var id=int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);var u=await auth.GetUserAsync(id);return u is null?NotFound(new{message="User not found."}):Ok(new{u.Id,u.Username,u.FullName,u.Role});}
[HttpPut("profile")]public async Task<IActionResult>UpdateProfile(ProfileRequest request){var id=int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);var u=await auth.UpdateProfileAsync(id,request);return u is null?NotFound(new{message="User not found."}):Ok(new{u.Id,u.Username,u.FullName,u.Role});}
[HttpPut("password")]public async Task<IActionResult>ChangePassword(ChangePasswordRequest request){var id=int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);return await auth.ChangePasswordAsync(id,request)?Ok(new{message="Password changed successfully."}):BadRequest(new{message="Current password is incorrect."});}}
