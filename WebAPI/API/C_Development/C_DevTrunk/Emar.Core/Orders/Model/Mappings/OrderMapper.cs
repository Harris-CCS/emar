using System;
using System.Linq;
using Emar.Core.Helpers;
using Emar.Core.Medications.Model.Mappings;
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

            var dateFormat = patientOrder.Patient.Site.SiteOptions.FirstOrDefault(si => si.Option.Name == AppConstants.LongDateFormat).OptionValue;

            PatientOrderDto patientOrderDto = new PatientOrderDto
            {
                Id = patientOrder.Id,
                PatientId = patientOrder.PatientId,
                AddUserId = patientOrder.AddUserId,
                AddUser = UserMapper.MapUser(patientOrder.AddUser),
                AddDatetime = patientOrder.AddDatetime,
                AddDate = DateTimeHelper.GetDate(patientOrder.AddDatetime, dateFormat),
                AddTime = DateTimeHelper.GetTime(patientOrder.AddDatetime),
                OrderingPhysicianId = patientOrder.OrderingPhysicianId,
                OrderingPhysicianUser = UserMapper.MapUser(patientOrder.OrderPhysicianUser),
                Ndc = patientOrder.Ndc,
                DrugId = patientOrder.DrugId,
                BrandName = patientOrder.BrandName,
                Dose = patientOrder.Dose,
                DoseUnit = MedicationMapper.MapMedicationUnit(patientOrder.MedicationUnit),
                MedicationRoute = MedicationMapper.MapMedicationRoute(patientOrder.MedicationRoute),
                ////Priority = (OrderPriorities)Enum.Parse(typeof(OrderPriorities), patientOrder.Priority),
                FrequencyId = patientOrder.FrequencyScheduleId,
                FrequencySchedule = MedicationMapper.MapMedicationFrequency(patientOrder.FrequencySchedule),
                Prn = patientOrder.Prn,
                PointInTime = patientOrder.PointInTime,
                OrderStatus = patientOrder.OrderStatus,
                OrderStatusCode = (OrderStatuses)Enum.Parse(typeof(OrderStatuses), patientOrder.OrderStatusCode),
                BeginDatetime = patientOrder.BeginDatetime,
                BeginDate = DateTimeHelper.GetDate(patientOrder.BeginDatetime, dateFormat),
                BeginTime = DateTimeHelper.GetTime(patientOrder.BeginDatetime),
                EndDatetime = patientOrder.EndDateTime,
                EndDate = DateTimeHelper.GetDate(patientOrder.EndDateTime, dateFormat),
                EndTime = DateTimeHelper.GetTime(patientOrder.EndDateTime),
                OrderNotes = patientOrder.OrderNotes,
                OrderAdministrations = patientOrder.OrderAdministrations?.Select(admin => MapOrderAdministration(admin)).ToList()
                ////OrderEvents = patientOrder.OrderEvents?.Select(OrderMapper.MapOrderEvent).Where(@event => @event.AdministrationId == null).ToList()
                ////OrderEvents = patientOrder.OrderEvents?.Select(ev => OrderMapper.MapOrderEvent(ev)).ToList()
            };

            return patientOrderDto;
        }

        public static OrderAdministrationDto MapOrderAdministration(OrderAdministration administration)
        {
            if (administration == null)
            {
                return null;
            }

            var dateFormat = administration.PatientOrder.Patient.Site.SiteOptions.FirstOrDefault(si => si.Option.Name == AppConstants.LongDateFormat).OptionValue;

            OrderAdministrationDto administrationDto = new OrderAdministrationDto
            {
                Id = administration.Id,
                OrderId = administration.PatientOrderId,
                AdministrationScheduledDatetime = administration.AdministrationScheduledDatetime,
                AdministrationScheduledDate = DateTimeHelper.GetDate(administration.AdministrationScheduledDatetime, dateFormat),
                AdministrationScheduledTime = DateTimeHelper.GetTime(administration.AdministrationScheduledDatetime),
                AdministrationInputDatetime = administration.AdministrationInputDatetime,
                AdministrationInputDate = DateTimeHelper.GetDate(administration.AdministrationInputDatetime, dateFormat),
                AdministrationInputTime = DateTimeHelper.GetTime(administration.AdministrationInputDatetime),
                AdministrationDatetime = administration.AdministrationDatetime,
                AdministrationDate = DateTimeHelper.GetDate(administration.AdministrationDatetime, dateFormat),
                AdministrationTime = DateTimeHelper.GetTime(administration.AdministrationDatetime),
                AdministeringUserId = administration.AdministeringUserId,
                StopScheduledDatetime = administration.StopScheduledDatetime,
                StopScheduledDate = DateTimeHelper.GetDate(administration.StopScheduledDatetime, dateFormat),
                StopScheduledTime = DateTimeHelper.GetTime(administration.StopScheduledDatetime),
                StopInputDatetime = administration.StopInputDatetime,
                StopInputDate = DateTimeHelper.GetDate(administration.StopInputDatetime, dateFormat),
                StopInputTime = DateTimeHelper.GetTime(administration.StopInputDatetime),
                StopDatetime = administration.StopDatetime,
                StopDate = DateTimeHelper.GetDate(administration.StopDatetime, dateFormat),
                StopTime = DateTimeHelper.GetTime(administration.StopDatetime),
                StopUserId = administration.StopUserId,
                AcknowledgeUserId = administration.AcknowledgeUserId,
                AcknowledgeDatetime = administration.AcknowledgeDatetime,
                AcknowledgeDate = DateTimeHelper.GetDate(administration.AcknowledgeDatetime, dateFormat),
                AcknowledgeTime = DateTimeHelper.GetTime(administration.AcknowledgeDatetime),
                PointInTime = administration.PointInTime,
                OnHold = administration.OnHold,
                MissedDose = administration.MissedDose
                ////AdministrationEvents = administration.OrderEvents?.Select(OrderMapper.MapOrderEvent).Where(@event => @event.AdministrationId == null).ToList()
                ////AdministrationEvents = administration.OrderEvents?.Select(MapOrderEvent).ToList()
            };

            return administrationDto;
        }

        public static OrderEventDto MapOrderEvent(OrderEvent @event)
        {
            if (@event == null)
            {
                return null;
            }

            var dateFormat = @event.PatientOrder.Patient.Site.SiteOptions.FirstOrDefault(si => si.Option.Name == AppConstants.LongDateFormat).OptionValue;

            OrderEventDto eventDto = new OrderEventDto
            {
                Id = @event.Id,
                OrderId = @event.PatientOrderId,
                AdministrationId = @event.OrderAdministrationId,
                EventDateTime = @event.EventDateTime,
                EventDate = DateTimeHelper.GetDate(@event.EventDateTime, dateFormat),
                EventTime = DateTimeHelper.GetTime(@event.EventDateTime),
                SystemDateTime = @event.AddDatetime,
                SystemDate = DateTimeHelper.GetDate(@event.AddDatetime, dateFormat),
                SystemTime = DateTimeHelper.GetTime(@event.AddDatetime),
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
                Ndc = dbObj.Ndc,
                DrugId = dbObj.DrugId,
                BrandName = dbObj.BrandName,
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
                Ndc = dbObj.Ndc,
                DrugId = dbObj.DrugId,
                BrandName = dbObj.BrandName,
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
                Ndc = dbObj.Ndc,
                DrugId = dbObj.DrugId,
                BrandName = dbObj.BrandName,
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