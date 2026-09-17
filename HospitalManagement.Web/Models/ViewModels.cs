using System.ComponentModel.DataAnnotations;

namespace HospitalManagement.Web.Models;

public class LoginViewModel
{
    [Required, StringLength(50)] public string Username { get; set; } = "";
    [Required, StringLength(100)] public string Password { get; set; } = "";
}

public class PatientViewModel
{
    public int Id { get; set; } public string PatientNumber { get; set; } = "";
    [Required, StringLength(50)] public string FirstName { get; set; } = "";
    [Required, StringLength(50)] public string LastName { get; set; } = "";
    [Required] public string Gender { get; set; } = "";
    [DataType(DataType.Date)] public DateTime DateOfBirth { get; set; }
    [Required, Phone] public string Phone { get; set; } = "";
    [EmailAddress] public string Email { get; set; } = "";
    [StringLength(300)] public string Address { get; set; } = "";
    [Required] public string Status { get; set; } = "Active";
}

public class DoctorViewModel
{
    public int Id { get; set; } public string DoctorNumber { get; set; } = "";
    [Required, StringLength(100)] public string Name { get; set; } = "";
    [Required, StringLength(100)] public string Specialization { get; set; } = "";
    [Required, StringLength(100)] public string Department { get; set; } = "";
    [Required, Phone] public string Phone { get; set; } = "";
    [EmailAddress] public string Email { get; set; } = "";
    [Range(0, 70)] public int ExperienceYears { get; set; }
}

public class AppointmentViewModel
{
    public int Id { get; set; } [Range(1,int.MaxValue)] public int PatientId { get; set; } public string PatientName { get; set; } = "";
    [Range(1,int.MaxValue)] public int DoctorId { get; set; } public string DoctorName { get; set; } = "";
    [DataType(DataType.Date)] public DateTime AppointmentDate { get; set; } = DateTime.Today;
    [Required] public string AppointmentTime { get; set; } = ""; [Required] public string Status { get; set; } = "Confirmed";
    [Required, StringLength(300)] public string Reason { get; set; } = "";
}

public class MedicineViewModel
{
    [Required] public string MedicineName { get; set; } = ""; [Required] public string Dosage { get; set; } = "";
    [Required] public string Frequency { get; set; } = ""; [Required] public string Duration { get; set; } = "";
    public string Instructions { get; set; } = "";
}

public class PrescriptionViewModel
{
    public int Id { get; set; } public int PatientId { get; set; } public PatientViewModel Patient { get; set; } = new();
    public int DoctorId { get; set; } public DoctorViewModel Doctor { get; set; } = new(); public int AppointmentId { get; set; }
    public string Diagnosis { get; set; } = ""; public string Notes { get; set; } = ""; public DateTime CreatedAt { get; set; }
    public List<MedicineViewModel> Medicines { get; set; } = new();
}

public class DashboardViewModel
{
    public int Patients { get; set; } public int Doctors { get; set; } public int TodayAppointments { get; set; }
    public int PendingPrescriptions { get; set; } public List<AppointmentViewModel> Appointments { get; set; } = new();
}

public class ReportViewModel
{
    public string From { get; set; } = ""; public string To { get; set; } = ""; public int TotalPatients { get; set; }
    public int TotalDoctors { get; set; } public int Appointments { get; set; } public int CompletedAppointments { get; set; }
    public int CancelledAppointments { get; set; } public int Prescriptions { get; set; }
}

public class SettingsViewModel
{
    [Required, StringLength(150)] public string HospitalName { get; set; } = "";
    [EmailAddress] public string ContactEmail { get; set; } = "";
    [Phone] public string ContactPhone { get; set; } = "";
    [StringLength(300)] public string Address { get; set; } = "";
    public string Username { get; set; } = ""; public string FullName { get; set; } = ""; public string Role { get; set; } = "";
}

public class ChangePasswordViewModel
{
    [Required] public string CurrentPassword { get; set; } = "";
    [Required, MinLength(6)] public string NewPassword { get; set; } = "";
    [Required, Compare(nameof(NewPassword))] public string ConfirmPassword { get; set; } = "";
}

public class SearchRequest { public string? Search { get; set; } public string? Date { get; set; } }
public class IdRequest { public int Id { get; set; } }
public class StatusUpdateViewModel { public int Id { get; set; } public string Status { get; set; } = ""; }
public class ReportRequest { public string From { get; set; } = ""; public string To { get; set; } = ""; }
