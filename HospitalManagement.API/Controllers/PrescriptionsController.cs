using HospitalManagement.API.DTOs;
using HospitalManagement.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HospitalManagement.API.Controllers;

[ApiController, Authorize, Route("api/prescriptions")]
public class PrescriptionsController: ControllerBase
{
    private readonly IPrescriptionService service;
    public PrescriptionsController(IPrescriptionService service)
    {
        this.service = service;
    }

    [HttpGet] public async Task<IActionResult> GetAll() => Ok(await service.GetAllAsync());
    [HttpGet("{id:int}")] public async Task<IActionResult> Get(int id) => (await service.GetAsync(id)) is { } p ? Ok(p) : NotFound();
    [HttpPost] public async Task<IActionResult> Create(PrescriptionRequest request) => Ok(await service.CreateAsync(request));
}
