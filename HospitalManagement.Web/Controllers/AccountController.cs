using HospitalManagement.Web.Models; using HospitalManagement.Web.Services; using Microsoft.AspNetCore.Mvc;
namespace HospitalManagement.Web.Controllers;
public class AccountController:Controller{private readonly HospitalApiService api;public AccountController(HospitalApiService api)=>this.api=api;
[HttpGet]public IActionResult Login(){if(!string.IsNullOrEmpty(HttpContext.Session.GetString("Token")))return RedirectToAction("Index","Dashboard");return View(new LoginViewModel());}
[HttpPost][IgnoreAntiforgeryToken]public async Task<IActionResult>LoginAjax([FromBody]LoginViewModel model){if(!ModelState.IsValid)return BadRequest(new{message="Please enter both username and password."});var r=await api.LoginAsync(model.Username.Trim(),model.Password);if(!r.ok)return Unauthorized(new{message=r.message});HttpContext.Session.SetString("Token",r.token!);HttpContext.Session.SetString("FullName",r.name!);HttpContext.Session.SetString("Role",r.role!);return Json(new{success=true,redirectUrl=Url.Action("Index","Dashboard")});}
[HttpPost][IgnoreAntiforgeryToken]public IActionResult LogoutAjax(){HttpContext.Session.Clear();return Json(new{success=true,redirectUrl=Url.Action(nameof(Login))});}
public IActionResult Logout(){HttpContext.Session.Clear();return RedirectToAction(nameof(Login));}}
