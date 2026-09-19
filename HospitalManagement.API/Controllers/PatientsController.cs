using HospitalManagement.API.DTOs;
using HospitalManagement.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HospitalManagement.API.Controllers;

[ApiController]
[Authorize]
[Route("api/patients")]
public class PatientsController : ControllerBase
{
    private readonly IPatientService _service;

    public PatientsController(IPatientService service)
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
        var patient = await _service.GetAsync(id);

        return patient == null
            ? NotFound(new { message = "Patient not found." })
            : Ok(patient);
    }

    [HttpPost]
    public async Task<IActionResult> Create(PatientRequest request)
    {
        return Ok(await _service.CreateAsync(request));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, PatientRequest request)
    {
        var patient = await _service.UpdateAsync(id, request);

        return patient == null
            ? NotFound(new { message = "Patient not found." })
            : Ok(patient);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _service.DeleteAsync(id);

        return deleted
            ? Ok(new { message = "Patient deleted successfully." })
            : NotFound(new { message = "Patient not found." });
    }
}
