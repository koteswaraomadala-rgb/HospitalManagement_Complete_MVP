using HospitalManagement.API.Models;
using Microsoft.EntityFrameworkCore;

namespace HospitalManagement.API.Data;

public class AppDbContext: DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
       : base(options)
    {
    }
    public DbSet<User> Users => Set<User>();
    public DbSet<Patient> Patients => Set<Patient>();
    public DbSet<Doctor> Doctors => Set<Doctor>();
    public DbSet<Appointment> Appointments => Set<Appointment>();
    public DbSet<Prescription> Prescriptions => Set<Prescription>();
    public DbSet<PrescriptionMedicine> PrescriptionMedicines => Set<PrescriptionMedicine>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>().HasIndex(x => x.Username).IsUnique();
        modelBuilder.Entity<Patient>().HasIndex(x => x.PatientNumber).IsUnique();
        modelBuilder.Entity<Doctor>().HasIndex(x => x.DoctorNumber).IsUnique();

        modelBuilder.Entity<Appointment>()
            .HasOne(x => x.Patient).WithMany(x => x.Appointments)
            .HasForeignKey(x => x.PatientId).OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Appointment>()
            .HasOne(x => x.Doctor).WithMany(x => x.Appointments)
            .HasForeignKey(x => x.DoctorId).OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Prescription>()
            .HasOne(x => x.Appointment).WithOne(x => x.Prescription)
            .HasForeignKey<Prescription>(x => x.AppointmentId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Prescription>()
            .HasOne(x => x.Patient).WithMany(x => x.Prescriptions)
            .HasForeignKey(x => x.PatientId).OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Prescription>()
            .HasOne(x => x.Doctor).WithMany(x => x.Prescriptions)
            .HasForeignKey(x => x.DoctorId).OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<PrescriptionMedicine>()
            .HasOne(x => x.Prescription).WithMany(x => x.Medicines)
            .HasForeignKey(x => x.PrescriptionId).OnDelete(DeleteBehavior.Cascade);
    }
}
