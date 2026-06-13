using System.Collections.Generic;
using Emar.Core.Sites.Model;

namespace Emar.Core.Medications.Model
{
    public class OverrideReasonDto
    {
        public int Id { get; set; }
        public int SiteId { get; set; }
        public bool IsMedication { get; set; }
        public string Description { get; set; }
        internal virtual SiteDto Site { get; set; }
        internal virtual ICollection<MedicationInteractionDto> MedicationInteractions { get; set; }
    }
}