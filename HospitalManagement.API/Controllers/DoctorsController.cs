using HospitalManagement.API.DTOs; using HospitalManagement.API.Services; using Microsoft.AspNetCore.Authorization; using Microsoft.AspNetCore.Mvc;
namespace HospitalManagement.API.Controllers;
[ApiController,Authorize,Route("api/doctors")]
public class DoctorsController:ControllerBase{private readonly IDoctorService service;public DoctorsController(IDoctorService service)=>this.service=service;
[HttpGet]public async Task<IActionResult> GetAll()=>Ok(await service.GetAllAsync());
[HttpGet("{id:int}")]public async Task<IActionResult> Get(int id)=>await service.GetAsync(id) is{ } d?Ok(d):NotFound(new{message="Doctor not found."});
[HttpPost]public async Task<IActionResult>Create(DoctorRequest request)=>Ok(await service.CreateAsync(request));
[HttpPut("{id:int}")]public async Task<IActionResult>Update(int id,DoctorRequest request)=>await service.UpdateAsync(id,request) is{ } d?Ok(d):NotFound(new{message="Doctor not found."});
[HttpDelete("{id:int}")]public async Task<IActionResult>Delete(int id)=>await service.DeleteAsync(id)?Ok(new{message="Doctor deleted successfully."}):NotFound(new{message="Doctor not found."});}
