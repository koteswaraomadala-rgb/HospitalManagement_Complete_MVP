using HospitalManagement.Web.Models;
using HospitalManagement.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace HospitalManagement.Web.Controllers;

public class AccountController : Controller
{
    private readonly HospitalApiService api;
    public AccountController(HospitalApiService api)
    {
        this.api = api;
    }

    [HttpGet] public IActionResult Login() => View(new LoginViewModel("", ""));

    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        var result = await api.LoginAsync(model.Username, model.Password);
        if (!result.ok)
        {
            ViewBag.Error = "Invalid username or password.";
            return View(model);
        }
        HttpContext.Session.SetString("Token", result.token!);
        HttpContext.Session.SetString("FullName", result.name!);
        HttpContext.Session.SetString("Role", result.role!);
        return RedirectToAction("Index", "Dashboard");
    }

    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction(nameof(Login));
    }
}
