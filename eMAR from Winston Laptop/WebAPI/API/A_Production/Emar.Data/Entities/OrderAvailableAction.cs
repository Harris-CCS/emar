using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Emar.Data.Entities
{
    [Table("order_available_actions")]
    public class OrderAvailableAction
    {
        [Column("id", TypeName = "int"), Key]
        public int Id { get; set; }

        [Column("site_id", TypeName = "int")]
        public int SiteId { get; set; }

        [Column("order_status", TypeName = "varchar(25)"), Required]
        public string OrderStatus { get; set; }

        [Column("available_action_id", TypeName = "int")]
        public int AvailableActionId { get; set; }

        [Column("is_pit", TypeName = "bit")]
        public bool? IsPit { get; set; }

        [Column("is_prn_only", TypeName = "bit")]
        public bool IsPrnOnly { get; set; }



        // For Foreign Key: fk__order_available_actions__actions
        [ForeignKey(nameof(AvailableActionId))]
        [InverseProperty(nameof(Entities.Action.OrderAvailableActions))]
        public virtual Action Action { get; set; }

        // For Foreign Key: fk__order_available_actions__sites
        [ForeignKey(nameof(SiteId))]
        [InverseProperty(nameof(Entities.Site.OrderAvailableActions))]
        public virtual Site Site { get; set; }
    }
}