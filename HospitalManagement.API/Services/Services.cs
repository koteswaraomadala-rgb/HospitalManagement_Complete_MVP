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
    Task<List<Patient>> GetAllAsync(string? search);
    Task<Patient?> GetAsync(int id);
    Task<Patient> CreateAsync(PatientRequest request);
    Task<Patient?> UpdateAsync(int id, PatientRequest request);
    Task<bool> DeleteAsync(int id);
}

public interface IDoctorService
{
    Task<List<Doctor>> GetAllAsync();
    Task<Doctor?> GetAsync(int id);
    Task<Doctor> CreateAsync(DoctorRequest request);
    Task<Doctor?> UpdateAsync(int id, DoctorRequest request);
    Task<bool> DeleteAsync(int id);
}

public interface IAppointmentService
{
    Task<List<Appointment>> GetAllAsync(string? search, DateTime? date);
    Task<Appointment?> GetAsync(int id);
    Task<Appointment> CreateAsync(AppointmentRequest request);
    Task<Appointment?> UpdateAsync(int id, AppointmentRequest request);
    Task<bool> UpdateStatusAsync(int id, string status);
    Task<bool> DeleteAsync(int id);
}

public interface IPrescriptionService
{
    Task<List<Prescription>> GetAllAsync(string? search);
    Task<Prescription?> GetAsync(int id);
    Task<Prescription> CreateAsync(PrescriptionRequest request);
    Task<Prescription?> UpdateAsync(int id, PrescriptionRequest request);
    Task<bool> DeleteAsync(int id);
}

public interface IReportService
{
    Task<object> GetSummaryAsync(DateTime? from, DateTime? to);
}

public interface ISettingsService
{
    Task<AppSetting> GetAsync();
    Task<AppSetting> UpdateAsync(AppSettingRequest request);
}

public class AuthService : IAuthService
{
    private readonly AppDbContext _db;
    private readonly IConfiguration _configuration;

    public AuthService(AppDbContext db, IConfiguration configuration)
    {
        _db = db;
        _configuration = configuration;
    }

    public async Task<LoginResponse?> LoginAsync(LoginRequest request)
    {
        var username = request.Username.Trim();

        var user = await _db.Users.FirstOrDefaultAsync(item =>
            item.Username == username && item.Password == request.Password);

        if (user == null)
        {
            return null;
        }

        var jwtKey = _configuration["Jwt:Key"]
            ?? throw new InvalidOperationException("Jwt:Key is not configured.");

        var issuer = _configuration["Jwt:Issuer"] ?? "HospitalManagement.API";
        var audience = _configuration["Jwt:Audience"] ?? "HospitalManagement.Web";

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.FullName),
            new Claim(ClaimTypes.Role, user.Role)
        };

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var expiryMinutes = int.TryParse(
            _configuration["Jwt:ExpiryMinutes"],
            out var configuredMinutes)
            ? configuredMinutes
            : 120;

        var expiry = DateTime.UtcNow.AddMinutes(expiryMinutes);

        var token = new JwtSecurityToken(
            issuer,
            audience,
            claims,
            expires: expiry,
            signingCredentials: credentials);

        var tokenValue = new JwtSecurityTokenHandler().WriteToken(token);

        return new LoginResponse(tokenValue, user.FullName, user.Role);
    }

    public Task<User?> GetUserAsync(int userId)
    {
        return _db.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == userId);
    }

    public async Task<bool> ChangePasswordAsync(int userId, ChangePasswordRequest request)
    {
        var user = await _db.Users.FindAsync(userId);

        if (user == null || user.Password != request.CurrentPassword)
        {
            return false;
        }

        user.Password = request.NewPassword;
        await _db.SaveChangesAsync();

        return true;
    }

    public async Task<User?> UpdateProfileAsync(int userId, ProfileRequest request)
    {
        var user = await _db.Users.FindAsync(userId);

        if (user == null)
        {
            return null;
        }

        user.FullName = request.FullName.Trim();
        user.Role = request.Role.Trim();

        await _db.SaveChangesAsync();
        return user;
    }
}

public class PatientService : IPatientService
{
    private readonly AppDbContext _db;

    public PatientService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<Patient>> GetAllAsync(string? search)
    {
        var query = _db.Patients.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(item =>
                item.PatientNumber.Contains(search) ||
                item.FirstName.Contains(search) ||
                item.LastName.Contains(search) ||
                item.Phone.Contains(search));
        }

