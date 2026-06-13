using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Emar.Data.Entities
{
    [Table("department_preferred_list_items")]
    public partial class DepartmentPreferredListItem
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("site_id")]
        public int SiteId { get; set; }

        [Column("department_code")]
        [StringLength(15)]
        public string DepartmentCode { get; set; }

        [Column("medication_id", TypeName = "int")]
        public int MedicationId { get; set; }

        [Column("dose", TypeName = "decimal(11, 2)")]
        public decimal? Dose { get; set; }

        [Column("medication_unit_id")]
        public int? MedicationUnitId { get; set; }

        [Column("medication_route_id")]
        public int? MedicationRouteId { get; set; }

        [Column("frequency_schedule_id", TypeName = "int")]
        public int? FrequencyScheduleId { get; set; }

        [Column("duration_in_minutes", TypeName = "int")]
        public int DurationInMinutes { get; set; }

        [Column("duration", TypeName = "int")]
        public int? Duration { get; set; }

        [Column("duration_unit_id", TypeName = "int")]
        public int? DurationUnitId { get; set; }

        [Column("priority", TypeName = "tinyint")]
        public byte? Priority { get; set; }

        [Column("order_notes")]
        public string OrderNotes { get; set; }

        [Column("ndc")]
        public string? Ndc { get; set; }


        // For Foreign Key: fk__department_preferred_list_items__duration_units
        [ForeignKey(nameof(DurationUnitId))]
        [InverseProperty(nameof(Entities.DurationUnit.DepartmentPreferredListItems))]
        public virtual DurationUnit DurationUnit { get; set; }

        [ForeignKey(nameof(FrequencyScheduleId))]
        [InverseProperty(nameof(Entities.FrequencySchedule.DepartmentPreferredListItems))]
        public virtual FrequencySchedule FrequencySchedule { get; set; }

        [ForeignKey(nameof(MedicationId))]
        [InverseProperty(nameof(Entities.Medication.DepartmentPreferredListItems))]
        public virtual Medication Medication { get; set; }

        [ForeignKey(nameof(MedicationRouteId))]
        [InverseProperty(nameof(Entities.MedicationRoute.DepartmentPreferredListItems))]
        public virtual MedicationRoute MedicationRoute { get; set; }

        [ForeignKey(nameof(MedicationUnitId))]
        [InverseProperty(nameof(Entities.MedicationUnit.DepartmentPreferredListItems))]
        public virtual MedicationUnit MedicationUnit { get; set; }

        [ForeignKey(nameof(SiteId))]
        [InverseProperty(nameof(Entities.Site.DepartmentPreferredListItems))]
        public virtual Site Site { get; set; }
    }
}