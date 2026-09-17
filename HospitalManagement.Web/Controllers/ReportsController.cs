using HospitalManagement.Web.Models;
using HospitalManagement.Web.Services; using Microsoft.AspNetCore.Mvc;
namespace HospitalManagement.Web.Controllers;
public class ReportsController:Controller{private readonly HospitalApiService api;public ReportsController(HospitalApiService api)=>this.api=api;[HttpGet]public IActionResult Index()=>View();[HttpPost][IgnoreAntiforgeryToken]public async Task<IActionResult>Summary([FromBody]ReportRequest request){var r=await api.ReportAsync(request.From,request.To);return r.Ok?Json(new{success=true,data=r.Data}):StatusCode(502,new{message=r.Message});}}
