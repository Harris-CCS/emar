using System.Collections.Generic;
using System.Data.SqlClient;

namespace Emar.Core.Helpers
{
    public interface IDrugDBUtility
    {
//        List<Dictionary<string, string>> GetAllergies(List<string> classes, Dictionary<string, Dictionary<string, string>> checklistDrugs);
//        List<string> GetAllergyClassByCategory(string category);
//        List<string> GetAllergyClassByDrug(string drugId);
//        List<Dictionary<string, string>> GetAllergyIntolerances(List<string> algDrugs, List<string> checklistDrugs);
        List<Dictionary<string, string>> GetComponentInfo(List<string> components);
//        List<Dictionary<string, string>> GetDrugInfoByFormulationId(string formulationId);
        Dictionary<string, string> GetDrugInfoByNDC(string ndc);
        List<Dictionary<string, string>> GetDrugInfoByNDCs(List<string> ndcs);
//        List<Dictionary<string, string>> GetDrugInteractions(List<string> components);
//        Dictionary<string, string> HasWarningsAndEffects(List<string> ids);
        List<Dictionary<string, string>> GetFormularyMatchInfo(byte siteId, List<string> ndcs, List<string> form_ids, List<string> drug_ids);
//        List<Dictionary<string, string>> GetFilteredQuickListData(byte siteId, string top, string formularyMatchClause, string categoryClause, List<SqlParameter> clauseParameters, string type, string sqlFormulary, int userId);
        bool CheckObsoletes();

//        List<Dictionary<string, string>> GetDrugInfoByBrand(byte siteId, string brand, string type = "M");
        string GetDBType();
        Dictionary<string, string> GetCategoryInfoById(int subcatId);
        string GetRxcuiByDrugId(string drugId);
    }
}

