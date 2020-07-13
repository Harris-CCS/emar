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
                OrderingProviderId = patientOrder.AddUserId,
                CreatedDateTime = patientOrder.AddDatetime,
                Ndc = patientOrder.Ndc,
                DrugId = patientOrder.DrugId,
                BrandName = patientOrder.BrandName,
                Dose = patientOrder.Dose,
                DoseUnit = patientOrder.DoseUnit,
                MedicationRouteId = patientOrder.MedicationRouteId,
                ////Priority = (OrderPriorities)Enum.Parse(typeof(OrderPriorities), patientOrder.Priority),
                FrequencyId = patientOrder.FrequencyId,
                Prn = patientOrder.Prn,
                PointInTime = patientOrder.PointInTime,
                OrderStatus = patientOrder.OrderStatus,
                OrderStatusCode = (OrderStatuses)Enum.Parse(typeof(OrderStatuses), patientOrder.OrderStatusCode),
                BeginDateTime = patientOrder.BeginDateTime,
                EndDateTime = patientOrder.EndDateTime,
                OrderNotes = patientOrder.OrderNotes,
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
                ScheduledAdministrationTime = administration.AdministrationScheduledDatetime,
                ActualAdministrationTime = administration.AdministrationInputDatetime,
                SystemAdministrationTime = administration.AdministrationDatetime,
                AdministrationUserId = administration.AdministeringUserId,
                ScheduledStopTime = administration.StopScheduledDatetime,
                ActualStopTime = administration.StopInputDatetime,
                SystemStopTime = administration.StopDatetime,
                StopUserId = administration.StopUserId,
                AcknowledgeUserId = administration.AcknowledgeUserId,
                AcknowledgeTime = administration.AcknowledgeDatetime,
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
                SystemDateTime = @event.AddDatetime,
                UserId = @event.AddUserId,
                ActionId = @event.ActionId
            };

            return _eventDto;
        }
    }
}
