using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using HospitalManagement.API.Data;
using HospitalManagement.API.DTOs;
using HospitalManagement.API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace HospitalManagement.API.Services;

public interface IAuthService
{
    Task<LoginResponse?> LoginAsync(LoginRequest request);
    Task<bool> ChangePasswordAsync(int userId, ChangePasswordRequest request);
    Task<User?> GetUserAsync(int userId);
    Task<User?> UpdateProfileAsync(int userId, ProfileRequest request);
}
public interface IPatientService
{
    Task<List<Patient>> GetAllAsync(string? search); Task<Patient?> GetAsync(int id);
    Task<Patient> CreateAsync(PatientRequest request); Task<Patient?> UpdateAsync(int id, PatientRequest request); Task<bool> DeleteAsync(int id);
}
public interface IDoctorService
{
    Task<List<Doctor>> GetAllAsync(); Task<Doctor?> GetAsync(int id);
    Task<Doctor> CreateAsync(DoctorRequest request); Task<Doctor?> UpdateAsync(int id, DoctorRequest request); Task<bool> DeleteAsync(int id);
}
public interface IAppointmentService
{
    Task<List<Appointment>> GetAllAsync(string? search, DateTime? date); Task<Appointment?> GetAsync(int id);
    Task<Appointment> CreateAsync(AppointmentRequest request); Task<Appointment?> UpdateAsync(int id, AppointmentRequest request);
    Task<bool> UpdateStatusAsync(int id, string status); Task<bool> DeleteAsync(int id);
}
public interface IPrescriptionService
{
    Task<List<Prescription>> GetAllAsync(string? search); Task<Prescription?> GetAsync(int id);
    Task<Prescription> CreateAsync(PrescriptionRequest request); Task<Prescription?> UpdateAsync(int id, PrescriptionRequest request); Task<bool> DeleteAsync(int id);
}
public interface IReportService
{
    Task<object> GetSummaryAsync(DateTime? from, DateTime? to);
}
public interface ISettingsService
{
    Task<AppSetting> GetAsync(); Task<AppSetting> UpdateAsync(AppSettingRequest request);
}

public class AuthService : IAuthService
{
    private readonly AppDbContext db; private readonly IConfiguration config;
    public AuthService(AppDbContext db, IConfiguration config) { this.db = db; this.config = config; }

    public async Task<LoginResponse?> LoginAsync(LoginRequest request)
    {
        var username = request.Username.Trim();
        var user = await db.Users.FirstOrDefaultAsync(x => x.Username == username && x.Password == request.Password);
        if (user is null) return null;
        var keyText = config["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key is not configured.");
        var issuer = config["Jwt:Issuer"] ?? "HospitalManagement.API";
        var audience = config["Jwt:Audience"] ?? "HospitalManagement.Web";
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()), new Claim(ClaimTypes.Name, user.FullName), new Claim(ClaimTypes.Role, user.Role) };
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyText));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expiry = DateTime.UtcNow.AddMinutes(int.TryParse(config["Jwt:ExpiryMinutes"], out var m) ? m : 120);
        var token = new JwtSecurityToken(issuer, audience, claims, expires: expiry, signingCredentials: creds);
        return new LoginResponse(new JwtSecurityTokenHandler().WriteToken(token), user.FullName, user.Role);
    }
    public Task<User?> GetUserAsync(int userId) => db.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Id == userId);
    public async Task<bool> ChangePasswordAsync(int userId, ChangePasswordRequest request)
    {
        var user = await db.Users.FindAsync(userId); if (user is null || user.Password != request.CurrentPassword) return false;
        user.Password = request.NewPassword; await db.SaveChangesAsync(); return true;
    }
    public async Task<User?> UpdateProfileAsync(int userId, ProfileRequest request)
    {
        var user = await db.Users.FindAsync(userId); if (user is null) return null;
        user.FullName = request.FullName.Trim(); user.Role = request.Role.Trim(); await db.SaveChangesAsync(); return user;
    }
}

