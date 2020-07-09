using System;
using System.Linq;
using Emar.Data.Entities;

namespace Emar.Core.Orders.Model.Mappings
{
    public static class OrderMapper
    {
        public static PatientOrderDto MapOrder(PatientOrder patientOrder)
        {
            if (patientOrder == null)
            {
                return null;
            }

            PatientOrderDto patientOrderDto = new PatientOrderDto
            {
                Id = patientOrder.Id,
                PatientId = patientOrder.PatientId,
                CreatedDateTime = patientOrder.CreatedDateTime,
                DrugId = patientOrder.DrugId,
                ////Priority = (OrderPriorities)Enum.Parse(typeof(OrderPriorities), patientOrder.Priority),
                Prn = patientOrder.Prn,
                PointInTime = patientOrder.PointInTime,
                OrderStatus = patientOrder.OrderStatus,
                OrderStatusCode = (OrderStatuses)Enum.Parse(typeof(OrderStatuses), patientOrder.OrderStatusCode),
                BeginDateTime = patientOrder.BeginDateTime,
                EndDateTime = patientOrder.EndDateTime,
                FrequencyId = patientOrder.FrequencyId,
                MedicationRouteId = patientOrder.MedicationRouteId,
                OrderNotes = patientOrder.OrderNotes,
                BrandName = patientOrder.Name,
                Unit = patientOrder.Unit,
                Dose = patientOrder.Dose,
                OrderingProviderId = patientOrder.OrderingProviderId,
                OrderAdministrations = patientOrder.Administrations,
                OrderEvents = (patientOrder.Events ?? Array.Empty<OrderEvent>()).Where(@event => @event.AdministrationId == null)
            };

            return patientOrderDto;
        }

        public static OrderAdministrationDto MapOrderAdministration(OrderAdministration administration)
        {
            if (administration == null)
            {
                return null;
            }

            OrderAdministrationDto administrationDto = new OrderAdministrationDto
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

            return administrationDto;
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
