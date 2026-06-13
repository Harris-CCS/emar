using Emar.Core.OutboundChart.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace Emar.Core.Helpers
{
    /// <summary>
    /// Library to handle interaction with drug databases
    /// </summary>
    public class DrugDB
    {
        private IDrugDBUtility instance;

        /// <summary>
        /// Site instance for this Drug DB
        /// </summary>
        public ISite Site { get; set; }

        /// <summary>
        /// Drug DB Vendor identifier
        /// </summary>
        public string Vendor { get; private set; }

        /// <summary>
        /// Drug DB type
        /// </summary>
        public string DBType { get; private set; }

        /// <summary>
        /// Stores information about drug classifications
        /// </summary>
        private Dictionary<string, List<Dictionary<string, string>>> classInfo = new Dictionary<string, List<Dictionary<string, string>>>();

        /// <summary>
        /// Stores classifications per drug
        /// </summary>
        private Dictionary<string, List<string>> DrugClass = new Dictionary<string, List<string>>();

        /// <summary>
        /// Stores classifications per category
        /// </summary>
        private Dictionary<string, List<string>> CatClass = new Dictionary<string, List<string>>();

        /// <summary>
        /// Stores component info
        /// </summary>
        private Dictionary<string, List<Dictionary<string, string>>> ComponentInfo = new Dictionary<string, List<Dictionary<string, string>>>();

        /// <summary>
        /// Stores info about Medication Services
        /// </summary>
        private List<Dictionary<string, string>> MedSvcInfo = new List<Dictionary<string, string>>();

        /// <summary>
        /// DrugDB constructor with site and SqlConnection
        /// </summary>
        /// <param name="site">ISite instance</param>
        /// <param name="con">Optional SqlConnection</param>
        public DrugDB(ISite site, SqlConnection con = null)
        {
            Site = site;
            var vendor = Site.GetOrgOption("DRUG_DB_VENDOR", con);
            if (vendor.Equals("M"))
            {
//                instance = new DrugDBMultum();
            }
            else if (vendor.Equals("F"))
            {
                instance = new DrugDBFDB();
            }
            else if (vendor.Equals("1"))
            {
//                instance = new DrugDBFDBCa();
            }
            else if (vendor.Equals("2"))
            {
                //instance = new DrugDBMedispan();
            }
            else
            {
                throw new NotSupportedException("Unknown drug database selector (" + vendor + ")");
            }

            Vendor = vendor;
        }

        /// <summary>
        /// Get the underlying DrugDB instance
        /// </summary>
        /// <returns>IDrugDBUtility</returns>
        public IDrugDBUtility GetInstance()
        {
            return instance;
        }

//        public ReactionsCheckResult CheckReactions(byte siteId, string ibex, Dictionary<string, string> checklist, List<IMedication> patientMeds = null)

//        private void DrugDChecklist(ref Dictionary<string, string> rAlgReact, string cls, string cat, string drugId, ref Dictionary<string, string> checklist)

//        public List<Dictionary<string, string>> LoadAlgMedTable(byte siteId, string ibex, bool confirmedOnly = false)

//        public List<Dictionary<string, string>> LoadQuickListData(byte siteId, int userId, string type, string cat, int limit)

//        public Dictionary<string, string> LoadQuickListEntry(string ibex, int userId, int num)

        // TODO: Implement this
        public void SetCurrentMeds(byte siteId, string ibex, string target)
        {
            MedSvcInfo.Clear();

        }

//        public class ReactionsCheckResult

        /// <summary>
        /// Constants used in drug databases
        /// </summary>
        public class Constants
        {
            /// <summary>
            /// List of different drug database vendors
            /// </summary>
            public class Vendors
            {
                public const string FDB = "F";
                public const string FDB_CANADIAN = "1";
                public const string MEDISPAN = "2";
                public const string MULTUM = "M";
            }

            /// <summary>
            /// Drug vendor mapping from vendor char to vendor label
            /// </summary>
            public static readonly Dictionary<string, string> VENDOR_MAP = new Dictionary<string, string>
            {
                { "F", "FDB" },
                { "1", "FDB" },
                { "2", "MEDISPAN" },
                { "M", "MULTUM" }
            };
        }
    }
}

