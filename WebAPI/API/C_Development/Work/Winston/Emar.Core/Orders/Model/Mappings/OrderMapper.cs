using System;
using System.Linq;
using Emar.Core.Medications.Model;
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
                AddUser = UserMapper.MapUser(patientOrder.AddUser),
                AddDatetime = patientOrder.AddDatetime,
                OrderingPhysicianId = patientOrder.OrderingPhysicianId,
                OrderingPhysicianUser = UserMapper.MapUser(patientOrder.OrderPhysicianUser),
                Ndc = patientOrder.Ndc,
                DrugId = patientOrder.DrugId,
                BrandName = patientOrder.BrandName,
                Dose = patientOrder.Dose,
                DoseUnit = patientOrder.DoseUnit,
                MedicationRoute = OrderMapper.MapMedicationRoute(patientOrder.MedicationRoute),
                ////Priority = (OrderPriorities)Enum.Parse(typeof(OrderPriorities), patientOrder.Priority),
                FrequencyId = patientOrder.FrequencyId,
                Prn = patientOrder.Prn,
                PointInTime = patientOrder.PointInTime,
                OrderStatus = patientOrder.OrderStatus,
                OrderStatusCode = (OrderStatuses)Enum.Parse(typeof(OrderStatuses), patientOrder.OrderStatusCode),
                BeginDatetime = patientOrder.BeginDatetime,
                EndDatetime = patientOrder.EndDateTime,
                OrderNotes = patientOrder.OrderNotes,
                OrderAdministrations = patientOrder.OrderAdministrations?.Select(admin => MapOrderAdministration(admin))
                    .ToList(),
                OrderEvents = patientOrder.OrderEvents?.Select(ev => OrderMapper.MapOrderEvent(ev)).ToList()
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
                AdministrationEvents = administration.OrderEvents?.Select(MapOrderEvent).ToList()
            };

            return administrationDto;
        }

        public static OrderEventDto MapOrderEvent(OrderEvent @event)
        {
            if (@event == null)
            {
                return null;
            }

            OrderEventDto eventDto = new OrderEventDto
            {
                Id = @event.Id,
                OrderId = @event.PatientOrderId,
                AdministrationId = @event.OrderAdministrationId,
                EventDateTime = @event.EventDateTime,
                SystemDateTime = @event.AddDatetime,
                UserId = @event.AddUserId,
                ActionId = @event.ActionId
            };

            return eventDto;
        }

        public static UserQuickListItemDto MapUserQuickListItem(UserQuickListItem dbObj)
        {
            if (dbObj == null)
                return null;

            var ret = new UserQuickListItemDto
            {
                UserId = dbObj.UserId,
                SiteId = dbObj.SiteId,
                Id = dbObj.Id,
                Ndc = dbObj.Ndc,
                DrugId = dbObj.DrugId,
                BrandName = dbObj.BrandName,
                Dose = dbObj.Dose,
                DoseUnit = dbObj.DoseUnit,
                MedicationRoute = MapMedicationRoute(dbObj.MedicationRoute),
                FrequencyId = dbObj.FrequencyId,
                OrderNotes = dbObj.OrderNotes
            };

            ret.PointInTime = ret.MedicationRoute?.PointInTime ?? true;
            return ret;
        }

        public static MedicationRouteDto MapMedicationRoute(MedicationRoute dbObj)
        {
            if (dbObj == null)
                return null;

            var ret = new MedicationRouteDto
            {
                Id = dbObj.Id,
                RouteName = dbObj.Name,
                SiteId = dbObj.SiteId
            };

            return ret;
        }

        public static PatientOrderCreationDto MapOrderForCreation(PatientOrder patientOrder)
        {
            if (patientOrder == null)
            {
                return null;
            }

            PatientOrderCreationDto patientOrderCreationDto = new PatientOrderCreationDto
            {
                PatientId = patientOrder.PatientId,
                AddUserId = patientOrder.AddUserId,
                AddDatetime = patientOrder.AddDatetime,
                Ndc = patientOrder.Ndc,
                DrugId = patientOrder.DrugId,
                BrandName = patientOrder.BrandName,
                Dose = patientOrder.Dose,
                DoseUnit = patientOrder.DoseUnit,
                MedicationRouteId = patientOrder.MedicationRoute.Id,
                Priority = patientOrder.Priority,
                FrequencyId = patientOrder.FrequencyId,
                Prn = patientOrder.Prn,
                PointInTime = patientOrder.PointInTime,
                OrderStatus = patientOrder.OrderStatus,
                BeginDatetime = patientOrder.BeginDatetime,
                EndDatetime = patientOrder.EndDateTime,
                OrderNotes = patientOrder.OrderNotes,
                OrderAdministrations = patientOrder.OrderAdministrations,
                OrderEvents = (patientOrder.OrderEvents ?? Array.Empty<OrderEvent>()).Where(@event => @event.OrderAdministrationId == null)
            };

            return patientOrderCreationDto;
        }
    }
}
