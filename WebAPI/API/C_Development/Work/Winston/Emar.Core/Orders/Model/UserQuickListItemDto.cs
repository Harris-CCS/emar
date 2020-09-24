using System.Collections.Generic;
using Emar.Core.Helpers;
using Emar.Data.Entities;

namespace Emar.Core.Orders.Model
{
    public class UserQuickListItemDto : OrderBase
    {
        /// <summary>
        /// Unique User identifier
        /// </summary>
        public int UserId { get; set; }

        public int? MedicationId { get; set; }
        public MedicationDto Medication { get; set; }

        /// <summary>
        /// User Quick Lists are site-specific
        /// </summary>
        public int SiteId { get; set; }

        public IEnumerable<HateOasLinkDto> Links;
    }

    public class MedicationDto
    {
        public int Id { get; set; }
        public int SiteId { get; set; }
        public string DisplayName { get; set; }
        public IEnumerable<MedicationDetailDto> MedicationDetails { get; set; }
    }

    public class MedicationDetailDto
    {
        public int Id { get; set; }
        public int MedicationId { get; set; }
        public string Ndc { get; set; }
        public string DrugId { get; set; }
        public string BrandName { get; set; }
        public decimal? Dose { get; set; }
        public int? MedicationUnitId { get; set; }
        public int? MedicationRouteId { get; set; }
    }
}
