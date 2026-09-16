using HospitalManagement.Web.Models;
using HospitalManagement.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace HospitalManagement.Web.Controllers;

public class AppointmentsController: Controller
{
    private readonly HospitalApiService api;
    public AppointmentsController(HospitalApiService api)
    {
        this.api = api;
    }
    public async Task<IActionResult> Index() => View(await api.AppointmentsAsync());
    [HttpGet] public async Task<IActionResult> Create()
    {
        ViewBag.Patients = await api.PatientsAsync();
        ViewBag.Doctors = await api.DoctorsAsync();
        return View(new AppointmentViewModel { AppointmentDate = DateTime.Today, Status = "Confirmed" });
    }
    [HttpPost] public async Task<IActionResult> Create(AppointmentViewModel model)
    {
        await api.CreateAppointmentAsync(model);
        return RedirectToAction(nameof(Index));
    }
}
