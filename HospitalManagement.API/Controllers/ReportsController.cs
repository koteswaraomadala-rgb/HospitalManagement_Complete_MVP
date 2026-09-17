using HospitalManagement.API.Services; using Microsoft.AspNetCore.Authorization; using Microsoft.AspNetCore.Mvc;
namespace HospitalManagement.API.Controllers;
[ApiController,Authorize,Route("api/reports")]
public class ReportsController:ControllerBase{private readonly IReportService service;public ReportsController(IReportService service)=>this.service=service;[HttpGet("summary")]public async Task<IActionResult>Summary([FromQuery]DateTime? from,[FromQuery]DateTime? to)=>Ok(await service.GetSummaryAsync(from,to));}
