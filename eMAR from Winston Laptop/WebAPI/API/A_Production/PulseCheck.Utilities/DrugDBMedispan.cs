using System.Collections.Generic;

namespace PulseCheck.Utilities
{
    /// <summary>
    /// Library to handle interaction with the Medispan drug database
    /// </summary>
    // TODO: fix this so it uses IDrugDBUtility
    public class DrugDBMedispan// : IDrugDBUtility 
    {
        public string Vendor = DrugDB.Constants.Vendors.MEDISPAN;
        public string Name = "Medispan";
        public string DBName = "medispan";
        public string DBType = "2";

        /// <summary>
        /// Get the name of the Drug database vendor
        /// </summary>
        /// <returns>Medispan (name of the vendor)</returns>
        public string GetDBType()
        {
            return DBType;
        }

        /// <summary>
        /// Whether this vendor's drug information should have obsoletes checked
        /// </summary>
        /// <returns></returns>
        public bool CheckObsoletes()
        {
            return false;
        }

        /// <summary>
        /// Get the code and description for a drug by NDC
        /// </summary>
        /// <param name="ndc">Drug NDC</param>
        /// <returns>Dictionary of information in the basic denorm table mapping</returns>
        public Dictionary<string, string> GetDrugInfoByNDC(string ndc)
        {
            var info = new Dictionary<string, string>();

            return info;
        }

        public List<Dictionary<string, string>> GetDrugInfoByBrand(byte siteId, string brand, string type = "M")
        {
            var info = new List<Dictionary<string, string>>();

            return info;
        }


        /// <summary>
        /// Get the codes and descriptions for drugs by NDC
        /// </summary>
        /// <param name="ndcs">List of drug NDCs</param>
        /// <returns>List of Dictionaries of information in the basic denorm table mapping</returns>
        public List<Dictionary<string, string>> GetDrugInfoByNDCs(List<string> ndcs)
        {
            var info = new List<Dictionary<string, string>>();

            return info;
        }
    }
}