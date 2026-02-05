using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PatientManagmentSubsystem.Models
{
    public class Address
    {

        [Key]                                     
        public int AddressId { get; set; }

        
        public int? PatientId { get; set; }

        [ForeignKey(nameof(PatientId))]
        public Patient? Patient { get; set; } = null!;

        [Required, StringLength(100)]
        public string Street { get; set; } = string.Empty;

        [Required, StringLength(50)]
        public string City { get; set; } = string.Empty;

        [StringLength(50)]
        public string? Province { get; set; }

        [Required, StringLength(20)]
        public string PostalCode { get; set; } = string.Empty;

        [StringLength(50)]
        public string? Country { get; set; } = "South Africa";

        public bool IsActive { get; set; } = true;

        public DateTime? CreatedOn { get; set; }
        public DateTime? ModifiedOn { get; set; }
    }
}
