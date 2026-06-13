using Emar.Core.OutboundChart.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text.RegularExpressions;

namespace Emar.Core.Helpers
{
    /// <summary>
    /// Library to handle interaction with the Formulary
    /// </summary>
    public class Formulary
    {
        /// <summary>
        /// Drug database instance
        /// </summary>
        private DrugDB DrugDB { get; set; }

        /// <summary>
        /// Site instance
        /// </summary>
        private ISite Site { get; set; }

        /// <summary>
        /// User instance
        /// </summary>
        private IUser User { get; set; }

        /// <summary>
        /// Stores fatal errors encountered during execution
        /// </summary>
        private List<string> FatalErrors = new List<string>();

        /// <summary>
        /// Stores non-fatal errors encountered during execution
        /// </summary>
        private List<string> NonFatalErrors = new List<string>();

        /// <summary>
        /// Stores NDC information from the drug database
        /// </summary>
        private Dictionary<string, Dictionary<string, string>> NDC = new Dictionary<string, Dictionary<string, string>>();

        /// <summary>
        /// Boolean flag indicating whether the formulary is using exact matching
        /// </summary>
        public bool ExactMatch { get; private set; }

        /// <summary>
        /// Count of formulary entries for the site/instance
        /// </summary>
        public int HasFormu { get; private set; }

        /// <summary>
        /// Inpatient formulary entries
        /// </summary>
        public Dictionary<string, Dictionary<string, List<Dictionary<string, string>>>> Inpat { get; set; }

        /// <summary>
        /// Outpatient formulary entries
        /// </summary>
        public Dictionary<string, Dictionary<string, List<Dictionary<string, string>>>> Outpat { get; set; }

        /// <summary>
        /// Pyxis formulary entries
        /// </summary>
        public Dictionary<string, Dictionary<string, List<Dictionary<string, string>>>> Pyxis { get; set; }

        /// <summary>
        /// Stores org information/site settings
        /// </summary>
        private static Dictionary<byte, Dictionary<string, string>> Org = new Dictionary<byte, Dictionary<string, string>>();

        /// <summary>
        /// Stores site prefernces for formulary flags
        /// </summary>
        private static Dictionary<byte, Dictionary<string, string>> SitePrefs = new Dictionary<byte, Dictionary<string, string>>();

        /// <summary>
        /// Formulary constructor
        /// </summary>
        public Formulary(ISite site, IUser user, List<string> ndcList, string matchbase = "med")
        {
            Site = site;
            User = user;
            DrugDB = new DrugDB(site);

            HasFormu = 0;
            ExactMatch = false;

            Inpat = new Dictionary<string, Dictionary<string, List<Dictionary<string, string>>>>();
            Outpat = new Dictionary<string, Dictionary<string, List<Dictionary<string, string>>>>();
            Pyxis = new Dictionary<string, Dictionary<string, List<Dictionary<string, string>>>>();

            LoadOrg(site);
            if (matchbase.ToLower().Equals("med") || matchbase.ToLower().Equals("rx"))
            {
                ExactMatch = Org[site.Id][matchbase + "exactmatch"].Equals("Y");
            }

            HasFormu = new DB.Select
            {
                Sql = "SELECT COUNT(1) AS cnt FROM frm WHERE site=@site",
                Parameters = new SqlParameter[]
                {
                    new SqlParameter("@site", SqlDbType.TinyInt) { Value = site.Id }
                }
            }.RunForInt();

            if (HasFormu > 0)
            {
                LoadFormulary(ndcList);
            }
        }

        /// <summary>
        /// Add a fatal error to the list of fatal error messages
        /// </summary>
        /// <param name="msg"></param>
        private void AddFatalError(string msg)
        {
            FatalErrors.Add(msg);
        }

        /// <summary>
        /// Add a non-fatal error to the list of non-fatal error messages
        /// </summary>
        /// <param name="msg"></param>
        private void AddNonFatalError(string msg)
        {
            NonFatalErrors.Add(msg);
        }

