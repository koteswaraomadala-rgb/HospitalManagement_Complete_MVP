using HospitalManagement.API.Models;
using Microsoft.EntityFrameworkCore;

namespace HospitalManagement.API.Data;

public class AppDbContext : DbContext
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
    public DbSet<AppSetting> AppSettings => Set<AppSetting>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>()
            .HasIndex(item => item.Username)
            .IsUnique();

        modelBuilder.Entity<Patient>()
            .HasIndex(item => item.PatientNumber)
            .IsUnique();

        modelBuilder.Entity<Doctor>()
            .HasIndex(item => item.DoctorNumber)
            .IsUnique();

        modelBuilder.Entity<AppSetting>()
            .HasIndex(item => item.Id)
            .IsUnique();

        modelBuilder.Entity<Appointment>()
            .HasOne(item => item.Patient)
            .WithMany(item => item.Appointments)
            .HasForeignKey(item => item.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Appointment>()
            .HasOne(item => item.Doctor)
            .WithMany(item => item.Appointments)
            .HasForeignKey(item => item.DoctorId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Prescription>()
            .HasOne(item => item.Appointment)
            .WithOne(item => item.Prescription)
            .HasForeignKey<Prescription>(item => item.AppointmentId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Prescription>()
            .HasOne(item => item.Patient)
            .WithMany(item => item.Prescriptions)
            .HasForeignKey(item => item.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Prescription>()
            .HasOne(item => item.Doctor)
            .WithMany(item => item.Prescriptions)
            .HasForeignKey(item => item.DoctorId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<PrescriptionMedicine>()
            .HasOne(item => item.Prescription)
            .WithMany(item => item.Medicines)
            .HasForeignKey(item => item.PrescriptionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
