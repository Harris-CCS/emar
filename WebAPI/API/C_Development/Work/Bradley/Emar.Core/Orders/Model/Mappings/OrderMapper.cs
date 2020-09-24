using System;
using System.Linq;
using Emar.Core.Helpers;
using Emar.Core.Medications.Model.Mappings;
using Emar.Core.Orders.Service;
using Emar.Core.Users.Model.Mappings;
using Emar.Data.Entities;

namespace Emar.Core.Orders.Model.Mappings
{
    public static class OrderMapper
    {
        public static PatientOrderDto MapOrder(PatientOrder patientOrder, string dateFormat, string orderBase,
            string adminBase)
        {
            if (patientOrder == null)
            {
                return null;
            }

            PatientOrderDto patientOrderDto = new PatientOrderDto
            {
                DateFormat = dateFormat,
                Id = patientOrder.Id,
                PatientId = patientOrder.PatientId,
                AddUserId = patientOrder.AddUserId,
                AddUser = UserMapper.MapUser(patientOrder.AddUser),
                AddDatetime = patientOrder.AddDatetime,
                //AddDate = DateTimeHelper.GetDate(patientOrder.AddDatetime, dateFormat),
                //AddTime = DateTimeHelper.GetTime(patientOrder.AddDatetime),
                OrderingPhysicianId = patientOrder.OrderingPhysicianId,
                OrderingPhysicianUser = UserMapper.MapUser(patientOrder.OrderPhysicianUser),
                MedicationId = patientOrder.MedicationId,
                Medication = MedicationMapper.MapMedication(patientOrder.Medication),
                //Ndc = patientOrder.Ndc,
                //DrugId = patientOrder.DrugId,
                //BrandName = patientOrder.BrandName,
                Dose = patientOrder.Dose,
                DoseUnit = MedicationMapper.MapMedicationUnit(patientOrder.MedicationUnit),
                MedicationRoute = MedicationMapper.MapMedicationRoute(patientOrder.MedicationRoute),
                ////Priority = (OrderPriorities)Enum.Parse(typeof(OrderPriorities), patientOrder.Priority),
                FrequencyId = patientOrder.FrequencyScheduleId,
                FrequencySchedule = MedicationMapper.MapMedicationFrequency(patientOrder.FrequencySchedule),
                Prn = patientOrder.Prn,
                PointInTime = patientOrder.PointInTime,
                OrderStatus = patientOrder.OrderStatus,
                //OrderStatusCode = (OrderStatuses)Enum.Parse(typeof(OrderStatuses), patientOrder.OrderStatus),
                BeginDatetime = patientOrder.BeginDatetime,
                //BeginDate = DateTimeHelper.GetDate(patientOrder.BeginDatetime, dateFormat),
                //BeginTime = DateTimeHelper.GetTime(patientOrder.BeginDatetime),
                EndDatetime = patientOrder.EndDateTime,
                //EndDate = DateTimeHelper.GetDate(patientOrder.EndDateTime, dateFormat),
                //EndTime = DateTimeHelper.GetTime(patientOrder.EndDateTime),
                OrderNotes = patientOrder.OrderNotes
            };

            patientOrderDto.OrderAdministrations =
                patientOrder.OrderAdministrations?
                    .Select(admin =>
                        MapOrderAdministration(admin, dateFormat, patientOrderDto.OrderStatusCode, adminBase))
                    .ToList();

            if (!Enum.TryParse(patientOrder.OrderStatus, out OrderStatuses orderStatus))
                orderStatus = OrderStatuses.Pending;
            else
                patientOrderDto.OrderStatusCode = orderStatus;

            if (!string.IsNullOrWhiteSpace(orderBase) && !string.IsNullOrWhiteSpace(adminBase))
                patientOrderDto.AvailableActions = ActionService.AvailableOrderActions(patientOrderDto, orderBase);

            patientOrderDto.NextActionTime = null;
            if (patientOrderDto.OrderAdministrations == null) return patientOrderDto;

            foreach (var admin in patientOrderDto.OrderAdministrations.Where(admin =>
                    admin.TimeNeedingAction.HasValue)
                .Where(admin => !patientOrderDto.NextActionTime.HasValue
                                || patientOrderDto.NextActionTime > admin.TimeNeedingAction))
                patientOrderDto.NextActionTime = admin.TimeNeedingAction;

            return patientOrderDto;
        }

