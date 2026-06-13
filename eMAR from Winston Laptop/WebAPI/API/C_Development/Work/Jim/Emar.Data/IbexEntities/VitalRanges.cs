using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Emar.Data.IbexEntities
{
    [Table("vital_ranges")]
    public class VitalRanges
    {
        public VitalRanges()
        {

        }

        [Column("id", TypeName = "int"), Key] public int Id { get; set; }
        [Column("typeId", TypeName = "int")] public int TypeId { get; set; }
        [Column("rangeTypeId", TypeName = "int")] public int RangeTypeId { get; set; }
        [Column("ageStart", TypeName = "int")] public int AgeStart { get; set; }
        [Column("ageEnd", TypeName = "int")] public int AgeEnd { get; set; }
        [Column("value", TypeName = "decimal(9,1)")] public decimal Value { get; set; }
        [Column("site", TypeName = "tinyint"), Key] public int Site { get; set; }

    }
}
