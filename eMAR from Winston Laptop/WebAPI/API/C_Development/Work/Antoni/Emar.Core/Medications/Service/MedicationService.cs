using System;
using System.Collections.Generic;
using System.Linq;
using Emar.Core.Medications.Model;
using Emar.Core.Medications.Model.Mappings;
using Emar.Core.Medications.Repository;
using Emar.Core.Orders.Model.Mappings;
using Emar.Core.Orders.Repository;
using Emar.Core.Orders.Service;

namespace Emar.Core.Medications.Service
{
    public class MedicationService : IMedicationService
    {
        private readonly IMedicationRepository _medicationRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly IOrderService _orderService;

        public MedicationService(IMedicationRepository medicationRepository, IOrderRepository orderRepository, IOrderService orderService)
        {
            _medicationRepository = medicationRepository;
            _orderRepository = orderRepository;
            _orderService = orderService;
        }

        public IEnumerable<BrandNameSearchDto> GetMedsByBrandName(int siteId, string search, int userId, EmarOrderType searchType,
            string deptCode, string schedulerDataRetrieveBase)
        {
            //throw new System.NotImplementedException();
            var records = _medicationRepository.GetMedsByBrandName(siteId, search, userId, searchType, deptCode);

            //Return the list of strings.
            return records.Select(r => MedicationMapper.MapBrandName(r, schedulerDataRetrieveBase));
        }

        public IEnumerable<AntimicrobialIndicationDto> GetIndicationsBySite(int siteId)
        {
            //Return the list.
            return _medicationRepository.GetIndicationsBySite(siteId).Select(MedicationMapper.MapAntimicrobial);
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