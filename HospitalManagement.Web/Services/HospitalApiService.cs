using System.Net.Http.Headers;
using System.Net.Http.Json;
using HospitalManagement.Web.Models;

namespace HospitalManagement.Web.Services;

public class HospitalApiService
{
    private readonly HttpClient http; private readonly IHttpContextAccessor accessor;
    public HospitalApiService(HttpClient http,IHttpContextAccessor accessor){this.http=http;this.accessor=accessor;}
    private void Auth(){var token=accessor.HttpContext?.Session.GetString("Token");http.DefaultRequestHeaders.Authorization=string.IsNullOrWhiteSpace(token)?null:new AuthenticationHeaderValue("Bearer",token);}
    private async Task<(bool Ok,T? Data,string Message)> SendAsync<T>(HttpMethod method,string url,object? body=null){Auth();using var req=new HttpRequestMessage(method,url);if(body is not null)req.Content=JsonContent.Create(body);using var res=await http.SendAsync(req);var text=await res.Content.ReadAsStringAsync();if(res.IsSuccessStatusCode){T? data=default;if(!string.IsNullOrWhiteSpace(text))data=System.Text.Json.JsonSerializer.Deserialize<T>(text,new System.Text.Json.JsonSerializerOptions{PropertyNameCaseInsensitive=true});return(true,data,"");}return(false,default,ExtractMessage(text,$"Request failed ({(int)res.StatusCode})."));}
    private static string ExtractMessage(string text,string fallback){try{var d=System.Text.Json.JsonSerializer.Deserialize<Dictionary<string,object>>(text);if(d is not null&&d.TryGetValue("message",out var m))return m.ToString()??fallback;}catch{}return string.IsNullOrWhiteSpace(text)?fallback:text;}
    public async Task<(bool ok,string? token,string? name,string? role,string message)> LoginAsync(string username,string password){using var res=await http.PostAsJsonAsync("api/auth/login",new{username,password});var text=await res.Content.ReadAsStringAsync();if(!res.IsSuccessStatusCode)return(false,null,null,null,ExtractMessage(text,"Invalid username or password."));var d=System.Text.Json.JsonSerializer.Deserialize<LoginResponse>(text,new System.Text.Json.JsonSerializerOptions{PropertyNameCaseInsensitive=true});return(d is null?false:true,d?.Token,d?.FullName,d?.Role,d is null?"Invalid server response.":"");}
    public Task<(bool Ok,List<PatientViewModel>? Data,string Message)> PatientsAsync(string? search=null)=>SendAsync<List<PatientViewModel>>(HttpMethod.Get,$"api/patients?search={Uri.EscapeDataString(search??"")}");
    public Task<(bool Ok,PatientViewModel? Data,string Message)> GetPatientAsync(int id)=>SendAsync<PatientViewModel>(HttpMethod.Get,$"api/patients/{id}");
    public Task<(bool Ok,PatientViewModel? Data,string Message)> CreatePatientAsync(PatientViewModel p)=>SendAsync<PatientViewModel>(HttpMethod.Post,"api/patients",new{p.FirstName,p.LastName,p.Gender,p.DateOfBirth,p.Phone,p.Email,p.Address,p.Status});
    public Task<(bool Ok,PatientViewModel? Data,string Message)> UpdatePatientAsync(int id,PatientViewModel p)=>SendAsync<PatientViewModel>(HttpMethod.Put,$"api/patients/{id}",new{p.FirstName,p.LastName,p.Gender,p.DateOfBirth,p.Phone,p.Email,p.Address,p.Status});
    public Task<(bool Ok,object? Data,string Message)> DeletePatientAsync(int id)=>SendAsync<object>(HttpMethod.Delete,$"api/patients/{id}");
    public Task<(bool Ok,List<DoctorViewModel>? Data,string Message)> DoctorsAsync()=>SendAsync<List<DoctorViewModel>>(HttpMethod.Get,"api/doctors");
    public Task<(bool Ok,DoctorViewModel? Data,string Message)> GetDoctorAsync(int id)=>SendAsync<DoctorViewModel>(HttpMethod.Get,$"api/doctors/{id}");
    public Task<(bool Ok,DoctorViewModel? Data,string Message)> CreateDoctorAsync(DoctorViewModel d)=>SendAsync<DoctorViewModel>(HttpMethod.Post,"api/doctors",new{d.Name,d.Specialization,d.Department,d.Phone,d.Email,d.ExperienceYears});
    public Task<(bool Ok,DoctorViewModel? Data,string Message)> UpdateDoctorAsync(int id,DoctorViewModel d)=>SendAsync<DoctorViewModel>(HttpMethod.Put,$"api/doctors/{id}",new{d.Name,d.Specialization,d.Department,d.Phone,d.Email,d.ExperienceYears});
    public Task<(bool Ok,object? Data,string Message)> DeleteDoctorAsync(int id)=>SendAsync<object>(HttpMethod.Delete,$"api/doctors/{id}");
    public async Task<(bool Ok,List<AppointmentViewModel>? Data,string Message)> AppointmentsAsync(string? search=null,string? date=null)
    {
        var r=await SendAsync<List<ApiAppointment>>(HttpMethod.Get,$"api/appointments?search={Uri.EscapeDataString(search??"")}{(string.IsNullOrWhiteSpace(date)?"":$"&date={Uri.EscapeDataString(date)}")}");
        if(!r.Ok||r.Data is null)return(r.Ok,new List<AppointmentViewModel>(),r.Message);
        return(true,r.Data.Select(x=>new AppointmentViewModel{Id=x.Id,PatientId=x.PatientId,PatientName=$"{x.Patient.FirstName} {x.Patient.LastName}",DoctorId=x.DoctorId,DoctorName=x.Doctor.Name,AppointmentDate=x.AppointmentDate,AppointmentTime=x.AppointmentTime,Status=x.Status,Reason=x.Reason}).ToList(),"");
    }
    public async Task<(bool Ok,AppointmentViewModel? Data,string Message)> GetAppointmentAsync(int id){var r=await SendAsync<ApiAppointment>(HttpMethod.Get,$"api/appointments/{id}");if(!r.Ok||r.Data is null)return(r.Ok,null,r.Message);var x=r.Data;return(true,new AppointmentViewModel{Id=x.Id,PatientId=x.PatientId,PatientName=$"{x.Patient.FirstName} {x.Patient.LastName}",DoctorId=x.DoctorId,DoctorName=x.Doctor.Name,AppointmentDate=x.AppointmentDate,AppointmentTime=x.AppointmentTime,Status=x.Status,Reason=x.Reason},"");}
    public Task<(bool Ok,AppointmentViewModel? Data,string Message)> CreateAppointmentAsync(AppointmentViewModel a)=>SendAsync<AppointmentViewModel>(HttpMethod.Post,"api/appointments",new{a.PatientId,a.DoctorId,a.AppointmentDate,a.AppointmentTime,a.Status,a.Reason});
    public Task<(bool Ok,AppointmentViewModel? Data,string Message)> UpdateAppointmentAsync(int id,AppointmentViewModel a)=>SendAsync<AppointmentViewModel>(HttpMethod.Put,$"api/appointments/{id}",new{a.PatientId,a.DoctorId,a.AppointmentDate,a.AppointmentTime,a.Status,a.Reason});
    public Task<(bool Ok,object? Data,string Message)> UpdateAppointmentStatusAsync(int id,string status)=>SendAsync<object>(HttpMethod.Patch,$"api/appointments/{id}/status",new{status});
    public Task<(bool Ok,object? Data,string Message)> DeleteAppointmentAsync(int id)=>SendAsync<object>(HttpMethod.Delete,$"api/appointments/{id}");
    public Task<(bool Ok,List<PrescriptionViewModel>? Data,string Message)> PrescriptionsAsync(string? search=null)=>SendAsync<List<PrescriptionViewModel>>(HttpMethod.Get,$"api/prescriptions?search={Uri.EscapeDataString(search??"")}");
    public Task<(bool Ok,PrescriptionViewModel? Data,string Message)> CreatePrescriptionAsync(object request)=>SendAsync<PrescriptionViewModel>(HttpMethod.Post,"api/prescriptions",request);
    public Task<(bool Ok,PrescriptionViewModel? Data,string Message)> GetPrescriptionAsync(int id)=>SendAsync<PrescriptionViewModel>(HttpMethod.Get,$"api/prescriptions/{id}");
    public Task<(bool Ok,PrescriptionViewModel? Data,string Message)> UpdatePrescriptionAsync(int id,object request)=>SendAsync<PrescriptionViewModel>(HttpMethod.Put,$"api/prescriptions/{id}",request);
    public Task<(bool Ok,object? Data,string Message)> DeletePrescriptionAsync(int id)=>SendAsync<object>(HttpMethod.Delete,$"api/prescriptions/{id}");
    public Task<(bool Ok,ReportViewModel? Data,string Message)> ReportAsync(string from,string to)=>SendAsync<ReportViewModel>(HttpMethod.Get,$"api/reports/summary?from={Uri.EscapeDataString(from)}&to={Uri.EscapeDataString(to)}");
    public Task<(bool Ok,SettingsViewModel? Data,string Message)> SettingsAsync()=>SendAsync<SettingsViewModel>(HttpMethod.Get,"api/settings");
    public Task<(bool Ok,SettingsViewModel? Data,string Message)> UpdateSettingsAsync(SettingsViewModel s)=>SendAsync<SettingsViewModel>(HttpMethod.Put,"api/settings",new{s.HospitalName,s.ContactEmail,s.ContactPhone,s.Address});
    public Task<(bool Ok,SettingsViewModel? Data,string Message)> ProfileAsync()=>SendAsync<SettingsViewModel>(HttpMethod.Get,"api/settings/profile");
    public Task<(bool Ok,SettingsViewModel? Data,string Message)> UpdateProfileAsync(SettingsViewModel s)=>SendAsync<SettingsViewModel>(HttpMethod.Put,"api/settings/profile",new{s.FullName,s.Role});
    public Task<(bool Ok,object? Data,string Message)> ChangePasswordAsync(ChangePasswordViewModel p)=>SendAsync<object>(HttpMethod.Put,"api/settings/password",new{p.CurrentPassword,p.NewPassword});
    private record LoginResponse(string Token,string FullName,string Role);
    private record ApiAppointment(int Id,int PatientId,PatientViewModel Patient,int DoctorId,DoctorViewModel Doctor,DateTime AppointmentDate,string AppointmentTime,string Status,string Reason);
}
