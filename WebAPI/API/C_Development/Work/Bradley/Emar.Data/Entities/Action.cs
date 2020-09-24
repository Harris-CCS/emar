using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Emar.Data.Entities
{
    [Table("actions")]
    public class Action
    {
        public Action()
        {
            // For Foreign Key: fk__order_events__actions
            OrderEvents = new HashSet<OrderEvent>();
        }

        [Key]
        [Column("id")]
        public int Id { get; set; }
        [Required]
        [Column("title")]
        [StringLength(20)]
        public string Title { get; set; }
        [Required]
        [Column("description")]
        [StringLength(100)]
        public string Description { get; set; }
        [Column("site_id")]
        public int SiteId { get; set; }
        [Column("is_active")]
        public bool IsActive { get; set; }

        // For Foreign Key: fk__order_events__actions
        [InverseProperty("Action")]
        public virtual ICollection<OrderEvent> OrderEvents { get; set; }
    }
}
