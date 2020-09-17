using System.Collections.Generic;
using System.Data.SqlClient;

namespace Emar.Core.MedicationReactions
{
    public interface IDrugDBUtility
    {
        List<Dictionary<string, string>> GetAllergies(List<string> classes, Dictionary<string, Dictionary<string, string>> checklistDrugs);
        List<string> GetAllergyClassByCategory(string category);
        List<string> GetAllergyClassByDrug(string drugId);
        List<Dictionary<string, string>> GetAllergyIntolerances(List<string> algDrugs, List<string> checklistDrugs);
        List<Dictionary<string, string>> GetComponentInfo(List<string> components);
        List<Dictionary<string, string>> GetDrugInteractions(List<string> components);
        Dictionary<string, string> HasWarningsAndEffects(List<string> ids);
    }
}