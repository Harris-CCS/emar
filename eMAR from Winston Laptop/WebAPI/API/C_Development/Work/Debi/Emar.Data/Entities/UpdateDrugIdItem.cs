using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Emar.Data.Entities
{
    [NotMapped]
    public class UpdateDrugIdItem
    {
        [Column("medication_id", TypeName = "int")]
        public int MedicationId { get; set; }

        [Column("site_id", TypeName = "int"), Key]
        public int SiteId { get; set; }

        [Column("ndc", TypeName = "varchar(32)"), Key]
        public string Ndc { get; set; }

        [Column("drug_id", TypeName = "varchar(32)"), Key]
        public string DrugId { get; set; }

        [Column("brand_name", TypeName = "nvarchar(255)"), Key]
        public string BrandName { get; set; }

        [Column("match", TypeName = "nvarchar(255)"), Key]
        public string Match { get; set; }
    }
}