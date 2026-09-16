using HospitalManagement.Web.Models;
using HospitalManagement.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace HospitalManagement.Web.Controllers;

public class DashboardController: Controller
{

    private readonly HospitalApiService api;
    public DashboardController(HospitalApiService api)
    {
        this.api = api;
    }
    public async Task<IActionResult> Index()
    {
        if (string.IsNullOrEmpty(HttpContext.Session.GetString("Token"))) return RedirectToAction("Login", "Account");
        var patients = await api.PatientsAsync();
        var doctors = await api.DoctorsAsync();
        var appointments = await api.AppointmentsAsync();
        var prescriptions = await api.PrescriptionsAsync();

        var vm = new DashboardViewModel
        {
            Patients = patients.Count,
            Doctors = doctors.Count,
            TodayAppointments = appointments.Count(x => x.AppointmentDate.Date == DateTime.Today),
            PendingPrescriptions = Math.Max(0, appointments.Count(x => x.Status != "Cancelled") - prescriptions.Count),
            Appointments = appointments.Where(x => x.AppointmentDate.Date == DateTime.Today).Take(8).ToList()
        };
        return View(vm);
    }
}
