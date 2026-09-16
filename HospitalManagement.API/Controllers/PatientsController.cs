using HospitalManagement.API.DTOs;
using HospitalManagement.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HospitalManagement.API.Controllers;

[ApiController, Authorize, Route("api/patients")]
public class PatientsController : ControllerBase
{
    private readonly IPatientService service;
    public PatientsController(IPatientService service)
    {
        this.service = service;
    }


    [HttpGet] public async Task<IActionResult> GetAll([FromQuery] string? search) => Ok(await service.GetAllAsync(search));
    [HttpGet("{id:int}")] public async Task<IActionResult> Get(int id) => (await service.GetAsync(id)) is { } p ? Ok(p) : NotFound();
    [HttpPost] public async Task<IActionResult> Create(PatientRequest request) => Ok(await service.CreateAsync(request));
    [HttpDelete("{id:int}")] public async Task<IActionResult> Delete(int id) => await service.DeleteAsync(id) ? NoContent() : NotFound();
}