        /// <summary>
        /// Retrieve the formulary match flags 
        /// </summary>
        /// <param name="ndc">A single ndc code</param>
        /// <param name="con">Open SqlConnection</param>
        /// <returns>A dictionary identifying formulary type and match level</returns>
        public Dictionary<string, int> GetFlags(string ndc, SqlConnection con = null)
        {
            var flags = new Dictionary<string, int>
            {
                { Constants.INPAT_TYPE, Constants.NON_MATCH },
                { Constants.OUTPAT_TYPE, Constants.NON_MATCH },
                { Constants.PYXIS_TYPE, Constants.NON_MATCH }
            };

            if (string.IsNullOrWhiteSpace(ndc) || ndc.Equals(Constants.FREE_TEXT) || HasFormu == 0)
            {
                return flags;
            }

            var columns = string.Join(",", Constants.FLAG_TYPE);

            var info = new DB.Select
            {
                Connection = con,
                Sql = "SELECT " + columns + " FROM formulary_match WHERE site=@site AND ndc=@ndc",
                Parameters = new SqlParameter[]
                {
                    new SqlParameter("@site", SqlDbType.Int) { Value = Site.Id },
                    new SqlParameter("@ndc", SqlDbType.Char) { Value = ndc }
                }
            }.RunForDataRow();

            if (info == null)
                return flags;

            if (Regex.IsMatch(info[Constants.INPAT_TYPE].ToString(), @"^\d+$"))
            {
                foreach (var formu in Constants.FLAG_TYPE)
                {
                    var matchLevel = Convert.ToInt32(info[formu]);
                    if (ExactMatch && matchLevel < Constants.EXACT_MATCH)
                    {
                        info[formu] = Constants.NON_MATCH.ToString();
                        matchLevel = Constants.NON_MATCH;
                    }
                    flags[formu] = matchLevel;
                }
                return flags;
            }

            LoadFormulary(ndc);

            var drug = NDC[ndc];
            foreach (var formu in Constants.FLAG_TYPE)
            {
                int flag = Constants.NON_MATCH;

                var useFormu = (formu.Equals(Constants.INPAT_TYPE)) ? Inpat :
                               (formu.Equals(Constants.OUTPAT_TYPE)) ? Outpat :
                               (formu.Equals(Constants.PYXIS_TYPE)) ? Pyxis :
                               null;

                if (useFormu != null)
                {
                    if (useFormu["ndc"].ContainsKey(ndc))
                    {
                        flag = Constants.EXACT_MATCH;
                    }
                    else if (drug.ContainsKey("multum") && useFormu["multum"].ContainsKey(drug["multum"]))
                    {
                        var multum = drug["multum"];
                        flag = ExactMatch ? Constants.NON_MATCH : Constants.EQUIV_MATCH;
                        var basendc = NDC[ndc].ContainsKey("base_ndc") ? NDC[ndc]["base_ndc"] : null;
                        if (basendc != null)
                        {
                            foreach (var mdrug in useFormu["multum"][multum])
                            {
                                if (mdrug.ContainsKey("base_ndc") && basendc.Equals(mdrug["base_ndc"]))
                                {
                                    flag = Constants.EXACT_MATCH;
                                    break;
                                }
                            }
                        }
                    }
                    else if (drug.ContainsKey("dnum") && useFormu["dnum"].ContainsKey(drug["dnum"]))
                    {
                        flag = ExactMatch ? Constants.NON_MATCH : Constants.PARTIAL_MATCH;
                    }
                }

                flags[formu] = flag;
            }

            var result = new DB.Insert
            {
                Connection = con,
                Sql = "INSERT INTO formulary_match(site,ndc,inpat,outpat,pyxis) VALUES (@site,@ndc,@inpat,@outpat,@pyxis)",
                Parameters = new SqlParameter[]
                {
                    new SqlParameter("@site", SqlDbType.Int) { Value = Site.Id },
                    new SqlParameter("@ndc", SqlDbType.Char) { Value = ndc },
                    new SqlParameter("@inpat", SqlDbType.Char) { Value = flags[Constants.INPAT_TYPE] },
                    new SqlParameter("@outpat", SqlDbType.Char) { Value = flags[Constants.OUTPAT_TYPE] },
                    new SqlParameter("@pyxis", SqlDbType.Char) { Value = flags[Constants.PYXIS_TYPE] }
                }
            }.Run();

            return flags;
        }

