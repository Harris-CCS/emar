using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Emar.Data.Entities
{
    [Table("duration_units")]
    public class DurationUnit
    {
        public DurationUnit()
        {
            // For Foreign Key: fk__department_preferred_list_items__duration_units
            DepartmentPreferredListItems = new HashSet<DepartmentPreferredListItem>();

            // For Foreign Key: fk__group_list_items__duration_units
            GroupListItems = new HashSet<GroupListItem>();

            // For Foreign Key: fk__patient_cart_orders__duration_units
            PatientCartOrders = new HashSet<PatientCartOrder>();

            // For Foreign Key: fk__patient_orders__duration_units
            PatientOrders = new HashSet<PatientOrder>();

            // For Foreign Key: fk__user_quick_list_items__duration_units
            UserQuickListItems = new HashSet<UserQuickListItem>();
        }

        [Column("id", TypeName = "int"), Key]
        public int Id { get; set; }

        [Column("duration_in_minutes", TypeName = "int")]
        public int DurationInMinutes { get; set; }

        [Column("name", TypeName = "varchar(40)"), Required]
        public string Name { get; set; }


        // For Foreign Key: fk__department_preferred_list_items__duration_units
        [InverseProperty("DurationUnit")]
        public virtual ICollection<DepartmentPreferredListItem> DepartmentPreferredListItems { get; set; }

        // For Foreign Key: fk__group_list_items__duration_units
        [InverseProperty("DurationUnit")]
        public virtual ICollection<GroupListItem> GroupListItems { get; set; }

        // For Foreign Key: fk__patient_cart_orders__duration_units
        [InverseProperty("DurationUnit")]
        public virtual ICollection<PatientCartOrder> PatientCartOrders { get; set; }

        // For Foreign Key: fk__patient_orders__duration_units
        [InverseProperty("DurationUnit")]
        public virtual ICollection<PatientOrder> PatientOrders { get; set; }

        // For Foreign Key: fk__user_quick_list_items__duration_units
        [InverseProperty("DurationUnit")]
        public virtual ICollection<UserQuickListItem> UserQuickListItems { get; set; }
    }
}