public class PatientService : IPatientService
{
    private readonly AppDbContext db; public PatientService(AppDbContext db) { this.db = db; }
    public async Task<List<Patient>> GetAllAsync(string? search)
    {
        var q = db.Patients.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(search)) q = q.Where(x => x.PatientNumber.Contains(search) || x.FirstName.Contains(search) || x.LastName.Contains(search) || x.Phone.Contains(search));
        return await q.OrderByDescending(x => x.Id).ToListAsync();
    }
    public Task<Patient?> GetAsync(int id) => db.Patients.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
    public async Task<Patient> CreateAsync(PatientRequest r)
    {
        var next = (await db.Patients.MaxAsync(x => (int?)x.Id) ?? 0) + 1;
        var p = new Patient { PatientNumber = $"P{next:0000}", FirstName = r.FirstName.Trim(), LastName = r.LastName.Trim(), Gender = r.Gender, DateOfBirth = DateTime.SpecifyKind(r.DateOfBirth.Date, DateTimeKind.Utc), Phone = r.Phone.Trim(), Email = r.Email?.Trim() ?? "", Address = r.Address?.Trim() ?? "", Status = r.Status };
        db.Patients.Add(p); await db.SaveChangesAsync(); return p;
    }
    public async Task<Patient?> UpdateAsync(int id, PatientRequest r)
    {
        var p = await db.Patients.FindAsync(id); if (p is null) return null;
        p.FirstName=r.FirstName.Trim(); p.LastName=r.LastName.Trim(); p.Gender=r.Gender; p.DateOfBirth=DateTime.SpecifyKind(r.DateOfBirth.Date, DateTimeKind.Utc); p.Phone=r.Phone.Trim(); p.Email=r.Email?.Trim()??""; p.Address=r.Address?.Trim()??""; p.Status=r.Status;
        await db.SaveChangesAsync(); return p;
    }
    public async Task<bool> DeleteAsync(int id)
    {
        var p = await db.Patients.FindAsync(id); if (p is null) return false;
        if (await db.Appointments.AnyAsync(x=>x.PatientId==id) || await db.Prescriptions.AnyAsync(x=>x.PatientId==id)) throw new InvalidOperationException("Patient cannot be deleted because related appointments or prescriptions exist.");
        db.Patients.Remove(p); await db.SaveChangesAsync(); return true;
    }
}

public class DoctorService : IDoctorService
{
    private readonly AppDbContext db; public DoctorService(AppDbContext db) { this.db=db; }
    public Task<List<Doctor>> GetAllAsync()=>db.Doctors.AsNoTracking().OrderBy(x=>x.Name).ToListAsync();
    public Task<Doctor?> GetAsync(int id)=>db.Doctors.AsNoTracking().FirstOrDefaultAsync(x=>x.Id==id);
    public async Task<Doctor> CreateAsync(DoctorRequest r){ var next=(await db.Doctors.MaxAsync(x=>(int?)x.Id)??0)+1; var d=new Doctor{DoctorNumber=$"D{next:0000}",Name=r.Name.Trim(),Specialization=r.Specialization.Trim(),Department=r.Department.Trim(),Phone=r.Phone.Trim(),Email=r.Email?.Trim()??"",ExperienceYears=r.ExperienceYears}; db.Doctors.Add(d); await db.SaveChangesAsync(); return d; }
    public async Task<Doctor?> UpdateAsync(int id, DoctorRequest r){var d=await db.Doctors.FindAsync(id); if(d is null)return null; d.Name=r.Name.Trim();d.Specialization=r.Specialization.Trim();d.Department=r.Department.Trim();d.Phone=r.Phone.Trim();d.Email=r.Email?.Trim()??"";d.ExperienceYears=r.ExperienceYears;await db.SaveChangesAsync();return d;}
    public async Task<bool> DeleteAsync(int id){var d=await db.Doctors.FindAsync(id);if(d is null)return false;if(await db.Appointments.AnyAsync(x=>x.DoctorId==id)||await db.Prescriptions.AnyAsync(x=>x.DoctorId==id))throw new InvalidOperationException("Doctor cannot be deleted because related appointments or prescriptions exist.");db.Doctors.Remove(d);await db.SaveChangesAsync();return true;}
}

