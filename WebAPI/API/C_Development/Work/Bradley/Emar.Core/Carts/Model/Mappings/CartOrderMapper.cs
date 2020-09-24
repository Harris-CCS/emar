using System;
using System.Linq;
using Emar.Core.Helpers;
using Emar.Core.Medications.Model.Mappings;
using Emar.Core.Orders.Model;
using Emar.Core.Users.Model.Mappings;
using Emar.Data.Entities;

namespace Emar.Core.Carts.Model.Mappings
{
    public static class CartOrderMapper
    {
        public static CartOrderDto MapCartOrder(PatientCartOrder order, string dateFormat)
        {
            if (order == null)
                return null;

            CartOrderDto orderDto = new CartOrderDto
            {
                Id = order.Id,
                PatientId = order.PatientId,
                DateFormat = dateFormat,
                UserId = order.UserId,
                User = UserMapper.MapUser(order.User),
                AddDatetime = order.AddDatetime,
                //AddDate = DateTimeHelper.GetDate(order.AddDatetime, dateFormat),
                //AddTime = DateTimeHelper.GetTime(order.AddDatetime),
                //Ndc = order.Ndc,
                //DrugId = order.DrugId,
                //BrandName = order.BrandName,
                MedicationId = order.MedicationId,
                Medication = MedicationMapper.MapMedication(order.Medication),
                Dose = order.Dose,
                DoseUnit = MedicationMapper.MapMedicationUnit(order.MedicationUnit),
                MedicationRoute = MedicationMapper.MapMedicationRoute(order.MedicationRoute),
                //Priority = (OrderPriorities)Enum.Parse(typeof(OrderPriorities), order.Priority),
                FrequencyId = order.FrequencyScheduleId,
                FrequencySchedule = MedicationMapper.MapMedicationFrequency(order.FrequencySchedule),
                Prn = order.Prn,
                PointInTime = order.PointInTime,
                BeginDatetime = order.BeginDatetime,
                //BeginDate = DateTimeHelper.GetDate(order.BeginDatetime, dateFormat),
                //BeginTime = DateTimeHelper.GetTime(order.BeginDatetime),
                EndDatetime = order.EndDatetime,
                //EndDate = DateTimeHelper.GetDate(order.EndDatetime, dateFormat),
                //EndTime = DateTimeHelper.GetTime(order.EndDatetime),
                UserQuickListItemId = order.UserQuickListItemId,
                OrderNotes = order.OrderNotes,
                CartOrderAdministrations = order.CartOrderAdministrations
                    .Select(a => MapCartOrderAdministration(a, dateFormat)).ToList()
            };

            return orderDto;
        }

        public static PatientCartOrder MapCartOrderDto(CartOrderIuDto orderDto)
        {
            if (orderDto == null)
                return null;

            PatientCartOrder order = new PatientCartOrder
            {
                Id = orderDto.Id,
                PatientId = orderDto.PatientId,
                UserId = orderDto.UserId,
                AddDatetime = orderDto.AddDatetime,
                //Ndc = orderDto.Ndc,
                //DrugId = orderDto.DrugId,
                //BrandName = orderDto.BrandName,
                MedicationId = orderDto.MedicationId,
                Dose = orderDto.Dose,
                MedicationUnitId = orderDto.MedicationUnitId,
                MedicationRouteId = orderDto.MedicationRouteId,
                Priority = Convert.ToByte(orderDto.Priority),
                FrequencyScheduleId = orderDto.FrequencyId,
                Prn = orderDto.Prn,
                PointInTime = orderDto.PointInTime,
                BeginDatetime = orderDto.BeginDatetime,
                EndDatetime = orderDto.EndDatetime,
                OrderNotes = orderDto.OrderNotes,
                CartOrderAdministrations = orderDto.CartOrderAdministrations?.Select(MapCartOrderAdministrationToDto).ToList()
            };

            return order;
        }

        public static CartOrderAdministrationDto MapCartOrderAdministration(CartOrderAdministration administration,
            string dateFormat)
        {
            if (administration == null)
                return null;

            CartOrderAdministrationDto administrationDto = new CartOrderAdministrationDto
            {
                Id = administration.Id,
                DateFormat = dateFormat,
                PatientCartOrderId = administration.PatientCartOrderId,
                AdministrationScheduledDatetime = administration.AdministrationScheduledDatetime,
                //AdministrationScheduledDate = DateTimeHelper.GetDate(administration.AdministrationScheduledDatetime, dateFormat),
                //AdministrationScheduledTime = DateTimeHelper.GetTime(administration.AdministrationScheduledDatetime),
                StopScheduledDatetime = administration.StopScheduledDatetime,
                //StopScheduledDate = DateTimeHelper.GetDate(administration.StopScheduledDatetime, dateFormat),
                //StopScheduledTime = DateTimeHelper.GetTime(administration.StopScheduledDatetime),
                PointInTime = administration.PointInTime
            };

            return administrationDto;
        }

        private static CartOrderAdministration MapCartOrderAdministrationToDto(CartOrderAdministrationDto adminDto)
        {
            if (adminDto == null)
                return null;

            CartOrderAdministration admin = new CartOrderAdministration
            {
                Id = adminDto.Id,
                PatientCartOrderId = adminDto.PatientCartOrderId,
                AdministrationScheduledDatetime = adminDto.AdministrationScheduledDatetime,
                StopScheduledDatetime = adminDto.StopScheduledDatetime,
                PointInTime = adminDto.PointInTime
            };

            return admin;
        }

        public static CartOrderAdministration MapFrequencyScheduleAdminToCartOrderAdmin(FrequencyScheduleAdministration admin)
        {
            if (admin == null)
                return null;

            CartOrderAdministration administration = new CartOrderAdministration
            {
                AdministrationScheduledDatetime = admin.ScheduleDateTime,
                StopScheduledDatetime = admin.StopDateTime,
                PointInTime = admin.PointInTime
            };

            return administration;
        }
    }
}
