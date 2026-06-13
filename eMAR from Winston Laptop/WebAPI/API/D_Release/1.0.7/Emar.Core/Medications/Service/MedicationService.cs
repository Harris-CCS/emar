using System;
using System.Collections.Generic;
using System.Linq;
using Emar.Core.Carts.Repository;
using Emar.Core.HomeMedications.Repository;
using Emar.Core.Medications.Model;
using Emar.Core.Medications.Model.Mappings;
using Emar.Core.Medications.Repository;
using Emar.Core.Orders.Model;
using Emar.Core.Orders.Model.Mappings;
using Emar.Core.Orders.Repository;
using Emar.Core.Orders.Service;
using Emar.Core.Patients.Repository;
using Emar.Data.Entities;

namespace Emar.Core.Medications.Service
{
    public class MedicationService : IMedicationService
    {
        private readonly IMedicationRepository _medicationRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly IOrderService _orderService;
        private readonly IPatientRepository _patientRepository;
        private readonly ICartOrderRepository _cartOrderRepository;
        private readonly IHomeMedicationRepository _homeMedicationRepository;

        public MedicationService(IMedicationRepository medicationRepository, IOrderRepository orderRepository, IOrderService orderService, IPatientRepository patientRepository, ICartOrderRepository cartOrderRepository, IHomeMedicationRepository homeMedicationRepository)
        {
            _medicationRepository = medicationRepository;
            _orderRepository = orderRepository;
            _orderService = orderService;
            _patientRepository = patientRepository;
            _cartOrderRepository = cartOrderRepository;
            _homeMedicationRepository = homeMedicationRepository;
        }

        public IEnumerable<BrandNameSearchDto> GetMedsByBrandName(int siteId, string search, int userId, EmarOrderType searchType,
            string deptCode, string schedulerDataRetrieveBase, string? groupLink = null)
        {
            var records = _medicationRepository.GetMedsByBrandName(siteId, search, userId, searchType, deptCode);

            IEnumerable<BrandNameSearchDto> recordsMapped;

            //Map gropup items differently from other items.
            if (searchType == EmarOrderType.GroupRememberedOrder)
            {
                recordsMapped = records.Select(r => MedicationMapper.MapGroupBrandName(r, groupLink));
            }
            else
            {
                //Trying another method of sorting.
                //Put the result of the mapping call into a variable rather than just returning it.
                //Winston Murdock, 03/11/2021.  EMAR-828.
                recordsMapped = records.Select(r => MedicationMapper.MapBrandName(r, schedulerDataRetrieveBase));
            } //end if

            //Now use LINQ to select from recordsMapped and then order the return.
            //The lambda sorting that I was doing worked with only two sorts specified.
            //But it went haywire when I added a third sort level.
            //https://stackoverflow.com/a/3831749
            var retValue = from row in recordsMapped
                                orderby row.IsBrandNameMatch descending, row.SearchPos, row.BrandName
                                select row;

            //Lastly, return the sorted list of DTO objects.
            return retValue;
        }

        public IEnumerable<AntimicrobialIndicationDto> GetIndicationsBySite(int siteId)
        {
            //Get the code share site id.
            var codeShareSites = _orderRepository.GetCodeShareSites(siteId);

            //Now get the site id for antimicrobial indications.
            var sharedSiteId = codeShareSites
            .FirstOrDefault(c =>
                c.Entity == OrderRepository.CodeShareEntity.AntimicrobialIndicationReason)?
            .SharedSiteId;

            //IF there is no shared site id, use the parameter site id.
            if (sharedSiteId == null)
            {
                sharedSiteId = siteId;
            }
            
            //Return the list.
            return _medicationRepository.GetIndicationsBySite(sharedSiteId.Value).Select(MedicationMapper.MapAntimicrobial);
        }

        public Dictionary<string, bool> GetSearchDropdownList(int siteId)
        {
            return _medicationRepository.GetSearchDropdownList(siteId);
        } //end GetSearchDropdownList

        public MedicationDto GetMedication(int medicationId)
        {
            var medication = _medicationRepository.GetMedication(medicationId);
            var codeShareSiteMedicationUnit = _orderRepository.GetCodeShareSites(medication.SiteId)
                .FirstOrDefault(c =>
                    c.Entity == OrderRepository.CodeShareEntity.MedicationUnit)?
                .SharedSiteId;

            return MedicationMapper.MapMedication(medication, codeShareSiteMedicationUnit);
        }

