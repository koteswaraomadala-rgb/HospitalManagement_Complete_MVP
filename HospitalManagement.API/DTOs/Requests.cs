using System.ComponentModel.DataAnnotations;

namespace HospitalManagement.API.DTOs;

public record LoginRequest(
    [Required] string Username,
    [Required] string Password);

public record LoginResponse(
    string Token,
    string FullName,
    string Role);

public record PatientRequest(
    [Required, StringLength(50)] string FirstName,
    [Required, StringLength(50)] string LastName,
    [Required] string Gender,
    DateTime DateOfBirth,
    [Required, Phone] string Phone,
    [EmailAddress] string Email,
    [StringLength(300)] string Address,
    [Required] string Status);

public record DoctorRequest(
    [Required, StringLength(100)] string Name,
    [Required, StringLength(100)] string Specialization,
    [Required, StringLength(100)] string Department,
    [Required, Phone] string Phone,
    [EmailAddress] string Email,
    [Range(0, 70)] int ExperienceYears);

public record AppointmentRequest(
    [Range(1, int.MaxValue)] int PatientId,
    [Range(1, int.MaxValue)] int DoctorId,
    DateTime AppointmentDate,
    [Required] string AppointmentTime,
    [Required] string Status,
    [Required, StringLength(300)] string Reason);

public record StatusRequest(
    [Required] string Status);

public record MedicineRequest(
    [Required, StringLength(150)] string MedicineName,
    [Required, StringLength(100)] string Dosage,
    [Required, StringLength(100)] string Frequency,
    [Required, StringLength(100)] string Duration,
    [StringLength(300)] string Instructions);

public record PrescriptionRequest(
    [Range(1, int.MaxValue)] int PatientId,
    [Range(1, int.MaxValue)] int DoctorId,
    [Range(1, int.MaxValue)] int AppointmentId,
    [Required, StringLength(300)] string Diagnosis,
    [StringLength(1000)] string Notes,
    [Required, MinLength(1)] List<MedicineRequest> Medicines);

public record ChangePasswordRequest(
    [Required] string CurrentPassword,
    [Required, MinLength(6)] string NewPassword);

public record ProfileRequest(
    [Required, StringLength(100)] string FullName,
    [Required] string Role);

public record AppSettingResponse(
    string HospitalName,
    string ContactEmail,
    string ContactPhone,
    string Address);

public record AppSettingRequest(
    [Required, StringLength(150)] string HospitalName,
    [EmailAddress] string ContactEmail,
    [Phone] string ContactPhone,
    [StringLength(300)] string Address);
