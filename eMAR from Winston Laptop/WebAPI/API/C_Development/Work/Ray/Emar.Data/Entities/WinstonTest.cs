using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Emar.Data.Entities
{
    [Table("winston_tests")]
    public class WinstonTest
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("column_one")]
        [StringLength(50)]
        public string ColumnOne { get; set; }

        [Column("column_two")]
        public bool ColumnTwo { get; set; }

        [Column("column_three")]
        [StringLength(25)]
        public string ColumnThree { get; set; }
    }
}
