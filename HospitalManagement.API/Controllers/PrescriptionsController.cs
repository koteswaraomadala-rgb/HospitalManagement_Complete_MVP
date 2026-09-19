using HospitalManagement.API.DTOs;
using HospitalManagement.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HospitalManagement.API.Controllers;

[ApiController]
[Authorize]
[Route("api/prescriptions")]
public class PrescriptionsController : ControllerBase
{
    private readonly IPrescriptionService _service;

    public PrescriptionsController(IPrescriptionService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? search)
    {
        return Ok(await _service.GetAllAsync(search));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id)
    {
        var prescription = await _service.GetAsync(id);

        return prescription == null
            ? NotFound(new { message = "Prescription not found." })
            : Ok(prescription);
    }

    [HttpPost]
    public async Task<IActionResult> Create(PrescriptionRequest request)
    {
        return Ok(await _service.CreateAsync(request));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, PrescriptionRequest request)
    {
        var prescription = await _service.UpdateAsync(id, request);

        return prescription == null
            ? NotFound(new { message = "Prescription not found." })
            : Ok(prescription);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _service.DeleteAsync(id);

        return deleted
            ? Ok(new { message = "Prescription deleted successfully." })
            : NotFound(new { message = "Prescription not found." });
    }
}
