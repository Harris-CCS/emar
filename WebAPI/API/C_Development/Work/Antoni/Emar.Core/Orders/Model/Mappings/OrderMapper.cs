using System;
using System.Linq;
using Emar.Core.Medications.Model;
using Emar.Core.Users.Model;
using Emar.Core.Users.Model.Mappings;
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
                AddUserId = patientOrder.AddUserId,
                AddUser = patientOrder.AddUser,
                AddDatetime = patientOrder.AddDatetime,
                OrderPhysicianUserId = patientOrder.OrderPhysicianUserId,
                OrderPhysicianUser = patientOrder.OrderPhysicianUser,
                Ndc = patientOrder.Ndc,
                DrugId = patientOrder.DrugId,
                BrandName = patientOrder.BrandName,
                Dose = patientOrder.Dose,
                DoseUnit = patientOrder.DoseUnit,
                MedicationRouteId = patientOrder.MedicationRouteId,
                MedicationRoute = patientOrder.MedicationRoute,
                ////Priority = (OrderPriorities)Enum.Parse(typeof(OrderPriorities), patientOrder.Priority),
                FrequencyId = patientOrder.FrequencyId,
                Prn = patientOrder.Prn,
                PointInTime = patientOrder.PointInTime,
                OrderStatus = patientOrder.OrderStatus,
                OrderStatusCode = (OrderStatuses)Enum.Parse(typeof(OrderStatuses), patientOrder.OrderStatusCode),
                BeginDatetime = patientOrder.BeginDatetime,
                EndDatetime = patientOrder.EndDatetime,
                OrderNotes = patientOrder.OrderNotes,
                OrderAdministrations = patientOrder.OrderAdministrations,
                OrderEvents = (patientOrder.OrderEvents ?? Array.Empty<OrderEvent>()).Where(@event => @event.OrderAdministrationId == null)
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
                OrderId = administration.PatientOrderId,
                AdministrationScheduledDatetime = administration.AdministrationScheduledDatetime,
                AdministrationInputDatetime = administration.AdministrationInputDatetime,
                AdministrationDatetime = administration.AdministrationDatetime,
                AdministeringUserId = administration.AdministeringUserId,
                StopScheduledDatetime = administration.StopScheduledDatetime,
                StopInputDatetime = administration.StopInputDatetime,
                StopDatetime = administration.StopDatetime,
                StopUserId = administration.StopUserId,
                AcknowledgeUserId = administration.AcknowledgeUserId,
                AcknowledgeDatetime = administration.AcknowledgeDatetime,
                PointInTime = administration.PointInTime,
                OnHold = administration.OnHold,
                MissedDose = administration.MissedDose,
                AdministrationEvents = administration.OrderEvents
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
                OrderId = @event.PatientOrderId,
                AdministrationId = @event.OrderAdministrationId,
                EventDateTime = @event.EventDateTime,
                SystemDateTime = @event.AddDatetime,
                UserId = @event.AddUserId,
                ActionId = @event.ActionId
            };

            return _eventDto;
        }

        //////public static MedicationRouteDto MapMedicationRoute(MedicationRoute medRoute)
        //////{
        //////    if (medRoute == null)
        //////    {
        //////        return null;
        //////    }

        //////    var ret = new MedicationRouteDto
        //////    {
        //////        Id = medRoute.Id,
        //////        Name = medRoute.Name,
        //////        SiteId = medRoute.SiteId
        //////    };

        //////    return ret;
        //////}
    }
}
