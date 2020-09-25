using System;
using System.Collections.Generic;
using System.Linq;
using Emar.Core.Orders.Model;
using Emar.Data;
using Emar.Data.Entities;

namespace Emar.Core.Orders.Service
{
    internal class ActionService
    {
        private static string _linkBase;
        private static List<AvailableActionDto> _returnActionList;

        //internal static void AssignActions(List<PatientOrderDto> orders, string orderLinkBase)
        //{
        //    for (var o = 0; o < orders.Count; o++)
        //    {
        //        var order = orders[o];
        //        var thisOrderLink = orderLinkBase.Replace("-99", order.Id.ToString());
        //        order.AvailableActions = AvailableOrderActions(order.OrderStatusCode, order.PointInTime, thisOrderLink);
        //    }
        //}

        internal static IEnumerable<AvailableActionDto> AvailableOrderActions(PatientOrderDto order,
            string orderLinkBase)
        {
            _linkBase = orderLinkBase.Replace("/-99/", $"/{order.Id}/");
            _returnActionList = new List<AvailableActionDto>();

            switch (order.OrderStatusCode)
            {
                case OrderStatuses.Pending:
                    AddAction(ActionEnum.Cancel);
                    AddAction(ActionEnum.Delete);
                    AddAction(ActionEnum.OrderDiscontinue);
                    AddAction(ActionEnum.Repeat);
                    break;
                case OrderStatuses.OnGoing:
                    AddAction(ActionEnum.OrderDiscontinue);
                    AddAction(ActionEnum.Repeat);
                    break;
                case OrderStatuses.OnHold:
                    AddAction(ActionEnum.Cancel);
                    AddAction(ActionEnum.Delete);
                    AddAction(ActionEnum.OrderDiscontinue);
                    AddAction(ActionEnum.Repeat);
                    break;
                case OrderStatuses.PendingDiscontinue:
                    AddAction(ActionEnum.CompleteDiscontinue);
                    AddAction(ActionEnum.Repeat);
                    break;
                case OrderStatuses.Discontinued:
                case OrderStatuses.Completed:
                    AddAction(ActionEnum.Repeat);
                    break;
                case OrderStatuses.Cancelled:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(order.OrderStatusCode), order.OrderStatusCode,
                        "From ActionService.AvailableOrderActions()");
            }

            return _returnActionList;
        }

        private static void AddAction(ActionEnum action)
        {
            _returnActionList.Add(new AvailableActionDto(action, _linkBase));
        }

