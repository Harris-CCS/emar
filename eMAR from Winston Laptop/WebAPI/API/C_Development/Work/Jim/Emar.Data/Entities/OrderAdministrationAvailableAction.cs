using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Emar.Data.Entities
{
    [Table("order_administration_available_actions")]
    public class OrderAdministrationAvailableAction
    {
        [Column("id", TypeName = "int"), Key]
        public int Id { get; set; }

        [Column("site_id", TypeName = "int")]
        public int SiteId { get; set; }

        [Column("order_status", TypeName = "varchar(25)"), Required]
        public string OrderStatus { get; set; }

        [Column("administration_status", TypeName = "varchar(20)"), Required]
        public string AdministrationStatus { get; set; }

        [Column("point_in_time", TypeName = "bit")]
        public bool? PointInTime { get; set; }

        [Column("available_action_id", TypeName = "int")]
        public int AvailableActionId { get; set; }


        // For Foreign Key: fk__order_administration_available_actions__actions
        [ForeignKey(nameof(AvailableActionId))]
        [InverseProperty(nameof(Entities.Action.OrderAdministrationAvailableActions))]
        public virtual Action Action { get; set; }

        // For Foreign Key: fk__order_administration_available_actions__sites
        [ForeignKey(nameof(SiteId))]
        [InverseProperty(nameof(Entities.Site.OrderAdministrationAvailableActions))]
        public virtual Site Site { get; set; }
    }
}