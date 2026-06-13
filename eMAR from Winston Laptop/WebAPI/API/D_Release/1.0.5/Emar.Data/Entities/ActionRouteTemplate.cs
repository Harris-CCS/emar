using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Emar.Data.Entities
{
    [Table("action_route_templates")]
    public class ActionRouteTemplate
    {
        [Column("id", TypeName = "int"), Key]
        public int Id { get; set; }

        [Column("action_id", TypeName = "int")]
        public int ActionId { get; set; }

        [Column("medication_route_id", TypeName = "int")]
        public int? MedicationRouteId { get; set; }

        [Column("template_id", TypeName = "int")]
        public int TemplateId { get; set; }

        [Column("site_id", TypeName = "int")]
        public int? SiteId { get; set; }

        // For Foreign Key: fk__action_route_templates__actions
        [ForeignKey(nameof(ActionId))]
        [InverseProperty(nameof(Entities.Action.ActionRouteTemplates))]
        public virtual Action Action { get; set; }

        // For Foreign Key: fk__action_route_templates__medication_routes
        [ForeignKey(nameof(MedicationRouteId))]
        [InverseProperty(nameof(Entities.MedicationRoute.ActionRouteTemplates))]
        public virtual MedicationRoute MedicationRoute { get; set; }

        // For Foreign Key: fk__action_route_templates__sites
        [ForeignKey(nameof(SiteId))]
        [InverseProperty(nameof(Entities.Site.ActionRouteTemplates))]
        public virtual Site Site { get; set; }

        // For Foreign Key: fk__action_route_templates__templates
        [ForeignKey(nameof(TemplateId))]
        [InverseProperty(nameof(Entities.Template.ActionRouteTemplates))]
        public virtual Template Template { get; set; }
    }
}