using System;
using System.Collections.Generic;
using System.Linq;
using Emar.Core.Orders.Model;
using Emar.Core.Templates.Repository;

namespace Emar.Core.Templates.Model.Mappings
{
    public class OrderActionMapperHelper
    {
        private readonly string _orderBase;
        private readonly string _adminBase;
        private readonly List<OrderAvailableActionDto> _orderAvailableActionDtos;
        private readonly List<OrderAdministrationAvailableActionDto> _orderAdministrationAvailableActionDtos;

        public OrderActionMapperHelper(ITemplateRepository templateRepository, in int siteId, string orderBase,
            string adminBase)
        {
            _orderBase = orderBase;
            _adminBase = adminBase;

            if (!string.IsNullOrWhiteSpace(orderBase))
            {
                _orderAvailableActionDtos = templateRepository.GetSiteOrderActions(siteId)
                    .Select(TemplateMapper.MapOrderAvailableAction).ToList();
            }

            if (!string.IsNullOrWhiteSpace(adminBase))
            {
                _orderAdministrationAvailableActionDtos =
                    templateRepository.GetSiteOrderAdministrationActions(siteId)
                        .Select(TemplateMapper.MapOrderAdministrationAvailableAction).ToList();
            }
        }

        public bool AdminLinkBaseExists => !string.IsNullOrWhiteSpace(_adminBase);

        public IEnumerable<AvailableActionDto> AvailableOrderActions(PatientOrderDto order)
        {
            IEnumerable<OrderAvailableActionDto> actionList;
            actionList = _orderAvailableActionDtos?.Where(o
                => o.OrderStatus == order.OrderStatusCode
                   && (o.PointInTime ?? order.PointInTime) == order.PointInTime
                   && (!o.IsPrnOnly || order.Prn)
            );

            return actionList?.Select(actionInfo => new AvailableActionDto
            {
                ActionId = actionInfo.AvailableActionId, 
                AvailableAction = actionInfo.Action.ActionCode,
                ButtonText = actionInfo.Action.ButtonText, 
                Link = string.Format(_orderBase, order.Id, actionInfo.AvailableActionId)
            }).ToList();
        }

        public IEnumerable<AvailableActionDto> AvailableAdministrationActions(OrderAdministrationDto admin,
            OrderStatus orderStatus)
        {
            IEnumerable<OrderAdministrationAvailableActionDto> actionList;
            if (admin.AcknowledgeUserId.HasValue)
                actionList = _orderAdministrationAvailableActionDtos.Where(a
                    => a.OrderStatus == orderStatus
                       && a.AdministrationStatus == admin.AdministrationStatusCode
                       && (a.PointInTime ?? admin.PointInTime) == admin.PointInTime
                       && a.AvailableActionId != 1 /*ActionEnum.Acknowledge*/);
            else
                actionList = _orderAdministrationAvailableActionDtos.Where(a
                    => a.OrderStatus == orderStatus
                       && a.AdministrationStatus == admin.AdministrationStatusCode
                       && (a.PointInTime ?? admin.PointInTime) == admin.PointInTime);

            return actionList.Select(actionInfo =>
                new AvailableActionDto
                {
                    ActionId = actionInfo.AvailableActionId,
                    AvailableAction = actionInfo.Action.ActionCode,
                    ButtonText = actionInfo.Action.ButtonText,
                    Link = string.Format(_adminBase, admin.Id, actionInfo.AvailableActionId)
                });
        }
    }
}