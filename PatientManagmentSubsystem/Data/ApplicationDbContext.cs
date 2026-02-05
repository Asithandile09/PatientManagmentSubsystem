using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PatientManagmentSubsystem.Models;

namespace PatientManagmentSubsystem.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        // Existing DbSets
        public DbSet<Patient> Patients { get; set; }
        public DbSet<Doctor> Doctors { get; set; }
        public DbSet<Ward> Wards { get; set; }
        public DbSet<Bed> Beds { get; set; }
        public DbSet<PatientMovement> PatientMovements { get; set; }
        public DbSet<Address> Addresses { get; set; }
        public DbSet<MedicalHistory> MedicalHistories { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Seed default wards
            builder.Entity<Ward>().HasData(
                new Ward { WardId = 1, Name = "General Ward", TotalBeds = 20, AvailableBeds = 20 },
                new Ward { WardId = 2, Name = "Emergency", TotalBeds = 25, AvailableBeds = 20 },
                new Ward { WardId = 3, Name = "ICU", TotalBeds = 10, AvailableBeds = 10 },
                new Ward { WardId = 4, Name = "Pediatric", TotalBeds = 15, AvailableBeds = 15 });

            // Seed sample doctors
            builder.Entity<Doctor>().HasData(
                new Doctor { DoctorId = 1, FirstName = "Dr Sipho Ndlovu", Specialty = "General Practitioner" },
                new Doctor { DoctorId = 2, FirstName = "Dr Lerato Mokoena", Specialty = "Paediatrics" },
                new Doctor { DoctorId = 3, FirstName = "Dr Themba Dlamini", Specialty = "Cardiology" }
            );
        }
    }

    // Keep ApplicationUser in Data namespace only
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;
    }
}