using HospitalManagement.API.DTOs;
using HospitalManagement.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace HospitalManagement.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService auth;

    public AuthController(IAuthService service)
    {
        this.auth = service;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var result = await auth.LoginAsync(request);
        return result is null ? Unauthorized(new { message = "Invalid username or password" }) : Ok(result);
    }
}
