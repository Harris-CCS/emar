using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Emar.Data.Entities
{
    [Table("preferred_frequency_schedules")]
    public class PreferredFrequencySchedule
    {
        [Column("id", TypeName = "int"), Key]
        public int Id { get; set; }

        [Column("medication_id", TypeName = "int")]
        public int MedicationId { get; set; }

        [Column("frequency_schedule_id", TypeName = "int")]
        public int FrequencyScheduleId { get; set; }

        [Column("site_id", TypeName = "int")]
        public int SiteId { get; set; }

        // For Foreign Key: fk__preferred_frequency_schedules__frequency_schedules
        [ForeignKey(nameof(FrequencyScheduleId))]
        [InverseProperty(nameof(Entities.FrequencySchedule.PreferredFrequencySchedules))]
        public virtual FrequencySchedule FrequencySchedule { get; set; }

        // For Foreign Key: fk__preferred_frequency_schedules__medications
        [ForeignKey(nameof(MedicationId))]
        [InverseProperty(nameof(Entities.Medication.PreferredFrequencySchedules))]
        public virtual Medication Medication { get; set; }

        // For Foreign Key: fk__preferred_frequency_schedules__sites
        [ForeignKey(nameof(SiteId))]
        [InverseProperty(nameof(Entities.Site.PreferredFrequencySchedules))]
        public virtual Site Site { get; set; }
    }
}