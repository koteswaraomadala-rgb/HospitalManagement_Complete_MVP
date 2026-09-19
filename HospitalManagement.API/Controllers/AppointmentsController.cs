using HospitalManagement.API.DTOs;
using HospitalManagement.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HospitalManagement.API.Controllers;

[ApiController]
[Authorize]
[Route("api/appointments")]
public class AppointmentsController : ControllerBase
{
    private readonly IAppointmentService _service;

    public AppointmentsController(IAppointmentService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? search, [FromQuery] DateTime? date)
    {
        return Ok(await _service.GetAllAsync(search, date));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id)
    {
        var appointment = await _service.GetAsync(id);

        return appointment == null
            ? NotFound(new { message = "Appointment not found." })
            : Ok(appointment);
    }

    [HttpPost]
    public async Task<IActionResult> Create(AppointmentRequest request)
    {
        return Ok(await _service.CreateAsync(request));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, AppointmentRequest request)
    {
        var appointment = await _service.UpdateAsync(id, request);

        return appointment == null
            ? NotFound(new { message = "Appointment not found." })
            : Ok(appointment);
    }

    [HttpPatch("{id:int}/status")]
    public async Task<IActionResult> UpdateStatus(int id, StatusRequest request)
    {
        var updated = await _service.UpdateStatusAsync(id, request.Status);

        return updated
            ? Ok(new { message = "Appointment status updated." })
            : NotFound(new { message = "Appointment not found." });
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _service.DeleteAsync(id);

        return deleted
            ? Ok(new { message = "Appointment deleted successfully." })
            : NotFound(new { message = "Appointment not found." });
    }
}
