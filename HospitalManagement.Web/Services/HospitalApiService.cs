using System.Net.Http.Headers;
using System.Net.Http.Json;
using HospitalManagement.Web.Models;

namespace HospitalManagement.Web.Services;

public class HospitalApiService
{
    private readonly HttpClient http;
    private readonly IHttpContextAccessor accessor;

    public HospitalApiService(
        HttpClient http,
        IHttpContextAccessor accessor)
    {
        this.http = http;
        this.accessor = accessor;
    }

    // Keep the rest of your existing code here

    private void Auth()
    {
        var token = accessor.HttpContext?.Session.GetString("Token");
        http.DefaultRequestHeaders.Authorization = string.IsNullOrEmpty(token)
            ? null : new AuthenticationHeaderValue("Bearer", token);
    }

    public async Task<(bool ok, string? token, string? name, string? role)> LoginAsync(string username, string password)
    {
        var response = await http.PostAsJsonAsync("api/auth/login", new { username, password });
        if (!response.IsSuccessStatusCode) return (false, null, null, null);
        var data = await response.Content.ReadFromJsonAsync<LoginResponse>();
        return (true, data?.token, data?.fullName, data?.role);
    }

    public async Task<List<PatientViewModel>> PatientsAsync(string? search = null)
    {
        Auth();
        return await http.GetFromJsonAsync<List<PatientViewModel>>($"api/patients?search={Uri.EscapeDataString(search ?? "")}") ?? new List<PatientViewModel>();
    }

    public async Task CreatePatientAsync(PatientViewModel p)
    {
        Auth();
        await http.PostAsJsonAsync("api/patients", new { p.FirstName, p.LastName, p.Gender, p.DateOfBirth, p.Phone, p.Email, p.Address, p.Status });
    }

    public async Task DeletePatientAsync(int id)
    {
        Auth(); await http.DeleteAsync($"api/patients/{id}");
    }

    public async Task<List<DoctorViewModel>> DoctorsAsync()
    {
        Auth(); return await http.GetFromJsonAsync<List<DoctorViewModel>>("api/doctors") ?? new List<DoctorViewModel>();
    }

    public async Task CreateDoctorAsync(DoctorViewModel d)
    {
        Auth();
        await http.PostAsJsonAsync("api/doctors", new { d.Name, d.Specialization, d.Department, d.Phone, d.Email, d.ExperienceYears });
    }

    public async Task<List<AppointmentViewModel>> AppointmentsAsync()
    {
        Auth();
        var data = await http.GetFromJsonAsync<List<ApiAppointment>>("api/appointments") ?? new List<ApiAppointment>();
        return data.Select(x => new AppointmentViewModel
        {
            Id = x.id, PatientId = x.patientId, PatientName = $"{x.patient.FirstName} {x.patient.LastName}",
            DoctorId = x.doctorId, DoctorName = x.doctor.Name, AppointmentDate = x.appointmentDate,
            AppointmentTime = x.appointmentTime, Status = x.status, Reason = x.reason
        }).ToList();
    }

    public async Task CreateAppointmentAsync(AppointmentViewModel a)
    {
        Auth();
        await http.PostAsJsonAsync("api/appointments", new { a.PatientId, a.DoctorId, a.AppointmentDate, a.AppointmentTime, a.Status, a.Reason });
    }

    public async Task<List<PrescriptionApi>> PrescriptionsAsync()
    {
        Auth(); return await http.GetFromJsonAsync<List<PrescriptionApi>>("api/prescriptions") ?? new List<PrescriptionApi>();
    }

    private record LoginResponse(string token, string fullName, string role);
    public record PrescriptionApi(int id, PatientViewModel patient, DoctorViewModel doctor, string diagnosis, string notes, DateTime createdAt, List<MedicineApi> medicines);
    public record MedicineApi(int id, string medicineName, string dosage, string frequency, string duration, string instructions);
    private record ApiAppointment(int id, int patientId, PatientViewModel patient, int doctorId, DoctorViewModel doctor,
        DateTime appointmentDate, string appointmentTime, string status, string reason);
}
