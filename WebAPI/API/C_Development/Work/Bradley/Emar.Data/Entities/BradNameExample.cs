using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Emar.Data.Entities
{
    [Table("BradNameExample")]

    public class BradNameExample
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }
    }
}