        /// <summary>
        /// Retrieve a hosp_id for the supplied NDC
        /// </summary>
        /// <param name="ndc">A single NDC</param>
        /// <returns>The hosp_id string</returns>
        public string GetHospIdByNDC(string ndc)
        {
            var hospId = new DB.Select
            {
                Sql = "SELECT aliencode FROM frm WHERE site=@site AND ndc=@ndc",
                Parameters = new SqlParameter[]
                {
                    new SqlParameter("@site", SqlDbType.TinyInt) { Value = Site.Id },
                    new SqlParameter("@ndc", SqlDbType.VarChar) { Value = ndc }
                }
            }.RunForScalar();

            return hospId == null ? "" : hospId.ToString();
        }

        /// <summary>
        /// Load formulary information
        /// </summary>
        /// <param name="ndc">A single drug identifier</param>
        /// <returns>Boolean flag for whether this instance has fatal errors</returns>
        private bool LoadFormulary(string ndc)
        {
            return LoadFormulary(new List<string> { ndc });
        }

        /// <summary>
        /// Load formulary information
        /// </summary>
        /// <param name="ndcList">List of drug identifiers</param>
        /// <returns>Boolean flag for whether this instance has fatal errors</returns>
        private bool LoadFormulary(List<string> ndcList)
        {
            if (HasFormu == 0)
            {
                return false;
            }

            if (ndcList == null || ndcList.Count == 0)
            {
                return false;
            }

            var lookupNDCs = ndcList.Where(n => !NDC.Keys.Any(n2 => !n2.Equals(n))).Distinct().ToList();
            if (lookupNDCs.Count == 0)
            {
                return false;
            }

            var ndc_part = new Dictionary<string, Dictionary<string, string>>();
            var multum_part = new Dictionary<string, int>();
            var dnum_part = new Dictionary<string, int>();

            foreach (var ndc in lookupNDCs)
            {
                ndc_part.Add(ndc, new Dictionary<string, string>());
                NDC.Add(ndc, new Dictionary<string, string>());
            }
            foreach (var info in DrugDB.GetInstance().GetDrugInfoByNDCs(lookupNDCs))
            {
                if (!info.ContainsKey("ndc") || String.IsNullOrWhiteSpace(info["ndc"]))
                {
                    continue;
                }
                info["dnum"] = info["drug"];

                // There is only 1 drug associated with any 'ndc'
                NDC[info["ndc"]] = info;

                if (info.ContainsKey("multum") && !String.IsNullOrWhiteSpace(info["multum"]) && !multum_part.ContainsKey(info["multum"]))
                {
                    multum_part.Add(info["multum"], 1);
                }

                if (info.ContainsKey("dnum") && !String.IsNullOrWhiteSpace(info["dnum"]) && !dnum_part.ContainsKey(info["dnum"]))
                {
                    dnum_part.Add(info["dnum"], 1);
                }

                // This will reduce SQL overhead when getting skip info and brand names
                ndc_part[info["ndc"]] = info;
            }

            var loaded_ndcs = new Dictionary<string, Dictionary<string, string>>();
            // Gather the formulary information for the specified drugs.  There will
            // always be at least 1 ndc.  A bad formulary could contain no multum or dnum
            // values which can cause SQL errors on IN() clauses, so the IN() clauses are
            // only created if there are values to match on.
            foreach (var drug in DrugDB.GetInstance().GetFormularyMatchInfo(Site.Id, ndc_part.Keys.ToList(), multum_part.Keys.ToList(), dnum_part.Keys.ToList()))
            {
                var ndc = drug["ndc"];
                loaded_ndcs[ndc] = drug;
                NDC[ndc] = drug;

                if (ndc_part.ContainsKey(ndc))
                {
                    if (drug.ContainsKey("multum") && !String.IsNullOrWhiteSpace(drug["multum"]))
                    {
                        if (ndc_part[ndc].ContainsKey("multum") && !String.IsNullOrWhiteSpace(ndc_part[ndc]["multum"]) && !ndc_part[ndc]["multum"].Equals(drug["multum"]))
                        {
                            AddNonFatalError(ndc + ": formulation id mismatch " + ndc_part[ndc]["multum"] + " vs " + drug["multum"] + " in Formulary.");
                        }
                    }
                    if (drug.ContainsKey("dnum") && !String.IsNullOrWhiteSpace(drug["dnum"]))
                    {
                        if (ndc_part[ndc].ContainsKey("dnum") && !String.IsNullOrWhiteSpace(ndc_part[ndc]["dnum"]) && !ndc_part[ndc]["dnum"].Equals(drug["dnum"]))
                        {
                            AddNonFatalError(ndc + ": drug id mismatch " + ndc_part[ndc]["dnum"] + " vs " + drug["dnum"] + " in Formulary.");
                        }
                    }
                }

                var inpat = (drug.ContainsKey(Constants.INPAT_TYPE) && drug[Constants.INPAT_TYPE].Equals("Y"));
                var outpat = (drug.ContainsKey(Constants.OUTPAT_TYPE) && drug[Constants.OUTPAT_TYPE].Equals("Y"));
                var pyxis = (drug.ContainsKey(Constants.PYXIS_TYPE) && drug[Constants.PYXIS_TYPE].Equals("Y"));
                var multum = (drug.ContainsKey("multum") && !String.IsNullOrWhiteSpace(drug["multum"]));
                var dnum = (drug.ContainsKey("dnum") && !String.IsNullOrWhiteSpace(drug["dnum"]));

                if (inpat)
                {
                    Inpat["ndc"][ndc].Add(drug);
                    if (multum)
                    {
                        Inpat["multum"][drug["multum"]].Add(drug);
                    }
                    if (dnum)
                    {
                        Inpat["dnum"][drug["dnum"]].Add(drug);
                    }
                }

                if (outpat)
                {
                    Outpat["ndc"][ndc].Add(drug);
                    if (multum)
                    {
                        Outpat["multum"][drug["multum"]].Add(drug);
                    }
                    if (dnum)
                    {
                        Outpat["dnum"][drug["dnum"]].Add(drug);
                    }
                }

                if (pyxis)
                {
                    Pyxis["ndc"][ndc].Add(drug);
                    if (multum)
                    {
                        Pyxis["multum"][drug["multum"]].Add(drug);
                    }
                    if (dnum)
                    {
                        Pyxis["dnum"][drug["dnum"]].Add(drug);
                    }
                }
            }

            foreach (var ndc in loaded_ndcs.Keys)
            {
                if (!NDC.ContainsKey(ndc) || !NDC[ndc].ContainsKey("brand") || String.IsNullOrWhiteSpace(NDC[ndc]["brand"]))
                {
                    UpdateFormularyDrugInfo(ndc);
                }
            }

            return !HasFatalErrors();
        }

