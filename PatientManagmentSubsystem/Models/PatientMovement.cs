using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PatientManagmentSubsystem.Models
{
    public class PatientMovement
    {
        [Key]
        public int PatientMovementId { get; set; }

        [Required(ErrorMessage = "Patient is required.")]
        public int PatientId { get; set; }

        [ForeignKey(nameof(PatientId))]
        public Patient Patient { get; set; } = null!;

        [Required(ErrorMessage = "Please select a starting location.")]
        [StringLength(100)]
        public string FromLocation { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please select a destination.")]
        [StringLength(100)]
        public string ToLocation { get; set; } = string.Empty;

     
        public int? FromWardId { get; set; }
        [ForeignKey(nameof(FromWardId))]
        public Ward? FromWard { get; set; }

        public int? ToWardId { get; set; }
        [ForeignKey(nameof(ToWardId))]
        public Ward? ToWard { get; set; }

        public int? ToBedId { get; set; }
        [ForeignKey(nameof(ToBedId))]
        public Bed? ToBed { get; set; }

        [Required(ErrorMessage = "Movement date/time is required.")]
        public DateTime MovedAt { get; set; } = DateTime.Now;

       
        public MovementType Type { get; set; } = MovementType.Transfer;

        public string? Notes { get; set; }
    }

    public enum MovementType
    {
        Admission,    
        Transfer,     
        Discharge,    
        Temporary     // Temporary movement (surgery, radiology, etc.)
    }
}