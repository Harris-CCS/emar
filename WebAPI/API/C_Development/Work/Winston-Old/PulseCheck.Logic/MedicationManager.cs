using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using PulseCheck.Domain;
using PulseCheck.IDomain;
using PulseCheck.ILogic;
using PulseCheck.IRepository;
using PulseCheck.Utilities;
using Group = PulseCheck.Domain.Group;

namespace PulseCheck.Logic
{
    /// <summary>
    /// Medication services
    /// </summary>
    public class MedicationManager : IMedicationManager
    {
        /// <summary>
        /// Medication respository instance
        /// </summary>
        private readonly IMedicationRepository _medicationRepository;
        private readonly IUserRepository _userRepository;

        /// <summary>
        /// Site service constructor
        /// </summary>
        /// <param name="medicationRepository">IMedicationRepository instance</param>
        public MedicationManager(IMedicationRepository medicationRepository, IUserRepository userRepository)
        {
            _medicationRepository = medicationRepository;
            _userRepository = userRepository;
        }

        /// <summary>
        /// Add interactions and reactions to a set of medications for a patient
        /// </summary>
        /// <param name="site">ISite instance</param>
        /// <param name="meds">List of Medication objects</param>
        /// <param name="patientId">Patient identifier</param>
        public static async void AddInteractionsAndReactionsToMedications(ISite site, List<Medication> meds, string patientId, IMedicationRepository medRepository)
        {
            var checkMeds = new List<Medication>(meds);
            var otherMeds = await medRepository.GetMedicationsByPatientIdAsync(site.Id, patientId);
            checkMeds.AddRange(otherMeds);

            var reactionsCheckResult = GetReactionsCheckResult(site, checkMeds, null, patientId);
            foreach(var med in meds)
            {
                AddInteractionsAndReactions(med, reactionsCheckResult);
            }
        }

        /// <summary>
        /// Given a list of meds, retrieve the reactions check result for those meds
        /// </summary>
        /// <param name="site">ISite instance</param>
        /// <param name="meds">List of Medication objects to check</param>
        /// <param name="checklist">Checklist dictionary</param>
        /// <param name="patientId">Patient identifier</param>
        /// <returns>ReactionsCheckResult object</returns>
        private static DrugDB.ReactionsCheckResult GetReactionsCheckResult(ISite site, List<Medication> meds, Dictionary<string, string> checklist = null, string patientId = null)
        {
            var drugDB = new DrugDB(site);
            var reactionsCheckResult = new DrugDB.ReactionsCheckResult();
            if (!string.IsNullOrWhiteSpace(patientId))
            {
                foreach (var med in meds)
                {
                    if (med.IsCancelled() || med.IsDeleted())
                        continue;

                    var info = new Dictionary<string, string>();
                    var componentList = new List<Medication.Component>();

                    if (med.IsFreeText())
                    {
                        info.Add("name", med.GetName());
                    }
                    else if (med.IsCombo())
                    {
                        componentList = new List<Medication.Component>(med.Components);
                        info.Add("name", med.GetName());
                    }
                    else
                    {
                        componentList = new List<Medication.Component>(med.Components);
                        info.Add("name", componentList[0].BrandName);
                    }

                    var _t = new Time(site.Id);
                    var temp_dosage = med.Dose;
                    if (String.IsNullOrWhiteSpace(temp_dosage))
                    {
                        temp_dosage = "*";
                    }
                    info.Add("dosage", temp_dosage);
                    info.Add("unit", med.GetUnitDescription());
                    info.Add("route", med.GetRouteDescription());
                    info.Add("type", med.Type);

                    if (med.IsGiven())
                    {
                        info.Add("given", _t.LongDateTime(med.GiveDate));
                    }

                    foreach (var component in componentList)
                    {
                        var name = component.GetName();
                        var dnum = component.ActiveId;
                        if (!string.IsNullOrWhiteSpace(name) && !string.IsNullOrWhiteSpace(dnum))
                        {
                            // TODO: There is more going on here for med svc info in the Perl code, but right
                            // now we only care about interaction/reaction checking
                            drugDB.AddDnumAndName(dnum, name);
                        }
                    }
                }

                // TODO: Set current meds details here

                if (checklist == null)
                {
                    checklist = GetChecklist(meds);
                }

                reactionsCheckResult = drugDB.CheckReactions(site.Id, patientId, checklist);
            }

            return reactionsCheckResult;
        }

