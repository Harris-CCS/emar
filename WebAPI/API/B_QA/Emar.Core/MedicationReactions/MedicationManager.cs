using System;
using System.Collections.Generic;
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

            DrugDb.ReactionsCheckResult reactionsCheckResult = GetReactionsCheckResult(
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
        /// <param name="patientRepository"></param>
        /// <param name="homeMedicationRepository"></param>
        /// <param name="optionRepository"></param>
        /// <param name="siteId">Site Id</param>
        /// <param name="checkMedications">List of Medication objects to check</param>
        /// <param name="checklist">Checklist dictionary</param>
        /// <param name="patientId">Patient identifier</param>
        /// <returns></returns>
        private static DrugDb.ReactionsCheckResult GetReactionsCheckResult(
            IPatientRepository patientRepository,
            IHomeMedicationRepository homeMedicationRepository,
            IOptionRepository optionRepository,
            int siteId,
            List<MedicationModel> checkMedications,
            Dictionary<string, string> checklist = null,
            long? patientId = null)
        {
            var drugDbVendor = optionRepository.GetOption(siteId, OptionNames.DRUG_DB_VENDOR);
            var drugDb = new DrugDb(
                patientRepository,
                homeMedicationRepository,
                optionRepository,
                drugDbVendor);
            var reactionsCheckResult = new DrugDb.ReactionsCheckResult();

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

                    if (medication.Medication?.MedicationDetails == null)
                    {
                        continue;
                    }

                    foreach (MedicationDetailDto medicationDetail in medication.Medication.MedicationDetails)
                    {
                        var dnum = medicationDetail.FdbBrandName?.PcRoutedGenId;

                        if (string.IsNullOrWhiteSpace(dnum))
                        {
                            continue;
                        }

                        var name = medicationDetail.GetName();

                        if (!string.IsNullOrWhiteSpace(name))
                        {
                            drugDb.AddDnumNameSourceTableSourceTableId(dnum, name, medication.SourceTable, medication.SourceTableId?.ToString());
                        }
                    }
                }

                checklist ??= GetChecklist(checkMedications);

                reactionsCheckResult = drugDb.CheckReactions(siteId, (long)patientId, checklist, drugDbVendor);
            }

            return reactionsCheckResult;
        }

        /// <summary>
        /// Add interactions and reactions to a Medication
        /// </summary>
        /// <param name="medicationItem">Medication object to check</param>
        /// <param name="reactionsCheckResult">DrugDB ReactionsCheckResult</param>
        private static MedicationModel AddInteractionsAndReactions(MedicationModel medicationItem, DrugDb.ReactionsCheckResult reactionsCheckResult)
        {
            var rTrigger = new List<string>();
            var drugTrigger = new Dictionary<string, List<Dictionary<string, string>>>();
            var comboComponentIds = new Dictionary<string, int>();

            if (medicationItem.Medication?.MedicationDetails == null)
            {
                return medicationItem;
            }

            if (medicationItem.Medication.MedicationDetails.Count > 1)
            {
                foreach (var medicationDetail in medicationItem.Medication.MedicationDetails)
                {
                    comboComponentIds[medicationDetail.FdbBrandName.PcRoutedGenId] = 1;
                }
            }

            foreach (var medicationDetail in medicationItem.Medication.MedicationDetails)
            {
                var dnum = medicationDetail.FdbBrandName?.PcRoutedGenId;

                if (string.IsNullOrWhiteSpace(dnum))
                {
                    continue;
                }

                // Generate the information for displaying drug interactions to ordered medications
                var interDone = new Dictionary<string, Dictionary<string, int>>();

                if (reactionsCheckResult.Interactions.ContainsKey(dnum))
                {
                    foreach (var react in reactionsCheckResult.Interactions[dnum])
                    {
                        // Only require confirmations if the reaction is for a component of the combo med
                        if (!comboComponentIds.ContainsKey(react["dnum2"]))
                        {
                            continue;
                        }

                        var drug = react["dname2"];
                        var sev = react["sevtxt"];

                        if (interDone.ContainsKey(drug) && interDone[drug].ContainsKey(sev))
                        {
                            continue;
                        }

                        if (!interDone.ContainsKey(drug))
                        {
                            interDone.Add(drug, new Dictionary<string, int>());
                        }

                        if (!interDone[drug].ContainsKey(sev))
                        {
                            interDone[drug].Add(sev, 1);
                        }

                        var key = react["int_id"] + "|" + medicationDetail.GetName() + "|" + react["dname2"];
                        rTrigger.Add(key);
                    }
                }

                // Generate the information for displaying interactions between components of a combo medication
                if (reactionsCheckResult.ComboInteractions.ContainsKey(dnum))
                {
                    foreach (var react in reactionsCheckResult.ComboInteractions[dnum])
                    {
                        var drug = react["dname2"];
                        var sev = react["sevtxt"];

                        if (interDone.ContainsKey(drug) && interDone[drug].ContainsKey(sev))
                        {
                            continue;
                        }

                        if (!interDone.ContainsKey(drug))
                        {
                            interDone.Add(drug, new Dictionary<string, int>());
                        }

                        if (!interDone[drug].ContainsKey(sev))
                        {
                            interDone[drug].Add(sev, 1);
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
                    var algList = String.Join(", ", keySet.Select(x => "'" + x + "'"));

                    if (!drugTrigger.ContainsKey(dnum))
                    {
                        drugTrigger.Add(dnum, new List<Dictionary<string, string>>());
                    }

                    drugTrigger[dnum].Add(new Dictionary<string, string>
                    {
                        {"dname2", algList},
                        {"severity_id", "0"},
                        {"sevtxt", "ALLERGY"}
                    });
                }

                // Generate the information for displaying the allergy reactions
                interDone.Clear();
                var accInters = new List<Dictionary<string, string>>();
                var accReacts = new List<Dictionary<string, string>>();

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

                        if (interDone.ContainsKey(drug) && interDone[drug].ContainsKey(sev))
                        {
                            continue;
                        }

                        if (!interDone.ContainsKey(drug))
                        {
                            interDone.Add(drug, new Dictionary<string, int>());
                        }

                        if (!interDone[drug].ContainsKey(sev))
                        {
                            interDone[drug].Add(sev, 1);
                        }

                        var key = rsel.ContainsKey("sevtxt") && rsel["sevtxt"].Equals("ALLERGY") ? "A" : "M";
                        var d = key.Equals("A") ? dnum : rsel.ContainsKey("drug_id_2") ? rsel["drug_id_2"] : null;
                        rsel["dnum"] = d;
                        rsel["drug"] = drug;

                        if (key.Equals("A"))
                        {
                            if (reactionsCheckResult.Allergies[dnum].ContainsKey(drug.Trim('\'')))
                            {
                                rsel["SourceTable"] =
                                    reactionsCheckResult.Allergies[dnum][drug.Trim('\'')]["SourceTable"];
                                rsel["SourceTableId"] =
                                    reactionsCheckResult.Allergies[dnum][drug.Trim('\'')]["SourceTableId"];
                            }

                            rsel["type"] = "alg";
                            rsel["interaction"] = sev + " REACTION";
                            accReacts.Add(rsel);
                        }
                        else
                        {
                            rsel["type"] = "drug";
                            rsel["interaction"] = sev + " INTERACTION";
                            accInters.Add(rsel);
                        }
                    }
                }

                medicationItem.Interactions.AddRange(accInters);
                medicationItem.Reactions.AddRange(accReacts);
            }

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

            foreach (var medication in checkMedications)
            {
                if (medication.Medication?.MedicationDetails == null)
                {
                    continue;
                }

                foreach (var medicationDetail in medication.Medication.MedicationDetails)
                {
                    var activeId = medicationDetail.FdbBrandName?.PcRoutedGenId;

                    if (string.IsNullOrWhiteSpace(activeId))
                    {
                        continue;
                    }

                    var name = medicationDetail.GetName();

                    if (!checklist.ContainsKey(activeId))
                    {
                        checklist.Add(activeId, name);
                    }
                }
            }

            return checklist;
        }
    }
}