        return await query
            .OrderByDescending(item => item.Id)
            .ToListAsync();
    }

    public Task<Patient?> GetAsync(int id)
    {
        return _db.Patients
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == id);
    }

    public async Task<Patient> CreateAsync(PatientRequest request)
    {
        var nextId = (await _db.Patients.MaxAsync(item => (int?)item.Id) ?? 0) + 1;

        var patient = new Patient
        {
            PatientNumber = $"P{nextId:0000}",
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            Gender = request.Gender,
            DateOfBirth = DateTime.SpecifyKind(request.DateOfBirth.Date, DateTimeKind.Utc),
            Phone = request.Phone.Trim(),
            Email = request.Email?.Trim() ?? string.Empty,
            Address = request.Address?.Trim() ?? string.Empty,
            Status = request.Status,
            CreatedAt = DateTime.UtcNow
        };

        _db.Patients.Add(patient);
        await _db.SaveChangesAsync();

        return patient;
    }

    public async Task<Patient?> UpdateAsync(int id, PatientRequest request)
    {
        var patient = await _db.Patients.FindAsync(id);

        if (patient == null)
        {
            return null;
        }

        patient.FirstName = request.FirstName.Trim();
        patient.LastName = request.LastName.Trim();
        patient.Gender = request.Gender;
        patient.DateOfBirth = DateTime.SpecifyKind(request.DateOfBirth.Date, DateTimeKind.Utc);
        patient.Phone = request.Phone.Trim();
        patient.Email = request.Email?.Trim() ?? string.Empty;
        patient.Address = request.Address?.Trim() ?? string.Empty;
        patient.Status = request.Status;

        await _db.SaveChangesAsync();
        return patient;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var patient = await _db.Patients.FindAsync(id);

        if (patient == null)
        {
            return false;
        }

        var hasAppointments = await _db.Appointments.AnyAsync(item => item.PatientId == id);
        var hasPrescriptions = await _db.Prescriptions.AnyAsync(item => item.PatientId == id);

        if (hasAppointments || hasPrescriptions)
        {
            throw new InvalidOperationException(
                "Patient cannot be deleted because related appointments or prescriptions exist.");
        }

        _db.Patients.Remove(patient);
        await _db.SaveChangesAsync();

        return true;
    }
}

public class DoctorService : IDoctorService
{
    private readonly AppDbContext _db;

    public DoctorService(AppDbContext db)
    {
        _db = db;
    }

    public Task<List<Doctor>> GetAllAsync()
    {
        return _db.Doctors
            .AsNoTracking()
            .OrderBy(item => item.Name)
            .ToListAsync();
    }

    public Task<Doctor?> GetAsync(int id)
    {
        return _db.Doctors
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == id);
    }

    public async Task<Doctor> CreateAsync(DoctorRequest request)
    {
        var nextId = (await _db.Doctors.MaxAsync(item => (int?)item.Id) ?? 0) + 1;

        var doctor = new Doctor
        {
            DoctorNumber = $"D{nextId:0000}",
            Name = request.Name.Trim(),
            Specialization = request.Specialization.Trim(),
            Department = request.Department.Trim(),
            Phone = request.Phone.Trim(),
            Email = request.Email?.Trim() ?? string.Empty,
            ExperienceYears = request.ExperienceYears
        };

        _db.Doctors.Add(doctor);
        await _db.SaveChangesAsync();

        return doctor;
    }

    public async Task<Doctor?> UpdateAsync(int id, DoctorRequest request)
    {
        var doctor = await _db.Doctors.FindAsync(id);

        if (doctor == null)
        {
            return null;
        }

        doctor.Name = request.Name.Trim();
        doctor.Specialization = request.Specialization.Trim();
        doctor.Department = request.Department.Trim();
        doctor.Phone = request.Phone.Trim();
        doctor.Email = request.Email?.Trim() ?? string.Empty;
        doctor.ExperienceYears = request.ExperienceYears;

        await _db.SaveChangesAsync();
        return doctor;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var doctor = await _db.Doctors.FindAsync(id);

        if (doctor == null)
        {
            return false;
        }

        var hasAppointments = await _db.Appointments.AnyAsync(item => item.DoctorId == id);
        var hasPrescriptions = await _db.Prescriptions.AnyAsync(item => item.DoctorId == id);

        if (hasAppointments || hasPrescriptions)
        {
            throw new InvalidOperationException(
                "Doctor cannot be deleted because related appointments or prescriptions exist.");
        }

        _db.Doctors.Remove(doctor);
        await _db.SaveChangesAsync();

        return true;
    }
}

public class AppointmentService : IAppointmentService
{
    private readonly AppDbContext _db;

    public AppointmentService(AppDbContext db)
    {
        _db = db;
    }

    private IQueryable<Appointment> Query()
    {
        return _db.Appointments
            .Include(item => item.Patient)
            .Include(item => item.Doctor)
            .AsNoTracking();
    }

