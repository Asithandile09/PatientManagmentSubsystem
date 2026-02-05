using System.ComponentModel.DataAnnotations;

namespace PatientManagmentSubsystem.Models
{
    public class Ward
    {
        [Key]
        public int WardId { get; set; }

        [Required, StringLength(100)]
        public string? Name { get; set; }

        [Range(1, 1000)]
        public int TotalBeds { get; set; }

        [Range(0, 1000)]
        [Display(Name = "Available Beds")]
        public int AvailableBeds { get; set; }

        public ICollection<Bed>? Beds { get; set; }
    }
}
