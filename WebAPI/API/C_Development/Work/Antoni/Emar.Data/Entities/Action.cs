using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Emar.Data.Entities
{
    [Table("actions")]
    public class Action
    {
        //////////public Action()
        //////////{
        //////////    OrderEvents = new HashSet<OrderEvent>();
        //////////}

        [Column("id"), Key]
        public int Id { get; set; }

        [Column("title", TypeName = "varchar(20)"), Required]
        public string Title { get; set; }

        [Column("description", TypeName = "varchar(100)"), Required]
        public string Description { get; set; }

        [Column("site_id", TypeName = "int"), Required]
        public int SiteId { get; set; }

        [Column("is_active", TypeName = "bit"), Required]
        public bool IsActive { get; set; }

        //////////[InverseProperty("Action")]
        //////////public virtual ICollection<OrderEvent> OrderEvents { get; set; }
    }
}