        public static OrderAdministrationDto MapOrderAdministration(OrderAdministration administration,
            string dateFormat, OrderStatuses orderStatusCode, string adminBase)
        {
            if (administration == null)
            {
                return null;
            }

            OrderAdministrationDto administrationDto = new OrderAdministrationDto
            {
                DateFormat = dateFormat,
                Id = administration.Id,
                OrderId = administration.PatientOrderId,
                AdministrationScheduledDatetime = administration.AdministrationScheduledDatetime,
                AdministrationSystemDatetime = administration.AdministrationSystemDatetime,
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
                MissedDose = administration.MissedDose
                ////AdministrationEvents = administration.OrderEvents?.Select(OrderMapper.MapOrderEvent).Where(@event => @event.AdministrationId == null).ToList()
                ////AdministrationEvents = administration.OrderEvents?.Select(MapOrderEvent).ToList()
            };

            if (!string.IsNullOrWhiteSpace(adminBase))
                administrationDto.AvailableActions =
                    ActionService.AvailableAdministrationActions(administrationDto, orderStatusCode, adminBase);

            return administrationDto;
        }

        public static OrderEventDto MapOrderEvent(OrderEvent @event, string dateFormat)
        {
            if (@event == null)
            {
                return null;
            }

            OrderEventDto eventDto = new OrderEventDto
            {
                Id = @event.Id,
                DateFormat =  dateFormat,
                OrderId = @event.PatientOrderId,
                AdministrationId = @event.OrderAdministrationId,
                EventDateTime = @event.EventDateTime,
                //EventDate = DateTimeHelper.GetDate(@event.EventDateTime, dateFormat),
                //EventTime = DateTimeHelper.GetTime(@event.EventDateTime),
                SystemDateTime = @event.AddDatetime,
                //SystemDate = DateTimeHelper.GetDate(@event.AddDatetime, dateFormat),
                //SystemTime = DateTimeHelper.GetTime(@event.AddDatetime),
                UserId = @event.AddUserId,
                ActionId = @event.ActionId
            };

            return eventDto;
        }

        public static UserQuickListItemDto MapUserQuickListItem(UserQuickListItem dbObj, string orderLinkBase)
        {
            if (dbObj == null)
                return null;

            var ret = new UserQuickListItemDto
            {
                UserId = dbObj.UserId,
                SiteId = dbObj.SiteId,
                Id = dbObj.Id,
                //Ndc = dbObj.Ndc,
                //DrugId = dbObj.DrugId,
                //BrandName = dbObj.BrandName,
                MedicationId = dbObj.MedicationId,
                Medication = MedicationMapper.MapMedication(dbObj.Medication),
                Dose = dbObj.Dose,
                DoseUnit = MedicationMapper.MapMedicationUnit(dbObj.MedicationUnit),
                MedicationRoute = MedicationMapper.MapMedicationRoute(dbObj.MedicationRoute),
                FrequencySchedule = MedicationMapper.MapMedicationFrequency(dbObj.FrequencySchedule),
                OrderNotes = dbObj.OrderNotes
            };

            ret.PointInTime = ret.MedicationRoute?.PointInTime ?? true;

            if (!string.IsNullOrEmpty(orderLinkBase))
                ret.Links = new[]
                {
                    new HateOasLinkDto(orderLinkBase.Replace("/-99/", string.Concat("/", dbObj.Id, "/")),
                        "add_quicklist_order_to_cart",
                        "POST")
                };
            return ret;
        }

