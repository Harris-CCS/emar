using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Emar.Data.Entities
{
    [NotMapped]
    public class AntimicrobialRequiredIndicator
    {
        [Key]
        public int MedicationId { get; set; }
        public bool AntimicrobialRequired { get; set; }


        [ForeignKey(nameof(MedicationId))]
        [InverseProperty(nameof(Entities.Medication.AntimicrobialRequiredIndicators))]
        public virtual Medication Medication { get; set; }
    }
}