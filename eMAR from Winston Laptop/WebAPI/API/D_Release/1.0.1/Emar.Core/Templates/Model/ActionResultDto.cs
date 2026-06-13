using Emar.Core.Orders.Model;

namespace Emar.Core.Templates.Model
{
    public class ActionResultDto
    {
        public OrderEventDto NewEvent { get; set; }
        public PatientOrderDto UpdatedOrder { get; set; }
        public TemplateDto Template { get; set; }
    }
}