        internal static PatientCartOrder MapUserQuickListItemToPatientCartOrder(UserQuickListItem dbObj)
        {
            if (dbObj == null)
                return null;

            var ret = new PatientCartOrder
            {
                // Properties From the OrderBase
                //Ndc = dbObj.Ndc,
                //DrugId = dbObj.DrugId,
                //BrandName = dbObj.BrandName,
                MedicationId = dbObj.MedicationId,
                Dose = dbObj.Dose,
                MedicationUnitId = dbObj.MedicationUnitId,
                MedicationRouteId = dbObj.MedicationRouteId,
                FrequencyScheduleId = dbObj.FrequencyScheduleId,
                //PointInTime = dbObj.point
                OrderNotes = dbObj.OrderNotes,

                // Properties from CartOrderDto
                PatientId = -1,
                UserId = -1,
                AddDatetime = DateTimeOffset.Now,
                Priority = 1,
                Prn = false,
                BeginDatetime = DateTimeOffset.Now,
                //EndDatetime = ??? -- need to look at duration
                UserQuickListItemId = dbObj.Id
            };

            return ret;
        }

        public static DepartmentPreferredItemDto MapDepartmentPreferredListItem(DepartmentPreferredListItem dbObj,
            string linkBase)
        {
            if (dbObj == null)
                return null;

            var ret = new DepartmentPreferredItemDto
            {
                DepartmentCode = dbObj.DepartmentCode,
                SiteId = dbObj.SiteId,
                Id = dbObj.Id,
                //Ndc = dbObj.Ndc,
                //DrugId = dbObj.DrugId,
                //BrandName = dbObj.BrandName,
                MedicationId = dbObj.MedicationId,
                Medication = MedicationMapper.MapMedication(dbObj.Medication),
                Dose = dbObj.Dose,
                DoseUnit = MedicationMapper.MapMedicationUnit(dbObj.MedicationUnit),
                MedicationRoute = MedicationMapper.MapMedicationRoute(dbObj.MedicationRoute),
                FrequencySchedule = MedicationMapper.MapMedicationFrequency(dbObj.FrequencySchedule),
                DurationInMinutes = dbObj.DurationInMinutes,
                OrderNotes = dbObj.OrderNotes
            };

            ret.PointInTime = ret.MedicationRoute?.PointInTime ?? true;

            if (!string.IsNullOrEmpty(linkBase))
                ret.Links = new[]
                {
                    new HateOasLinkDto(linkBase.Replace("/-99/", string.Concat("/", dbObj.Id, "/")),
                        "add_dept_preferred_order_to_cart",
                        "POST")
                };
            return ret;
        }

        public static GroupListItemDto MapGroupListItem(GroupListItem dbObj, string linkBase)
        {
            if (dbObj == null)
                return null;

            var ret = new GroupListItemDto
            {
                DepartmentCode = dbObj.DepartmentCode,
                SiteId = dbObj.SiteId,
                GroupName = dbObj.GroupName,
                Id = dbObj.Id,
                //Ndc = dbObj.Ndc,
                //DrugId = dbObj.DrugId,
                //BrandName = dbObj.BrandName,
                MedicationId = dbObj.MedicationId,
                Medication = MedicationMapper.MapMedication(dbObj.Medication),
                Dose = dbObj.Dose,
                DoseUnit = MedicationMapper.MapMedicationUnit(dbObj.MedicationUnit),
                MedicationRoute = MedicationMapper.MapMedicationRoute(dbObj.MedicationRoute),
                FrequencySchedule = MedicationMapper.MapMedicationFrequency(dbObj.FrequencySchedule),
                OrderNotes = dbObj.OrderNotes
            };

            ret.PointInTime = ret.MedicationRoute?.PointInTime ?? true;

            if (!string.IsNullOrEmpty(linkBase))
                ret.Links = new[]
                {
                    new HateOasLinkDto(linkBase.Replace("/-99/", string.Concat("/", dbObj.Id, "/")),
                        "add_group_order_to_cart",
                        "POST")
                };
            return ret;
        }
    }
}