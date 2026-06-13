using System.Collections.Generic;
using Emar.Core.Medications.Model;
using Emar.Core.Users.Model;

namespace Emar.Core.Carts.Model
{
    public class CartPreCheckoutRequestDataDto
    {
        public OrderingPhysicianDataDto OrderingPhysicianData { get; set; }

        public ICollection<CartOrderDto> DrugInteractionOrders { get; set; }

        public ICollection<OverrideReasonDto> DrugInteractionOverrideReasons { get; set; }

        public ICollection<CartOrderDto> AllergyReactionOrders { get; set; }

        public ICollection<OverrideReasonDto> AllergyReactionOverrideReasons { get; set; }
    }
}