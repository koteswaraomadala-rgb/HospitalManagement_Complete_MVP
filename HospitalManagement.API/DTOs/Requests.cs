namespace HospitalManagement.API.DTOs;

public record LoginRequest(string Username, string Password);
public record LoginResponse(string Token, string FullName, string Role);

public record PatientRequest(
    string FirstName, string LastName, string Gender, DateTime DateOfBirth,
    string Phone, string Email, string Address, string Status);

public record DoctorRequest(
    string Name, string Specialization, string Department,
    string Phone, string Email, int ExperienceYears);

public record AppointmentRequest(
    int PatientId, int DoctorId, DateTime AppointmentDate,
    string AppointmentTime, string Status, string Reason);

public record MedicineRequest(
    string MedicineName, string Dosage, string Frequency,
    string Duration, string Instructions);

public record PrescriptionRequest(
    int PatientId, int DoctorId, int AppointmentId,
    string Diagnosis, string Notes, List<MedicineRequest> Medicines);
