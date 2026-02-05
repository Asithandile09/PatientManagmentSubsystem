using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Net;

namespace PatientManagmentSubsystem.Models
{
    public class Patient
    {
        [Key]
        public int PatientId { get; set; }

        [Required, StringLength(50)]
        [Display(Name = "First Name")]
        public string FirstName { get; set; } = string.Empty;

        [Required, StringLength(50)]
        [Display(Name = "Last Name")]
        public string LastName { get; set; } = string.Empty;

        [NotMapped]
        public string FullName => $"{FirstName} {LastName}";

        [Required, DataType(DataType.Date)]
        [Display(Name = "Date of Birth")]
        public DateTime DOB { get; set; }

        [Required, StringLength(10)]
        public string Gender { get; set; } = string.Empty;

        [Phone]
        [Display(Name = "Phone Number")]
        public string? Phone { get; set; }

        [EmailAddress]
        public string? Email { get; set; }

        [Required, DataType(DataType.Date)]
        [Display(Name = "Admission Date")]
        public DateTime AdmissionDate { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Discharge Date")]
        public DateTime? DischargeDate { get; set; }

        public bool IsActive { get; set; } = true;

        // ---- Relationships ----
        [Required, ForeignKey(nameof(Doctor))]
        public int DoctorId { get; set; }
        public Doctor? Doctor { get; set; }

        [Required, ForeignKey(nameof(Ward))]
        public int? WardId { get; set; }
        public Ward? Ward { get; set; }
        [Display(Name = "Taking Medication?")]
        public bool IsTakingMedication { get; set; }

        [Display(Name = "Medication Details")]
        [StringLength(255)]
        public string? MedicationName { get; set; }
        //public string? IdNo { get; set; } seyt to ID NUMBER
        //[Display(Name = "ID Number")]
        public MedicalHistory? MedicalHistory { get; set; }



        
        public ICollection<PatientMovement> PatientMovements { get; set; } = new List<PatientMovement>();
        public ICollection<Address> Addresses { get; set; } = new List<Address>();


    }
}
