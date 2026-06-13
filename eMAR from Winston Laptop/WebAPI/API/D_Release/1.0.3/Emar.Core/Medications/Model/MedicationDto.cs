using System.Collections.Generic;
using Emar.Core.Sites.Model;

namespace Emar.Core.Medications.Model
{
    public class MedicationDto
    {
        public int Id { get; set; }
        internal int SiteId { get; set; }
        public SiteDto Site { get; set; }
        public string DrugId { get; set; }
        public string DisplayName { get; set; }
        public string DrugVendor { get; set; }
        public List<MedicationDetailDto> MedicationDetails { get; set; }
    }
}