        public IEnumerable<MedicationInteractionReaction> GetInteractionsReactions(int userId, long patientId, EmarOrderType itemType, int itemId)
        {
            int? codeShareSiteMedicationUnit = null;
            var interactionsReactions = new List<MedicationInteractionReaction>();

            object listItem = null;

            int medicationId;

            switch (itemType)
            {
                case (EmarOrderType.UserQuickListItem):
                    {
                        var quickItem = _orderRepository.GetUserQuickListItem(itemId);
                        codeShareSiteMedicationUnit = _orderRepository.GetCodeShareSites(quickItem.SiteId)
                            .FirstOrDefault(c =>
                                c.Entity == OrderRepository.CodeShareEntity.MedicationUnit)?
                            .SharedSiteId;
                        listItem = quickItem;

                        medicationId = quickItem.MedicationId;

                        break;
                    }
                case (EmarOrderType.DepartmentPreferredListItem):
                    {
                        var deptItem = _orderRepository.GetDepartmentPreferredItem(itemId);
                        codeShareSiteMedicationUnit = _orderRepository.GetCodeShareSites(deptItem.SiteId)
                            .FirstOrDefault(c =>
                                c.Entity == OrderRepository.CodeShareEntity.MedicationUnit)?
                            .SharedSiteId;
                        listItem = deptItem;

                        medicationId = deptItem.MedicationId;

                        break;
                    }
                case (EmarOrderType.GroupRememberedOrder):
                    {
                        var groupItem = _orderRepository.GetGroupRememberedOrderItem(itemId);
                        codeShareSiteMedicationUnit = _orderRepository.GetCodeShareSites(groupItem.SiteId)
                            .FirstOrDefault(c =>
                                c.Entity == OrderRepository.CodeShareEntity.MedicationUnit)?
                            .SharedSiteId;
                        listItem = groupItem;

                        medicationId = groupItem.MedicationId;

                        break;
                    }
                case (EmarOrderType.MedicationItem):
                    {
                        listItem = _medicationRepository.GetMedication(itemId);

                        medicationId = itemId;

                        break;
                    }
                //Winston Murdock, 01/06/2021.
                case (EmarOrderType.PatientOrder):
                    {
                        //I changed the GetOrders call (that GetOrder calls) to get the
                        //FdbBrandName child entity for any MedicationDetails.
                        //This lets reaction checking work when repeating an order.
                        //Winston Murdock, 03/01/2022.  PC-27061
                        var patientOrder = _orderRepository.GetOrder(itemId);
                        var patient = _patientRepository.GetPatient(patientOrder.PatientId, null, false);

                        codeShareSiteMedicationUnit = _orderRepository.GetCodeShareSites(patient.SiteId)
                            .FirstOrDefault(c =>
                                c.Entity == OrderRepository.CodeShareEntity.MedicationUnit)?
                            .SharedSiteId;
                        listItem = patientOrder;

                        medicationId = patientOrder.MedicationId;

                        break;
                    }


                //If the type is PatientCartOrder, then get the PatientCartOrder.
                //I don't know why we weren't already doing this.
                //Without this, we weren't doing any interaction/reaction
                //checking on the composer screen when editing a cart order.
                //Winston Murdock, 03/01/2022.  PC-27061
                case (EmarOrderType.PatientCartOrder):
                    {
                        var cartOrder = _cartOrderRepository.GetOrder(itemId);
                        var patient = _patientRepository.GetPatient(cartOrder.PatientId, null, false);

                        //Need to get the FdbBrandName for the medication details for any cart orders.
                        //I tried doing this in the cart order repository method we were already in.
                        //But I got an error about having two data readers open at the same time.
                        //I found a workaround by changing the connection string to allow that.
                        //But I'd rather not make a global change like that.
                        //So I'll make a new repository method and call it for each medicaiton detail.
                        foreach (var md in cartOrder.Medication.MedicationDetails)
                        {
                            md.FdbBrandName = _cartOrderRepository.GetFdbBrandNameForCartOrder(md.Id, patient.SiteId);
                        } //end foreach.

                        codeShareSiteMedicationUnit = _orderRepository.GetCodeShareSites(patient.SiteId)
                            .FirstOrDefault(c =>
                                c.Entity == OrderRepository.CodeShareEntity.MedicationUnit)?
                            .SharedSiteId;
                        listItem = cartOrder;

                        medicationId = cartOrder.MedicationId;

                        break;
                    }
                default:
                    {
                        medicationId = 0;
                        break;
                    }
            } //end switch (itemType)

            if (listItem != null)
            {
                //Get the lists of orders and cart orders so that we can check interactions against them.
                //I changed the method down below to assume they are passed in via parameters and not to
                //pull them from the DB.  If those parameters are null, then the patient just doesn't have
                //any orders or cart orders.
                var orders = _orderRepository.GetPatientOrders(order => order.PatientId == patientId);

                orders = orders.Where
                    (x =>
                        x.OrderStatus != OrderStatus.Cancelled.ToString() &&
                        x.OrderStatus != OrderStatus.Deleted.ToString()
                    );

                var cartOrders = _cartOrderRepository.GetPatientCartOrders(order => order.PatientId == patientId && order.UserId == userId);

                IEnumerable<PatientCartOrder> cartOrdersMinusThisOne;
                IEnumerable<PatientOrder> patientOrdersMinusThisOne;

                //If we don't have a medicationId, then just use the lists.  Else filter them to exclude this medication.
                if (medicationId == 0)
                {
                    cartOrdersMinusThisOne = cartOrders;
                    patientOrdersMinusThisOne = orders;
                }
                else
                {
                    //Remove cart order for this medication from the list of cart orders so that we don't do interaction checking against itself.
                    cartOrdersMinusThisOne = cartOrders.Where(co => co.MedicationId != medicationId);

                    //Also remove any patient orders with the same medication id as this one.
                    //That way we don't check a GI Cocktail against itself.
                    patientOrdersMinusThisOne = orders.Where(o => o.MedicationId != medicationId);

                } //end if

                //Get the patient's allergies and home medications here.
                //Then pass them along to the methods farther down the call stack.
                //This prevents us from pulling these from the DB once for each order (or detail in a combo med order).
                //Winston Murdock, 09/27/2022.  PC-27110
                IEnumerable<PatientAllergy>? patientAllergies = null;
                IEnumerable<PatientHomeMedication>? patientHomeMedications = null;
                patientAllergies = _patientRepository.GetAllergiesByPatientId(patientId, a => a.IsActive && (a.ActionStatus == "C" || a.ActionStatus == "U"));
                patientHomeMedications = _homeMedicationRepository.GetPatientHomeMedications(a => a.PatientId == patientId && a.IsActive);

                interactionsReactions = _orderService
                    .CheckInteractionsReactions(
                        in userId,
                        new List<MedicationModel>
                        {
                            OrderMapper.MapOrderItemToModel(itemType, listItem, patientId, userId, codeShareSiteMedicationUnit)
                        },
                        patientId,
                        true,
                        null,
                        patientOrdersMinusThisOne,
                        cartOrdersMinusThisOne,
                        patientAllergies,
                        patientHomeMedications)
                    .ToList();
            }

            return interactionsReactions;
        }

