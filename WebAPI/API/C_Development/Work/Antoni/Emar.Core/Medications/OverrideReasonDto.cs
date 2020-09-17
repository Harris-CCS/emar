using System.Collections.Generic;
using Emar.Core.Medications.Model;
using Emar.Core.Sites.Model;

namespace Emar.Core.Medications
{
    public class OverrideReasonDto
    {
        public int Id { get; set; }
        public int SiteId { get; set; }
        public bool IsMedication { get; set; }
        public string Description { get; set; }

        public virtual SiteDto Site { get; set; }
        public virtual ICollection<MedicationInteractionDto> MedicationInteractions { get; set; }
    }
}