        /// <summary>
        /// Get all meds for a patient
        /// </summary>
        /// <param name="siteId">Site identifier</param>
        /// <param name="patientId">Patient identifier</param>
        /// <returns>List of Medication objects</returns>
        public async Task<List<Medication>> GetMedicationsByPatientIdAsync(byte siteId, string patientId)
        {
            var result = await _medicationRepository.GetMedicationsByPatientIdAsync(siteId, patientId);
            return result;
        }

        /// <summary>
        /// Get a medication for a patient from its order identifier
        /// </summary>
        /// <param name="siteId">Site identifier</param>
        /// <param name="patientId">Patient identifier</param>
        /// <param name="orderId">Order identifier</param>
        /// <returns></returns>
        public async Task<Medication> GetMedicationByIdAsync(byte siteId, string patientId, int orderId)
        {
            var result = await _medicationRepository.GetMedicationByIdAsync(siteId, patientId, orderId);
            return result;
        }

        /// <summary>
        /// Get a medication for a patient from its losecs identifier
        /// </summary>
        /// <param name="siteId">Site identifier</param>
        /// <param name="patientId">Patient identifier</param>
        /// <param name="losecs">Medication losecs value</param>
        /// <returns>Medication object</returns>
        public async Task<Medication> GetMedicationByLosecsAsync(byte siteId, string patientId, int losecs)
        {
            var result = await _medicationRepository.GetMedicationByLosecsAsync(siteId, patientId, losecs);
            return result;
        }

        /// <summary>
        /// Get the contents of a particular medication group, checked against a particular patient
        /// </summary>
        /// <param name="user">User object</param>
        /// <param name="site">Site object</param>
        /// <param name="group">Group object</param> 
        /// <param name="patientId">Patient identifier</param>
        /// <returns>Group with medications populated</returns>
        public async Task<Group> GetMedicationGroup(User user, Site site, Group group, string patientId = null)
        {
            if (!user.HasAtLeastReadPermission(Permission.MED_SVC))
            {
                return null;
            }

            var checklist = new Dictionary<string, string>();
            var groupData = (new Medication(user)).LoadGroup(user, patientId, group.Num);
            var groupMeds = groupData.Values.OrderBy(o => o.Name).ToList();
            if (groupMeds != null)
            {
                checklist = GetChecklist(groupMeds);
            }

            group.Medications = groupMeds;

            var checkMeds = new List<Medication>(groupMeds);
            checkMeds.AddRange(await _medicationRepository.GetMedicationsByPatientIdAsync(site.Id, patientId));

            var reactionsCheckResult = GetReactionsCheckResult(site, checkMeds, checklist, patientId);

            // TODO: Populate this using logic in ibex4w.
            var catOrder = new List<string>();
            group.Medications = GetFilteredMedList(site, user, catOrder, group.Medications, reactionsCheckResult);
            return group;
        }

        /// <summary>
        /// Get a user's most used medications list, checked against patient information
        /// </summary>
        /// <param name="user">User object</param>
        /// <param name="site">Site object</param>
        /// <param name="patientId">Patient identifier</param>
        /// <returns>List of QLItem objects</returns>
        public async Task<List<QLItem>> GetMedMostUsedList(User user, Site site, string patientId = null)
        {
            var type = "M";
            var limit = 20; // This limit was specified in MOB-33 and later changed to 20 by Pete in a call. Purposefully not using drs.*ql_user_limit column value.

            var db = new DrugDB(site);
            var mostUsedData = new List<Dictionary<string, string>>();
            var qlData = db.LoadQuickListData(site.Id, user.Id, type, null, 0);
            var usageMeds = new Dictionary<int, List<Dictionary<string, string>>>();
            foreach (var info in qlData)
            {
                var usage = Convert.ToInt32(string.IsNullOrWhiteSpace(info["usage"]) ? "0" : info["usage"]);
                if (usage > 0)
                {
                    if (!usageMeds.ContainsKey(usage))
                    {
                        usageMeds[usage] = new List<Dictionary<string, string>>();
                    }
                    usageMeds[usage].Add(info);
                }
            }

            var usageKeys = usageMeds.Keys.OrderByDescending(x => x);
            foreach (var k in usageKeys)
            {
                if (mostUsedData.Count == limit)
                {
                    break;
                }

                var meds = usageMeds[k].ToList();
                foreach (var m in meds)
                {
                    mostUsedData.Add(m);
                    if (mostUsedData.Count == limit)
                    {
                        break;
                    }
                }
            }

            var ql = GetExtraQuickListData(db, user, site, mostUsedData);

            var finalMedList = ql;
            if (!string.IsNullOrWhiteSpace(patientId))
            {
                var medSvcMeds = await _medicationRepository.GetMedicationsByPatientIdAsync(site.Id, patientId);
                var medsList = new List<IMedication>();
                foreach (var m in medSvcMeds)
                {
                    medsList.Add((IMedication)m);
                }

                var checklist = GetChecklist(ql);
                var reactionsCheckResult = db.CheckReactions(site.Id, patientId, checklist, medsList);
                var catOrder = new List<string>();
                finalMedList = GetFilteredMedList(site, user, catOrder, ql, reactionsCheckResult);
            }

            return BuildQLItemList(type, finalMedList);
        }

