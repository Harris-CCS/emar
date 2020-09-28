using System.Collections.Generic;
using Emar.Core.Medications.Model;
using Emar.Data.Entities;

namespace Emar.Core.Medications.Repository
{
    public interface IInteractionRepository
    {
        void GetInteractionsReactions(IEnumerable<MedicationInteractionReaction> medicationInteractionsReactions, long orderId, EmarOrderType orderType, ref List<List<MedicationInteraction>> medicationInteractionsList, ref List<List<OrderReaction>> orderReactionsList);
        bool RecordNewInteractionsReactions(IEnumerable<MedicationInteractionReaction> interactionsReactions, long orderId, EmarOrderType orderType);
    }
}