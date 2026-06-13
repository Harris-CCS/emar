using System.Collections.Generic;

namespace Emar.Core.OutboundChart.Model
{
    public interface IComponent
    {
        string ActiveId { get; set; }
        string ActiveName { get; set; }
        string BrandName { get; set; }
        string DrugCategoryId { get; set; }
        string DrugForm { get; set; }
        string DrugFormId { get; set; }
        string DrugId { get; set; }
        string DrugRoute { get; set; }
        string DrugStrength { get; set; }
        string EnteredDose { get; set; }
        string EnteredUnit { get; set; }
        string Ibex { get; set; }
        int Id { get; set; }
        int Losecs { get; set; }
        string PackagingId { get; set; }
        short Site { get; set; }
        string Type { get; set; }

        List<Dictionary<string, string>> Interactions { get; set; }
        List<Dictionary<string, string>> Reactions { get; set; }

        string GetBrandName();
        string GetName();

    }
}