        /// <summary>
        /// Get a user's medications quick list, checked against patient information
        /// </summary>
        /// <param name="user">User object</param>
        /// <param name="site">Site object</param>
        /// <param name="patientId">Patient identifier</param>
        /// <returns>List of QLItem objects for the user's quick list</returns>
        public async Task<List<QLItem>> GetMedQuickList(User user, Site site, string patientId = null)
        {
            var type = "M";
            var db = new DrugDB(site);
            var qlData = db.LoadQuickListData(site.Id, user.Id, type, null, 0);
            var ql = GetExtraQuickListData(db, user, site, qlData);

            var finalMedList = ql;
            if (!string.IsNullOrWhiteSpace(patientId))
            {
                var medSvcMeds = await _medicationRepository.GetMedicationsByPatientIdAsync(site.Id, patientId);
                var medsList = new List<IMedication>();
                foreach (var m in medSvcMeds)
                {
                    medsList.Add((IMedication)m);
                }

                var checklist = GetChecklist(ql);
                var reactionsCheckResult = db.CheckReactions(site.Id, patientId, checklist, medsList);
                var catOrder = new List<string>();
                finalMedList = GetFilteredMedList(site, user, catOrder, ql, reactionsCheckResult);
            }

            return BuildQLItemList(type, finalMedList);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="user"></param>
        /// <param name="site"></param>
        /// <param name="patientId"></param>
        /// <param name="brand"></param>
        /// <returns></returns>
        public async Task<List<BasicMedication>> GetBrandMeds(User user, Site site, string patientId, string brand)
        {
            var db = new DrugDB(site);
            var brandMeds = db.GetInstance().GetDrugInfoByBrand(site.Id, brand);

            var allBrandMedsData = new List<Medication>();
            var baseMed = new Medication(user);
            foreach (var brandMed in brandMeds) { 
                var med = baseMed.Clone();
                med.Components = new List<Medication.Component>();
                foreach (var k in brandMed.Keys)
                {
                    med.set(k, brandMed[k]);
                }
                med.Name = brandMed["brand"];
                med.Code = brandMed["ndc"];

                allBrandMedsData.Add(med);

                var clone = new Dictionary<string, string>(brandMed);
                var component = new Medication.Component(site.Id, db.DBType);
                component.SetDrugInfo(clone);
                component.ProductCode = brandMed.ContainsKey("product_id") ? brandMed["product_id"] : "";
                component.ProcedureCode = brandMed.ContainsKey("procedure_id") ? brandMed["procedure_id"] : "";
                med.Components.Add(component);

                if ((med.IsFreeText() || med.IsCombo()) && brandMed.ContainsKey("brand"))
                {
                    med.Name = brandMed["brand"];
                }
            }

            var finalMedList = allBrandMedsData;
            if (!string.IsNullOrWhiteSpace(patientId))
            {
                var medSvcMeds = await _medicationRepository.GetMedicationsByPatientIdAsync(site.Id, patientId);
                var medsList = new List<IMedication>();
                foreach (var m in medSvcMeds)
                {
                    medsList.Add((IMedication)m);
                }

                var checklist = GetChecklist(allBrandMedsData);
                var reactionsCheckResult = db.CheckReactions(site.Id, patientId, checklist, medsList);
                var catOrder = new List<string>();
                finalMedList = GetFilteredMedList(site, user, catOrder, allBrandMedsData, reactionsCheckResult);
            }

            return BuildOrderableMedicationList(finalMedList);
        }

        /// <summary>
        /// Write a trigger file for the med interface
        /// </summary>
        /// <param name="site">ISite instance</param>
        /// <param name="patientId">Patient identifiefr</param>
        /// <param name="userId">User identifier</param>
        /// <param name="msg">Message</param>
        /// <param name="losecs">Losecs value of med</param>
        public static void TriggerFile(ISite site, string patientId, int userId, string msg, int losecs)
        {
            if (site.GetOrgOption("TRIGGER_MED_CUSTOM").Equals("N"))
            {
                return;
            }

            var filePath = site.Root + "\\link\\med\\" + patientId + losecs + "_" + (new Time()).Timestamp();
            var line = userId + ":" + msg;

            FileWriter.Write(filePath, line);
        }

        /// <summary>
        /// Add interactions and reactions to a Medication
        /// </summary>
        /// <param name="med">Medication object to check</param>
        /// <param name="reactionsCheckResult">DrugDB ReactionsCheckResult</param>
        private static void AddInteractionsAndReactions(Medication med, DrugDB.ReactionsCheckResult reactionsCheckResult)
        {
            var rTrigger = new List<string>();
            var drugTrigger = new Dictionary<string, List<Dictionary<string, string>>>();

            var combo_comp_id = new Dictionary<string, int>();
            if (med.IsCombo())
            {
                foreach (var c in med.Components)
                {
                    combo_comp_id[c.ActiveId] = 1;
                }
            }

            foreach (var comp in med.Components)
            {
                var dnum = comp.ActiveId;
                if (string.IsNullOrWhiteSpace(dnum))
                {
                    continue;
                }

                // Generate the information for displaying drug interactions to ordered medications
                var inter_done = new Dictionary<string, Dictionary<string, int>>();
                if (reactionsCheckResult.Interactions.ContainsKey(dnum))
                {
                    foreach (var react in reactionsCheckResult.Interactions[dnum])
                    {
                        var drug = react["dname2"];
                        var sev = react["sevtxt"];
                        if (inter_done.ContainsKey(drug) && inter_done[drug].ContainsKey(sev))
                        {
                            continue;
                        }
                        if (!inter_done.ContainsKey(drug))
                        {
                            inter_done.Add(drug, new Dictionary<string, int>());
                        }
                        if (!inter_done[drug].ContainsKey(sev))
                        {
                            inter_done[drug].Add(sev, 1);
                        }
                        var key = react["int_id"] + "|" + comp.GetName() + "|" + react["dname2"];
                        rTrigger.Add(key);
                    }
                }

                // Generate the information for displaying interactions between components of a combo medication
                if (med.IsCombo() && reactionsCheckResult.ComboInteractions.ContainsKey(dnum))
                {
                    foreach (var react in reactionsCheckResult.ComboInteractions[dnum])
                    {
                        // Only require confirmations if the reaction is for a component of the combo med
                        if (!combo_comp_id.ContainsKey(react["dnum2"]))
                        {
                            continue;
                        }
                        var drug = react["dname2"];
                        var sev = react["sevtxt"];
                        if (inter_done.ContainsKey(drug) && inter_done[drug].ContainsKey(sev))
                        {
                            continue;
                        }
                        if (!inter_done.ContainsKey(drug))
                        {
                            inter_done.Add(drug, new Dictionary<string, int>());
                        }
                        if (!inter_done[drug].ContainsKey(sev))
                        {
                            inter_done[drug].Add(sev, 1);
                        }
                        if (!drugTrigger.ContainsKey(dnum))
                        {
                            drugTrigger.Add(dnum, new List<Dictionary<string, string>>());
                        }
                        drugTrigger[dnum].Add(react);
                    }
                }

                if (reactionsCheckResult.Allergies.ContainsKey(dnum))
                {
                    var keySet = reactionsCheckResult.Allergies[dnum].Keys;
                    var alg_list = String.Join(", ", keySet.Select(x => "'" + x + "'"));
                    if (!drugTrigger.ContainsKey(dnum))
                    {
                        drugTrigger.Add(dnum, new List<Dictionary<string, string>>());
                    }
                    drugTrigger[dnum].Add(new Dictionary<string, string>
                        {
                            { "dname2", alg_list },
                            { "severity_id", "0" },
                            { "sevtxt", "ALLERGY" }
                        });
                }

                // Generate the information for displaying the allergy reactions
                inter_done.Clear();
                var acc_inters = new List<Dictionary<string, string>>();
                var acc_reacts = new List<Dictionary<string, string>>();
                if (reactionsCheckResult.Interactions.ContainsKey(dnum))
                {
                    if (!drugTrigger.ContainsKey(dnum))
                    {
                        drugTrigger.Add(dnum, new List<Dictionary<string, string>>());
                    }
                    drugTrigger[dnum].AddRange(reactionsCheckResult.Interactions[dnum]);
                }
                if (drugTrigger.ContainsKey(dnum))
                {
                    var s = new List<Dictionary<string, string>>(drugTrigger[dnum]);
                    s.OrderBy(o => o["dname2"]).ThenBy(o => Convert.ToInt32(o["severity_id"]));
                    foreach (var sel in s)
                    {
                        var rsel = new Dictionary<string, string>(sel);
                        var drug = rsel["dname2"];
                        var sev = rsel["sevtxt"];
                        if (inter_done.ContainsKey(drug) && inter_done[drug].ContainsKey(sev))
                            continue;

                        if (!inter_done.ContainsKey(drug))
                            inter_done.Add(drug, new Dictionary<string, int>());

                        if (!inter_done[drug].ContainsKey(sev))
                            inter_done[drug].Add(sev, 1);

                        var key = rsel.ContainsKey("sevtxt") && rsel["sevtxt"].Equals("ALLERGY") ? "A" : "M";
                        var d = key.Equals("A") ? dnum : rsel.ContainsKey("drug_id_2") ? rsel["drug_id_2"] : null;
                        rsel["dnum"] = d;
                        rsel["drug"] = drug;

                        if (key.Equals("A"))
                        {
                            rsel["type"] = "alg";
                            rsel["interaction"] = sev + " REACTION";
                            acc_reacts.Add(rsel);
                        }
                        else
                        {
                            rsel["type"] = "drug";
                            rsel["interaction"] = sev + " INTERACTION";
                            acc_inters.Add(rsel);
                        }
                    }
                }

                comp.Interactions = acc_inters;
                comp.Reactions = acc_reacts;
            }
        }

        /// <summary>
        /// Take a list of Medication objects and build a list of OrderableMedication objects from it
        /// </summary>
        /// <param name="medList">The list of Medications objects</param>
        /// <returns>List of OrderableMedication objects</returns>
        private List<BasicMedication> BuildOrderableMedicationList(List<Medication> medList)
        {
            var meds = new List<BasicMedication>();
            foreach (var m in medList)
            {
                var orderableMed = new BasicMedication
                {
                    Id = m.Code.ToString(),
                    Name = string.Format("{0} {1} {2}", m.Name, m.Dose, m.Route),
                    HasIndication = m.HasIndication,
                };

                foreach (var comp in m.Components)
                {
                    if (comp.Interactions != null)
                    {
                        orderableMed.Interactions.AddRange(comp.Interactions);
                    }
                    if (comp.Reactions != null)
                    {
                        orderableMed.Reactions.AddRange(comp.Reactions);
                    }
                }

                meds.Add(orderableMed);
            }

            return meds.OrderBy(o => o.Name).ToList();
        }

        /// <summary>
        /// Take a list of Medication objects and build a list of QLItem objects from it
        /// </summary>
        /// <param name="type">QLItem type</param>
        /// <param name="medList">The list of Medications objects</param>
        /// <returns>List of QLItem objects</returns>
        private List<QLItem> BuildQLItemList(string type, List<Medication> medList)
        {
            var qlItems = new List<QLItem>();
            foreach (var m in medList)
            {
                var qlItem = new QLItem
                {
                    Type = type,
                    Id = m.Id.ToString(),
                    GroupKey = m.GetName(),
                    Name = m.GetFullName(),
                    Dose = m.Dose,
                    Unit = m.Unit,
                    Route = m.Route,
                    Schedule = m.Schedule,
                    Repeat = m.Repeat,
                    Notes = m.Notes,
                    HasIndication = m.HasIndication,
                };

                foreach (var comp in m.Components)
                {
                    if (comp.Interactions != null)
                    {
                        qlItem.Interactions.AddRange(comp.Interactions);
                    }
                    if (comp.Reactions != null)
                    {
                        qlItem.Reactions.AddRange(comp.Reactions);
                    }
                }

                qlItems.Add(qlItem);
            }

            return qlItems.OrderBy(o => o.Name).ToList();
        }

        /// <summary>
        /// Get the checklist information used for interaction/reaction checking
        /// </summary>
        /// <param name="meds">List of Medication objects to check</param>
        /// <returns>Dictionary of checklist information</returns>
        private static Dictionary<string, string> GetChecklist(List<Medication> meds)
        {
            var checklist = new Dictionary<string, string>();
            foreach (Medication med in meds)
            {
                foreach (Medication.Component comp in med.Components)
                {
                    var activeId = comp.ActiveId;
                    if (string.IsNullOrWhiteSpace(activeId))
                    {
                        continue;
                    }
                    var name = comp.GetName();
                    if (!checklist.ContainsKey(activeId))
                    {
                        checklist.Add(activeId, name);
                    }
                }
            }

            return checklist;
        }

        /// <summary>
        /// Get extra quick list data
        /// </summary>
        /// <param name="user">User object</param>
        /// <param name="site">ISite instance</param>
        /// <param name="qlData">Current quick list data</param>
        /// <returns>List of Medication objects with extra data included</returns>
        public List<Medication> GetExtraQuickListData(DrugDB drugDB, User user, ISite site, List<Dictionary<string, string>> qlData)
        {
            // First get the component info definitions for the drugs in one pass.
            var ndcs = new Dictionary<string, int>();
            var componentInfo = new Dictionary<string, Dictionary<string, string>>();
            var ndcRE = new Regex(@"^\d+$", RegexOptions.Compiled);
            foreach (var rxl in qlData)
            {
                var ndc = rxl["ndc"];
                if (ndcRE.IsMatch(ndc))
                    ndcs[ndc] = 1;
            }
            foreach (var info in drugDB.GetInstance().GetDrugInfoByNDCs(ndcs.Keys.ToList()))
            {
                componentInfo[info["ndc"]] = info;
            }

            return GetExtraMedData(drugDB, user, site, qlData, componentInfo);
        }

        private List<Medication> GetExtraMedData(DrugDB drugDB, User user, ISite site, List<Dictionary<string, string>> medData, Dictionary<string, Dictionary<string, string>> componentInfo)
        {
            var convertedMeds = new List<Medication>();

            // Process each quick list entry using the data
            var baseMed = new Medication(user);
            foreach (var medInfo in medData)
            {
                var med = baseMed.Clone();
                med.Components = new List<Medication.Component>();
                med.Id = Convert.ToInt32(medInfo["num"]);
                foreach (var k in medInfo.Keys)
                {
                    med.set(k, medInfo[k]);
                }
                var ndc = medInfo["ndc"];

                if (componentInfo.ContainsKey(ndc))
                {
                    var clone = new Dictionary<string, string>(componentInfo[ndc]);
                    var component = new Medication.Component(site.Id, drugDB.GetInstance().GetDBType());
                    component.SetDrugInfo(clone);
                    component.ProductCode = medInfo.ContainsKey("product_id") ? medInfo["product_id"] : "";
                    component.ProcedureCode = medInfo.ContainsKey("procedure_id") ? medInfo["procedure_id"] : "";
                    med.Components.Add(component);

                    if (drugDB.GetInstance().CheckObsoletes())
                    {
                        // TODO: Obsolete checks from 1208-1219 in DrugDB.pm
                    }
                }

                if ((med.IsFreeText() || med.IsCombo()) && medInfo.ContainsKey("brand"))
                {
                    med.Name = medInfo["brand"];
                }

                convertedMeds.Add(med);
            }

            return convertedMeds;
        }

        /// <summary>
        /// Perform the final filtering and interaction/reaction checking on the med list
        /// </summary>
        /// <param name="siteId">Site identifier</param>
        /// <param name="user">User object</param>
        /// <param name="catOrder">Category Order something or other</param>
        /// <param name="medications">List of Medication objects</param>
        /// <param name="reactionsCheckResult">DrugDB.ReactionsCheckResult instance</param>
        /// <returns>List of filtered Medication objects with interactions and reactions added</returns>
        private List<Medication> GetFilteredMedList(Site site, User user, List<string> catOrder, List<Medication> medications, DrugDB.ReactionsCheckResult reactionsCheckResult)
        {
            var frmCSSite = GetFormularyShareSite(site.Id);
            var FormularyChecker = new Formulary(frmCSSite, user, catOrder, "med");
            var meds = new List<Medication>();
            using (var con = new SqlConnection(DB.GetConnectionString()))
            {
                con.Open();
                foreach (var med in medications)
                {
                    var components = med.Components;
                    if (med.IsDrug() && !FormularyChecker.IsInFormulary(components.First().PackagingId, "med", frmCSSite, con))
                        continue;

                    if (components.Any())
                    {
                        foreach (var component in components)
                        {
                            // Check to see if we have an integer for the drug category.  If we don't, they should be Canadians and 
                            // not care about the indication.
                            int drugCatId;
                            if (Int32.TryParse(component.DrugCategoryId, out drugCatId) && ShouldHaveIndication(drugCatId, site, con))
                                med.HasIndication = true;

                            if (component.PackagingId.Substring(0, 1) == "-")
                                med.IsObsolete = true;
                        }

                        if (!med.IsObsolete)
                        {
                            AddInteractionsAndReactions(med, reactionsCheckResult);
                            meds.Add(med);
                        }
                    }
                }
                con.Close();
            }

            return meds;
        }

        /// <summary>
        /// Given a site ID, return a Site object for the ID'd site's Formulary Sharing site
        /// </summary>
        /// <param name="siteId">Current site identifier</param>
        /// <returns>Site object for Formulary Sharing site</returns>
        public static Site GetFormularyShareSite(int siteId)
        {
            return new Site(Convert.ToByte(
                new DB.Select
                {
                    Sql = "SELECT frmcs FROM org WHERE site=@site",
                    Parameters = new SqlParameter[]
                    {
                        new SqlParameter("@site", SqlDbType.TinyInt) { Value = siteId }
                    }
                }.RunForScalar()
            ));
        }

        private bool ShouldHaveIndication(int category, ISite site, SqlConnection con)
        {
            var milId = new DB.Select
            {
                Connection = con,
                Sql = "SELECT id FROM medication_indication_list WHERE site=@site AND sub_cat=@sub_cat",
                Parameters = new SqlParameter[]
                {
                    new SqlParameter("@site", SqlDbType.TinyInt) { Value = site.Id },
                    new SqlParameter("@sub_cat", SqlDbType.Int) { Value = category }
                }
            }.RunForScalar();

            return milId != null;
        }
        /// <summary>
        /// Get an EMR.Line object that represents this medication's information
        /// </summary>
        /// <param name="patient">IPatient instance</param>
        /// <param name="entryUserId">User entering the medication</param>
        /// <param name="sysDate">System date/time for entry</param>
        /// <param name="InterOverList">Dictionary of interaction override information</param>
        /// <param name="overrides">Overrides for this entry</param>
        /// <returns>EMR.Line object for entry</returns>
        public async Task<EMR.Line> ChartEntry(IPatient patient, Medication med, string sysDate, Dictionary<string, string> InterOverList, List<OverrideRationale> overrides)
        {
            var line = new EMR.Line();

            var medInfo = new StringBuilder();
            var NCTNAME = med.GetName();

            if (med.IsFreeText())
            {
                medInfo.Append(string.Format("Free Text order: {0} : {1} : {2}", NCTNAME, med.Notes, med.GetRouteDescription()));
            }
            else
            {
                medInfo.Append(string.Format("Order: {0}", med.GetFullNameForChart()));

                var rate = string.Format("{0} {1}", med.Rate, med.GetRateUnitDescription());
                if (!string.IsNullOrWhiteSpace(rate))
                {
                    medInfo.Append(string.Format("<b>Rate: </b> {0}", rate));
                }

                foreach (var comp in med.Components)
                {
                    var brandName = "";
                    if (med.IsCombo())
                    {
                        medInfo.Append(string.Format("\n** {0}", comp.GetFullName(med)));
                        brandName = comp.GetBrandName();
                    }

                    OverrideRationale applyToAll = null;
                    foreach (var drugInteraction in comp.Reactions)
                    {
                        foreach (var irOverride in overrides)
                        {
                            OverrideRationale rationale = null;
                            if (irOverride.Dnum.Equals(comp.DrugId))
                            {
                                rationale = irOverride;
                                if (irOverride.ApplyToAll)
                                {
                                    applyToAll = irOverride;
                                }
                            }
                            else if (applyToAll != null)
                            {
                                rationale = applyToAll;
                            }

                            if (rationale != null)
                            {
                                medInfo.Append(string.Format("\nPOTENTIAL {0}: {1} - ", drugInteraction["interaction"], drugInteraction["drug"]));
                                medInfo.Append(string.Join(", ", rationale.Overrides));
                                med.WriteTrx(patient, drugInteraction["sevtxt"], drugInteraction["drug"], NCTNAME, comp, Convert.ToInt32(med.OrderUserId));
                            }
                        }
                    }

                    applyToAll = null;
                    foreach (var allergyReaction in comp.Interactions)
                    {
                        foreach (var irOverride in overrides)
                        {
                            OverrideRationale rationale;
                            if (applyToAll != null)
                            {
                                rationale = applyToAll;
                            }
                            else
                            {
                                rationale = irOverride;
                                if (irOverride.ApplyToAll)
                                    applyToAll = irOverride;
                            }

                            medInfo.Append(string.Format("\nPOTENTIAL {0}: {1} - ", allergyReaction["interaction"], allergyReaction["drug"]));
                            medInfo.Append(string.Join(", ", rationale.Overrides));
                            med.WriteTrx(patient, allergyReaction["sevtxt"], allergyReaction["drug"], NCTNAME, comp, Convert.ToInt32(med.OrderUserId));
                        }
                    }
                }
            }

            // TODO: DRC information is written here in desktop PulseCheck. Currently not present in the mobile app.

            if (!string.IsNullOrWhiteSpace(med.Repeat) || !string.IsNullOrWhiteSpace(med.Time))
            {
                var lineParts = new List<string>();
                var timeDescription = med.GetMedTimeDescription();
                var useDescription = (!string.IsNullOrWhiteSpace(timeDescription) ? timeDescription : !string.IsNullOrWhiteSpace(med.Time) ? med.Time : "");
                if (!string.IsNullOrWhiteSpace(useDescription))
                {
                    lineParts.Add(string.Format("Schedule: {0}", useDescription));
                }
                if (!string.IsNullOrWhiteSpace(med.Repeat))
                {
                    lineParts.Add(med.Repeat);
                }
                medInfo.Append(string.Format("\n{0}", string.Join(" ", lineParts)));
            }

            if (!string.IsNullOrWhiteSpace(med.Notes))
            {
                medInfo.Append(string.Format("\nNotes: {0}", med.Notes));
            }

            if (med.OrderForUserId != null)
            {
                var orderer = await _userRepository.GetUserByIdAsync(Convert.ToInt32(med.OrderForUserId));
                medInfo.Append(string.Format("\nOrdered By: {0}", orderer.FullName));
            }

            if (med.OrderUserId != null)
            {
                var enterer = await _userRepository.GetUserByIdAsync(Convert.ToInt32(med.OrderUserId));
                medInfo.Append(string.Format("\nEntered By: {0} {1}", enterer.FullName, (new Time()).LongDateTime(med.OrderDate)));
            }

            if (!string.IsNullOrWhiteSpace(med.Authentication) && MedicationActions.Constants.AUTH_TEXT.ContainsKey(med.Authentication))
            {
                medInfo.Append(" ");
                medInfo.Append(MedicationActions.Constants.AUTH_TEXT[med.Authentication]);
            }

            line.LineHeader.sys_time = sysDate;
            line.LineHeader.user = med.OrderUserId ?? 0;
            line.LineHeader.losecs = med.Losecs.ToString();         // TODO: Make sure EF actually updates this value after a save. Never trust.
            line.LinePart.nct = EMR.Constants.NCT_MED_SVC;
            line.LinePart.section = EMR.Constants.SECT_MED_SVC;
            line.LinePart.part = NCTNAME;
            line.DataSegments = new List<EMR.Line.DataSegment>
            {
                new EMR.Line.DataSegment(EMR.Line.DataSegment.Constants.TYPE_DROPDOWN, medInfo.ToString())
            };

            return line;
        }

        /// <summary>
        /// MedicationService Constants
        /// </summary>
        public static class Constants
        {
            // Map medication actions to trigger actions
            public static readonly Dictionary<string, string> OPTS_MAPPING = new Dictionary<string, string>
            {
                { "ack", "order_ack" },
                { "hold", "order_held" },
                { "unhold", "order_unheld" },
                { "give", "order_given" },
                { "cancel", "order_cancel" },
                { "del", "order_delete" },
                { "place", "order_placed" },
                { "discontinue", "order_discontinue" },
                { "discontinued", "order_discontinued" }
            };
        }
    }
}
