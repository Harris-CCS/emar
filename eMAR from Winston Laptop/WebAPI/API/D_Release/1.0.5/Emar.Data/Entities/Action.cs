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
            // For Foreign Key: fk__action_route_templates__actions
            ActionRouteTemplates = new HashSet<ActionRouteTemplate>();
            // For Foreign Key: fk__order_administration_available_actions__actions
            OrderAdministrationAvailableActions = new HashSet<OrderAdministrationAvailableAction>();
            // For Foreign Key: fk__order_available_actions__actions
            OrderAvailableActions = new HashSet<OrderAvailableAction>();
            // For Foreign Key: fk__order_events__actions
            OrderEvents = new HashSet<OrderEvent>();
        }

        [Column("id"), Key]
        public int Id { get; set; }

        [Column("name", TypeName = "varchar(20)"), Required]
        public string Name { get; set; }
        
        [Required]
        [Column("description")]
        [StringLength(100)]
        public string Description { get; set; }


        // For Foreign Key: fk__action_route_templates__actions
        [InverseProperty("Action")]
        public virtual ICollection<ActionRouteTemplate> ActionRouteTemplates { get; set; }

        // For Foreign Key: fk__order_administration_available_actions__actions
        [InverseProperty("Action")]
        public virtual ICollection<OrderAdministrationAvailableAction> OrderAdministrationAvailableActions { get; set; }

        // For Foreign Key: fk__order_available_actions__actions
        [InverseProperty("Action")]
        public virtual ICollection<OrderAvailableAction> OrderAvailableActions { get; set; }

        // For Foreign Key: fk__order_events__actions
        [InverseProperty("Action")]
        public virtual ICollection<OrderEvent> OrderEvents { get; set; }
    }
}