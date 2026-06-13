using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Emar.Data.IbexEntities
{
    [Table("emar_patient_indicators_retrieve_view")]
    public class EmarPatientIndicatorsRetrieveView
    {
        [Key]
        [Column("external_patient_id", TypeName = "char(14)"), Required]
        public string ExternalId { get; set; }

        [Column("external_site_id", TypeName = "tinyint")]
        public byte ExternalSiteId { get; set; }

        [Column("ordinal_position", TypeName = "smallint")]
        public short OrdinalPosition { get; set; }

        [Column("code", TypeName = "varchar(10)")]
        public string Code { get; set; }

        [Column("type", TypeName = "varchar(10)")]
        public string Type { get; set; }

        [Column("type_description", TypeName = "nvarchar(255)")]
        public string TypeDescription { get; set; }

        [Column("description", TypeName ="varchar(255)")]
        public string Description { get; set; }

        [Column("image_name", TypeName ="nvarchar(255)")]
        public string ImageName { get; set; }
    }
}
