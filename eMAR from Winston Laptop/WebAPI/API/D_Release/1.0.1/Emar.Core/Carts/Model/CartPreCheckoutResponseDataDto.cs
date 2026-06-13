using System.Collections.Generic;

namespace Emar.Core.Carts.Model
{
    public class CartPreCheckoutResponseDataDto
    {
        public string OrderingPhysicianUserId { get; set; }
        public ICollection<DrugInteractionOverrideRationaliaDto> DrugInteractionOverrideRationalia { get; set; }
        public ICollection<AllergyReactionOverrideRationaliaDto> AllergyReactionOverrideRationalia { get; set; }
    }

    public class DrugInteractionOverrideRationaliaDto
    {
        public string MedicationInteractionId { get; set; }
        public string OverrideReasonId { get; set; }
    }

    public class AllergyReactionOverrideRationaliaDto
    {
        public string OrderReactionId { get; set; }
        public string OverrideReasonId { get; set; }
    }
}