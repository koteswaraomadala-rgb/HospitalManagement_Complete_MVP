using HospitalManagement.API.DTOs; using HospitalManagement.API.Services; using Microsoft.AspNetCore.Authorization; using Microsoft.AspNetCore.Mvc;
namespace HospitalManagement.API.Controllers;
[ApiController,Authorize,Route("api/appointments")]
public class AppointmentsController:ControllerBase{private readonly IAppointmentService service;public AppointmentsController(IAppointmentService service)=>this.service=service;
[HttpGet]public async Task<IActionResult>GetAll([FromQuery]string? search,[FromQuery]DateTime? date)=>Ok(await service.GetAllAsync(search,date));
[HttpGet("{id:int}")]public async Task<IActionResult>Get(int id)=>await service.GetAsync(id) is{ } a?Ok(a):NotFound(new{message="Appointment not found."});
[HttpPost]public async Task<IActionResult>Create(AppointmentRequest request)=>Ok(await service.CreateAsync(request));
[HttpPut("{id:int}")]public async Task<IActionResult>Update(int id,AppointmentRequest request)=>await service.UpdateAsync(id,request) is{ } a?Ok(a):NotFound(new{message="Appointment not found."});
[HttpPatch("{id:int}/status")]public async Task<IActionResult>Status(int id,StatusRequest request)=>await service.UpdateStatusAsync(id,request.Status)?Ok(new{message="Appointment status updated."}):NotFound(new{message="Appointment not found."});
[HttpDelete("{id:int}")]public async Task<IActionResult>Delete(int id)=>await service.DeleteAsync(id)?Ok(new{message="Appointment deleted successfully."}):NotFound(new{message="Appointment not found."});}
