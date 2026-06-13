using System.ComponentModel.DataAnnotations.Schema;

namespace Emar.Data.Entities
{
    [NotMapped]
    public class GetAntimicrobialRequiredFdbFunction
    {
        [Column("antimicrobial_required")]
        public bool AntimicrobialRequired { get; set; }
    }
}