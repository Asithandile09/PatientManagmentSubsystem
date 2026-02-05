using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PatientManagmentSubsystem.Models
{
    public class MedicalHistory
    {
        [Key]
        public int HistoryId { get; set; }

        [Required]
        public int PatientId { get; set; }

        [ForeignKey(nameof(PatientId))]
        public Patient Patient { get; set; } = null!;

        
        public bool HasAllergies { get; set; }
        [StringLength(200)]
        public string? AllergyDetails { get; set; }

        public bool HasChronicIllness { get; set; }
        [StringLength(200)]
        public string? ChronicIllnessDetails { get; set; }

        public bool IsPregnant { get; set; }
        public bool UsesContraceptives { get; set; }

        public bool Smokes { get; set; }
        public bool DrinksAlcohol { get; set; }

        public bool FamilyHeartDisease { get; set; }
        public bool FamilyDiabetes { get; set; }
        public bool FamilyCancer { get; set; }

        [StringLength(500)]
        public string? OtherNotes { get; set; }

        public DateTime CreatedOn { get; set; } = DateTime.Now;
    }
}
