using System;
using System.Collections.Generic;
using System.Linq;
using Emar.Core.Medications.Model;
using Emar.Core.Medications.Model.Mappings;
using Emar.Core.Medications.Repository;
using Emar.Core.Orders.Model.Mappings;
using Emar.Core.Orders.Repository;
using Emar.Core.Orders.Service;
using Emar.Core.Patients.Repository;

namespace Emar.Core.Medications.Service
{
    public class MedicationService : IMedicationService
    {
        private readonly IMedicationRepository _medicationRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly IOrderService _orderService;
        private readonly IPatientRepository _patientRepository;

        public MedicationService(IMedicationRepository medicationRepository, IOrderRepository orderRepository, IOrderService orderService, IPatientRepository patientRepository)
        {
            _medicationRepository = medicationRepository;
            _orderRepository = orderRepository;
            _orderService = orderService;
            _patientRepository = patientRepository;
        }

        public IEnumerable<BrandNameSearchDto> GetMedsByBrandName(int siteId, string search, int userId, EmarOrderType searchType,
            string deptCode, string schedulerDataRetrieveBase)
        {
            var records = _medicationRepository.GetMedsByBrandName(siteId, search, userId, searchType, deptCode);

            //Trying another method of sorting.
            //Put the result of the mapping call into a variable rather than just returning it.
            //Winston Murdock, 03/11/2021.  EMAR-828.
            var recordsMapped = records.Select(r => MedicationMapper.MapBrandName(r, schedulerDataRetrieveBase));

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
                        break;
                    }
                case (EmarOrderType.MedicationItem):
                    {
                        listItem = _medicationRepository.GetMedication(itemId);
                        break;
                    }
                //Winston Murdock, 01/06/2021.
                case (EmarOrderType.PatientOrder):
                    {
                        var patientOrder = _orderRepository.GetOrder(itemId);
                        var patient = _patientRepository.GetPatient(patientOrder.PatientId, null, false);

                        codeShareSiteMedicationUnit = _orderRepository.GetCodeShareSites(patient.SiteId)
                            .FirstOrDefault(c =>
                                c.Entity == OrderRepository.CodeShareEntity.MedicationUnit)?
                            .SharedSiteId;
                        listItem = patientOrder;
                        break;
                    }
            }

            if (listItem != null)
            {
                interactionsReactions = _orderService
                    .CheckInteractionsReactions(
                        in userId,
                        new List<MedicationModel>
                        {
                            OrderMapper.MapOrderItemToModel(itemType, listItem, patientId, userId, codeShareSiteMedicationUnit)
                        },
                        patientId)
                    .ToList();
            }

            return interactionsReactions;
        }
    }
}