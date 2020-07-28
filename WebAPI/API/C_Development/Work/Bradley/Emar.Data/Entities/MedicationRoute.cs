using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Emar.Data.Entities
{
    [Table("medication_routes")]
    public class MedicationRoute
    {
        public MedicationRoute()
        {
            PatientOrders = new HashSet<PatientOrder>();
            UserQuickListItems = new HashSet<UserQuickListItem>();
        }

        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("site_id", TypeName = "int"), Required]
        public int SiteId { get; set; }

        [Required]
        [Column("name")]
        [StringLength(50)]
        public string Name { get; set; }

        [InverseProperty("MedicationRoute")]
        public virtual ICollection<PatientOrder> PatientOrders { get; set; }

        [InverseProperty("MedicationRoute")]
        public virtual ICollection<UserQuickListItem> UserQuickListItems { get; set; }
    }
}