        /// <summary>
        /// Load org table information needed by this object
        /// </summary>
        /// <param name="site">Site instance</param>
        private void LoadOrg(ISite site)
        {
            if (!Org.ContainsKey(site.Id))
            {
                Org[site.Id] = DB.ConvertDataSetToListOfDictionaries(new DB.Select
                {
                    Sql = "SELECT site,medexactmatch,rxexactmatch,medinpat,medoutpat,medpyxis,rxinpat,rxoutpat,rxpyxis FROM org WHERE site=@site",
                    Parameters = new SqlParameter[]
                    {
                        new SqlParameter("@site", SqlDbType.TinyInt) { Value = site.Id }
                    }
                }.RunForDataSet())[0];
            }
        }

        /// <summary>
        /// Check whether this object has encountered and stored fatal errors
        /// </summary>
        /// <returns>Boolean flag for whether this object has stored fatal errors</returns>
        public bool HasFatalErrors()
        {
            return FatalErrors.Count > 0;
        }

        /// <summary>
        /// Check whether this object has encountered and stored non-fatal errors
        /// </summary>
        /// <returns>Boolean flag for whether this object has stored non-fatal errors</returns>
        public bool HasNonFatalErrors()
        {
            return NonFatalErrors.Count > 0;
        }

        /// <summary>
        /// Determine whether the ndc is in any formulary based on which is in the 
        /// match formularies based on site settings.
        /// </summary>
        /// <param name="ndc">A single ndc code</param>
        /// <param name="type">Formulary type to check (med or rx)</param>
        /// <param name="site">Site instance</param>
        /// <param name="con">Open SqlConnection</param>
        /// <returns>False - no matching drugs found in the formularies where a selection must exist. True - If no restrictions are set or the medication is flagged as being available.</returns>
        public bool IsInFormulary(string ndc, string type, ISite site, SqlConnection con)
        {
            type = type.ToLower();
            LoadOrg(site);
            var flags = GetFlags(ndc, con);

            // If the match level is set to exact match, 3 or 4 is expected in the flags.
            // Otherwise, this drug is not in the formulary.
            if (ExactMatch)
            {
                var match_level = Constants.EXACT_MATCH;
                var minMatches = 0;
                foreach (var formu in flags.Keys)
                {
                    if (flags[formu] >= match_level)
                    {
                        minMatches++;
                    }
                }
                if (minMatches == 0)
                {
                    return false;
                }
            }

            var chkd = false;
            foreach (var formu in Constants.FLAG_TYPE)
            {
                if (Org[site.Id][type + formu].Equals("Y"))
                {
                    if (flags[formu] > 0)
                    {
                        return true;
                    }
                    chkd = true;
                }
            }

            return !chkd;
        }

