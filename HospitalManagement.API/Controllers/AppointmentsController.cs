using HospitalManagement.API.DTOs;
using HospitalManagement.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HospitalManagement.API.Controllers;

[ApiController, Authorize, Route("api/appointments")]
public class AppointmentsController : ControllerBase
{
    private readonly IAppointmentService service;

    public AppointmentsController(IAppointmentService service)
    {
        this.service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        return Ok(await service.GetAllAsync());
    }

    [HttpPost]
    public async Task<IActionResult> Create(AppointmentRequest request)
    {
        return Ok(await service.CreateAsync(request));
    }

    [HttpPatch("{id:int}/status")]
    public async Task<IActionResult> Status(int id, [FromBody] string status)
    {
        return await service.UpdateStatusAsync(id, status)
            ? Ok()
            : NotFound();
    }
}