public class AppointmentService : IAppointmentService
{
    private readonly AppDbContext db; public AppointmentService(AppDbContext db){this.db=db;}
    private IQueryable<Appointment> Query()=>db.Appointments.Include(x=>x.Patient).Include(x=>x.Doctor).AsNoTracking();
    public async Task<List<Appointment>> GetAllAsync(string? search, DateTime? date){var q=Query();if(!string.IsNullOrWhiteSpace(search))q=q.Where(x=>x.Patient.FirstName.Contains(search)||x.Patient.LastName.Contains(search)||x.Doctor.Name.Contains(search)||x.Status.Contains(search));if(date.HasValue){var day=date.Value.Date;q=q.Where(x=>x.AppointmentDate.Date==day);}return await q.OrderByDescending(x=>x.AppointmentDate).ThenBy(x=>x.AppointmentTime).ToListAsync();}
    public Task<Appointment?> GetAsync(int id)=>Query().FirstOrDefaultAsync(x=>x.Id==id);
    public async Task<Appointment> CreateAsync(AppointmentRequest r){if(!await db.Patients.AnyAsync(x=>x.Id==r.PatientId))throw new InvalidOperationException("Patient not found.");if(!await db.Doctors.AnyAsync(x=>x.Id==r.DoctorId))throw new InvalidOperationException("Doctor not found.");var date=DateTime.SpecifyKind(r.AppointmentDate.Date,DateTimeKind.Utc);if(date<DateTime.UtcNow.Date)throw new InvalidOperationException("Appointment date cannot be in the past.");var a=new Appointment{PatientId=r.PatientId,DoctorId=r.DoctorId,AppointmentDate=date,AppointmentTime=r.AppointmentTime,Status=r.Status,Reason=r.Reason.Trim()};db.Appointments.Add(a);await db.SaveChangesAsync();return await GetAsync(a.Id)??a;}
    public async Task<Appointment?> UpdateAsync(int id, AppointmentRequest r){var a=await db.Appointments.FindAsync(id);if(a is null)return null;a.PatientId=r.PatientId;a.DoctorId=r.DoctorId;a.AppointmentDate=DateTime.SpecifyKind(r.AppointmentDate.Date,DateTimeKind.Utc);a.AppointmentTime=r.AppointmentTime;a.Status=r.Status;a.Reason=r.Reason.Trim();await db.SaveChangesAsync();return await GetAsync(id);}
    public async Task<bool> UpdateStatusAsync(int id,string status){var a=await db.Appointments.FindAsync(id);if(a is null)return false;a.Status=status;await db.SaveChangesAsync();return true;}
    public async Task<bool> DeleteAsync(int id){var a=await db.Appointments.FindAsync(id);if(a is null)return false;if(await db.Prescriptions.AnyAsync(x=>x.AppointmentId==id))throw new InvalidOperationException("Appointment cannot be deleted because a prescription is linked to it.");db.Appointments.Remove(a);await db.SaveChangesAsync();return true;}
}

