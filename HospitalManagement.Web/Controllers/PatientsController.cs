using HospitalManagement.Web.Models;
using HospitalManagement.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace HospitalManagement.Web.Controllers;

public class PatientsController: Controller
{

    private readonly HospitalApiService api;
    public PatientsController(HospitalApiService api)
    {
        this.api = api;
    }
    public async Task<IActionResult> Index(string? search) { ViewBag.Search = search; return View(await api.PatientsAsync(search)); }
    [HttpGet] public IActionResult Create() => View(new PatientViewModel { DateOfBirth = DateTime.Today.AddYears(-30), Status = "Active" });
    [HttpPost] public async Task<IActionResult> Create(PatientViewModel model) { await api.CreatePatientAsync(model); return RedirectToAction(nameof(Index)); }
    public async Task<IActionResult> Delete(int id) { await api.DeletePatientAsync(id); return RedirectToAction(nameof(Index)); }
}
