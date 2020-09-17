using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Emar.Core.Carts.Model.Mappings;
using Emar.Core.Carts.Repository;
using Emar.Core.HomeMedications.Repository;
using Emar.Core.Medications.Model;
using Emar.Core.Options.Model;
using Emar.Core.Options.Repository;
using Emar.Core.Orders.Model;
using Emar.Core.Orders.Model.Mappings;
using Emar.Core.Orders.Repository;
using Emar.Core.Patients.Repository;

namespace Emar.Core.MedicationReactions
{
    public class MedicationManager
    {
        /// <summary>
        /// Add interactions and reactions to a set of patient orders, cart orders and home medications for a patient
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="siteId"></param>
        /// <param name="patientId"></param>
        /// <param name="medicationModelItems"></param>
        /// <param name="orderRepository"></param>
        /// <param name="cartOrderRepository"></param>
        /// <param name="homeMedicationRepository"></param>
        /// <param name="patientRepository"></param>
        /// <param name="optionRepository"></param>
        public static List<MedicationModel> AddInteractionsAndReactionsToMedications(
            int userId,
            int siteId,
            long patientId,
            List<MedicationModel> medicationModelItems,
            IOrderRepository orderRepository,
            ICartOrderRepository cartOrderRepository,
            IHomeMedicationRepository homeMedicationRepository,
            IPatientRepository patientRepository,
            IOptionRepository optionRepository
            )
        {
            List<MedicationModel> checkMedications = new List<MedicationModel>();
            checkMedications.AddRange(medicationModelItems);
            checkMedications.AddRange(orderRepository
                .GetPatientOrders(order => order.PatientId == patientId)
                .ToList()
                .Select(order => OrderMapper.MapPatientOrderToModel(order, userId, siteId)));
            checkMedications.AddRange(cartOrderRepository
                .GetPatientCartOrders(order => order.PatientId == patientId)
                .ToList()
                .Select(order => CartOrderMapper.MapPatientCartOrderToModel(order, userId, siteId)));

            DrugDB.ReactionsCheckResult reactionsCheckResult = GetReactionsCheckResult(
                patientRepository,
                homeMedicationRepository,
                optionRepository,
                siteId,
                checkMedications,
                null,
                patientId);

            List<MedicationModel> interactionAndReactionMedications = new List<MedicationModel>();

            foreach (var medicationModelItem in medicationModelItems)
            {
                MedicationModel med = AddInteractionsAndReactions(medicationModelItem, reactionsCheckResult);

                if (med != null)
                {
                    interactionAndReactionMedications.Add(med);
                }
            }

            return interactionAndReactionMedications;
        }

        /// <summary>
        /// Given a list of meds, retrieve the reactions check result for those meds
        /// </summary>
        /// <param name="siteId">Site Id</param>
        /// <param name="drugDBVendor">Drug DB vendor</param>
        /// <param name="checkMedications">List of Medication objects to check</param>
        /// <param name="checklist">Checklist dictionary</param>
        /// <param name="patientId">Patient identifier</param>
        /// <returns>ReactionsCheckResult object</returns>
        private static DrugDB.ReactionsCheckResult GetReactionsCheckResult(
            IPatientRepository patientRepository,
            IHomeMedicationRepository homeMedicationRepository,
            IOptionRepository optionRepository,
            int siteId,
            List<MedicationModel> checkMedications,
            Dictionary<string, string> checklist = null,
            long? patientId = null)
        {
            var drugDB = new DrugDB(
                patientRepository,
                homeMedicationRepository,
                optionRepository,
                optionRepository.GetOption(siteId, OptionNames.DRUG_DB_VENDOR));
            var reactionsCheckResult = new DrugDB.ReactionsCheckResult();

            if (patientId != null)
            {
                foreach (var medication in checkMedications)
                {
                    if (medication.Type == EmarOrderType.PatientOrder &&
                        (medication.OrderStatus == OrderStatuses.Cancelled.ToString() ||
                         medication.OrderStatus == OrderStatuses.Deleted.ToString()))
                    {
                        continue;
                    }

                    var name = medication.GetName();
                    var dnum = medication.ActiveId;

                    if (!string.IsNullOrWhiteSpace(name) && !string.IsNullOrWhiteSpace(dnum))
                    {
                        drugDB.AddDnumNameSourceTableSourceTableId(dnum, name, medication.SourceTable, medication.SourceTableId?.ToString());
                    }
                }

                if (checklist == null)
                {
                    checklist = GetChecklist(checkMedications);
                }

                reactionsCheckResult = drugDB.CheckReactions(siteId, (long)patientId, checklist);
            }

            return reactionsCheckResult;
        }

        /// <summary>
        /// Add interactions and reactions to a Medication
        /// </summary>
        /// <param name="med">Medication object to check</param>
        /// <param name="reactionsCheckResult">DrugDB ReactionsCheckResult</param>
        private static MedicationModel AddInteractionsAndReactions(MedicationModel medicationItem, DrugDB.ReactionsCheckResult reactionsCheckResult)
        {
            var rTrigger = new List<string>();
            var drugTrigger = new Dictionary<string, List<Dictionary<string, string>>>();

            var dnum = medicationItem.ActiveId;

            if (string.IsNullOrWhiteSpace(dnum))
            {
                return null;
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

                    var key = react["int_id"] + "|" + medicationItem.GetName() + "|" + react["dname2"];
                    rTrigger.Add(key);
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

                    var key = rsel.ContainsKey("sevtxt") && rsel["sevtxt"].Equals("ALLERGY") ? "A" : "M";
                    var d = key.Equals("A") ? dnum : rsel.ContainsKey("drug_id_2") ? rsel["drug_id_2"] : null;
                    rsel["dnum"] = d;
                    rsel["drug"] = drug;

                    if (key.Equals("A"))
                    {
                        if (reactionsCheckResult.Allergies[dnum].ContainsKey(drug.Trim('\'')))
                        {
                            rsel["SourceTable"] = reactionsCheckResult.Allergies[dnum][drug.Trim('\'')]["SourceTable"];
                            rsel["SourceTableId"] = reactionsCheckResult.Allergies[dnum][drug.Trim('\'')]["SourceTableId"];
                        }

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

            medicationItem.Interactions = acc_inters;
            medicationItem.Reactions = acc_reacts;

            return medicationItem;
        }

        /// <summary>
        /// Get the checklist information used for interaction/reaction checking
        /// </summary>
        /// <param name="checkMedications">List of Medication objects to check</param>
        /// <returns>Dictionary of checklist information</returns>
        private static Dictionary<string, string> GetChecklist(List<MedicationModel> checkMedications)
        {
            var checklist = new Dictionary<string, string>();

            foreach (MedicationModel medication in checkMedications)
            {
                var activeId = medication.ActiveId;

                if (string.IsNullOrWhiteSpace(activeId))
                {
                    continue;
                }

                var name = medication.GetName();

                if (!checklist.ContainsKey(activeId))
                {
                    checklist.Add(activeId, name);
                }
            }

            return checklist;
        }
    }
}