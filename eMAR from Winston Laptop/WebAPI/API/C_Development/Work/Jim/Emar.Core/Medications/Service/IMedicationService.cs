using System.Collections.Generic;
using Emar.Core.Medications.Model;
using Emar.Core.Orders.Model;
using Emar.Data.Entities;

namespace Emar.Core.Medications.Service
{
    public interface IMedicationService
    {
        IEnumerable<BrandNameSearchDto> GetMedsByBrandName(int siteId, string search, int userId, EmarOrderType searchType,
            string deptCode, string schedulerDataRetrieveBase, string? groupLink);
        IEnumerable<AntimicrobialIndicationDto> GetIndicationsBySite(int siteId);
        Dictionary<string, bool> GetSearchDropdownList(int siteId);
        MedicationDto GetMedication(int medicationId);
        IEnumerable<MedicationInteractionReaction> GetInteractionsReactions(int userId, long patientId, EmarOrderType itemType, int itemId);

        List<MedicationInteractionReaction> GetGroupOrderInteractionsReactions(int userId, long patientId, int itemId);
        List<MedicationInteractionReaction> CompressComboMedInteractionsToOneEntry(GroupListItem groupItem, int userId, List<MedicationInteractionReaction> currentList);

        List<MedicationInteractionReaction> GetPatientCartOrderInteractionsReactions(int userId, long patientId, int itemId);

        List<MedicationInteractionReaction> CompressPatientCartOrderComboMedInteractionsToOneEntry(PatientCartOrder cartOrder, int userId, List<MedicationInteractionReaction> currentList);

        List<MedicationInteractionReaction> GetPatientOrderInteractionsReactions(int userId, long patientId, int itemId);

        List<MedicationInteractionReaction> CompressPatientOrderComboMedInteractionsToOneEntry(PatientOrder order, int userId, List<MedicationInteractionReaction> currentList);

    }
}