using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using HospitalManagement.API.Data;
using HospitalManagement.API.DTOs;
using HospitalManagement.API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace HospitalManagement.API.Services;

public interface IAuthService { Task<LoginResponse?> LoginAsync(LoginRequest request); }
public interface IPatientService { Task<List<Patient>> GetAllAsync(string? search); Task<Patient?> GetAsync(int id); Task<Patient> CreateAsync(PatientRequest request); Task<bool> DeleteAsync(int id); }
public interface IDoctorService { Task<List<Doctor>> GetAllAsync(); Task<Doctor?> GetAsync(int id); Task<Doctor> CreateAsync(DoctorRequest request); }
public interface IAppointmentService { Task<List<Appointment>> GetAllAsync(); Task<Appointment> CreateAsync(AppointmentRequest request); Task<bool> UpdateStatusAsync(int id, string status); }
public interface IPrescriptionService { Task<List<Prescription>> GetAllAsync(); Task<Prescription?> GetAsync(int id); Task<Prescription> CreateAsync(PrescriptionRequest request); }



public class AuthService : IAuthService
{
    private readonly AppDbContext db;
    private readonly IConfiguration config;

    public AuthService(AppDbContext db, IConfiguration config)
    {
        this.db = db;
        this.config = config;
    }
    public async Task<LoginResponse?> LoginAsync(LoginRequest request)
    {
        var user = await db.Users.FirstOrDefaultAsync(x => x.Username == request.Username && x.Password == request.Password);
        if (user is null) return null;

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.FullName),
            new Claim(ClaimTypes.Role, user.Role)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expiry = DateTime.UtcNow.AddMinutes(int.Parse(config["Jwt:ExpiryMinutes"] ?? "120"));

        var token = new JwtSecurityToken(
            config["Jwt:Issuer"], config["Jwt:Audience"], claims,
            expires: expiry, signingCredentials: creds);

        return new LoginResponse(new JwtSecurityTokenHandler().WriteToken(token), user.FullName, user.Role);
    }
}

public class PatientService : IPatientService
{
    private readonly AppDbContext db;

    public PatientService(AppDbContext db)
    {
        this.db = db; 
    }
    public async Task<List<Patient>> GetAllAsync(string? search)
    {
        var q = db.Patients.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
            q = q.Where(x => x.PatientNumber.Contains(search) || x.FirstName.Contains(search) ||
                             x.LastName.Contains(search) || x.Phone.Contains(search));
        return await q.OrderByDescending(x => x.Id).ToListAsync();
    }
    public Task<Patient?> GetAsync(int id) => db.Patients.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
    public async Task<Patient> CreateAsync(PatientRequest r)
    {
        var count = await db.Patients.CountAsync();
        var p = new Patient { PatientNumber = $"P{count + 1:0000}", FirstName = r.FirstName, LastName = r.LastName,
            Gender = r.Gender, DateOfBirth = r.DateOfBirth, Phone = r.Phone, Email = r.Email, Address = r.Address, Status = r.Status };
        db.Patients.Add(p); await db.SaveChangesAsync(); return p;
    }
    public async Task<bool> DeleteAsync(int id)
    {
        var p = await db.Patients.FindAsync(id); if (p is null) return false;
        db.Patients.Remove(p); await db.SaveChangesAsync(); return true;
    }
}

public class DoctorService: IDoctorService
{
    private readonly AppDbContext db;

    public DoctorService(AppDbContext db)
    {
        this.db = db;
    }
    public Task<List<Doctor>> GetAllAsync() => db.Doctors.AsNoTracking().OrderBy(x => x.Name).ToListAsync();
    public Task<Doctor?> GetAsync(int id) => db.Doctors.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
    public async Task<Doctor> CreateAsync(DoctorRequest r)
    {
        var count = await db.Doctors.CountAsync();
        var d = new Doctor { DoctorNumber = $"D{count + 1:0000}", Name = r.Name, Specialization = r.Specialization,
            Department = r.Department, Phone = r.Phone, Email = r.Email, ExperienceYears = r.ExperienceYears };
        db.Doctors.Add(d); await db.SaveChangesAsync(); return d;
    }
}

public class AppointmentService : IAppointmentService
{
    private readonly AppDbContext db;

    public AppointmentService(AppDbContext db)
    {
        this.db = db;
    }
    public Task<List<Appointment>> GetAllAsync() => db.Appointments.Include(x => x.Patient).Include(x => x.Doctor)
        .AsNoTracking().OrderByDescending(x => x.AppointmentDate).ToListAsync();

    public async Task<Appointment> CreateAsync(AppointmentRequest r)
    {
        var a = new Appointment { PatientId = r.PatientId, DoctorId = r.DoctorId, AppointmentDate = r.AppointmentDate,
            AppointmentTime = r.AppointmentTime, Status = r.Status, Reason = r.Reason };
        db.Appointments.Add(a); await db.SaveChangesAsync();
        return await db.Appointments.Include(x => x.Patient).Include(x => x.Doctor).FirstAsync(x => x.Id == a.Id);
    }

    public async Task<bool> UpdateStatusAsync(int id, string status)
    {
        var a = await db.Appointments.FindAsync(id); if (a is null) return false;
        a.Status = status; await db.SaveChangesAsync(); return true;
    }
}

public class PrescriptionService : IPrescriptionService
{
    private readonly AppDbContext db;

    public PrescriptionService(AppDbContext db)
    {
        this.db = db;
    }

    public Task<List<Prescription>> GetAllAsync() => db.Prescriptions.Include(x => x.Patient).Include(x => x.Doctor)
        .Include(x => x.Medicines).AsNoTracking().OrderByDescending(x => x.CreatedAt).ToListAsync();

    public Task<Prescription?> GetAsync(int id) => db.Prescriptions.Include(x => x.Patient).Include(x => x.Doctor)
        .Include(x => x.Medicines).AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);

    public async Task<Prescription> CreateAsync(PrescriptionRequest r)
    {
        var p = new Prescription { PatientId = r.PatientId, DoctorId = r.DoctorId, AppointmentId = r.AppointmentId,
            Diagnosis = r.Diagnosis, Notes = r.Notes };
        foreach (var m in r.Medicines)
            p.Medicines.Add(new PrescriptionMedicine { MedicineName = m.MedicineName, Dosage = m.Dosage,
                Frequency = m.Frequency, Duration = m.Duration, Instructions = m.Instructions });
        db.Prescriptions.Add(p); await db.SaveChangesAsync();
        return await GetAsync(p.Id) ?? p;
    }
}
