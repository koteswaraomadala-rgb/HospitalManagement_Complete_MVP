using HospitalManagement.Web.Models; using HospitalManagement.Web.Services; using Microsoft.AspNetCore.Mvc;
namespace HospitalManagement.Web.Controllers;
public class PatientsController:Controller{private readonly HospitalApiService api;public PatientsController(HospitalApiService api)=>this.api=api;
[HttpGet]public IActionResult Index()=>View();[HttpGet]public async Task<IActionResult>Get(int id){var r=await api.GetPatientAsync(id);return r.Ok?Json(new{success=true,data=r.Data}):NotFound(new{message=r.Message});}

[HttpGet]public IActionResult Create()=>View(new PatientViewModel{DateOfBirth=DateTime.Today.AddYears(-30),Status="Active"});
[HttpGet]public IActionResult Edit(int id)=>View("Create",new PatientViewModel{Id=id});
[HttpPost][IgnoreAntiforgeryToken]public async Task<IActionResult> List([FromBody]SearchRequest request){var r=await api.PatientsAsync(request.Search);return r.Ok?Json(new{success=true,data=r.Data}):StatusCode(502,new{message=r.Message});}
[HttpPost][IgnoreAntiforgeryToken]public async Task<IActionResult> Save([FromBody]PatientViewModel model){if(!ModelState.IsValid)return BadRequest(new{message="Please complete all required patient fields correctly."});var r=model.Id==0?await api.CreatePatientAsync(model):await api.UpdatePatientAsync(model.Id,model);return r.Ok?Json(new{success=true,message=model.Id==0?"Patient added successfully.":"Patient updated successfully.",data=r.Data}):StatusCode(502,new{message=r.Message});}
[HttpPost][IgnoreAntiforgeryToken]public async Task<IActionResult>Delete([FromBody]IdRequest request){var r=await api.DeletePatientAsync(request.Id);return r.Ok?Json(new{success=true,message="Patient deleted successfully."}):BadRequest(new{message=r.Message});}}