    public async Task<List<Appointment>> GetAllAsync(string? search, DateTime? date)
    {
        var query = Query();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(item =>
                item.Patient.FirstName.Contains(search) ||
                item.Patient.LastName.Contains(search) ||
                item.Doctor.Name.Contains(search) ||
                item.Status.Contains(search));
        }

        if (date.HasValue)
        {
            var appointmentDate = date.Value.Date;
            query = query.Where(item => item.AppointmentDate.Date == appointmentDate);
        }

        return await query
            .OrderByDescending(item => item.AppointmentDate)
            .ThenBy(item => item.AppointmentTime)
            .ToListAsync();
    }

    public Task<Appointment?> GetAsync(int id)
    {
        return Query().FirstOrDefaultAsync(item => item.Id == id);
    }

    public async Task<Appointment> CreateAsync(AppointmentRequest request)
    {
        if (!await _db.Patients.AnyAsync(item => item.Id == request.PatientId))
        {
            throw new InvalidOperationException("Patient not found.");
        }

        if (!await _db.Doctors.AnyAsync(item => item.Id == request.DoctorId))
        {
            throw new InvalidOperationException("Doctor not found.");
        }

        var appointmentDate = DateTime.SpecifyKind(
            request.AppointmentDate.Date,
            DateTimeKind.Utc);

        if (appointmentDate < DateTime.UtcNow.Date)
        {
            throw new InvalidOperationException("Appointment date cannot be in the past.");
        }

        var appointment = new Appointment
        {
            PatientId = request.PatientId,
            DoctorId = request.DoctorId,
            AppointmentDate = appointmentDate,
            AppointmentTime = request.AppointmentTime,
            Status = request.Status,
            Reason = request.Reason.Trim()
        };

        _db.Appointments.Add(appointment);
        await _db.SaveChangesAsync();

        return await GetAsync(appointment.Id) ?? appointment;
    }

    public async Task<Appointment?> UpdateAsync(int id, AppointmentRequest request)
    {
        var appointment = await _db.Appointments.FindAsync(id);

        if (appointment == null)
        {
            return null;
        }

        appointment.PatientId = request.PatientId;
        appointment.DoctorId = request.DoctorId;
        appointment.AppointmentDate = DateTime.SpecifyKind(
            request.AppointmentDate.Date,
            DateTimeKind.Utc);
        appointment.AppointmentTime = request.AppointmentTime;
        appointment.Status = request.Status;
        appointment.Reason = request.Reason.Trim();

        await _db.SaveChangesAsync();
        return await GetAsync(id);
    }

    public async Task<bool> UpdateStatusAsync(int id, string status)
    {
        var appointment = await _db.Appointments.FindAsync(id);

        if (appointment == null)
        {
            return false;
        }

        appointment.Status = status;
        await _db.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var appointment = await _db.Appointments.FindAsync(id);

        if (appointment == null)
        {
            return false;
        }

        if (await _db.Prescriptions.AnyAsync(item => item.AppointmentId == id))
        {
            throw new InvalidOperationException(
                "Appointment cannot be deleted because a prescription is linked to it.");
        }

        _db.Appointments.Remove(appointment);
        await _db.SaveChangesAsync();

        return true;
    }
}

public class PrescriptionService : IPrescriptionService
{
    private readonly AppDbContext _db;

    public PrescriptionService(AppDbContext db)
    {
        _db = db;
    }

    private IQueryable<Prescription> Query()
    {
        return _db.Prescriptions
            .Include(item => item.Patient)
            .Include(item => item.Doctor)
            .Include(item => item.Medicines)
            .AsNoTracking();
    }

    public async Task<List<Prescription>> GetAllAsync(string? search)
    {
        var query = Query();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(item =>
                item.Patient.FirstName.Contains(search) ||
                item.Patient.LastName.Contains(search) ||
                item.Doctor.Name.Contains(search) ||
                item.Diagnosis.Contains(search));
        }

