using System.Collections.Generic;

namespace Emar.Core.MedicationReactions
{
    public interface IDrugDbUtility
    {
        List<Dictionary<string, string>> GetAllergies(List<string> classes, Dictionary<string, Dictionary<string, string>> checklistDrugs);
        List<string> GetAllergyClassByCategory(string category);
        List<string> GetAllergyClassByDrug(string drugId);
        List<Dictionary<string, string>> GetAllergyIntolerances(List<string> algDrugs, List<string> checklistDrugs);
        List<Dictionary<string, string>> GetComponentInfo(List<string> components);
        List<Dictionary<string, object>> GetDrugInteractions(List<string> components);
    }
}