        /// <summary>
        /// Determine whether the NDC is in the inpatient formulary
        /// </summary>
        /// <param name="ndc">A single NDC code</param>
        /// <returns>Boolean for whether the NDC is in the inpatient formulary</returns>
        public bool IsInpatient(string ndc)
        {
            return FormularyCheck(ndc, Constants.INPAT_TYPE);
        }

        /// <summary>
        /// Determine whether the NDC is in the machine formulary
        /// </summary>
        /// <param name="ndc">A single NDC code</param>
        /// <returns>Boolean for whether the NDC is in the machine formulary</returns>
        public bool IsMachine(string ndc)
        {
            return FormularyCheck(ndc, Constants.PYXIS_TYPE);
        }

        /// <summary>
        /// Determine whether the NDC is in the outpatient formulary
        /// </summary>
        /// <param name="ndc">A single NDC code</param>
        /// <returns>Boolean for whether the NDC is in the outpatient formulary</returns>
        public bool IsOutpatient(string ndc)
        {
            return FormularyCheck(ndc, Constants.OUTPAT_TYPE);
        }

        /// <summary>
        /// Update the multum, drug, brand, active, and drugcat info for a drug to the current values in the multum database.
        /// </summary>
        /// <param name="ndc">A single ndc code</param>
        /// <returns>False: Failure. Necessary input is missing or a database error was encountered. Check fatal errors. True: success</returns>
        public bool UpdateFormularyDrugInfo(string ndc)
        {
            if (String.IsNullOrWhiteSpace(ndc))
            {
                return false;
            }

            var flds = DrugDB.GetInstance().GetDrugInfoByNDC(ndc);
            flds["dnum"] = flds["drug"];
            foreach (var zDefault in new string[] { "multum", "drugcat" })
            {
                if (!flds.ContainsKey(zDefault) || String.IsNullOrWhiteSpace(flds[zDefault]))
                {
                    flds[zDefault] = "0";
                }
            }
            foreach (var eDefault in new string[] { "drug", "brand", "active" })
            {
                if (!flds.ContainsKey(eDefault) || String.IsNullOrWhiteSpace(flds[eDefault]))
                {
                    flds[eDefault] = "";
                }
            }

            var changed = false;
            foreach (var key in flds.Keys)
            {
                var cacheKey = key.Equals("drug") ? "dnum" : key;
                if (!NDC.ContainsKey(ndc))
                {
                    changed = true;
                    NDC.Add(ndc, new Dictionary<string, string>());
                }
                if (!NDC[ndc].ContainsKey(cacheKey))
                {
                    changed = true;
                    NDC[ndc].Add(cacheKey, flds[key]);
                }
                else if (!NDC[ndc][cacheKey].Equals(flds[key]))
                {
                    changed = true;
                    NDC[ndc][cacheKey] = flds[key];
                }
            }

            // TODO: We don't care about site here?
            return changed ||
                new DB.Update
                {
                    Sql = "UPDATE frm SET multum=@multum, dnum=@dnum, brand=@brand, active=@active, drugcat=@drugcat WHERE ndc=@ndc",
                    Parameters = new SqlParameter[]
                    {
                        new SqlParameter("@multum", SqlDbType.VarChar) { Value = flds["multum"] },
                        new SqlParameter("@dnum", SqlDbType.Char) { Value = flds["dnum"] },
                        new SqlParameter("@brand", SqlDbType.VarChar) { Value = flds["brand"] },
                        new SqlParameter("@active", SqlDbType.VarChar) { Value = flds["active"] },
                        new SqlParameter("@drugcat", SqlDbType.VarChar) { Value = flds["drugcat"] },
                        new SqlParameter("@ndc", SqlDbType.VarChar) { Value = flds["ndc"] }
                    }
                }.Run() > 0;
        }

