using HospitalManagement.API.DTOs; using HospitalManagement.API.Services; using Microsoft.AspNetCore.Authorization; using Microsoft.AspNetCore.Mvc;
namespace HospitalManagement.API.Controllers;
[ApiController,Authorize,Route("api/prescriptions")]
public class PrescriptionsController:ControllerBase{private readonly IPrescriptionService service;public PrescriptionsController(IPrescriptionService service)=>this.service=service;
[HttpGet]public async Task<IActionResult>GetAll([FromQuery]string? search)=>Ok(await service.GetAllAsync(search));
[HttpGet("{id:int}")]public async Task<IActionResult>Get(int id)=>await service.GetAsync(id) is{ } p?Ok(p):NotFound(new{message="Prescription not found."});
[HttpPost]public async Task<IActionResult>Create(PrescriptionRequest request)=>Ok(await service.CreateAsync(request));
[HttpPut("{id:int}")]public async Task<IActionResult>Update(int id,PrescriptionRequest request)=>await service.UpdateAsync(id,request) is{ } p?Ok(p):NotFound(new{message="Prescription not found."});
[HttpDelete("{id:int}")]public async Task<IActionResult>Delete(int id)=>await service.DeleteAsync(id)?Ok(new{message="Prescription deleted successfully."}):NotFound(new{message="Prescription not found."});}