        internal static IEnumerable<AvailableActionDto> AvailableAdministrationActions(
            OrderAdministrationDto administration, OrderStatuses orderStatusCode, string adminBase)
        {
            _linkBase = adminBase.Replace("/-99/", $"/{administration.Id}/");
            _returnActionList = new List<AvailableActionDto>();
            
            switch (orderStatusCode)
            {
                case OrderStatuses.Pending:
                    switch (administration.AdministrationStatusCode)
                    {
                        case OrderAdministrationDto.AdministrationStatuses.Pending:
                            AddAction(ActionEnum.Give);
                            AddAction(ActionEnum.Hold);
                            if(administration.AcknowledgeUserId == null) AddAction(ActionEnum.Acknowledge);
                            AddAction(ActionEnum.CoSign);
                            AddAction(ActionEnum.Reschedule);
                            break;
                        case OrderAdministrationDto.AdministrationStatuses.Late:
                            AddAction(ActionEnum.Give);
                            AddAction(ActionEnum.Hold);
                            if (administration.AcknowledgeUserId == null) AddAction(ActionEnum.Acknowledge);
                            AddAction(ActionEnum.CoSign);
                            if(administration.PointInTime) AddAction(ActionEnum.MissedDose);
                            AddAction(ActionEnum.Reschedule);
                            break;
                    }
                    break;
                case OrderStatuses.OnGoing:
                    switch (administration.AdministrationStatusCode)
                    {
                        case OrderAdministrationDto.AdministrationStatuses.OnHold:
                            if (administration.PointInTime)
                            {
                                AddAction(ActionEnum.Give);
                                AddAction(ActionEnum.MissedDose);
                                AddAction(ActionEnum.UnHold);
                                if (administration.AcknowledgeUserId == null) AddAction(ActionEnum.Acknowledge);
                                AddAction(ActionEnum.CoSign);
                                AddAction(ActionEnum.Reschedule);
                            }
                            break;
                        case OrderAdministrationDto.AdministrationStatuses.Pending:
                            if (administration.PointInTime)
                            {
                                AddAction(ActionEnum.Give);
                                AddAction(ActionEnum.Hold);
                                if (administration.AcknowledgeUserId == null) AddAction(ActionEnum.Acknowledge);
                                AddAction(ActionEnum.CoSign);
                                AddAction(ActionEnum.Reschedule);
                            }
                            break;
                        case OrderAdministrationDto.AdministrationStatuses.Late:
                            if (administration.PointInTime)
                            {
                                AddAction(ActionEnum.Give);
                                AddAction(ActionEnum.MissedDose);
                                AddAction(ActionEnum.Hold);
                                if (administration.AcknowledgeUserId == null) AddAction(ActionEnum.Acknowledge);
                                AddAction(ActionEnum.CoSign);
                                AddAction(ActionEnum.Reschedule);
                            }
                            break;
                        case OrderAdministrationDto.AdministrationStatuses.OnGoing:
                            if (!administration.PointInTime)
                            {
                                AddAction(ActionEnum.FollowUp);
                                AddAction(ActionEnum.OrderDiscontinue);
                                AddAction(ActionEnum.Complete);
                                AddAction(ActionEnum.CoSign);
                            }
                            break;
                        case OrderAdministrationDto.AdministrationStatuses.Given:
                            if (administration.PointInTime)
                            {
                                AddAction(ActionEnum.CoSign);
                                AddAction(ActionEnum.FollowUp);
                            }
                            break;
                    }
                    break;
                case OrderStatuses.OnHold:
                    switch (administration.AdministrationStatusCode)
                    {
                        case OrderAdministrationDto.AdministrationStatuses.OnHold:
                            AddAction(ActionEnum.Give);
                            AddAction(ActionEnum.MissedDose);
                            AddAction(ActionEnum.UnHold);
                            if (administration.AcknowledgeUserId == null) AddAction(ActionEnum.Acknowledge);
                            AddAction(ActionEnum.CoSign);
                            AddAction(ActionEnum.Reschedule);
                            break;
                        case OrderAdministrationDto.AdministrationStatuses.Pending:
                            AddAction(ActionEnum.Give);
                            AddAction(ActionEnum.Hold);
                            if (administration.AcknowledgeUserId == null) AddAction(ActionEnum.Acknowledge);
                            AddAction(ActionEnum.CoSign);
                            AddAction(ActionEnum.Reschedule);
                            break;
                        case OrderAdministrationDto.AdministrationStatuses.Late:
                            AddAction(ActionEnum.Give);
                            AddAction(ActionEnum.MissedDose);
                            AddAction(ActionEnum.Hold);
                            if (administration.AcknowledgeUserId == null) AddAction(ActionEnum.Acknowledge);
                            AddAction(ActionEnum.CoSign);
                            AddAction(ActionEnum.Reschedule);
                            break;
                        case OrderAdministrationDto.AdministrationStatuses.Given:
                            AddAction(ActionEnum.CoSign);
                            AddAction(ActionEnum.FollowUp);
                            break;
                    }
                    break;
                case OrderStatuses.PendingDiscontinue:
                    switch (administration.AdministrationStatusCode)
                    {
                        case OrderAdministrationDto.AdministrationStatuses.OnHold:
                            AddAction(ActionEnum.Give);
                            AddAction(ActionEnum.MissedDose);
                            AddAction(ActionEnum.UnHold);
                            if (administration.AcknowledgeUserId == null) AddAction(ActionEnum.Acknowledge);
                            AddAction(ActionEnum.CoSign);
                            AddAction(ActionEnum.Reschedule);
                            break;
                        case OrderAdministrationDto.AdministrationStatuses.Pending:
                            AddAction(ActionEnum.Give);
                            AddAction(ActionEnum.Hold);
                            if (administration.AcknowledgeUserId == null) AddAction(ActionEnum.Acknowledge);
                            AddAction(ActionEnum.CoSign);
                            AddAction(ActionEnum.Reschedule);
                            break;
                        case OrderAdministrationDto.AdministrationStatuses.Late:
                            AddAction(ActionEnum.Give);
                            AddAction(ActionEnum.MissedDose);
                            AddAction(ActionEnum.Hold);
                            if (administration.AcknowledgeUserId == null) AddAction(ActionEnum.Acknowledge);
                            AddAction(ActionEnum.CoSign);
                            AddAction(ActionEnum.Reschedule);
                            break;
                        case OrderAdministrationDto.AdministrationStatuses.OnGoing:
                            AddAction(ActionEnum.CompleteDiscontinue);
                            AddAction(ActionEnum.CoSign);
                            AddAction(ActionEnum.FollowUp);
                            break;
                        case OrderAdministrationDto.AdministrationStatuses.Given:
                            AddAction(ActionEnum.CoSign);
                            AddAction(ActionEnum.FollowUp);
                            break;
                    }
                    break;
                case OrderStatuses.Discontinued:
                    switch (administration.AdministrationStatusCode)
                    {
                        case OrderAdministrationDto.AdministrationStatuses.OnHold:
                            AddAction(ActionEnum.Give);
                            AddAction(ActionEnum.MissedDose);
                            AddAction(ActionEnum.UnHold);
                            if (administration.AcknowledgeUserId == null) AddAction(ActionEnum.Acknowledge);
                            AddAction(ActionEnum.CoSign);
                            AddAction(ActionEnum.Reschedule);
                            break;
                        case OrderAdministrationDto.AdministrationStatuses.OnGoing:
                        case OrderAdministrationDto.AdministrationStatuses.Given:
                            AddAction(ActionEnum.CoSign);
                            AddAction(ActionEnum.FollowUp);
                            break;
                    }
                    break;
                case OrderStatuses.Completed:
                    if(administration.AdministrationStatusCode == OrderAdministrationDto.AdministrationStatuses.Given)
                    {
                        AddAction(ActionEnum.CoSign);
                        AddAction(ActionEnum.FollowUp);
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(orderStatusCode), orderStatusCode, null);
            }

            return _returnActionList;
        }
    }

    public enum ActionEnum
    {
        Acknowledge,
        Cancel,
        CompleteDiscontinue,
        CoSign,
        Delete,
        Give,
        Hold,
        MissedDose,
        OrderDiscontinue,
        Repeat,
        Reschedule,
        UnHold,
        FollowUp,
        Complete
    }
}