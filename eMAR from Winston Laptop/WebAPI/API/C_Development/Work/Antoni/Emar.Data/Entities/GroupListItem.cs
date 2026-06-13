using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Emar.Data.Entities
{
    [Table("group_list_items")]
    public class GroupListItem
    {
        [Column("id", TypeName = "int"), Key]
        public int Id { get; set; }

        [Column("site_id", TypeName = "int"), Required]
        public int SiteId { get; set; }

        [Column("department_code", TypeName = "varchar(15)")]
        public string DepartmentCode { get; set; }

        [Column("group_name", TypeName = "nvarchar(255)"), Required]
        public string GroupName { get; set; }

        [Column("medication_id", TypeName = "int")]
        public int MedicationId { get; set; }

        [Column("duration_in_minutes", TypeName = "int")]
        public int DurationInMinutes { get; set; }

        [Column("dose", TypeName = "decimal(11, 2)")]
        public decimal? Dose { get; set; }

        [Column("medication_unit_id")]
        public int? MedicationUnitId { get; set; }

        [Column("medication_route_id", TypeName = "int")]
        public int? MedicationRouteId { get; set; }

        [Column("frequency_schedule_id", TypeName = "int")]
        public int? FrequencyScheduleId { get; set; }

        [Column("duration", TypeName = "int")]
        public int? Duration { get; set; }

        [Column("duration_unit_id", TypeName = "int")]
        public int? DurationUnitId { get; set; }

        [Column("priority", TypeName = "tinyint")]
        public byte? Priority { get; set; }

        [Column("order_notes", TypeName = "nvarchar(MAX)")]
        public string OrderNotes { get; set; }


        // For Foreign Key: fk__group_list_items__duration_units
        [ForeignKey(nameof(DurationUnitId))]
        [InverseProperty(nameof(Entities.DurationUnit.GroupListItems))]
        public virtual DurationUnit DurationUnit { get; set; }

        [ForeignKey(nameof(FrequencyScheduleId))]
        [InverseProperty(nameof(Entities.FrequencySchedule.GroupListItems))]
        public virtual FrequencySchedule FrequencySchedule { get; set; }

        // For Foreign Key: fk__group_list_items__medications
        [ForeignKey(nameof(MedicationId))]
        [InverseProperty(nameof(Entities.Medication.GroupListItems))]
        public virtual Medication Medication { get; set; }

        [ForeignKey(nameof(MedicationRouteId))]
        [InverseProperty(nameof(Entities.MedicationRoute.GroupListItems))]
        public virtual MedicationRoute MedicationRoute { get; set; }

        [ForeignKey(nameof(MedicationUnitId))]
        [InverseProperty(nameof(Entities.MedicationUnit.GroupListItems))]
        public virtual MedicationUnit MedicationUnit { get; set; }

        [ForeignKey(nameof(SiteId))]
        [InverseProperty(nameof(Entities.Site.GroupListItems))]
        public virtual Site Site { get; set; }
    }
}