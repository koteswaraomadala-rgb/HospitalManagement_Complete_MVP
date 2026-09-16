using HospitalManagement.Web.Models;
using HospitalManagement.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace HospitalManagement.Web.Controllers;

public class DoctorsController : Controller
{

    private readonly HospitalApiService api;
    public DoctorsController(HospitalApiService api)
    {
        this.api = api;
    }
    public async Task<IActionResult> Index() => View(await api.DoctorsAsync());
    [HttpGet] public IActionResult Create() => View(new DoctorViewModel());
    [HttpPost] public async Task<IActionResult> Create(DoctorViewModel model) { await api.CreateDoctorAsync(model); return RedirectToAction(nameof(Index)); }
}
