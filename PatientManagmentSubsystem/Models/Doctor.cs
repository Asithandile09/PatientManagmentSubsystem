using System.ComponentModel.DataAnnotations;

namespace PatientManagmentSubsystem.Models
{
    public class Doctor
    {
        [Key]
        public int DoctorId { get; set; }

        [Required, StringLength(50,ErrorMessage ="First Name cannot exceed 50 characters")]
        [Display(Name = "First Name")]
        public string? FirstName { get; set; }

        [ StringLength(50, ErrorMessage = "Last Name cannot exceed 50 characters")]
        [Display(Name = "Last Name")]
        public string? LastName { get; set; }

        [StringLength(100)]
        public string? Specialty { get; set; }
        [Display(Name ="Medical Speciality")]

        public ICollection<Patient>? Patients { get; set; }
    }
}
