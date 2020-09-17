using System.Collections.Generic;
using Emar.Core.Medications.Model;

namespace Emar.Core.Medications.Repository
{
    public interface IInteractionRepository
    {
        bool RecordNewInteractionsReactions(IEnumerable<MedicationInteractionReaction> interactionsReactions, long OrderId, EmarOrderType orderType);
    }
}
