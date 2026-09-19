using HospitalManagement.API.DTOs;
using HospitalManagement.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HospitalManagement.API.Controllers;

[ApiController]
[Authorize]
[Route("api/doctors")]
public class DoctorsController : ControllerBase
{
    private readonly IDoctorService _service;

    public DoctorsController(IDoctorService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        return Ok(await _service.GetAllAsync());
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id)
    {
        var doctor = await _service.GetAsync(id);

        return doctor == null
            ? NotFound(new { message = "Doctor not found." })
            : Ok(doctor);
    }

    [HttpPost]
    public async Task<IActionResult> Create(DoctorRequest request)
    {
        return Ok(await _service.CreateAsync(request));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, DoctorRequest request)
    {
        var doctor = await _service.UpdateAsync(id, request);

        return doctor == null
            ? NotFound(new { message = "Doctor not found." })
            : Ok(doctor);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _service.DeleteAsync(id);

        return deleted
            ? Ok(new { message = "Doctor deleted successfully." })
            : NotFound(new { message = "Doctor not found." });
    }
}