        return await query
            .OrderByDescending(item => item.CreatedAt)
            .ToListAsync();
    }

    public Task<Prescription?> GetAsync(int id)
    {
        return Query().FirstOrDefaultAsync(item => item.Id == id);
    }

    public async Task<Prescription> CreateAsync(PrescriptionRequest request)
    {
        if (!await _db.Patients.AnyAsync(item => item.Id == request.PatientId))
        {
            throw new InvalidOperationException("Patient not found.");
        }

        if (!await _db.Doctors.AnyAsync(item => item.Id == request.DoctorId))
        {
            throw new InvalidOperationException("Doctor not found.");
        }

        if (!await _db.Appointments.AnyAsync(item => item.Id == request.AppointmentId))
        {
            throw new InvalidOperationException("Appointment not found.");
        }

        if (await _db.Prescriptions.AnyAsync(item => item.AppointmentId == request.AppointmentId))
        {
            throw new InvalidOperationException(
                "A prescription already exists for this appointment.");
        }

        var prescription = new Prescription
        {
            PatientId = request.PatientId,
            DoctorId = request.DoctorId,
            AppointmentId = request.AppointmentId,
            Diagnosis = request.Diagnosis.Trim(),
            Notes = request.Notes?.Trim() ?? string.Empty,
            CreatedAt = DateTime.UtcNow
        };

        foreach (var medicine in request.Medicines)
        {
            prescription.Medicines.Add(new PrescriptionMedicine
            {
                MedicineName = medicine.MedicineName.Trim(),
                Dosage = medicine.Dosage.Trim(),
                Frequency = medicine.Frequency.Trim(),
                Duration = medicine.Duration.Trim(),
                Instructions = medicine.Instructions?.Trim() ?? string.Empty
            });
        }

        _db.Prescriptions.Add(prescription);
        await _db.SaveChangesAsync();

        return await GetAsync(prescription.Id) ?? prescription;
    }

    public async Task<Prescription?> UpdateAsync(int id, PrescriptionRequest request)
    {
        var prescription = await _db.Prescriptions
            .Include(item => item.Medicines)
            .FirstOrDefaultAsync(item => item.Id == id);

        if (prescription == null)
        {
            return null;
        }

        prescription.PatientId = request.PatientId;
        prescription.DoctorId = request.DoctorId;
        prescription.AppointmentId = request.AppointmentId;
        prescription.Diagnosis = request.Diagnosis.Trim();
        prescription.Notes = request.Notes?.Trim() ?? string.Empty;

        _db.PrescriptionMedicines.RemoveRange(prescription.Medicines);
        prescription.Medicines = new List<PrescriptionMedicine>();

        foreach (var medicine in request.Medicines)
        {
            prescription.Medicines.Add(new PrescriptionMedicine
            {
                MedicineName = medicine.MedicineName.Trim(),
                Dosage = medicine.Dosage.Trim(),
                Frequency = medicine.Frequency.Trim(),
                Duration = medicine.Duration.Trim(),
                Instructions = medicine.Instructions?.Trim() ?? string.Empty
            });
        }

        await _db.SaveChangesAsync();
        return await GetAsync(id);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var prescription = await _db.Prescriptions.FindAsync(id);

        if (prescription == null)
        {
            return false;
        }

        _db.Prescriptions.Remove(prescription);
        await _db.SaveChangesAsync();

        return true;
    }
}

public class ReportService : IReportService
{
    private readonly AppDbContext _db;

    public ReportService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<object> GetSummaryAsync(DateTime? from, DateTime? to)
    {
        var start = DateTime.SpecifyKind(
            (from ?? DateTime.UtcNow.Date.AddDays(-29)).Date,
            DateTimeKind.Utc);

        var end = DateTime.SpecifyKind(
            (to ?? DateTime.UtcNow.Date).Date.AddDays(1).AddTicks(-1),
            DateTimeKind.Utc);

        return new
        {
            from = start,
            to = end,
            totalPatients = await _db.Patients.CountAsync(item =>
                item.CreatedAt >= start && item.CreatedAt <= end),
            totalDoctors = await _db.Doctors.CountAsync(),
            appointments = await _db.Appointments.CountAsync(item =>
                item.AppointmentDate >= start && item.AppointmentDate <= end),
            completedAppointments = await _db.Appointments.CountAsync(item =>
                item.AppointmentDate >= start &&
                item.AppointmentDate <= end &&
                item.Status == "Completed"),
            cancelledAppointments = await _db.Appointments.CountAsync(item =>
                item.AppointmentDate >= start &&
                item.AppointmentDate <= end &&
                item.Status == "Cancelled"),
            prescriptions = await _db.Prescriptions.CountAsync(item =>
                item.CreatedAt >= start && item.CreatedAt <= end)
        };
    }
}

public class SettingsService : ISettingsService
{
    private readonly AppDbContext _db;

    public SettingsService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<AppSetting> GetAsync()
    {
        var settings = await _db.AppSettings.FirstOrDefaultAsync();

        if (settings != null)
        {
            return settings;
        }

        settings = new AppSetting();
        _db.AppSettings.Add(settings);
        await _db.SaveChangesAsync();

        return settings;
    }

    public async Task<AppSetting> UpdateAsync(AppSettingRequest request)
    {
        var settings = await GetAsync();

        settings.HospitalName = request.HospitalName.Trim();
        settings.ContactEmail = request.ContactEmail?.Trim() ?? string.Empty;
        settings.ContactPhone = request.ContactPhone?.Trim() ?? string.Empty;
        settings.Address = request.Address?.Trim() ?? string.Empty;

        await _db.SaveChangesAsync();
        return settings;
    }
}
