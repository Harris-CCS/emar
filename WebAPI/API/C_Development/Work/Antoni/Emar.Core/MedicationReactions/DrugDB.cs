using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using Emar.Core.Helpers;
using Emar.Core.HomeMedications.Repository;
using Emar.Core.Medications.Model;
using Emar.Core.Options.Model;
using Emar.Core.Options.Repository;
using Emar.Core.Patients.Repository;

namespace Emar.Core.MedicationReactions
{
    /// <summary>
    /// Library to handle interaction with drug databases
    /// </summary>
    public class DrugDB
    {
        private readonly IPatientRepository _patientRepository;
        private readonly IHomeMedicationRepository _homeMedicationRepository;
        private readonly IOptionRepository _optionRepository;
        private IDrugDBUtility instance;

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
        /// Stores drug ID -> SourceTable links
        /// </summary>
        private Dictionary<string, string> Dnum2SourceTable = new Dictionary<string, string>();

        /// <summary>
        /// Stores drug ID -> SourceTableId links
        /// </summary>
        private Dictionary<string, string> Dnum2SourceTableId = new Dictionary<string, string>();

        /// <summary>
        /// Stores allergy drug ID -> SourceTable links
        /// </summary>
        private Dictionary<string, string> AlgDnum2SourceTable = new Dictionary<string, string>();

        /// <summary>
        /// Stores allergy drug ID -> SourceTableId links
        /// </summary>
        private Dictionary<string, string> AlgDnum2SourceTableId = new Dictionary<string, string>();

