using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Emar.Data.IbexEntities
{
    [Table("vital_types")]
    public class VitalTypes
    {
        public VitalTypes()
        {

        }

        [Column("id", TypeName = "int"), Key] public int Id { get; set; }
        [Column("name", TypeName = "varchar(100)")] public string Name { get; set; }

    }
}