public class PrescriptionService : IPrescriptionService
{
    private readonly AppDbContext db; public PrescriptionService(AppDbContext db){this.db=db;}
    private IQueryable<Prescription> Query()=>db.Prescriptions.Include(x=>x.Patient).Include(x=>x.Doctor).Include(x=>x.Medicines).AsNoTracking();
    public async Task<List<Prescription>> GetAllAsync(string? search){var q=Query();if(!string.IsNullOrWhiteSpace(search))q=q.Where(x=>x.Patient.FirstName.Contains(search)||x.Patient.LastName.Contains(search)||x.Doctor.Name.Contains(search)||x.Diagnosis.Contains(search));return await q.OrderByDescending(x=>x.CreatedAt).ToListAsync();}
    public Task<Prescription?> GetAsync(int id)=>Query().FirstOrDefaultAsync(x=>x.Id==id);
    public async Task<Prescription> CreateAsync(PrescriptionRequest r){if(!await db.Patients.AnyAsync(x=>x.Id==r.PatientId))throw new InvalidOperationException("Patient not found.");if(!await db.Doctors.AnyAsync(x=>x.Id==r.DoctorId))throw new InvalidOperationException("Doctor not found.");if(!await db.Appointments.AnyAsync(x=>x.Id==r.AppointmentId))throw new InvalidOperationException("Appointment not found.");if(await db.Prescriptions.AnyAsync(x=>x.AppointmentId==r.AppointmentId))throw new InvalidOperationException("A prescription already exists for this appointment.");var p=new Prescription{PatientId=r.PatientId,DoctorId=r.DoctorId,AppointmentId=r.AppointmentId,Diagnosis=r.Diagnosis.Trim(),Notes=r.Notes?.Trim()??""};foreach(var m in r.Medicines)p.Medicines.Add(new PrescriptionMedicine{MedicineName=m.MedicineName.Trim(),Dosage=m.Dosage.Trim(),Frequency=m.Frequency.Trim(),Duration=m.Duration.Trim(),Instructions=m.Instructions?.Trim()??""});db.Prescriptions.Add(p);await db.SaveChangesAsync();return await GetAsync(p.Id)??p;}
    public async Task<Prescription?> UpdateAsync(int id,PrescriptionRequest r){var p=await db.Prescriptions.Include(x=>x.Medicines).FirstOrDefaultAsync(x=>x.Id==id);if(p is null)return null;p.PatientId=r.PatientId;p.DoctorId=r.DoctorId;p.AppointmentId=r.AppointmentId;p.Diagnosis=r.Diagnosis.Trim();p.Notes=r.Notes?.Trim()??"";db.PrescriptionMedicines.RemoveRange(p.Medicines);p.Medicines=new List<PrescriptionMedicine>();foreach(var m in r.Medicines)p.Medicines.Add(new PrescriptionMedicine{MedicineName=m.MedicineName.Trim(),Dosage=m.Dosage.Trim(),Frequency=m.Frequency.Trim(),Duration=m.Duration.Trim(),Instructions=m.Instructions?.Trim()??""});await db.SaveChangesAsync();return await GetAsync(id);}
    public async Task<bool> DeleteAsync(int id){var p=await db.Prescriptions.FindAsync(id);if(p is null)return false;db.Prescriptions.Remove(p);await db.SaveChangesAsync();return true;}
}

public class ReportService : IReportService
{
    private readonly AppDbContext db; public ReportService(AppDbContext db){this.db=db;}
    public async Task<object> GetSummaryAsync(DateTime? from, DateTime? to){var start=DateTime.SpecifyKind((from??DateTime.UtcNow.Date.AddDays(-29)).Date,DateTimeKind.Utc);var end=DateTime.SpecifyKind((to??DateTime.UtcNow.Date).Date.AddDays(1).AddTicks(-1),DateTimeKind.Utc);return new {from=start,to=end,totalPatients=await db.Patients.CountAsync(x=>x.CreatedAt>=start&&x.CreatedAt<=end),totalDoctors=await db.Doctors.CountAsync(),appointments=await db.Appointments.CountAsync(x=>x.AppointmentDate>=start&&x.AppointmentDate<=end),completedAppointments=await db.Appointments.CountAsync(x=>x.AppointmentDate>=start&&x.AppointmentDate<=end&&x.Status=="Completed"),cancelledAppointments=await db.Appointments.CountAsync(x=>x.AppointmentDate>=start&&x.AppointmentDate<=end&&x.Status=="Cancelled"),prescriptions=await db.Prescriptions.CountAsync(x=>x.CreatedAt>=start&&x.CreatedAt<=end)};}
}

public class SettingsService : ISettingsService
{
    private readonly AppDbContext db; public SettingsService(AppDbContext db){this.db=db;}
    public async Task<AppSetting> GetAsync(){var s=await db.AppSettings.FirstOrDefaultAsync();if(s is not null)return s;s=new AppSetting();db.AppSettings.Add(s);await db.SaveChangesAsync();return s;}
    public async Task<AppSetting> UpdateAsync(AppSettingRequest r){var s=await GetAsync();s.HospitalName=r.HospitalName.Trim();s.ContactEmail=r.ContactEmail?.Trim()??"";s.ContactPhone=r.ContactPhone?.Trim()??"";s.Address=r.Address?.Trim()??"";await db.SaveChangesAsync();return s;}
}
