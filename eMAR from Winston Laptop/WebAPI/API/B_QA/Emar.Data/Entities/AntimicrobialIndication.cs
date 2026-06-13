using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Emar.Data.Entities
{
    [Table("antimicrobial_indications")]
    public class AntimicrobialIndication
    {
        public AntimicrobialIndication()
        {
            // For Foreign Key: FK_patient_cart_orders_antimicrobial_indications_antimicrobial_indication_id
            PatientCartOrders = new HashSet<PatientCartOrder>();

            // For Foreign Key: FK_patient_orders_antimicrobial_indications_antimicrobial_indication_id
            PatientOrders = new HashSet<PatientOrder>();
        }

        [Column("id", TypeName = "int"), Key]
        public int Id { get; set; }

        [Column("site_id", TypeName = "int")]
        public int SiteId { get; set; }

        [Column("code", TypeName = "varchar(20)"), Required]
        public string Code { get; set; }

        [Column("description", TypeName = "nvarchar(255)"), Required]
        public string Description { get; set; }

        [Column("is_active", TypeName = "bit")]
        public bool IsActive { get; set; }

        [Column("ordinal_position", TypeName = "int")]
        public int OrdinalPosition { get; set; }


        // For Foreign Key: fk__antimicrobial_indications__sites
        [ForeignKey(nameof(SiteId))]
        [InverseProperty(nameof(Entities.Site.AntimicrobialIndications))]
        public virtual Site Site { get; set; }

        // For Foreign Key: FK_patient_cart_orders_antimicrobial_indications_antimicrobial_indication_id
        [InverseProperty("AntimicrobialIndication")]
        public virtual ICollection<PatientCartOrder> PatientCartOrders { get; set; }

        // For Foreign Key: FK_patient_orders_antimicrobial_indications_antimicrobial_indication_id
        [InverseProperty("AntimicrobialIndication")]
        public virtual ICollection<PatientOrder> PatientOrders { get; set; }
    }
}