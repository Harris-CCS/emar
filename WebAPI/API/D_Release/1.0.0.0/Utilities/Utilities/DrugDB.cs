using Interfaces.DomainModel;
using Interfaces.Utilities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace PulseCheck.Utilities
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
        /// Checklist drugs
        /// </summary>
        private Dictionary<string, Dictionary<string, string>> CheckListDrugs = new Dictionary<string, Dictionary<string, string>>();

        /// <summary>
        /// List of drug IDs for checking
        /// </summary>
        private List<string> DrugChecklist = new List<string>();

        /// <summary>
        /// Flag for whether the checklist needs to be built/processed
        /// </summary>
        private bool doChecklist = true;

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
        /// Stores... Multum info?
        /// </summary>
        private Dictionary<string, List<Dictionary<string, string>>> MultInfo = new Dictionary<string, List<Dictionary<string, string>>>();

        /// <summary>
        /// Stores component info
        /// </summary>
        private Dictionary<string, List<Dictionary<string, string>>> ComponentInfo = new Dictionary<string, List<Dictionary<string, string>>>();

        /// <summary>
        /// Stores info about Medication Services
        /// </summary>
        private List<Dictionary<string, string>> MedSvcInfo = new List<Dictionary<string, string>>();

        /// <summary>
        /// Stores drug ID -> name links
        /// </summary>
        private Dictionary<string, string> Dnum2Name = new Dictionary<string, string>();

        /// <summary>
        /// Stores drug name -> ID links
        /// </summary>
        private Dictionary<string, string> Dname2Num = new Dictionary<string, string>();

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
                instance = new DrugDBMultum();
            }
            else if (vendor.Equals("F"))
            {
                instance = new DrugDBFDB();
            }
            else if (vendor.Equals("1"))
            {
                instance = new DrugDBFDBCa();
            } else if (vendor.Equals("2"))
            {
                //instance = new DrugDBMedispan();
            } else
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

        /// <summary>
        /// Add known drug ID <-> name links
        /// </summary>
        /// <param name="dnum">Drug identifier</param>
        /// <param name="name">Drug name</param>
        public void AddDnumAndName(string dnum, string name)
        {
            Dnum2Name[dnum] = name;
            Dname2Num[name] = dnum;
        }

        /// <summary>
        /// Check drug reactions against a patient's information
        /// </summary>
        /// <param name="siteId">Site identifier</param>
        /// <param name="ibex">Patient identifier</param>
        /// <param name="checklist">Drugs to check</param>
        /// <param name="patientMeds">List of medication service orders for the patient</param>
        /// <returns>ReactionsCheckResult object</returns>
        public ReactionsCheckResult CheckReactions(byte siteId, string ibex, Dictionary<string, string> checklist, List<IMedication> patientMeds = null)
        {
            var result = new ReactionsCheckResult();
            var algMedData = LoadAlgMedTable(siteId, ibex);
            List<Dictionary<string, string>> FTAllergyInfo = new List<Dictionary<string, string>>();
            List<Dictionary<string, string>> AllergyInfo = new List<Dictionary<string, string>>();
            List<Dictionary<string, string>> CurrentMedsInfo = new List<Dictionary<string, string>>();
            List<Dictionary<string, string>> FTCurrentMedsInfo = new List<Dictionary<string, string>>();
            Dictionary<string, string> Warning = new Dictionary<string, string>();
            Dictionary<string, List<Dictionary<string, string>>> RTrigger = new Dictionary<string, List<Dictionary<string, string>>>();
            Dictionary<string, List<Dictionary<string, string>>> OTrigger = new Dictionary<string, List<Dictionary<string, string>>>();

            foreach(var alg in algMedData)
            {
                var type = alg["type"];
                var drug = alg["drug"];

                if (type.Equals("A"))
                {
                    if (drug.Equals("ft"))
                    {
                        FTAllergyInfo.Add(alg);
                    } else
                    {
                        AllergyInfo.Add(alg);
                    }
                } else if (type.Equals("M"))
                {
                    if (!ibex.Equals(alg["ibex"]) || !alg["actionstatus"].Equals("C"))
                    {
                        continue;
                    }
                    if (drug.Equals("ft"))
                    {
                        FTCurrentMedsInfo.Add(alg);
                    } else
                    {
                        CurrentMedsInfo.Add(alg);
                    }

                    var name = alg["name"];
                    Dnum2Name[drug] = name + (alg.ContainsKey("alt_name") && !string.IsNullOrWhiteSpace(alg["alt_name"]) ? " (" + alg["alt_name"] + ")" : "");
                    Dname2Num[name] = drug;
                }
            }

            List<Dictionary<string, string>> checkInfo = new List<Dictionary<string, string>>();
            Dictionary<string, List<Dictionary<string, string>>> lookups = new Dictionary<string, List<Dictionary<string, string>>>();
            Dictionary<string, Dictionary<string, string>> drugInfo = new Dictionary<string, Dictionary<string, string>>();
            foreach (var alg in AllergyInfo)
            {
                var drugVal = alg.ContainsKey("alg_drug_id") && !string.IsNullOrWhiteSpace(alg["alg_drug_id"]) ? alg["alg_drug_id"] : 
                              alg.ContainsKey("drug") ? alg["drug"] : "";
                if (alg.ContainsKey("parent_id") && !string.IsNullOrWhiteSpace(alg["parent_id"]))
                {
                    checkInfo.Add(new Dictionary<string, string>
                    {
                        { "parent_name", alg["parent_name"] },
                        { "parent_id", alg["parent_id"] },
                        { "name", alg["name"] },
                        { "alg_drug_id", drugVal }
                    });
                }
                else if ((alg.ContainsKey("alg_drug_id") && !string.IsNullOrWhiteSpace(alg["alg_drug_id"]) && !alg["alg_drug_id"].Equals("0")) || 
                         alg.ContainsKey("drug") && !string.IsNullOrWhiteSpace(alg["drug"]) && !alg["drug"].Equals("0"))
                {
                    if (!lookups.ContainsKey(drugVal))
                    {
                        lookups.Add(drugVal, new List<Dictionary<string, string>>());
                    }
                    lookups[drugVal].Add(alg);
                    drugInfo[drugVal] = alg;
                }
                else
                {
                    checkInfo.Add(new Dictionary<string, string>
                    {
                        { "name", alg["name"] },
                        { "class", alg["class"] },
                        { "cat", alg["cat"] }
                    });
                }
            }

            HashSet<string> codeDup = new HashSet<string>();
            if (lookups.Keys.Count > 0)
            {
                var components = new List<string>(lookups.Keys);
                foreach (var dInfo in GetInstance().GetComponentInfo(components))
                {
                    var key = dInfo["drug"] + "|" + dInfo["cdrug"];
                    if (codeDup.Contains(key))
                    {
                        continue;
                    }
                    codeDup.Add(key);
                    var lookupKey = "drug";
                    if ((!lookups.ContainsKey(dInfo["drug"]) || lookups[dInfo["drug"]].Count == 0) && lookups.ContainsKey(dInfo["c_root"]))
                    {
                        lookupKey = "c_root";
                    }
                    foreach (var parentInfo in lookups[dInfo[lookupKey]])
                    {
                        var id = parentInfo.ContainsKey("alg_drug_id") && !string.IsNullOrWhiteSpace(parentInfo["alg_drug_id"]) ? parentInfo["alg_drug_id"] : 
                                 parentInfo.ContainsKey("drug") ? parentInfo["drug"] : "";
                        Dictionary<string, string> check = new Dictionary<string, string>
                        {
                            { "parent_name", parentInfo["name"] },
                            { "parent_id", id },
                            { "name", dInfo["name"] },
                            { "drug", dInfo["cdrug"] }
                        };
                        checkInfo.Add(check);
                    }
                }
            }

            Dictionary<string, Dictionary<string, Dictionary<string, string>>> AlgReact = new Dictionary<string, Dictionary<string, Dictionary<string, string>>>();

            if (checklist != null && checklist.Count > 0)
            {
                CheckListDrugs.Clear();
                Dictionary<string, Dictionary<string, Dictionary<string, Dictionary<string, Dictionary<string, string>>>>> allergyGroup = new Dictionary<string, Dictionary<string, Dictionary<string, Dictionary<string, Dictionary<string, string>>>>>();
                foreach (var fld in checkInfo)
                {
                    var id = fld.ContainsKey("alg_drug_id") && !string.IsNullOrWhiteSpace(fld["alg_drug_id"]) ? fld["alg_drug_id"] :
                             fld.ContainsKey("drug") && !string.IsNullOrWhiteSpace(fld["drug"]) ? fld["drug"] :
                             null;

                    if (!fld.ContainsKey("alt_name"))
                    {
                        fld["alt_name"] = "";
                    }

                    var tAlgReact = new Dictionary<string, string>();
                    DrugDChecklist(ref tAlgReact, fld.ContainsKey("class") ? fld["class"] : "0", fld.ContainsKey("cat") ? fld["cat"] : "0", id, ref checklist);
                    foreach (var sel in tAlgReact.Keys)
                    {
                        if (!checklist.ContainsKey(sel))
                        {
                            continue;
                        }
                        var topName = fld.ContainsKey("parent_name") && !string.IsNullOrWhiteSpace(fld["parent_name"]) ? fld["parent_name"] : fld.ContainsKey("name") ? fld["name"] : "";
                        if (!allergyGroup.ContainsKey(sel))
                        {
                            allergyGroup.Add(sel, new Dictionary<string, Dictionary<string, Dictionary<string, Dictionary<string, string>>>>());
                        }
                        if (!allergyGroup[sel].ContainsKey(topName))
                        {
                            allergyGroup[sel].Add(topName, new Dictionary<string, Dictionary<string, Dictionary<string, string>>>());
                        }
                        if (!allergyGroup[sel][topName].ContainsKey(fld["alt_name"]))
                        {
                            allergyGroup[sel][topName].Add(fld["alt_name"], new Dictionary<string, Dictionary<string, string>>());
                        }

                        var drug = fld.ContainsKey("parent_id") && !string.IsNullOrWhiteSpace(fld["parent_id"]) ? fld["parent_id"] :
                                   fld.ContainsKey("alg_drug_id") && !string.IsNullOrWhiteSpace(fld["alg_drug_id"]) ? fld["alg_drug_id"] :
                                   fld.ContainsKey("drug") ? fld["drug"] : "";
                        var name = fld.ContainsKey("parent_name") && !string.IsNullOrWhiteSpace(fld["parent_name"]) ? fld["parent_name"] :
                                   fld.ContainsKey("name") ? fld["name"] : "";

                        allergyGroup[sel][topName][fld["alt_name"]][fld["name"]] = new Dictionary<string, string>
                        {
                            { "class", (fld.ContainsKey("class") && !String.IsNullOrWhiteSpace(fld["class"])) ? fld["class"] : "0" },
                            { "cat",   (fld.ContainsKey("cat") && !String.IsNullOrWhiteSpace(fld["cat"])) ? fld["cat"] : "0" },
                            { "drug",  drug },
                            { "name",  name }
                        };
                    }
                }
                foreach (var sel in allergyGroup.Keys)
                {
                    foreach (var name in allergyGroup[sel].Keys)
                    {
                        foreach (var altName in allergyGroup[sel][name].Keys)
                        {
                            var algName = name;
                            if (!String.IsNullOrWhiteSpace(altName))
                            {
                                algName += " (" + altName + ")";
                            }
                            var components = new List<string>(allergyGroup[sel][name][altName].Keys);
                            components.Sort();
                            if (components.Count > 1 || (components.Count == 1 && !name.ToLowerInvariant().Equals(components[0].ToLowerInvariant())))
                            {
                                algName += " [" + String.Join("/", components) + "]";
                            }

                            if (!AlgReact.ContainsKey(sel))
                            {
                                AlgReact.Add(sel, new Dictionary<string, Dictionary<string, string>>());
                            }
                            AlgReact[sel].Remove(algName);
                            AlgReact[sel].Add(algName, allergyGroup[sel][name][altName][components[0]]);
                        }
                    }
                }
            }

            // Allergy checking is complete.
            // The remainder of the code is for Drug Interaction checking.

            // The following is only done if we need to retrieve Medication Services and/
            // Prescriptions for further processing.

            // Process Medication Services orders, if provided
            if (patientMeds != null)
            {
                foreach(var med in patientMeds)
                {
                    if (med.IsDeleted() || med.IsCancelled())
                    {
                        continue;
                    }

                    foreach(var comp in med.GetComponents())
                    {
                        var name = comp.GetName();
                        var dnum = comp.ActiveId;
                        if (!string.IsNullOrWhiteSpace(dnum))
                        {
                            Dnum2Name[dnum] = name;
                            Dname2Num[name] = dnum;
                        }
                    }
                }
            }

            // Include prescriptions in the interactions checks
            var rxDrugInfo = new DB.Select
            {
                Sql = "SELECT name, drug_id FROM rx WHERE ibex=@ibex AND site=@site AND status='A'",
                Parameters = new SqlParameter[]
                {
                    new SqlParameter("@ibex", SqlDbType.Char) { Value = ibex },
                    new SqlParameter("@site", SqlDbType.TinyInt) { Value = siteId }
                }
            }.RunForListOfDictionaries();
            var nameRE = new Regex(@":.*$", RegexOptions.Compiled);
            foreach(var info in rxDrugInfo)
            {
                var name = nameRE.Replace(info["name"], "");
                var dnum = info["drug_id"];
                Dnum2Name[dnum] = name;
                Dname2Num[name] = dnum;
            }

            var keyList = new List<string>(checklist.Keys);
            keyList.AddRange(Dnum2Name.Keys);
            keyList = keyList.Distinct().Where(x => !x.Equals("ft")).ToList();
            if (keyList.Count > 0)
            {
                Warning = GetInstance().HasWarningsAndEffects(keyList);
            }

            // This code identifies which medications interact with other medications
            // Get the data for components of multi-component medications.
            var component = new Dictionary<string, Dictionary<string, string>>();
            var parent = new Dictionary<string, List<string>>();
            if (keyList.Count > 0)
            {
                foreach (var info in GetInstance().GetComponentInfo(keyList))
                {
                    var drug = info["drug"];
                    var cdrug = info["cdrug"];
                    var name = info["name"];
                    if (!component.ContainsKey(drug))
                    {
                        component.Add(drug, new Dictionary<string, string>());
                    }
                    component[drug][cdrug] = name;

                    if (!parent.ContainsKey(cdrug))
                    {
                        parent.Add(cdrug, new List<string>());
                    }
                    parent[cdrug].Add(drug);
                }

                if (component.Keys.Count > 0)
                {
                    foreach(var d in component.Values)
                    {
                        foreach(var c in d.Keys)
                        {
                            keyList.Add(c);
                        }
                    }
                }
            }

            // Gather the interactions
            var inter_xref = new Dictionary<string, Dictionary<string, Dictionary<int, Dictionary<string, string>>>>();
            keyList = keyList.Distinct().Where(x => !x.Equals("ft")).ToList();
            if (keyList.Count > 0)
            {
                var rxalert = Convert.ToInt32(new DB.Select
                {
                    Sql = "SELECT rxalert FROM org WHERE site=@site",
                    Parameters = new SqlParameter[]
                    {
                        new SqlParameter("@site", SqlDbType.TinyInt) { Value = siteId }
                    }
                }.RunForScalar());
                var orgRxAlert = Constants.INTERACTION_RXALERT[rxalert];
                foreach (var rtrigger in GetInstance().GetDrugInteractions(keyList))
                {
                    // If the interaction severity is a recognized one, but not among the levels selected for display 
                    // in the rxalert site setting, then skip the trigger creation (that is, don't skip it if it is 
                    // not a recognized level in case something else is going on).
                    var severity = Convert.ToInt32(rtrigger["severity_id"]);
                    if ((Constants.INTERACTION_RXALERT[0].Contains(severity)) && !orgRxAlert.Contains(severity))
                    {
                        continue;
                    }
                    var d1 = rtrigger["drug_id_1"];
                    var d2 = rtrigger["drug_id_2"];
                    if (!inter_xref.ContainsKey(d1))
                    {
                        inter_xref.Add(d1, new Dictionary<string, Dictionary<int, Dictionary<string, string>>>());
                    }
                    if (!inter_xref[d1].ContainsKey(d2))
                    {
                        inter_xref[d1].Add(d2, new Dictionary<int, Dictionary<string, string>>());
                    }
                    inter_xref[d1][d2][severity] = rtrigger;
                }
            }

            // Find parent drugs that are current/ordered where the child component interaction is flagged, but 
            // the parent is not. Create a parent entry that contains the needed info.
            var dnum1List = inter_xref.Keys.ToArray();
            foreach(var dnum1 in dnum1List)
            {
                var dnum2List = inter_xref[dnum1].Keys.ToArray();
                foreach(var dnum2 in dnum2List)
                {
                    var sevList = inter_xref[dnum1][dnum2].Keys.ToArray();
                    foreach(var sev in sevList)
                    {
                        // Create entries for the parents
                        var parent1List = parent.ContainsKey(dnum1) ? parent[dnum1] : new List<string>();
                        foreach(var p in parent1List)
                        {
                            var info = new Dictionary<string, string>(inter_xref[dnum1][dnum2][sev]);
                            info["drug_id_1"] = p;
                            if (!inter_xref.ContainsKey(p))
                            {
                                inter_xref.Add(p, new Dictionary<string, Dictionary<int, Dictionary<string, string>>>());
                            }
                            if (!inter_xref[p].ContainsKey(dnum2))
                            {
                                inter_xref[p].Add(dnum2, new Dictionary<int, Dictionary<string, string>>());
                            }
                            if (!inter_xref[p][dnum2].ContainsKey(sev))
                            {
                                inter_xref[p][dnum2].Add(sev, new Dictionary<string, string>());
                            }
                            inter_xref[p][dnum2][sev] = info;
                        }

                        var parent2List = parent.ContainsKey(dnum2) ? parent[dnum2] : new List<string>();
                        foreach(var p in parent2List)
                        {
                            var info = new Dictionary<string, string>(inter_xref[dnum1][dnum2][sev]);
                            info["drug_id_2"] = p;
                            if (!inter_xref.ContainsKey(dnum1))
                            {
                                inter_xref.Add(dnum1, new Dictionary<string, Dictionary<int, Dictionary<string, string>>>());
                            }
                            if (!inter_xref[dnum1].ContainsKey(p))
                            {
                                inter_xref[dnum1].Add(p, new Dictionary<int, Dictionary<string, string>>());
                            }
                            if (!inter_xref[dnum1][p].ContainsKey(sev))
                            {
                                inter_xref[dnum1][p].Add(sev, new Dictionary<string, string>());
                            }
                            inter_xref[dnum1][p][sev] = info;
                        }
                    }
                }
            }

            dnum1List = inter_xref.Keys.ToArray();
            foreach(var dnum1 in dnum1List)
            {
                var dnum2List = inter_xref[dnum1].Keys.ToArray();
                foreach(var dnum2 in dnum2List)
                {
                    if (dnum1.Equals(dnum2))
                    {
                        continue;
                    }
                    var sevList = inter_xref[dnum1][dnum2].Keys.ToArray();
                    foreach (var sev in sevList)
                    {
                        var rtrigger = inter_xref[dnum1][dnum2][sev];
                        rtrigger["dname1"] = (Dnum2Name.ContainsKey(dnum1) && !String.IsNullOrWhiteSpace(Dnum2Name[dnum1])) ? Dnum2Name[dnum1] : (checklist.ContainsKey(dnum1) && !String.IsNullOrWhiteSpace(checklist[dnum1])) ? checklist[dnum1] : null;
                        rtrigger["dnum2"] = dnum2;
                        rtrigger["dname2"] = (Dnum2Name.ContainsKey(dnum2) && !String.IsNullOrWhiteSpace(Dnum2Name[dnum2])) ? Dnum2Name[dnum2] : (checklist.ContainsKey(dnum2) && !String.IsNullOrWhiteSpace(checklist[dnum2])) ? checklist[dnum2] : null;
                        rtrigger["sevtxt"] = Constants.SEVERITY_TEXT[sev];

                        // Store the information for interactions with current/ordered drugs
                        if (Dnum2Name.ContainsKey(dnum2))
                        {
                            // The parent drug is already flagged. The drug is flagged, but multi-component
                            // drugs need to show which component(s) interact.
                            var n = Dnum2Name.ContainsKey(dnum2) ? Dnum2Name[dnum2] : null;
                            if (n == null || inter_xref[dnum1].ContainsKey(n) && inter_xref[dnum1][n].ContainsKey(sev))
                            {
                                continue;
                            }
                            if (component.ContainsKey(dnum2))
                            {
                                // Check the components of a multi-component drug for interactions.
                                var componentList = new List<string>();
                                var componentKeyList = component[dnum2].Keys.ToArray();
                                foreach (var comp in componentKeyList)
                                {
                                    if (inter_xref[dnum1].ContainsKey(comp) && inter_xref[dnum1][comp].ContainsKey(sev))
                                    {
                                        componentList.Add(component[dnum2][comp]);
                                    }
                                }

                                if (componentList.Count > 0) {
                                    rtrigger["dname2"] = Dnum2Name[dnum2] + " " + String.Join("", componentList.Select(x => "[" + x + "]").ToList());
                                }
                            }

                            if (!RTrigger.ContainsKey(dnum1))
                            {
                                RTrigger.Add(dnum1, new List<Dictionary<string, string>>());
                            }
                            RTrigger[dnum1].Add(rtrigger);
                        }

                        // Store the information for Combo medications (custom IV bags, etc.)
                        if (checklist.ContainsKey(dnum2))
                        {
                            if (!OTrigger.ContainsKey(dnum1))
                            {
                                OTrigger.Add(dnum1, new List<Dictionary<string, string>>());
                            }
                            OTrigger[dnum1].Add(rtrigger);
                        }
                    }
                }
            }

            result.AllergyInfo = AllergyInfo;
            result.FTAllergyInfo = FTAllergyInfo;
            result.CurrentMedsInfo = CurrentMedsInfo;
            result.MedSvcInfo = MedSvcInfo;
            result.Dnum2Name = Dnum2Name;
            result.Dname2Num = Dname2Num;
            result.Allergies = AlgReact;
            result.Interactions = RTrigger;
            result.ComboInteractions = OTrigger;

            return result;
        }

        /// <summary>
        /// Allergy reaction lookup
        /// </summary>
        /// <param name="rAlgReact">Reference to a Dictionary where the allergy/reaction info will be stored</param>
        /// <param name="cls">Drug classification to check</param>
        /// <param name="cat">Drug category to check</param>
        /// <param name="drugId">Drug ID to use to identify a class to check</param>
        /// <param name="checklist">Reference to a dictionary of drugs to check</param>
        /// <returns>Dictionary of reaction checking results</returns>
        private void DrugDChecklist(ref Dictionary<string, string> rAlgReact, string cls, string cat, string drugId, ref Dictionary<string, string> checklist) {
            if (doChecklist)
            {
                doChecklist = false;
                if (checklist != null && checklist.Keys.Count > 0)
                {
                    // Filter out free text entries that do not contain drug IDs.
                    DrugChecklist = new List<string>(checklist.Keys).FindAll(o => !String.IsNullOrWhiteSpace(o) && !o.Equals("0"));
                    if (DrugChecklist.Count == 0)
                    {
                        return;
                    }
                }

                // Get the components of multi-component medications and add them to the list.
                // This is needed to assure getting the components for checking against class/cat selections.
                foreach(var info in GetInstance().GetComponentInfo(DrugChecklist))
                {
                    if (!CheckListDrugs.ContainsKey(info["cdrug"]))
                    {
                        CheckListDrugs.Add(info["cdrug"], new Dictionary<string, string>());
                    }
                    CheckListDrugs[info["cdrug"]][info["drug"]] = info["name"];
                }
            }

            List<string> classList = new List<string>();
            // If a 'drug' or 'cat' is passed in; Multum will have one class, 'fdb' can be 1 or more classes.
            // If there is no 'drug', a 'class' or 'cat' must exist.
            if (!String.IsNullOrWhiteSpace(drugId))
            {
                if (!DrugClass.ContainsKey(drugId))
                {
                    DrugClass.Add(drugId, GetInstance().GetAllergyClassByDrug(drugId));
                }
                classList.AddRange(DrugClass[drugId]);
            } else if (!String.IsNullOrWhiteSpace(cat) && Convert.ToInt32(cat) > 0)
            {
                if (!CatClass.ContainsKey(cat))
                {
                    CatClass.Add(cat, GetInstance().GetAllergyClassByCategory(cat));
                }
                classList.AddRange(CatClass[cat]);
            } else
            {
                classList.Add(cls);
            }

            // Gather all the allergy info for any of the classes
            if (classList.Count > 0)
            {
                // Only get classes that haven't already been retrieved
                List<string> getClasses = new List<string>();
                foreach(var c in classList)
                {
                    if (!classInfo.ContainsKey(c))
                    {
                        getClasses.Add(c);
                    }
                }
                if (getClasses.Count > 0)
                {
                    foreach(var info in GetInstance().GetAllergies(classList, CheckListDrugs))
                    {
                        if (!classInfo.ContainsKey(info["class"]))
                        {
                            classInfo.Add(info["class"], new List<Dictionary<string, string>>());
                        }
                        classInfo[info["class"]].Add(info);
                    }

                    foreach (var c in classList)
                    {
                        if (classInfo.ContainsKey(c))
                        {
                            foreach (var info in classInfo[c])
                            {
                                rAlgReact[info["drug"]] = info["name"];
                            }
                        }
                    }
                }

                // Marks the components of the Known Allergy drug entry as an Allergy
                // If we ever start displaying the text to identify Potential Intolerance,
                // the information will be stored in a new hash for intolerances when
                // the info is not already in the allergy reaction hash
                var components = new Dictionary<string, string>();
                if (!String.IsNullOrWhiteSpace(drugId))
                {
                    if (!MultInfo.ContainsKey(drugId))
                    {
                        foreach(var info in GetInstance().GetComponentInfo(new List<string> { drugId }))
                        {
                            if (!MultInfo.ContainsKey(info["cdrug"]))
                                MultInfo.Add(info["cdrug"], new List<Dictionary<string, string>>());

                            MultInfo[info["cdrug"]].Add(info);
                        }
                    }
                    foreach(var d in MultInfo[drugId])
                    {
                        if (components.ContainsKey(d["cdrug"]))
                            continue;

                        components.Add(d["cdrug"], d["cdrug"]);
                    }
                }

                // Gather drugs based on intolerance to the current drug
                if (components.Keys.Count > 0)
                {
                    var compList = new List<string>();
                    foreach(var c in components.Keys)
                    {
                        if (!ComponentInfo.ContainsKey(c))
                        {
                            compList.Add(c);
                        }
                    }
                    if (compList.Count > 0)
                    {
                        foreach(var info in GetInstance().GetAllergyIntolerances(compList, DrugChecklist))
                        {
                            if (!ComponentInfo.ContainsKey(info["cdrug"]))
                            {
                                ComponentInfo.Add(info["cdrug"], new List<Dictionary<string, string>>());
                            }
                            ComponentInfo[info["cdrug"]].Add(info);
                        }
                    }
                    foreach(var k in components.Keys)
                    {
                        if (ComponentInfo.ContainsKey(k))
                        {
                            foreach(var i in ComponentInfo[k])
                            {
                                rAlgReact.Remove(i["drug"]);
                                rAlgReact.Add(i["drug"], i["brand"]);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Load alg/med entries from the database
        /// </summary>
        /// <param name="siteId">Site identifier</param>
        /// <param name="ibex">Patient identifier</param>
        /// <param name="confirmedOnly">Flag for whether the results should only include confirmed entries</param>
        /// <returns>List of Dictionary objects representing alg/med entries</returns>
        public List<Dictionary<string, string>> LoadAlgMedTable(byte siteId, string ibex, bool confirmedOnly = false)
        {
            // TODO: The Perl code allows caching here, which may be useful. It used patient ibex, medrec, and site as a hash key.

            var algMedData = new List<Dictionary<string, string>>();
            if (!confirmedOnly)
            {
                var gotHIE = new DB.Select
                {
                    Sql = "SELECT gothie FROM org WHERE site = @site",
                    Parameters = new SqlParameter[]
                    {
                    new SqlParameter("@site", SqlDbType.VarChar) { Value = siteId }
                    }
                }.RunForScalar();
                if (gotHIE.Equals("Y"))
                {
                    foreach (var tbl in new string[] { "pat", "hst" })
                    {
                        var res = new DB.Select
                        {
                            Sql = "SELECT acctnum,person FROM ibex.." + tbl + " WHERE ibex=@ibex AND site=@site",
                            Parameters = new SqlParameter[]
                            {
                            new SqlParameter("@ibex", SqlDbType.Char) { Value = ibex },
                            new SqlParameter("@site", SqlDbType.TinyInt) { Value = siteId }
                            }
                        }.RunForDataRow();
                        if (res != null && !String.IsNullOrWhiteSpace(res["acctnum"]?.ToString()) && !String.IsNullOrWhiteSpace(res["person"]?.ToString()))
                        {
                            var l = new DB.Select
                            {
                                Sql = "SELECT * FROM ibex..hie_alg WHERE person=@person AND acctnum=@acctnum AND site=@site AND actionstatus=@actionstatus AND status IN('A','N')",
                                Parameters = new SqlParameter[]
                                {
                                new SqlParameter("@person", SqlDbType.VarChar) { Value = res["person"]?.ToString() },
                                new SqlParameter("@acctnum", SqlDbType.Char) { Value = res["acctnum"]?.ToString() },
                                new SqlParameter("@site", SqlDbType.SmallInt) { Value = siteId },
                                new SqlParameter("@actionstatus", SqlDbType.Char) { Value = "U" }
                                }
                            }.RunForListOfDictionaries();
                            foreach (var info in l)
                            {
                                info.Add("type", "A");
                                info.Add("cmt", info["comment"]);
                                info.Add("data_source", "HIE");
                                algMedData.Add(info);
                            }
                            break;
                        }
                    }
                }
            }

            var algResult = new DB.Select
            {
                Sql = "SELECT * FROM ibex..alg WHERE ibex=@ibex AND site=@site AND status<>'I' AND (type='M' OR (type='A' AND actionstatus='C' OR actionstatus='U'))",
                Parameters = new SqlParameter[]
                {
                    new SqlParameter("@ibex", SqlDbType.Char) { Value = ibex },
                    new SqlParameter("@site", SqlDbType.TinyInt) { Value = siteId }
                }
            }.RunForListOfDictionaries();
            foreach(var alg in algResult)
            {
                alg["alt_name"] = alg["alt_name"]?.Trim();
                alg["parent_name"] = alg["parent_name"]?.Trim();
                alg["parent_id"] = alg["parent_id"]?.Trim();

                if (alg["type"].Equals("M"))
                {
                    if (alg["actionstatus"].Equals("R") && (alg["provider"].Equals("PC") || (!alg["provider"].Equals("PC") && Convert.ToInt32(alg["usrchg"]) > 0 && Convert.ToInt32(alg["usr"]) == 0))) {
                        alg["actionstatus"] = "C";
                    }
                    if (!alg["actionstatus"].Equals("C"))
                    {
                        continue;
                    }
                }
                alg.Add("data_source",
                    alg["type"].Equals("M") && alg["actionstatus"].Equals("U") ? "HIE" : "PC"
                );

                if ((alg["actionstatus"].Equals("C") && confirmedOnly) || !confirmedOnly)
                {
                    algMedData.Add(alg);
                }
            }

            var c = new CultureInfo(CultureInfo.CurrentCulture.Name);
            foreach (var alg in algMedData)
            {
                alg.Add("sorting_date",
                    !String.IsNullOrWhiteSpace(alg["datechg"]) ? alg["datechg"] :
                    !String.IsNullOrWhiteSpace(alg["dateadd"]) ? alg["dateadd"] :
                    alg["statusdt"]
                );
                if (String.IsNullOrWhiteSpace(alg["alg_drug_id"]) && String.IsNullOrWhiteSpace(alg["drug"]) && Convert.ToInt32(alg["cat"]) == 0 && Convert.ToInt32(alg["class"]) == 0)
                {
                    alg["drug"] = "ft";
                }

                var n = (alg["drug"].Equals("ft") ? c.TextInfo.ToTitleCase(alg["name"].ToLowerInvariant()) : alg["name"]);
                var f = c.TextInfo.ToTitleCase(alg["cmt"].ToLowerInvariant());
                alg.Add("paltxt", n + (!String.IsNullOrWhiteSpace(f) ? " - " + f : ""));
            }
            
            return algMedData;
        }

        /// <summary>
        /// Load quick list data
        /// </summary>
        /// <param name="siteId">Site identifier</param>
        /// <param name="userId">User identifier</param>
        /// <param name="type">(M)edication or (P)rescription</param>
        /// <param name="cat">Category</param>
        /// <param name="top">Top limit</param>
        /// <returns>List of Dictionary objects containing quick list data</returns>
        public List<Dictionary<string, string>> LoadQuickListData(byte siteId, int userId, string type, string cat, int limit)
        {
            var org = new DB.Select
            {
                Sql = "SELECT * FROM org WHERE site=@site",
                Parameters = new SqlParameter[]
                {
                    new SqlParameter("@site", SqlDbType.TinyInt) { Value = siteId }
                }
            }.RunForDataRow();

            var parameters = new List<SqlParameter>();
            var sqlFormulary = "";
            var exact = "";
            if (type.Equals("P"))
            {
                var inpat = org["rxinpat"].ToString().Equals("Y");
                var outpat = org["rxoutpat"].ToString().Equals("Y");
                var pyxis = org["rxpyxis"].ToString().Equals("Y");
                var exactMatch = org["rxexactmatch"].ToString().Equals("Y");
                exact = exactMatch ? "> 2" : "> 0";

                if (inpat && outpat && pyxis)
                {
                    sqlFormulary = string.Format(" AND (formulary_match.inpat {0} OR formulary_match.outpat {0} OR formulary_match.pyxis {0})", exact);
                } else if (inpat && outpat)
                {
                    sqlFormulary = string.Format(" AND (formulary_match.inpat {0} OR formulary_match.outpat {0})", exact);
                } else if (inpat && pyxis)
                {
                    sqlFormulary = string.Format(" AND (formulary_match.inpat {0} OR formulary_match.pyxis {0})", exact);
                } else if (outpat && pyxis)
                {
                    sqlFormulary = string.Format(" AND (formulary_match.outpat {0} OR formulary_match.pyxis {0})", exact);
                } else if (inpat)
                {
                    sqlFormulary = string.Format(" AND formulary_match.inpat {0}", exact);
                } else if (outpat)
                {
                    sqlFormulary = string.Format(" AND formulary_match.outpat {0}", exact);
                } else if (pyxis)
                {
                    sqlFormulary = string.Format(" AND formulary_match.pyxis {0}", exact);
                } else if (exactMatch && !inpat && !outpat && !pyxis) {
                    sqlFormulary = string.Format(" AND (formulary_match.inpat {0} OR formulary_match.outpat {0} OR formulary_match.pyxis {0})", exact);
                }
            } else
            {
                var inpat = org["medinpat"].ToString().Equals("Y");
                var outpat = org["medoutpat"].ToString().Equals("Y");
                var pyxis = org["medpyxis"].ToString().Equals("Y");
                var exactMatch = org["medexactmatch"].ToString().Equals("Y");
                exact = exactMatch ? "> 2" : "> 0";

                if (inpat && outpat && pyxis)
                {
                    sqlFormulary = string.Format(" AND (formulary_match.inpat {0} OR formulary_match.outpat {0} OR formulary_match.pyxis {0})", exact);
                }
                else if (inpat && outpat)
                {
                    sqlFormulary = string.Format(" AND (formulary_match.inpat {0} OR formulary_match.outpat {0})", exact);
                }
                else if (inpat && pyxis)
                {
                    sqlFormulary = string.Format(" AND (formulary_match.inpat {0} OR formulary_match.pyxis {0})", exact);
                }
                else if (outpat && pyxis)
                {
                    sqlFormulary = string.Format(" AND (formulary_match.outpat {0} OR formulary_match.pyxis {0})", exact);
                }
                else if (inpat)
                {
                    sqlFormulary = string.Format(" AND formulary_match.inpat {0}", exact);
                }
                else if (outpat)
                {
                    sqlFormulary = string.Format(" AND formulary_match.outpat {0}", exact);
                }
                else if (pyxis)
                {
                    sqlFormulary = string.Format(" AND formulary_match.pyxis {0}", exact);
                }
                else if (exactMatch && !inpat && !outpat && !pyxis)
                {
                    sqlFormulary = string.Format(" AND (formulary_match.inpat {0} OR formulary_match.outpat {0} OR formulary_match.pyxis {0})", exact);
                }
            }

            // Gather all the 'rxl' entries to be displayed
            var catClause = "";
            if (!string.IsNullOrWhiteSpace(cat))
            {
                catClause = "AND rxl.listcat = @listcat";
                parameters.Add(new SqlParameter("@listcat", SqlDbType.VarChar) { Value = cat });
            }

            var formularyMatch = "";
            if (!string.IsNullOrWhiteSpace(sqlFormulary))
            {
                formularyMatch = "formulary_match,";
                sqlFormulary += " AND rxl.ndc = formulary_match.ndc AND formulary_match.site = @matchsite";
                parameters.Add(new SqlParameter("@matchsite", SqlDbType.TinyInt) { Value = org["frmcs"] });
            }

            var top = "";
            if (limit > 0)
            {
                top = "TOP " + limit;
            }

            return GetInstance().GetFilteredQuickListData(siteId, top, formularyMatch, catClause, parameters, type, sqlFormulary, userId);
        }

        /// <summary>
        /// Get the data for a quicklist entry from the rxl table
        /// </summary>
        /// <param name="ibex">Patient identifier</param>
        /// <param name="userId">User identifier</param>
        /// <param name="num">Quicklist entry number</param>
        /// <returns>Dictionary of quicklist information</returns>
        public Dictionary<string, string> LoadQuickListEntry(string ibex, int userId, int num)
        {
            var res = new DB.Select
            {
                Sql = "SELECT * FROM rxl LEFT JOIN rxl_qcpr ON rxl_qcpr.rxl_num = rxl.num WHERE num=@num AND usr=@usr AND site=@site",
                Parameters = new SqlParameter[]
                {
                    new SqlParameter("@num", SqlDbType.Int) { Value = num },
                    new SqlParameter("@usr", SqlDbType.Int) { Value = userId },
                    new SqlParameter("@site", SqlDbType.TinyInt) { Value = Site.Id }
                }
            }.RunForDictionary();

            return res;
        }

        // TODO: Implement this
        public void SetCurrentMeds(byte siteId, string ibex, string target) {
            MedSvcInfo.Clear();

        } 

        /// <summary>
        /// Object to store results from reaction checking in a format that is only slightly easier to deal with than a regular Dictionary.
        /// </summary>
        public class ReactionsCheckResult
        {
            /// <summary>
            /// Allergy information
            /// </summary>
            public List<Dictionary<string, string>> AllergyInfo { get; set; }

            /// <summary>
            /// Freetext Allergy information
            /// </summary>
            public List<Dictionary<string, string>> FTAllergyInfo { get; set; }

            /// <summary>
            /// Current Medications information
            /// </summary>
            public List<Dictionary<string, string>> CurrentMedsInfo { get; set; }

            /// <summary>
            /// Freetext Current Medications information
            /// </summary>
            public List<Dictionary<string, string>> FTCurrentMedsInfo { get; set; }

            /// <summary>
            /// Medication Services information
            /// </summary>
            public List<Dictionary<string, string>> MedSvcInfo { get; set; }

            /// <summary>
            /// Maps dnums to drug names
            /// </summary>
            public Dictionary<string, string> Dnum2Name { get; set; }

            /// <summary>
            /// Maps drug names to dnums
            /// </summary>
            public Dictionary<string, string> Dname2Num { get; set; }

            /// <summary>
            /// Allergy information
            /// </summary>
            public Dictionary<string, Dictionary<string, Dictionary<string, string>>> Allergies { get; set; }

            /// <summary>
            /// Reaction triggers
            /// </summary>
            public Dictionary<string, List<Dictionary<string, string>>> Interactions { get; set; }

            /// <summary>
            /// Override triggers
            /// </summary>
            public Dictionary<string, List<Dictionary<string, string>>> ComboInteractions { get; set; }

            /// <summary>
            /// Warning information
            /// </summary>
            public Dictionary<string, string> Warning { get; set; }

            /// <summary>
            /// Default constructor
            /// </summary>
            public ReactionsCheckResult()
            {
                AllergyInfo = new List<Dictionary<string, string>>();
                FTAllergyInfo = new List<Dictionary<string, string>>();
                CurrentMedsInfo = new List<Dictionary<string, string>>();
                FTCurrentMedsInfo = new List<Dictionary<string, string>>();
                MedSvcInfo = new List<Dictionary<string, string>>();
                Dnum2Name = new Dictionary<string, string>();
                Dname2Num = new Dictionary<string, string>();
                Allergies = new Dictionary<string, Dictionary<string, Dictionary<string, string>>>();
                Interactions = new Dictionary<string, List<Dictionary<string, string>>>();
                ComboInteractions = new Dictionary<string, List<Dictionary<string, string>>>();
                Warning = new Dictionary<string, string>();
            }
        }

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
            /// Allergy category identifier
            /// </summary>
            public const string ALLERGY_CAT = "cat";

            /// <summary>
            /// Allergy class identifier
            /// </summary>
            public const string ALLERGY_CLASS = "class";

            /// <summary>
            /// Allergy drug identifier
            /// </summary>
            public const string ALLERGY_DRUG = "drug";

            // Interaction severity text
            // Items [1,2,3,4] are for Multum
            // Items [5,6,7,8] are for FDB (First Data Bank)
            public static readonly List<string> SEVERITY_TEXT = new List<string> {
                "",
                "MINOR",
                "MODERATE",
                "SEVERE",
                "ALLERGY",
                "UNDETERMINED",
                "MODERATE",
                "SEVERE",
                "CONTRAINDICATED",
            };

            // Translation between severities and display level (rxalert value in admin->sites)
            public static readonly Dictionary<int, List<int>> INTERACTION_RXALERT = new Dictionary<int, List<int>> {
                { 0, new List<int> { 1, 2, 3, 4, 5, 6, 7, 8 } },
                { 1, new List<int> { 2, 3} },
                { 2, new List<int> { 3 } },
                { 5, new List<int> { 6, 7, 8 } },
                { 6, new List<int> { 7, 8 } }
            };

            /// <summary>
            /// Map certain field names to other field names
            /// </summary>
            public static readonly Dictionary<string, string> rxl_obj_map = new Dictionary<string, string>
            {
                { "ndc",      "ndc" },
                { "strength", "dose" },
                { "unit",     "unit" },
                { "route",    "route" },
                { "notes",    "med_notes" },
                { "schedule", "schedule" },
                { "repeat",   "med_repeat" }
            };
        }
    }
}