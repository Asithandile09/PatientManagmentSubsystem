using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PatientManagmentSubsystem.Models
{
    public class Bed
    {
        [Key]
        public int BedId { get; set; }
        [Required, StringLength(50)]
        public string? Name { get; set; }

        [Required]
        public int WardId { get; set; }
        public Ward ?Ward { get; set; }

        public bool IsOccupied{ get; set; } = false;
        //bed can be empty hence nullable
        public int? PatientId { get; set; }
        public Patient? Patient { get; set; }

        [NotMapped]
        public bool IsAvailable => !IsOccupied;
    }
}
