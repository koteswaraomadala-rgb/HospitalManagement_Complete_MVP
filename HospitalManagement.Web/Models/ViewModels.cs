namespace HospitalManagement.Web.Models;

public record LoginViewModel(string Username, string Password);

public class PatientViewModel
{
    public int Id { get; set; }
    public string PatientNumber { get; set; } = "";
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public string Gender { get; set; } = "";
    public DateTime DateOfBirth { get; set; }
    public string Phone { get; set; } = "";
    public string Email { get; set; } = "";
    public string Address { get; set; } = "";
    public string Status { get; set; } = "Active";
}

public class DoctorViewModel
{
    public int Id { get; set; }
    public string DoctorNumber { get; set; } = "";
    public string Name { get; set; } = "";
    public string Specialization { get; set; } = "";
    public string Department { get; set; } = "";
    public string Phone { get; set; } = "";
    public string Email { get; set; } = "";
    public int ExperienceYears { get; set; }
}

public class AppointmentViewModel
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public string PatientName { get; set; } = "";
    public int DoctorId { get; set; }
    public string DoctorName { get; set; } = "";
    public DateTime AppointmentDate { get; set; }
    public string AppointmentTime { get; set; } = "";
    public string Status { get; set; } = "";
    public string Reason { get; set; } = "";
}

public class DashboardViewModel
{
    public int Patients { get; set; }
    public int Doctors { get; set; }
    public int TodayAppointments { get; set; }
    public int PendingPrescriptions { get; set; }
    public List<AppointmentViewModel> Appointments { get; set; } = new List<AppointmentViewModel>();
}