        /// <summary>
        /// Check for a particular NDC code's presence in a type of formulary
        /// </summary>
        /// <param name="ndc">NDC code</param>
        /// <param name="formularyType">Formulary type</param>
        /// <returns>Boolean for whether the NDC is in the formulary</returns>
        private bool FormularyCheck(string ndc, string formularyType)
        {
            var flags = GetFlags(ndc);
            var frm = (formularyType == Constants.PYXIS_TYPE) ? "machine_form" :
                      (formularyType == Constants.INPAT_TYPE) ? "in_form" :
                      (formularyType == Constants.OUTPAT_TYPE) ? "out_form" :
                      "";

            if (!SitePrefs.ContainsKey(Site.Id))
            {
                SitePrefs[Site.Id] = new Dictionary<string, string>
                {
                    { "machine_form", "N" },
                    { "in_form", "N" },
                    { "out_form", "N" }
                };

                var res = new DB.Select
                {
                    Sql = "SELECT field_name, field_val FROM site_preferences WHERE field_num = 1 AND field_name IN ('machine_form', 'in_form', 'out_form') AND site=@site",
                    Parameters = new SqlParameter[]
                    {
                        new SqlParameter("@site", SqlDbType.TinyInt) { Value = Site.Id }
                    }
                }.RunForListOfDictionaries();

                foreach (var dict in res)
                {
                    SitePrefs[Site.Id][dict["field_name"]] = dict["field_val"];
                }
            }

            var sitePrefs = SitePrefs[Site.Id];
            if (frm.Equals("") || !sitePrefs.ContainsKey(frm))
            {
                return false;
            }

            var frmPref = sitePrefs[frm];
            if (frmPref.Equals("A"))
            {
                // Everything should be returned.
                return flags[formularyType.ToString()] >= Constants.NON_MATCH;
            }
            else if (frmPref.Equals("F"))
            {
                // Only send matching items on formulary, including similar, equivalent, and exact matches.
                return flags[formularyType.ToString()] >= Constants.PARTIAL_MATCH;
            }
            else if (frmPref.Equals("M"))
            {
                // Only send exact matches. This includes NDC skips and exact match.
                return flags[formularyType.ToString()] >= Constants.EXACT_MATCH;
            }
            else if (frmPref.Equals("D"))
            {
                // Do not send exact for machine - only send similar or equivalent.
                return flags[formularyType.ToString()] >= Constants.PARTIAL_MATCH && flags[formularyType.ToString()] <= Constants.EQUIV_MATCH;
            }
            else if (frmPref.Equals("N"))
            {
                // Return nothing.
                return false;
            }

            return false;
        }

        /// <summary>
        /// Formulary constants
        /// </summary>
        public static class Constants
        {
            // A drug with the 'ndc' specified is contained in the formulary.
            public const int EXACT_MATCH_NDC = 4;

