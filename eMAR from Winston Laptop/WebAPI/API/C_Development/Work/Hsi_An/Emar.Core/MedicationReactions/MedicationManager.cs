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
    public static class MedicationManager
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
        /// <param name="checkAgainstCartOrders"></param>
        public static IEnumerable<MedicationModel> AddInteractionsAndReactionsToMedications(
            int userId,
            int siteId,
            long patientId,
            List<MedicationModel> medicationModelItems,
            IOrderRepository orderRepository,
            ICartOrderRepository cartOrderRepository,
            IHomeMedicationRepository homeMedicationRepository,
            IPatientRepository patientRepository,
            IOptionRepository optionRepository,
            bool checkAgainstCartOrders
            )
        {
            var codeShareSiteMedicationUnit = orderRepository.GetCodeShareSites(siteId)
                .FirstOrDefault(c =>
                    c.Entity == OrderRepository.CodeShareEntity.MedicationUnit)?
                .SharedSiteId;

            var checkMedications = new List<MedicationModel>();
            checkMedications.AddRange(medicationModelItems);

            // When checking against signed orders, make sure we avoid orders that are canceled or deleted.
            checkMedications.AddRange(orderRepository
                .GetPatientOrders(order => 
                    order.PatientId == patientId && 
                    order.OrderStatus != OrderStatus.Cancelled.ToString() && 
                    order.OrderStatus != OrderStatus.Deleted.ToString()
                )
                .ToList()
                .Select(order => OrderMapper.MapPatientOrderToModel(order, userId, siteId, codeShareSiteMedicationUnit))
            );

            if (checkAgainstCartOrders)
            {
                // When checking against cart orders, be sure to only include the orders in this user's cart.
                checkMedications.AddRange(cartOrderRepository
                    .GetPatientCartOrders(order => 
                        order.PatientId == patientId && 
                        order.UserId == userId
                    )
                    .ToList()
                    .Select(order => CartOrderMapper.MapPatientCartOrderToModel(order, userId, siteId, codeShareSiteMedicationUnit))
                );
            }

            var reactionsCheckResult = GetReactionsCheckResult(
                patientRepository,
                homeMedicationRepository,
                optionRepository,
                siteId,
                checkMedications,
                null,
                patientId
            );

            return medicationModelItems
                .Select(medicationModelItem => AddInteractionsAndReactions(medicationModelItem, reactionsCheckResult))
                .Where(med => med != null)
                .ToList();
        }

        /// <summary>
        /// Given a list medications, retrieve the reactions check result for those medications
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
            long patientId = 0)
        {
            var drugDbVendor = optionRepository.GetOption(siteId, OptionNames.DRUG_DB_VENDOR);
            var drugDb = new DrugDb(
                patientRepository,
                homeMedicationRepository,
                optionRepository,
                drugDbVendor);
            var reactionsCheckResult = new DrugDb.ReactionsCheckResult();

            if (patientId != 0)
            {
                foreach (var medication in checkMedications)
                {
                    if (medication.Type == EmarOrderType.PatientOrder &&
                        (medication.OrderStatus == OrderStatus.Cancelled.ToString() ||
                         medication.OrderStatus == OrderStatus.Deleted.ToString()))
                    {
                        continue;
                    }

                    if (medication.Medication?.MedicationDetails == null)
                    {
                        continue;
                    }

                    foreach (var medicationDetail in medication.Medication.MedicationDetails)
                    {
                        var dnum = medicationDetail.FdbBrandName?.PcRoutedGenId;

                        if (string.IsNullOrWhiteSpace(dnum))
                        {
                            continue;
                        }

                        var name = medicationDetail.GetName();

                        if (!string.IsNullOrWhiteSpace(name))
                        {
                            drugDb.AddDnumToDictionaries(dnum, name, medication.SourceTable, medication.SourceTableId?.ToString(), medication.Medication);
                        }
                    }
                }

                checklist ??= GetChecklist(checkMedications);

                reactionsCheckResult = drugDb.CheckReactions(siteId, patientId, checklist, drugDbVendor);
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
            var drugTrigger = new Dictionary<string, List<Dictionary<string, object>>>();
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
                        if (!comboComponentIds.ContainsKey(react["dnum2"].ToString()))
                        {
                            continue;
                        }

                        var drug = react["dname2"];
                        var sev = react["sevtxt"];

                        if (interDone.ContainsKey(drug.ToString()) && interDone[drug.ToString()].ContainsKey(sev.ToString()))
                        {
                            continue;
                        }

                        if (!interDone.ContainsKey(drug.ToString()))
                        {
                            interDone.Add(drug.ToString(), new Dictionary<string, int>());
                        }

                        if (!interDone[drug.ToString()].ContainsKey(sev.ToString()))
                        {
                            interDone[drug.ToString()].Add(sev.ToString(), 1);
                        }
                    }
                }

                // Generate the information for displaying interactions between components of a combo medication
                if (reactionsCheckResult.ComboInteractions.ContainsKey(dnum))
                {
                    foreach (var react in reactionsCheckResult.ComboInteractions[dnum])
                    {
                        var drug = react["dname2"];
                        var sev = react["sevtxt"];

                        if (interDone.ContainsKey(drug.ToString()) && interDone[drug.ToString()].ContainsKey(sev.ToString()))
                        {
                            continue;
                        }

                        if (!interDone.ContainsKey(drug.ToString()))
                        {
                            interDone.Add(drug.ToString(), new Dictionary<string, int>());
                        }

                        if (!interDone[drug.ToString()].ContainsKey(sev.ToString()))
                        {
                            interDone[drug.ToString()].Add(sev.ToString(), 1);
                        }

                        if (!drugTrigger.ContainsKey(dnum))
                        {
                            drugTrigger.Add(dnum, new List<Dictionary<string, object>>());
                        }

                        drugTrigger[dnum].Add(react);
                    }
                }

                if (reactionsCheckResult.Allergies.ContainsKey(dnum))
                {
                    var keySet = reactionsCheckResult.Allergies[dnum].Keys;
                    var algList = string.Join(", ", keySet.Select(x => "'" + x + "'"));

                    if (!drugTrigger.ContainsKey(dnum))
                    {
                        drugTrigger.Add(dnum, new List<Dictionary<string, object>>());
                    }

                    drugTrigger[dnum].Add(new Dictionary<string, object>
                    {
                        { "dname2", algList},
                        { "severity_id", "0"},
                        { "sevtxt", "ALLERGY"}
                    });
                }

                // Generate the information for displaying the allergy reactions
                interDone.Clear();
                var accInters = new List<Dictionary<string, object>>();
                var accReacts = new List<Dictionary<string, object>>();

                if (reactionsCheckResult.Interactions.ContainsKey(dnum))
                {
                    if (!drugTrigger.ContainsKey(dnum))
                    {
                        drugTrigger.Add(dnum, new List<Dictionary<string, object>>());
                    }

                    drugTrigger[dnum].AddRange(reactionsCheckResult.Interactions[dnum]);
                }

                if (drugTrigger.ContainsKey(dnum))
                {
                    var s = new List<Dictionary<string, object>>(drugTrigger[dnum])
                        .OrderBy(o => o["dname2"])
                        .ThenBy(o => Convert.ToInt32(o["severity_id"]));

                    foreach (var sel in s)
                    {
                        var drug = sel["dname2"];
                        var sev = sel["sevtxt"];

                        if (interDone.ContainsKey(drug.ToString()) && interDone[drug.ToString()].ContainsKey(sev.ToString()))
                        {
                            continue;
                        }

                        if (!interDone.ContainsKey(drug.ToString()))
                        {
                            interDone.Add(drug.ToString(), new Dictionary<string, int>());
                        }

                        if (!interDone[drug.ToString()].ContainsKey(sev.ToString()))
                        {
                            interDone[drug.ToString()].Add(sev.ToString(), 1);
                        }

                        var key = sel.ContainsKey("sevtxt") && sel["sevtxt"].Equals("ALLERGY") ? "A" : "M";
                        var d = key.Equals("A") ? sel.GetValueOrDefault("drug_id_2", null) : null;

                        if (key.Equals("A"))
                        {
                            string drugTrimmed = drug.ToString().Trim('\'');
                            if (reactionsCheckResult.Allergies[dnum].ContainsKey(drugTrimmed))
                            {
                                var compKeys = reactionsCheckResult.Allergies[dnum][drugTrimmed].Keys.ToList();
                                compKeys.Sort();

                                foreach (var compKey in compKeys)
                                {
                                    var comp = (Dictionary<string, object>)reactionsCheckResult.Allergies[dnum][drugTrimmed][compKey];
                                    var rsel = new Dictionary<string, object>(sel);
                                    rsel["dnum"] = d;
                                    rsel["drug"] = drug;
                                    rsel["type"] = "alg";
                                    rsel["interaction"] = sev + " REACTION";
                                    rsel["SourceTable"] = comp["SourceTable"];
                                    rsel["SourceTableId"] = comp["SourceTableId"];
                                    rsel["Severity"] = comp["Severity"];
                                    accReacts.Add(rsel);
                                }
                            }
                        }
                        else
                        {
                            var rsel = new Dictionary<string, object>(sel);
                            rsel["dnum"] = d;
                            rsel["drug"] = drug;
                            rsel["type"] = "drug";
                            rsel["interaction"] = sev + " INTERACTION";
                            accInters.Add(rsel);
                        }
                    }
                }

                medicationItem.Interactions = accInters;
                medicationItem.Reactions = accReacts;
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