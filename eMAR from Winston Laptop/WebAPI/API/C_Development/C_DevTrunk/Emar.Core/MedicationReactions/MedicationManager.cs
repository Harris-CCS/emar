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
using Emar.Data.Entities;

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
            bool checkAgainstCartOrders,
            IEnumerable<PatientOrder>? newOrders = null,
            IEnumerable<PatientOrder>? existingOrders = null,
            IEnumerable<PatientCartOrder>? cartOrders = null,
            IEnumerable<PatientAllergy>? patientAllergies = null,
            IEnumerable<PatientHomeMedication>? patientHomeMedications = null
            )
        {
            //Added newOrders, existingOrders, and cartOrders as optional parameters.
            //We need them to speed up the cart checkout process, specifically where
            //we recalculate interactions and reactions for all orders and cart orders.
            //Winston Murdock, 03/14/2022.

            var codeShareSiteMedicationUnit = orderRepository.GetCodeShareSites(siteId)
                .FirstOrDefault(c =>
                    c.Entity == OrderRepository.CodeShareEntity.MedicationUnit)?
                .SharedSiteId;
            
            var checkMedications = new List<MedicationModel>();
            checkMedications.AddRange(medicationModelItems);

            //medicationModelItems is a list, with one item, of one of the patient's orders or cart orders.
            //We end up in this method once for each patient order (and cart order).

            //If we have a list of new orders, existing orders, and cart orders,
            //then use those lists rather than querying the database to get them.
            //This should make this call noticably faster and save trips to the DB.
            //
            //1) If there is anything in newOrders...
            //  A) If the item is in newOrders, then we need to check against newOrders and existingOrders.
            //      i) Add everything in newOrders to checkMedications.
            //      ii) If there is anything in existingOrders add everything in it to checkMedications.
            //      iii) Else there is not anything in existingOrders, grab them from the DB and then add them to checkMedications.
            //  B) Else the item is an existing order, and we only need to compare it against newOrders.
            //      i) Add everything in newOrders to checkMedications.
            //2) Else, there is not anything in newOrders...
            //  A) If there is anything in existingOrders add everything in it to checkMedications.
            //  B) Else there is not anything in existingOrders, grab them from the DB and then add them to checkMedications.
            //3) If there is anything in cartOrders, add everything in cartOrders to checkMedications.
            //4) Else, there is nothing in cartOrders and we pull the patient's cart orders from the DB and add them to checkMedications.
            //Winston Murdock, 03/14/2022.


            //If the list of existing orders comes in null,
            //then this patient actually doesn't have any orders.
            //Or I already filtered out any orders for the same medication
            //as the one we are checking against right now.
            //I've already touched every place that calls this guy.
            //So I don't want to go get the orders from the DB any more.
            //When the patient has no orders, and I put a GI Cocktil in their cart,
            //Then I am seeing that the GI Cocktail interacts with itself.
            //Yes, it has two meds inside it that do interact with each other.
            //But I already removed any GI Cocktails from the existing order list
            //so that we don't show the GI Cocktail interacting to itself.
            //In that case, we were grabbing the patient's orders/cart orders from the DB
            //and ended up comparing the GI Cocktail to itself, which we don't want to do.
            //Winston Murdock, 09/06/2022.  PC-27249

            //1
            //If newOrders is not null.
            if (!(newOrders == null))
            {
                //1
                //If newOrders has at least one item in it.
                if (newOrders.Any())
                {
                    //We have new orders.

                    //1-A If the item is in newOrders...
                    //See if the medicationId for the one item (is it only one?) is in newOrders.
                    if (newOrders.Any(x => x.MedicationId == medicationModelItems[0].MedicationId))
                    {
                        //1-a-i
                        //This medication id is in new orders.
                        //We need to add everything from both newOrders and existingOrders to
                        //checkMedications.
                        checkMedications.AddRange(newOrders
                            .ToList()
                            .Select(order => OrderMapper.MapPatientOrderToModel(order, userId, siteId, codeShareSiteMedicationUnit))
                        );

                        //1-A-ii
                        //Just to make sure we do have something in existingOrders.
                        //We always should, but I'd rather not assume.
                        if (!(existingOrders == null))
                        {
                            //1-A-ii
                            if (existingOrders.Any())
                            {
                                //Add the existing orders to checkMedications
                                //Except the medication we're checking.
                                checkMedications.AddRange(existingOrders
                                    .ToList()
                                    .Select(order => OrderMapper.MapPatientOrderToModel(order, userId, siteId, codeShareSiteMedicationUnit))
                                );
                            }
                            //If the list of existing orders comes in null,
                            //then this patient actually doesn't have any orders.
                            //Or I already filtered out any orders for the same medication
                            //as the one we are checking against right now.
                            //I've already touched every place that calls this guy.
                            //So I don't want to go get the orders from the DB any more.
                            //When the patient has no orders, and I put a GI Cocktil in their cart,
                            //Then I am seeing that the GI Cocktail interacts with itself.
                            //Yes, it has two meds inside it that do interact with each other.
                            //But I already removed any GI Cocktails from the existing order list
                            //so that we don't show the GI Cocktail interacting to itself.
                            //In that case, we were grabbing the patient's orders/cart orders from the DB
                            //and ended up comparing the GI Cocktail to itself, which we don't want to do.
                            //Winston Murdock, 09/06/2022.  PC-27249
                            //else
                            //{
                            //    //1=A-iii
                            //    //Nothing in existing orders.
                            //    //To ensure that we check against those orders, go out to the DB.

                            //    // When checking against signed orders, make sure we avoid orders that are canceled or deleted.
                            //    checkMedications.AddRange(orderRepository
                            //        .GetPatientOrders(order =>
                            //            order.PatientId == patientId &&
                            //            order.OrderStatus != OrderStatus.Cancelled.ToString() &&
                            //            order.OrderStatus != OrderStatus.Deleted.ToString()
                            //        )
                            //        .ToList()
                            //        .Select(order => OrderMapper.MapPatientOrderToModel(order, userId, siteId, codeShareSiteMedicationUnit))
                            //    );
                            //} //end if
                        }
                        //If the list of existing orders comes in null,
                        //then this patient actually doesn't have any orders.
                        //Or I already filtered out any orders for the same medication
                        //as the one we are checking against right now.
                        //I've already touched every place that calls this guy.
                        //So I don't want to go get the orders from the DB any more.
                        //When the patient has no orders, and I put a GI Cocktil in their cart,
                        //Then I am seeing that the GI Cocktail interacts with itself.
                        //Yes, it has two meds inside it that do interact with each other.
                        //But I already removed any GI Cocktails from the existing order list
                        //so that we don't show the GI Cocktail interacting to itself.
                        //In that case, we were grabbing the patient's orders/cart orders from the DB
                        //and ended up comparing the GI Cocktail to itself, which we don't want to do.
                        //Winston Murdock, 09/06/2022.  PC-27249
                        //else
                        //{
                        //    //1-A-iii
                        //    //Nothing in existing orders.
                        //    //To ensure that we check against those orders, go out to the DB.

                        //    // When checking against signed orders, make sure we avoid orders that are canceled or deleted.
                        //    checkMedications.AddRange(orderRepository
                        //        .GetPatientOrders(order =>
                        //            order.PatientId == patientId &&
                        //            order.OrderStatus != OrderStatus.Cancelled.ToString() &&
                        //            order.OrderStatus != OrderStatus.Deleted.ToString()
                        //        )
                        //        .ToList()
                        //        .Select(order => OrderMapper.MapPatientOrderToModel(order, userId, siteId, codeShareSiteMedicationUnit))
                        //    );
                        //} //end if
                    }
                    else
                    {
                        //1-B
                        //The one? entry in items is not in the list of new orders.
                        //We only need/want to check it against the new orders and not existing orders.
                        checkMedications.AddRange(newOrders
                            .ToList()
                            .Select(order => OrderMapper.MapPatientOrderToModel(order, userId, siteId, codeShareSiteMedicationUnit))
                        );
                    } //end if
                }
            }
            else
            {
                //2
                //newOrders is empty.
                //I.e. this was called by something other than checking out the chart.
                //Use the existing logic to populate checkMedications.

                //Instead of immediately grabbing the list of orders from the DB, check the parameter first.
                //If we already have the list of existing orders, then use that and don't query the DB.
                //If it has zero entries or is null, then do go grab the orders from the DB.
                //Winston Murdock, 05/03/2022.  PC-27193
                // When checking against signed orders, make sure we avoid orders that are canceled or deleted.
                //checkMedications.AddRange(orderRepository
                //    .GetPatientOrders(order =>
                //        order.PatientId == patientId &&
                //        order.OrderStatus != OrderStatus.Cancelled.ToString() &&
                //        order.OrderStatus != OrderStatus.Deleted.ToString()
                //    )
                //    .ToList()
                //    .Select(order => OrderMapper.MapPatientOrderToModel(order, userId, siteId, codeShareSiteMedicationUnit))
                //);

                if (!(existingOrders == null))
                {
                    //2-A
                    //Do we have existing orders?
                    if (existingOrders.Any())
                    {
                        //Add the existing orders to checkMedications
                        //Except the medication we're checking.
                        checkMedications.AddRange(existingOrders
                            .ToList()
                            .Select(order => OrderMapper.MapPatientOrderToModel(order, userId, siteId, codeShareSiteMedicationUnit))
                        );
                    }
                    //If the list of existing orders comes in null,
                    //then this patient actually doesn't have any orders.
                    //Or I already filtered out any orders for the same medication
                    //as the one we are checking against right now.
                    //I've already touched every place that calls this guy.
                    //So I don't want to go get the orders from the DB any more.
                    //When the patient has no orders, and I put a GI Cocktil in their cart,
                    //Then I am seeing that the GI Cocktail interacts with itself.
                    //Yes, it has two meds inside it that do interact with each other.
                    //But I already removed any GI Cocktails from the existing order list
                    //so that we don't show the GI Cocktail interacting to itself.
                    //In that case, we were grabbing the patient's orders/cart orders from the DB
                    //and ended up comparing the GI Cocktail to itself, which we don't want to do.
                    //Winston Murdock, 09/06/2022.  PC-27249
                    //else
                    //{
                    //    //2-B
                    //    //Nothing in existing orders.
                    //    //To ensure that we check against those orders, go out to the DB.

                    //    // When checking against signed orders, make sure we avoid orders that are canceled or deleted.
                    //    checkMedications.AddRange(orderRepository
                    //        .GetPatientOrders(order =>
                    //            order.PatientId == patientId &&
                    //            order.OrderStatus != OrderStatus.Cancelled.ToString() &&
                    //            order.OrderStatus != OrderStatus.Deleted.ToString()
                    //        )
                    //        .ToList()
                    //        .Select(order => OrderMapper.MapPatientOrderToModel(order, userId, siteId, codeShareSiteMedicationUnit))
                    //    );
                    //} //end if
                }
                //If the list of existing orders comes in null,
                //then this patient actually doesn't have any orders.
                //Or I already filtered out any orders for the same medication
                //as the one we are checking against right now.
                //I've already touched every place that calls this guy.
                //So I don't want to go get the orders from the DB any more.
                //When the patient has no orders, and I put a GI Cocktil in their cart,
                //Then I am seeing that the GI Cocktail interacts with itself.
                //Yes, it has two meds inside it that do interact with each other.
                //But I already removed any GI Cocktails from the existing order list
                //so that we don't show the GI Cocktail interacting to itself.
                //In that case, we were grabbing the patient's orders/cart orders from the DB
                //and ended up comparing the GI Cocktail to itself, which we don't want to do.
                //Winston Murdock, 09/06/2022.  PC-27249
                //else
                //{
                //    //2-B
                //    //Nothing in existing orders.
                //    //To ensure that we check against those orders, go out to the DB.

                //    // When checking against signed orders, make sure we avoid orders that are canceled or deleted.
                //    checkMedications.AddRange(orderRepository
                //        .GetPatientOrders(order =>
                //            order.PatientId == patientId &&
                //            order.OrderStatus != OrderStatus.Cancelled.ToString() &&
                //            order.OrderStatus != OrderStatus.Deleted.ToString()
                //        )
                //        .ToList()
                //        .Select(order => OrderMapper.MapPatientOrderToModel(order, userId, siteId, codeShareSiteMedicationUnit))
                //    );
                //} //end if

            } //end if

            //3
            //If we have a list of cart orders, then use that list.
            //Else, use the existing behavior of querying the DB for that list.
            if (!(cartOrders == null))
            {
                //3
                if (cartOrders.Any())
                {
                    //3
                    //Now we need to add the patient cart orders.
                    if (checkAgainstCartOrders)
                    {
                        // When checking against cart orders, be sure to only include the orders in this user's cart.
                        checkMedications.AddRange(cartOrders
                            .ToList()
                            .Select(order => CartOrderMapper.MapPatientCartOrderToModel(order, userId, siteId, codeShareSiteMedicationUnit))
                        );
                    } //end if
                }
                //If the list of cart orders comes in null,
                //then this patient actually doesn't have any cart orders.
                //Or I already filtered out any orders for the same medication
                //as the one we are checking against right now.
                //I've already touched every place that calls this guy.
                //So I don't want to go get the orders from the DB any more.
                //When the patient has no orders, and I put a GI Cocktil in their cart,
                //Then I am seeing that the GI Cocktail interacts with itself.
                //Yes, it has two meds inside it that do interact with each other.
                //But I already removed any GI Cocktails from the existing order list
                //so that we don't show the GI Cocktail interacting to itself.
                //In that case, we were grabbing the patient's orders/cart orders from the DB
                //and ended up comparing the GI Cocktail to itself, which we don't want to do.
                //Winston Murdock, 09/06/2022.  PC-27249
                //else
                //{
                //    //4
                //    //Nothing in the cart orders list.
                //    //Use existing behavior.
                //    //Now we need to add the patient cart orders.
                //    if (checkAgainstCartOrders)
                //    {
                //        // When checking against cart orders, be sure to only include the orders in this user's cart.
                //        checkMedications.AddRange(cartOrderRepository
                //            .GetPatientCartOrders(order =>
                //                order.PatientId == patientId &&
                //                order.UserId == userId
                //            )
                //            .ToList()
                //            .Select(order => CartOrderMapper.MapPatientCartOrderToModel(order, userId, siteId, codeShareSiteMedicationUnit))
                //        );
                //    }
                //} //end if
            }
            //If the list of existing orders comes in null,
            //then this patient actually doesn't have any orders.
            //Or I already filtered out any orders for the same medication
            //as the one we are checking against right now.
            //I've already touched every place that calls this guy.
            //So I don't want to go get the orders from the DB any more.
            //When the patient has no orders, and I put a GI Cocktil in their cart,
            //Then I am seeing that the GI Cocktail interacts with itself.
            //Yes, it has two meds inside it that do interact with each other.
            //But I already removed any GI Cocktails from the existing order list
            //so that we don't show the GI Cocktail interacting to itself.
            //In that case, we were grabbing the patient's orders/cart orders from the DB
            //and ended up comparing the GI Cocktail to itself, which we don't want to do.
            //Winston Murdock, 09/06/2022.  PC-27249
            //else
            //{
            //    //4
            //    //Nohing in the cart orders list.
            //    //Use existing behavior.
            //    //Now we need to add the patient cart orders.
            //    if (checkAgainstCartOrders)
            //    {
            //        // When checking against cart orders, be sure to only include the orders in this user's cart.
            //        checkMedications.AddRange(cartOrderRepository
            //            .GetPatientCartOrders(order =>
            //                order.PatientId == patientId &&
            //                order.UserId == userId
            //            )
            //            .ToList()
            //            .Select(order => CartOrderMapper.MapPatientCartOrderToModel(order, userId, siteId, codeShareSiteMedicationUnit))
            //        );
            //    }
            //} //end if

            //Existing code from before PC-27067.
            //// When checking against signed orders, make sure we avoid orders that are canceled or deleted.
            //checkMedications.AddRange(orderRepository
            //    .GetPatientOrders(order => 
            //        order.PatientId == patientId && 
            //        order.OrderStatus != OrderStatus.Cancelled.ToString() && 
            //        order.OrderStatus != OrderStatus.Deleted.ToString()
            //    )
            //    .ToList()
            //    .Select(order => OrderMapper.MapPatientOrderToModel(order, userId, siteId, codeShareSiteMedicationUnit))
            //);

            //if (checkAgainstCartOrders)
            //{
            //    // When checking against cart orders, be sure to only include the orders in this user's cart.
            //    checkMedications.AddRange(cartOrderRepository
            //        .GetPatientCartOrders(order => 
            //            order.PatientId == patientId && 
            //            order.UserId == userId
            //        )
            //        .ToList()
            //        .Select(order => CartOrderMapper.MapPatientCartOrderToModel(order, userId, siteId, codeShareSiteMedicationUnit))
            //    );
            //}

            //this call takes the medications we have in checkMedications
            //and puts them into a list of DrugDb.ReactionsCheckResult objects.
            //There is no interaction/reaction checking in it.
            var reactionsCheckResult = GetReactionsCheckResult(
                patientRepository,
                homeMedicationRepository,
                optionRepository,
                siteId,
                checkMedications,
                null,
                patientId,
                patientAllergies,
                patientHomeMedications
            );

            //We have a list of DrugDb.ReactionsCheckResult objects here.
            //I think this guy actually does the interaction checking.
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
        private static DrugDb.ReactionsCheckResult GetReactionsCheckResult
        (
            IPatientRepository patientRepository,
            IHomeMedicationRepository homeMedicationRepository,
            IOptionRepository optionRepository,
            int siteId,
            List<MedicationModel> checkMedications,
            Dictionary<string, string> checklist = null,
            long patientId = 0,
            IEnumerable<PatientAllergy>? patientAllergies = null,
            IEnumerable<PatientHomeMedication>? patientHomeMedications = null
        )
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

                //This returns the PcRoutedGenId and medication name in a dictionary.
                checklist ??= GetChecklist(checkMedications);

                //Pass the dictionary from above and the MedicationId here.
                reactionsCheckResult = drugDb.CheckReactions(siteId, patientId, checklist, drugDbVendor, patientAllergies, patientHomeMedications);
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

                            //This is the spot we're having an issue.
                            //When drugTrimmed has more than one medication in it (abc, def)
                            //we check for "abc, def" in the array rather than for "abc" or "def."
                            //I have the existing behavior first and then I do my new behavior
                            //(to handle this case) in the else below.
                            //Winston Murdock, 02/24/2022.  PC-27029
                            if (reactionsCheckResult.Allergies[dnum].ContainsKey(drugTrimmed))
                            {
                                var compKeys = reactionsCheckResult.Allergies[dnum][drugTrimmed].Keys.ToList();
                                compKeys.Sort();

                                //Keep track of which meds we've already added.
                                //Winston Murdock, 02/25/2022.  PC-27029
                                List<string> sAdded = new List<string>();

                                foreach (var compKey in compKeys)
                                {
                                    //If this drug has already been added to the reactions list, don't add it a second time.
                                    if (!sAdded.Contains(drug.ToString()))
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
                                        
                                        //Add this drug to the list of drugs we have added to the
                                        //reaction list so that we don't add it multiple times.
                                        sAdded.Add(drug.ToString());
                                    } //end if (sAdded)
                                }
                            }
                            else
                            {
                                //This could be that there is no reaction here.
                                //Or it could be that we have multiple items in the drugTrimmed variable (Tramdol, Darvocet).
                                //If it's the latter, we'll need to check them both one by one.
                                //See if we have a "', '" in the string (which we're using as the separator between drugs).
                                if (drugTrimmed.IndexOf("', '") > 0)
                                {
                                    //Split it into a list.
                                    var drugTrimmedList = drugTrimmed.Split("', '");

                                    //Loop through the array, if we have one.
                                    if (drugTrimmedList.Count() > 0)
                                    {
                                        foreach (var oneDrug in drugTrimmedList)
                                        {
                                            string tempDrug = oneDrug;

                                            //For each drug name in the list...
                                            if (reactionsCheckResult.Allergies[dnum].ContainsKey(tempDrug))
                                            {
                                                var compKeys = reactionsCheckResult.Allergies[dnum][tempDrug].Keys.ToList();
                                                compKeys.Sort();

                                                //Keep track of which meds we've already added.
                                                //Winston Murdock, 02/25/2022.  PC-27029
                                                List<string> sAdded = new List<string>();

                                                foreach (var compKey in compKeys)
                                                {
                                                    //If this drug has already been added to the reactions list, don't add it a second time.
                                                    if (!sAdded.Contains(tempDrug))
                                                    {
                                                        var comp = (Dictionary<string, object>)reactionsCheckResult.Allergies[dnum][tempDrug][compKey];
                                                        var rsel = new Dictionary<string, object>(sel);

                                                        //Set dname2 to the individual drug, not the original string
                                                        //of "drug one', 'drug two".
                                                        //This is what the UI is actually showing on the individual reaction rows.
                                                        //This will prevent the name from being the same on each row.
                                                        //Winston Murdock, 02/25/2022.  PC=27029
                                                        rsel["dname2"] = tempDrug;

                                                        rsel["dnum"] = d;
                                                        //rsel["drug"] = drug;
                                                        rsel["drug"] = tempDrug;
                                                        rsel["type"] = "alg";
                                                        rsel["interaction"] = sev + " REACTION";
                                                        rsel["SourceTable"] = comp["SourceTable"];
                                                        rsel["SourceTableId"] = comp["SourceTableId"];
                                                        rsel["Severity"] = comp["Severity"];
                                                        accReacts.Add(rsel);

                                                        //Add this drug to the list of drugs we have added to the
                                                        //reaction list so that we don't add it multiple times.
                                                        sAdded.Add(tempDrug);
                                                    } //end if (sAdded)
                                                } //end foreeach
                                            } //end if (is this drug name in the drug list?)
                                        } //end foreach
                                    } //end if
                                } //end if
                            } //end if (is drugTrimmed in the drug list?)
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
        }  //end GetCheckList
    }
}