            // A drug with the 'ndc' specified is in the 'multum_ndc_codes' table as
            // 'ndc_skip' and the associated 'ndc_code' is in the formulary. The name,
            // component, dosage, route, etc. match the drug in the multum_denorm table
            // based on the cross-reference to the 'ndc_code'.
            public const int EXACT_MATCH = 3;

            // A drug with a matching 'multum' ID that is associated with the 'ndc'
            // specified is in the formulary but the 'ndc' cross-referencing does not
            // result in an EXACT_MATCH_NDC or an EXACT MATCH. This means that the
            // components, dosage, route, etc. are the same, but the brand name is
            // different and from a different manufacturer.
            public const int EQUIV_MATCH = 2;

            // A drug with the 'dnum' ID that is associated with the 'ndc' specified is
            // in the formulary. Drugs with the same dnum ID may or may not have the same
            // name, route, etc. and there will always be some differences to consider.
            public const int PARTIAL_MATCH = 1;

            // The drug is not contained in the formulary based on any matching criteria.
            public const int NON_MATCH = 0;

            // Individual items
            public const string UNKNOWN = "*** UNKNOWN ***";
            public const string FREE_TEXT = "ft";

            // Formulary identification
            public const string INPAT_TYPE = "inpat";
            public const string INPAT_NAME = "Inpatient";
            public const string INPAT_ICON = "I";
            public const string OUTPAT_TYPE = "outpat";
            public const string OUTPAT_NAME = "Outpatient";
            public const string OUTPAT_ICON = "O";
            public const string PYXIS_TYPE = "pyxis";
            public const string PYXIS_NAME = "Automated Dispensing Machine";
            public const string PYXIS_ICON = "M";

            // Used for selections when processing reasons
            // The 'ALL' value is necessary for gathering 'reasons' by individual
            // formulary or for all in separate formats
            public const int ALL = 0;
            public const int PYXIS_REASON = 1;
            public const int INPAT_REASON = 2;
            public const int OUTPAT_REASON = 3;

            // Used for selecting formulary information from the 'flags' array.
            public const int PYXIS_FLAG = 0;
            public const int INPAT_FLAG = 1;
            public const int OUTPAT_FLAG = 2;

            /// <summary>
            /// Reason selections
            /// </summary>
            public static readonly List<int> REASON_SEL = new List<int> { PYXIS_REASON, INPAT_REASON, OUTPAT_REASON };

            /// <summary>
            /// Reason types
            /// </summary>
            public static readonly List<string> REASON_TYPE = new List<string> { null, PYXIS_TYPE, INPAT_TYPE, OUTPAT_TYPE };

            /// <summary>
            /// Reason names
            /// </summary>
            public static readonly List<string> REASON_NAME = new List<string> { null, PYXIS_NAME, INPAT_NAME, OUTPAT_NAME };

            /// <summary>
            /// Valid formulary types
            /// </summary>
            public static readonly HashSet<string> VALID_TYPES = new HashSet<string> { PYXIS_TYPE, INPAT_TYPE, OUTPAT_TYPE };

            /// <summary>
            /// Flag types
            /// </summary>
            public static readonly List<string> FLAG_TYPE = new List<string> { PYXIS_TYPE, INPAT_TYPE, OUTPAT_TYPE };

            /// <summary>
            /// Flag names
            /// </summary>
            public static readonly List<string> FLAG_NAME = new List<string> { PYXIS_NAME, INPAT_NAME, OUTPAT_NAME };

            /// <summary>
            /// Flag icons
            /// </summary>
            public static readonly List<string> FLAG_ICON = new List<string> { PYXIS_ICON, INPAT_ICON, OUTPAT_ICON };

            /// <summary>
            /// Flag reasons
            /// </summary>
            public static readonly List<int> FLAG_REASON = new List<int> { PYXIS_REASON, INPAT_REASON, OUTPAT_REASON };

            /// <summary>
            /// Prescription display flags
            /// </summary>
            public static readonly List<string> FLAG_RX_DISP = new List<string> { "rx_pyxis_icon", "rx_inpat_icon", "rx_outpat_icon" };
        }
    }
}