        public List<MedicationInteractionReaction> GetGroupOrderInteractionsReactions(int userId, long patientId, int itemId)
        {
            //Calculate the interactions and reactions for a specific group order.
            //If it's a combo med, then we'll need to go to the MedicationDetails children,
            //get the actual Medication for them, and then call CheckInteractionReactions for each one of them.


            int? codeShareSiteMedicationUnit = null;
            List<MedicationInteractionReaction> interactionsReactions = new List<MedicationInteractionReaction>();

            var groupItem = _orderRepository.GetGroupRememberedOrderItem(itemId);
            codeShareSiteMedicationUnit = _orderRepository.GetCodeShareSites(groupItem.SiteId)
                .FirstOrDefault(c =>
                    c.Entity == OrderRepository.CodeShareEntity.MedicationUnit)?
                .SharedSiteId;

            //Get all of the orders for this patient.
            //Then we'll pass this list down rather than retrieving it each time.
            var orders = _orderRepository.GetPatientOrders(order => order.PatientId == patientId);

            orders = orders.Where
                (x =>
                    x.OrderStatus != OrderStatus.Cancelled.ToString() &&
                    x.OrderStatus != OrderStatus.Deleted.ToString()
                );

            //Remove any orders that have the same medication id as this medication.
            //That way we don't check a combo med's interactions against another
            //instance of that combo med.
            //Winston Murdock, 09/06/2022.  PC-27429
            orders = orders.Where(o => o.MedicationId != groupItem.MedicationId);

            //Get all of the cartOrders for this patient / user(we are recalculating for each of them).
            var cartOrders = _cartOrderRepository.GetPatientCartOrders(order => order.PatientId == patientId && order.UserId == userId);

            //Remove any cart orders that have the same medication id.
            //This way we don't check a combo med against another instance of itself.
            var cartOrdersMinusThisOne = cartOrders.Where(co => co.MedicationId != groupItem.MedicationId);

            //Get the patient's allergies and home medications here.
            //Then pass them along to the methods farther down the call stack.
            //This prevents us from pulling these from the DB once for each order (or detail in a combo med order).
            //Winston Murdock, 09/27/2022.  PC-27110
            IEnumerable<PatientAllergy>? patientAllergies = null;
            IEnumerable<PatientHomeMedication>? patientHomeMedications = null;
            patientAllergies = _patientRepository.GetAllergiesByPatientId(patientId, a => a.IsActive && (a.ActionStatus == "C" || a.ActionStatus == "U"));
            patientHomeMedications = _homeMedicationRepository.GetPatientHomeMedications(a => a.PatientId == patientId && a.IsActive);

            if (groupItem.Medication.DrugId == "COMBO")
            {
                //Combo med.
                //Need to get the actual Medication for each Medication Detail.
                //MedicationDetail.MedicationId points to the combo med.
                //So we'll need to find the matching row in medications
                //where Medication.DrugId != "COMBO"
                //and where we match on drug_id.
                //This will only work for FDB, but that's all we've got right now.
                List<Medication> comboMeds = new List<Medication>();

                foreach (var medDetail in groupItem.Medication.MedicationDetails)
                {
                    //Get the actual Medication for this one (not the combo med).
                    //It will match on drug_id.

                    //This might be a bit faster.
                    //Rather than going to the DB to get the medication's ID and then
                    //getting the medication just get the medication.
                    //var medicationId = _medicationRepository.GetMedicationByDrugId(medDetail.DrugId).Id;
                    //var medication = _medicationRepository.GetMedication(medicationId);
                    var medication = _medicationRepository.GetMedicationByDrugId(medDetail.DrugId);

                    //Go get the interactions and reactions for this one medication and add them to the list.

                    //Not sure why, but this was passing in false for "check against cart orders."
                    //After consultation with Romel, I set this to true.
                    //This probably happened because wherever I copied this
                    //from was not checking against cart orders.
                    //Winston Murdock, 09/07/2022.  PC-27429
                    interactionsReactions.AddRange(_orderService
                    .CheckInteractionsReactions(
                        in userId,
                        new List<MedicationModel>
                        {
                            OrderMapper.MapOrderItemToModel(EmarOrderType.MedicationItem, medication, patientId, userId, codeShareSiteMedicationUnit)
                        },
                        patientId,
                        true,
                        null,
                        orders,
                        cartOrdersMinusThisOne,
                        patientAllergies,
                        patientHomeMedications)
                    .ToList());
                } //end foreach.

                //At this point, we've got one entry for each of the medications in the combo med.
                //Lets combine them into one item for the combo med.
                //And with the interactions and reactions for each of them.
                //Write a helper function to accomplish this.
                //Winston Murdock, 04/27/2022.
                interactionsReactions = CompressComboMedInteractionsToOneEntry(groupItem, userId, interactionsReactions);
            }
            else
            {
                //Not a combo med.
                //Handle normally.

                interactionsReactions = _orderService
                    .CheckInteractionsReactions(
                        in userId,
                        new List<MedicationModel>
                        {
                            OrderMapper.MapOrderItemToModel(EmarOrderType.GroupRememberedOrder, groupItem, patientId, userId, codeShareSiteMedicationUnit)
                        },
                        patientId,
                        true,
                        null,
                        orders,
                        cartOrdersMinusThisOne,
                        patientAllergies,
                        patientHomeMedications)
                    .ToList();
            }

            return interactionsReactions;
        }

        public List<MedicationInteractionReaction> CompressComboMedInteractionsToOneEntry(GroupListItem groupItem, int userId, List<MedicationInteractionReaction> currentList)
        {
            List<MedicationInteractionReaction> ret = new List<MedicationInteractionReaction>();

            MedicationInteractionReaction mir = new MedicationInteractionReaction
            {
                SiteId = groupItem.SiteId,
                UserId = userId,
                SourceTable = "group_list_items",
                SourceTableId = groupItem.Id,
                Type = EmarOrderType.GroupRememberedOrder,
                BrandName = groupItem.GroupName,
                ActiveName = groupItem.GroupName,
                ActiveId = groupItem.Id.ToString() //We don't have an FDB id here.  So possibly the group's ID will work?
            };

            //Loop through the interactions and reactions so that we have only one entry for them, not multiple.
            foreach (var item in currentList)
            {
                if (item.Interactions.Any())
                {
                    mir.Interactions.AddRange(item.Interactions);
                } //end if

                if (item.Reactions.Any())
                {
                    mir.Reactions.AddRange(item.Reactions);
                } //end if
            } //end foreach

            ret.Add(mir);

            return ret;
        } //end CompressComboMedInteractionsToOneEntry
    }
}