using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Emar.Data.Entities
{
    [Table("frequency_types")]
    public class FrequencyType
    {
        public FrequencyType()
        {
            // For Foreign Key: fk__frequency_schedules__frequency_types
            FrequencySchedules = new HashSet<FrequencySchedule>();
        }

        [Column("id", TypeName = "int"), Key]
        public int Id { get; set; }

        [Column("name", TypeName = "nvarchar(128)")]
        public string Name { get; set; }

        // For Foreign Key: fk__frequency_schedules__frequency_types
        [InverseProperty("FrequencyType")]
        public virtual ICollection<FrequencySchedule> FrequencySchedules { get; set; }
    }
}