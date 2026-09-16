using HospitalManagement.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace HospitalManagement.Web.Controllers;

public class PrescriptionsController: Controller
{

    private readonly HospitalApiService api;
    public PrescriptionsController(HospitalApiService api)
    {
        this.api = api;
    }
    public async Task<IActionResult> Index() => View(await api.PrescriptionsAsync());
}
