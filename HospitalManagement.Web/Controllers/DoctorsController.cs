using HospitalManagement.Web.Models;
using HospitalManagement.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace HospitalManagement.Web.Controllers;

public class DoctorsController : Controller
{
    private readonly HospitalApiService api;
    public DoctorsController(HospitalApiService api) => this.api = api;

    [HttpGet] public IActionResult Index() => View();
    [HttpGet] public async Task<IActionResult> Get(int id)
    {
        var r = await api.GetDoctorAsync(id);
        return r.Ok ? Json(new { success = true, data = r.Data }) : NotFound(new { message = r.Message });
    }
    [HttpGet] public IActionResult Create() => View(new DoctorViewModel());
    [HttpGet] public IActionResult Edit(int id) => View("Create", new DoctorViewModel { Id = id });

    [HttpPost, IgnoreAntiforgeryToken]
    public async Task<IActionResult> List()
    {
        var r = await api.DoctorsAsync();
        return r.Ok ? Json(new { success = true, data = r.Data }) : StatusCode(502, new { message = r.Message });
    }

    [HttpPost, IgnoreAntiforgeryToken]
    public async Task<IActionResult> Save([FromBody] DoctorViewModel model)
    {
        if (!ModelState.IsValid)
            return BadRequest(new { message = "Please correct the highlighted fields.", errors = ModelState.ToDictionary(x => x.Key, x => x.Value!.Errors.Select(e => string.IsNullOrWhiteSpace(e.ErrorMessage) ? "Invalid value." : e.ErrorMessage).ToArray()) });

        var r = model.Id == 0 ? await api.CreateDoctorAsync(model) : await api.UpdateDoctorAsync(model.Id, model);
        return r.Ok
            ? Json(new { success = true, message = model.Id == 0 ? "Doctor added successfully." : "Doctor updated successfully.", data = r.Data })
            : StatusCode(502, new { message = r.Message });
    }

    [HttpPost, IgnoreAntiforgeryToken]
    public async Task<IActionResult> Delete([FromBody] IdRequest request)
    {
        var r = await api.DeleteDoctorAsync(request.Id);
        return r.Ok ? Json(new { success = true, message = "Doctor deleted successfully." }) : BadRequest(new { message = r.Message });
    }
}
