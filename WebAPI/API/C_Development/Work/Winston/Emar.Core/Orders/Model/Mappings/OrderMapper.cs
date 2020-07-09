using System;
using System.Linq;
using Emar.Data.Entities;
using static Emar.Core.Orders.Model.PatientOrderDto;

namespace Emar.Core.Orders.Model.Mappings
{
    public static class OrderMapper
    {
        public static PatientOrderDto MapOrder(Order order)
        {
            if (order == null)
            {
                return null;
            }

            PatientOrderDto _orderDto = new PatientOrderDto
            {
                Id = order.Id,
                PatientId = order.PatientId,
                CreatedDateTime = order.CreatedDateTime,
                MedicationId = order.MedicationId,
                ////Priority = (OrderPriorities)Enum.Parse(typeof(OrderPriorities), order.Priority),
                Prn = order.Prn,
                PointInTime = order.PointInTime,
                OrderStatus = order.OrderStatus,
                OrderStatusCode = (OrderBase.OrderStatuses)Enum.Parse(typeof(OrderBase.OrderStatuses), order.OrderStatusCode),
                BeginDateTime = order.BeginDateTime,
                EndDateTime = order.EndDateTime,
                FrequencyId = order.FrequencyId,
                MedicationRouteId = order.MedicationRouteId,
                OrderNotes = order.OrderNotes,
                Name = order.Name,
                Unit = order.Unit,
                Dose = order.Dose,
                OrderingProviderId = order.OrderingProviderId,
                OrderAdministrations = order.Administrations,
                OrderEvents = order.Events.Where(@event => @event.AdministrationId == null)
            };

            return _orderDto;
        }

        public static OrderAdministrationDto MapOrderAdministration(OrderAdministration administration)
        {
            if (administration == null)
            {
                return null;
            }

            OrderAdministrationDto _administrationDto = new OrderAdministrationDto
            {
                Id = administration.Id,
                OrderId = administration.OrderId,
                ScheduledAdministrationTime = administration.ScheduledAdministrationTime,
                ActualAdministrationTime = administration.ActualAdministrationTime,
                SystemAdministrationTime = administration.SystemAdministrationTime,
                AdministrationUserId = administration.AdministrationUserId,
                ScheduledStopTime = administration.ScheduledStopTime,
                ActualStopTime = administration.ActualStopTime,
                SystemStopTime = administration.SystemStopTime,
                StopUserId = administration.StopUserId,
                AcknowledgeUserId = administration.AcknowledgeUserId,
                AcknowledgeTime = administration.AcknowledgeTime,
                //////Continuous = administration.Continuous,
                OnHold = administration.OnHold,
                MissedDose = administration.MissedDose,
                AdministrationEvents = administration.Events
            };

            return _administrationDto;
        }

        public static OrderEventDto MapOrderEvent(OrderEvent @event)
        {
            if (@event == null)
            {
                return null;
            }

            OrderEventDto _eventDto = new OrderEventDto
            {
                Id = @event.Id,
                OrderId = @event.OrderId,
                AdministrationId = @event.AdministrationId,
                EventDateTime = @event.EventDateTime,
                SystemDateTime = @event.SystemDateTime,
                UserId = @event.UserId,
                ActionId = @event.ActionId
            };

            return _eventDto;
        }
    }
}
