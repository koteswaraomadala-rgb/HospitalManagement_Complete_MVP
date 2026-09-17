namespace HospitalManagement.API.Models;

public class User
{
    public int Id { get; set; }
    public string Username { get; set; } = "";
    public string Password { get; set; } = "";
    public string FullName { get; set; } = "";
    public string Role { get; set; } = "Receptionist";
}

public class Patient
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
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    public ICollection<Prescription> Prescriptions { get; set; } = new List<Prescription>();
}

public class Doctor
{
    public int Id { get; set; }
    public string DoctorNumber { get; set; } = "";
    public string Name { get; set; } = "";
    public string Specialization { get; set; } = "";
    public string Department { get; set; } = "";
    public string Phone { get; set; } = "";
    public string Email { get; set; } = "";
    public int ExperienceYears { get; set; }
    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    public ICollection<Prescription> Prescriptions { get; set; } = new List<Prescription>();
}

public class Appointment
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public Patient Patient { get; set; } = null!;
    public int DoctorId { get; set; }
    public Doctor Doctor { get; set; } = null!;
    public DateTime AppointmentDate { get; set; }
    public string AppointmentTime { get; set; } = "";
    public string Status { get; set; } = "Confirmed";
    public string Reason { get; set; } = "";
    public Prescription? Prescription { get; set; }
}

public class Prescription
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public Patient Patient { get; set; } = null!;
    public int DoctorId { get; set; }
    public Doctor Doctor { get; set; } = null!;
    public int AppointmentId { get; set; }
    public Appointment Appointment { get; set; } = null!;
    public string Diagnosis { get; set; } = "";
    public string Notes { get; set; } = "";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public ICollection<PrescriptionMedicine> Medicines { get; set; } = new List<PrescriptionMedicine>();
}

public class PrescriptionMedicine
{
    public int Id { get; set; }
    public int PrescriptionId { get; set; }
    public Prescription Prescription { get; set; } = null!;
    public string MedicineName { get; set; } = "";
    public string Dosage { get; set; } = "";
    public string Frequency { get; set; } = "";
    public string Duration { get; set; } = "";
    public string Instructions { get; set; } = "";
}

public class AppSetting
{
    public int Id { get; set; }
    public string HospitalName { get; set; } = "MediCare Hospital";
    public string ContactEmail { get; set; } = "admin@medicare.local";
    public string ContactPhone { get; set; } = "0000000000";
    public string Address { get; set; } = "Hospital Address";
}
