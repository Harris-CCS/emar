using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace Emar.Data.Entities
{
    [Table("global_options")]

    public partial class GlobalOptions
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("name")]
        [StringLength(40)]
        public string Name { get; set; }

        [Column("description", TypeName = "varchar(1000)"), Required]
        public string Description { get; set; }

        [Column("value", TypeName = "varchar(1000)"), Required]
        public string Value { get; set; }
    }
}