        /// <summary>
        /// DrugDB constructor
        /// </summary>
        /// <param name="drugDBVendor">Drug DB vendor</param>
        public DrugDB(
            IPatientRepository patientRepository,
            IHomeMedicationRepository homeMedicationRepository,
            IOptionRepository optionRepository,
            string drugDBVendor)
        {
            if (drugDBVendor.Equals("F"))
            {
                instance = new DrugDBFDB();
            }
            else
            {
                throw new NotSupportedException("Unknown drug database selector (" + drugDBVendor + ")");
            }

            _patientRepository = patientRepository;
            _homeMedicationRepository = homeMedicationRepository;
            _optionRepository = optionRepository;
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
        public void AddDnumNameSourceTableSourceTableId(string dnum, string name, string sourceTable, string sourceTableId)
        {
            Dnum2Name[dnum] = name;
            Dname2Num[name] = dnum;
            Dnum2SourceTable[dnum] = sourceTable;
            Dnum2SourceTableId[dnum] = sourceTableId;
        }

        /// <summary>
        /// Check drug reactions against a patient's information
        /// </summary>
        /// <param name="siteId">Site identifier</param>
        /// <param name="patientId">Patient identifier</param>
        /// <param name="checklist">Drugs to check</param>
        /// <returns>ReactionsCheckResult object</returns>
        public ReactionsCheckResult CheckReactions(int siteId, long patientId, Dictionary<string, string> checklist, string drugDBVendor)
        {
            var result = new ReactionsCheckResult();
            var algMedData = LoadAlgMedTable(patientId, drugDBVendor);
            List<Dictionary<string, string>> FTAllergyInfo = new List<Dictionary<string, string>>();
            List<Dictionary<string, string>> AllergyInfo = new List<Dictionary<string, string>>();
            List<Dictionary<string, string>> CurrentMedsInfo = new List<Dictionary<string, string>>();
            List<Dictionary<string, string>> FTCurrentMedsInfo = new List<Dictionary<string, string>>();
            Dictionary<string, string> Warning = new Dictionary<string, string>();
            Dictionary<string, List<Dictionary<string, string>>> RTrigger = new Dictionary<string, List<Dictionary<string, string>>>();
            Dictionary<string, List<Dictionary<string, string>>> OTrigger = new Dictionary<string, List<Dictionary<string, string>>>();

            foreach (var alg in algMedData)
            {
                var type = alg["type"];
                var drug = alg["InternalDrugId"];

                if (type.Equals("A"))
                {
                    if (drug.Equals("ft"))
                    {
                        FTAllergyInfo.Add(alg);
                    }
                    else
                    {
                        AllergyInfo.Add(alg);
                    }
                }
                else if (type.Equals("M"))
                {
                    if (!patientId.ToString().Equals(alg["PatientId"]) || alg.ContainsKey("ActionStatus") ? !alg["ActionStatus"].Equals("C") : false)
                    {
                        continue;
                    }

                    if (drug.Equals("ft"))
                    {
                        FTCurrentMedsInfo.Add(alg);
                    }
                    else
                    {
                        CurrentMedsInfo.Add(alg);
                    }

                    var name = alg.ContainsKey("Name") && !string.IsNullOrWhiteSpace(alg["Name"]) ? alg["Name"] : alg["BrandName"];

                    Dnum2SourceTable[drug] = alg.ContainsKey("SourceTable") ? alg["SourceTable"] : "";
                    Dnum2SourceTableId[drug] = alg.ContainsKey("SourceTableId") ? alg["SourceTableId"] : "";
                    Dnum2Name[drug] = name + (alg.ContainsKey("AlternateName") && !string.IsNullOrWhiteSpace(alg["AlternateName"]) ? " (" + alg["AlternateName"] + ")" : "");
                    Dname2Num[name] = drug;
                }
            }

            List<Dictionary<string, string>> checkInfo = new List<Dictionary<string, string>>();
            Dictionary<string, List<Dictionary<string, string>>> lookups = new Dictionary<string, List<Dictionary<string, string>>>();

            foreach (var alg in AllergyInfo)
            {
                var drugVal = alg.ContainsKey("AllergyDrugId") && !string.IsNullOrWhiteSpace(alg["AllergyDrugId"])
                                ? alg["AllergyDrugId"]
                                    : alg.ContainsKey("InternalDrugId")
                                        ? alg["InternalDrugId"]
                                    : "";

                if (alg.ContainsKey("ParentDrugId") && !string.IsNullOrWhiteSpace(alg["ParentDrugId"]))
                {
                    checkInfo.Add(new Dictionary<string, string>
                    {
                        { "ParentDrugName", alg["ParentDrugName"] },
                        { "ParentDrugId", alg["ParentDrugId"] },
                        { "Name", alg["Name"] },
                        { "AllergyDrugId", drugVal },
                        { "SourceTable", alg["SourceTable"] },
                        { "SourceTableId", alg["SourceTableId"] }
                    });
                }
                else if ((alg.ContainsKey("AllergyDrugId") && !string.IsNullOrWhiteSpace(alg["AllergyDrugId"]) && !alg["AllergyDrugId"].Equals("0")) ||
                        (alg.ContainsKey("InternalDrugId") && !string.IsNullOrWhiteSpace(alg["InternalDrugId"]) && !alg["InternalDrugId"].Equals("0")))
                {
                    if (!lookups.ContainsKey(drugVal))
                    {
                        lookups.Add(drugVal, new List<Dictionary<string, string>>());
                    }

                    lookups[drugVal].Add(alg);
                }
                else
                {
                    checkInfo.Add(new Dictionary<string, string>
                    {
                        { "Name", alg["Name"] },
                        { "Class", alg["Class"] },
                        { "Category", alg["Category"] },
                        { "SourceTable", alg["SourceTable"] },
                        { "SourceTableId", alg["SourceTableId"] }
                    });
                }
            }

            HashSet<string> codeDup = new HashSet<string>();

            if (lookups.Keys.Count > 0)
            {
                var components = new List<string>(lookups.Keys);    // allergy_drug_id or internal_drug_id

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
                        var id = parentInfo.ContainsKey("AllergyDrugId") && !string.IsNullOrWhiteSpace(parentInfo["AllergyDrugId"])
                                    ? parentInfo["AllergyDrugId"]
                                    : parentInfo.ContainsKey("InternalDrugId")
                                        ? parentInfo["InternalDrugId"]
                                        : "";

                        Dictionary<string, string> check = new Dictionary<string, string>
                        {
                            { "ParentDrugName", parentInfo["Name"] },
                            { "ParentDrugId", id },
                            { "Name", dInfo["name"] },
                            { "InternalDrugId", dInfo["cdrug"] },
                            { "SourceTable", parentInfo["SourceTable"] },
                            { "SourceTableId", parentInfo["SourceTableId"] }
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
                    var id = fld.ContainsKey("AllergyDrugId") && !string.IsNullOrWhiteSpace(fld["AllergyDrugId"])
                                ? fld["AllergyDrugId"]
                                    : fld.ContainsKey("InternalDrugId") && !string.IsNullOrWhiteSpace(fld["InternalDrugId"])
                                        ? fld["InternalDrugId"]
                                        : null;

                    if (!fld.ContainsKey("AlternateName"))
                    {
                        fld["AlternateName"] = "";
                    }

                    var tAlgReact = new Dictionary<string, string>();
                    DrugDChecklist(ref tAlgReact, fld.ContainsKey("Class") ? fld["Class"] : "0", fld.ContainsKey("Category") ? fld["Category"] : "0", id, ref checklist);

                    foreach (var sel in tAlgReact.Keys)
                    {
                        if (!checklist.ContainsKey(sel))
                        {
                            continue;
                        }

                        var topName = fld.ContainsKey("ParentDrugName") && !string.IsNullOrWhiteSpace(fld["ParentDrugName"])
                                        ? fld["ParentDrugName"]
                                        : fld.ContainsKey("Name")
                                            ? fld["Name"]
                                            : "";

                        if (!allergyGroup.ContainsKey(sel))
                        {
                            allergyGroup.Add(sel, new Dictionary<string, Dictionary<string, Dictionary<string, Dictionary<string, string>>>>());
                        }

                        if (!allergyGroup[sel].ContainsKey(topName))
                        {
                            allergyGroup[sel].Add(topName, new Dictionary<string, Dictionary<string, Dictionary<string, string>>>());
                        }

                        if (!allergyGroup[sel][topName].ContainsKey(fld["AlternateName"]))
                        {
                            allergyGroup[sel][topName].Add(fld["AlternateName"], new Dictionary<string, Dictionary<string, string>>());
                        }

                        var drug = fld.ContainsKey("ParentDrugId") && !string.IsNullOrWhiteSpace(fld["ParentDrugId"])
                                    ? fld["ParentDrugId"]
                                    : fld.ContainsKey("AllergyDrugId") && !string.IsNullOrWhiteSpace(fld["AllergyDrugId"])
                                        ? fld["AllergyDrugId"]
                                        : fld.ContainsKey("InternalDrugId")
                                            ? fld["InternalDrugId"]
                                            : "";

                        var name = fld.ContainsKey("ParentDrugName") && !string.IsNullOrWhiteSpace(fld["ParentDrugName"])
                                    ? fld["ParentDrugName"]
                                    : fld.ContainsKey("Name")
                                        ? fld["Name"]
                                        : "";

                        allergyGroup[sel][topName][fld["AlternateName"]][fld["Name"]] = new Dictionary<string, string>
                        {
                            { "Class", (fld.ContainsKey("Class") && !String.IsNullOrWhiteSpace(fld["Class"])) ? fld["Class"] : "0" },
                            { "Category",   (fld.ContainsKey("Category") && !String.IsNullOrWhiteSpace(fld["Category"])) ? fld["Category"] : "0" },
                            { "InternalDrugId",  drug },
                            { "Name",  name },
                            { "SourceTable", fld["SourceTable"] },
                            { "SourceTableId", fld["SourceTableId"] }
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

            var keyList = new List<string>(checklist.Keys);
            keyList.AddRange(Dnum2Name.Keys);
            keyList = keyList.Distinct().Where(x => !x.Equals("ft") && !string.IsNullOrWhiteSpace(x)).ToList();

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
                    foreach (var d in component.Values)
                    {
                        foreach (var c in d.Keys)
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
                var rxalert = Convert.ToInt32(_optionRepository.GetOption(siteId, OptionNames.RXALERT));
                var siteRxAlert = Constants.INTERACTION_RXALERT[rxalert];

                foreach (var rtrigger in GetInstance().GetDrugInteractions(keyList))
                {
                    // If the interaction severity is a recognized one, but not among the levels selected for display 
                    // in the rxalert site setting, then skip the trigger creation (that is, don't skip it if it is 
                    // not a recognized level in case something else is going on).
                    var severity = Convert.ToInt32(rtrigger["severity_id"]);

                    if ((Constants.INTERACTION_RXALERT[0].Contains(severity)) && !siteRxAlert.Contains(severity))
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

            foreach (var dnum1 in dnum1List)
            {
                var dnum2List = inter_xref[dnum1].Keys.ToArray();

                foreach (var dnum2 in dnum2List)
                {
                    var sevList = inter_xref[dnum1][dnum2].Keys.ToArray();

                    foreach (var sev in sevList)
                    {
                        // Create entries for the parents
                        var parent1List = parent.ContainsKey(dnum1) ? parent[dnum1] : new List<string>();

                        foreach (var p in parent1List)
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

                        foreach (var p in parent2List)
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

            foreach (var dnum1 in dnum1List)
            {
                var dnum2List = inter_xref[dnum1].Keys.ToArray();

                foreach (var dnum2 in dnum2List)
                {
                    if (dnum1.Equals(dnum2))
                    {
                        continue;
                    }
                    var sevList = inter_xref[dnum1][dnum2].Keys.ToArray();

                    foreach (var sev in sevList)
                    {
                        var rtrigger = inter_xref[dnum1][dnum2][sev];
                        //rtrigger["dname1"] = (Dnum2Name.ContainsKey(dnum1) && !String.IsNullOrWhiteSpace(Dnum2Name[dnum1])) ? Dnum2Name[dnum1] : (checklist.ContainsKey(dnum1) && !String.IsNullOrWhiteSpace(checklist[dnum1])) ? checklist[dnum1] : null;
                        rtrigger["dnum2"] = dnum2;
                        rtrigger["dname2"] = (Dnum2Name.ContainsKey(dnum2) && !String.IsNullOrWhiteSpace(Dnum2Name[dnum2])) ? Dnum2Name[dnum2] : (checklist.ContainsKey(dnum2) && !String.IsNullOrWhiteSpace(checklist[dnum2])) ? checklist[dnum2] : null;
                        rtrigger["SourceTable2"] = (Dnum2SourceTable.ContainsKey(dnum2) && !String.IsNullOrWhiteSpace(Dnum2SourceTable[dnum2])) ? Dnum2SourceTable[dnum2] : null;
                        rtrigger["SourceTableId2"] = (Dnum2SourceTableId.ContainsKey(dnum2) && !String.IsNullOrWhiteSpace(Dnum2SourceTableId[dnum2])) ? Dnum2SourceTableId[dnum2] : null;
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

                                if (componentList.Count > 0)
                                {
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
            result.Dnum2SourceTable = Dnum2SourceTable;
            result.Dnum2SourceTableId = Dnum2SourceTableId;
            result.AlgDnum2SourceTable = AlgDnum2SourceTable;
            result.AlgDnum2SourceTableId = AlgDnum2SourceTableId;

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
        private void DrugDChecklist(ref Dictionary<string, string> rAlgReact, string cls, string cat, string drugId, ref Dictionary<string, string> checklist)
        {
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
                foreach (var info in GetInstance().GetComponentInfo(DrugChecklist))
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
            }
            else if (!String.IsNullOrWhiteSpace(cat) && Convert.ToInt32(cat) > 0)
            {
                if (!CatClass.ContainsKey(cat))
                {
                    CatClass.Add(cat, GetInstance().GetAllergyClassByCategory(cat));
                }

                classList.AddRange(CatClass[cat]);
            }
            else
            {
                classList.Add(cls);
            }

            // Gather all the allergy info for any of the classes
            if (classList.Count > 0)
            {
                // Only get classes that haven't already been retrieved
                List<string> getClasses = new List<string>();

                foreach (var c in classList)
                {
                    if (!classInfo.ContainsKey(c))
                    {
                        getClasses.Add(c);
                    }
                }

                if (getClasses.Count > 0)
                {
                    foreach (var info in GetInstance().GetAllergies(classList, CheckListDrugs))
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
                        foreach (var info in GetInstance().GetComponentInfo(new List<string> { drugId }))
                        {
                            if (!MultInfo.ContainsKey(info["cdrug"]))
                            {
                                MultInfo.Add(info["cdrug"], new List<Dictionary<string, string>>());
                            }

                            MultInfo[info["cdrug"]].Add(info);
                        }
                    }

                    foreach (var d in MultInfo[drugId])
                    {
                        if (components.ContainsKey(d["cdrug"]))
                        {
                            continue;
                        }

                        components.Add(d["cdrug"], d["cdrug"]);
                    }
                }

                // Gather drugs based on intolerance to the current drug
                if (components.Keys.Count > 0)
                {
                    var compList = new List<string>();

                    foreach (var c in components.Keys)
                    {
                        if (!ComponentInfo.ContainsKey(c))
                        {
                            compList.Add(c);
                        }
                    }

                    if (compList.Count > 0)
                    {
                        foreach (var info in GetInstance().GetAllergyIntolerances(compList, DrugChecklist))
                        {
                            if (!ComponentInfo.ContainsKey(info["cdrug"]))
                            {
                                ComponentInfo.Add(info["cdrug"], new List<Dictionary<string, string>>());
                            }

                            ComponentInfo[info["cdrug"]].Add(info);
                        }
                    }

                    foreach (var k in components.Keys)
                    {
                        if (ComponentInfo.ContainsKey(k))
                        {
                            foreach (var i in ComponentInfo[k])
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
        /// <param name="patientId">Patient identifier</param>
        /// <param name="confirmedOnly">Flag for whether the results should only include confirmed entries</param>
        /// <returns>List of Dictionary objects representing alg/med entries</returns>
        public List<Dictionary<string, string>> LoadAlgMedTable(long patientId, string drugDBVendor, bool confirmedOnly = false)
        {
            var algMedData = new List<Dictionary<string, string>>();

            var algResult = DB.ConvertDataSetToListOfDictionaries(
                                _patientRepository.GetAllergiesByPatientId(patientId, a => a.IsActive == true && (a.ActionStatus == "C" || a.ActionStatus == "U"))
                                .ToList()
                                .ToDataSet());

            foreach (var alg in algResult)
            {
                if (drugDBVendor != null &&
                    drugDBVendor == DrugDBVendors.FDB &&
                    !_patientRepository.GetAllergyFdbAllergyNamesByPcHiclSeqno(alg["AllergyDrugId"]).Any())
                {
                    continue;
                }

                alg["AlternateName"] = alg["AlternateName"]?.Trim();
                alg["ParentDrugName"] = alg["ParentDrugName"]?.Trim();
                alg["ParentDrugId"] = alg["ParentDrugId"]?.Trim();
                alg["type"] = "A";

                alg["SourceTable"] = SourceTables.PatientAllergies;
                alg["SourceTableId"] = alg["Id"];
                alg["OrderType"] = EmarOrderType.PatientAllergy.ToString();

                alg.Add("data_source", "PC");

                if ((alg["ActionStatus"].Equals("C") && confirmedOnly) || !confirmedOnly)
                {
                    algMedData.Add(alg);
                }
            }

            var homeMedsResult = DB.ConvertDataSetToListOfDictionaries(
                                _homeMedicationRepository.GetPatientHomeMedications(a => a.PatientId == patientId && a.IsActive == true)
                                .ToList()
                                .ToDataSet());

            foreach (var alg in homeMedsResult)
            {
                if (drugDBVendor != null &&
                    drugDBVendor == DrugDBVendors.FDB &&
                    _homeMedicationRepository.GetPatientHomeMedicationFdbBrandNameByPcRoutedGenId(alg["InternalDrugId"]) == null)
                {
                    continue;
                }

                alg["AlternateName"] = alg["AlternateName"]?.Trim();
                alg["ParentDrugName"] = alg["ParentDrugName"]?.Trim();
                alg["ParentDrugId"] = alg["ParentDrugId"]?.Trim();
                alg["type"] = "M";

                alg["SourceTable"] = SourceTables.PatientHomeMedications;
                alg["SourceTableId"] = alg["Id"];
                alg["OrderType"] = EmarOrderType.HomeMedication.ToString();

                if (alg["ActionStatus"].Equals("R") && (alg["InformationSource"].Equals("PC") || (!alg["InformationSource"].Equals("PC") && Convert.ToInt32(alg["ChangeUserId"]) > 0 && Convert.ToInt32(alg["AddUserId"]) == 0)))
                {
                    alg["ActionStatus"] = "C";
                }

                if (!alg["ActionStatus"].Equals("C"))
                {
                    continue;
                }

                alg.Add("data_source", alg["ActionStatus"].Equals("U") ? "HIE" : "PC");

                if ((alg["ActionStatus"].Equals("C") && confirmedOnly) || !confirmedOnly)
                {
                    algMedData.Add(alg);
                }
            }

            var c = new CultureInfo(CultureInfo.CurrentCulture.Name);

            foreach (var alg in algMedData)
            {
                alg.Add("sorting_date",
                    alg.ContainsKey("ChangeDatetime") && !String.IsNullOrWhiteSpace(alg["ChangeDatetime"]) ? alg["ChangeDatetime"] :
                    alg.ContainsKey("AddDatetime") && !String.IsNullOrWhiteSpace(alg["AddDatetime"]) ? alg["AddDatetime"] :
                    "");

                if ((!alg.ContainsKey("AllergyDrugId") || String.IsNullOrWhiteSpace(alg["AllergyDrugId"])) &&
                    (!alg.ContainsKey("InternalDrugId") || String.IsNullOrWhiteSpace(alg["InternalDrugId"])) &&
                    (!alg.ContainsKey("Category") || String.IsNullOrWhiteSpace(alg["Category"]) || Convert.ToInt32(alg["Category"]) == 0) &&
                    (!alg.ContainsKey("Class") || String.IsNullOrWhiteSpace(alg["Class"]) || Convert.ToInt32(alg["Class"]) == 0))
                {
                    alg["InternalDrugId"] = "ft";
                }

                var n = (alg["InternalDrugId"].Equals("ft")
                            ? c.TextInfo.ToTitleCase((alg.ContainsKey("Name") ? alg["Name"] : alg["BrandName"]).ToLowerInvariant())
                            : alg.ContainsKey("Name") ? alg["Name"] : alg["BrandName"]);

                var f = c.TextInfo.ToTitleCase((alg.ContainsKey("Comment") ? alg["Comment"] : "").ToLowerInvariant());
                alg.Add("paltxt", n + (!String.IsNullOrWhiteSpace(f) ? " - " + f : ""));
            }

            return algMedData;
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
            /// Maps dnums to source table
            /// </summary>
            public Dictionary<string, string> Dnum2SourceTable { get; set; }

            /// <summary>
            /// Maps dnums to source table id
            /// </summary>
            public Dictionary<string, string> Dnum2SourceTableId { get; set; }

            /// <summary>
            /// Maps allergy dnums to source table
            /// </summary>
            public Dictionary<string, string> AlgDnum2SourceTable { get; set; }

            /// <summary>
            /// Maps allergy dnums to source table id
            /// </summary>
            public Dictionary<string, string> AlgDnum2SourceTableId { get; set; }

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
                Dnum2SourceTable = new Dictionary<string, string>();
                Dnum2SourceTableId = new Dictionary<string, string>();
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
        }
    }
}