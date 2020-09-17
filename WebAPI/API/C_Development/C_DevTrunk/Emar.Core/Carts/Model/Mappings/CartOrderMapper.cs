using System;
using System.Linq;
using Emar.Core.Medications.Model;
using Emar.Core.Medications.Model.Mappings;
using Emar.Core.Users.Model.Mappings;
using Emar.Data.Entities;

namespace Emar.Core.Carts.Model.Mappings
{
    public static class CartOrderMapper
    {
        public static CartOrderDto MapCartOrder(PatientCartOrder order, string dateFormat, string drugDBVendor)
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
                Ndc = order.Ndc,
                DrugId = order.DrugId,
                BrandName = order.BrandName,
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
                CartOrderAdministrations = order.CartOrderAdministrations.Select(a => MapCartOrderAdministration(a, dateFormat)).ToList(),
                OrderInteractions = order.OrderInteractions?.Select(interaction => MedicationMapper.MapOrderInteraction(interaction, drugDBVendor)).ToList(),
                AllergyReactions = order.AllergyReactionsView?.Select(reaction => MedicationMapper.MapAllergyReactionView(reaction, drugDBVendor)).ToList()
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
                Ndc = orderDto.Ndc,
                DrugId = orderDto.DrugId,
                BrandName = orderDto.BrandName,
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
                CartOrderAdministrations = orderDto.CartOrderAdministrations?.Select(MapCartOrderAdministrationToDto).ToList(),
                OrderInteractions = orderDto.OrderInteractions?.Select(MedicationMapper.MapOrderInteractionDto).ToList(),
                AllergyReactionsView = orderDto.AllergyReactions?.Select(MedicationMapper.MapAllergyReactionViewDto).ToList()
            };

            return order;
        }

        public static CartOrderAdministrationDto MapCartOrderAdministration(CartOrderAdministration administration, string dateFormat)
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

        public static MedicationModel MapPatientCartOrderToModel(PatientCartOrder order, int userId, int siteId)
        {
            if (order == null)
            {
                return null;
            }

            return new MedicationModel
            {
                SiteId = siteId,
                PatientId = order.PatientId,
                UserId = userId,
                SourceTable = SourceTables.PatientCartOrders,
                SourceTableId = order.Id,
                Type = EmarOrderType.PatientCartOrder,
                ActionStatus = null,
                AddDatetime = order.AddDatetime,
                AddUserId = order.UserId,
                AlternateName = null,
                BeginDatetime = null,
                BrandName = order.BrandName,
                ActiveName = order.FdbBrandName?.Active,
                ActiveId = order.FdbBrandName?.PcRoutedGenId?.ToString(),
                Category = null,
                ChangeDatetime = null,
                ChangeUserId = null,
                Class = null,
                Comment = order.OrderNotes,
                Dose = order.Dose,
                DrugId = order.DrugId,
                EndDatetime = null,
                FrequencyScheduleId = order.FrequencyScheduleId,
                InternalDrugId = null,
                IsActive = null,
                MedicationDrugId = null,
                MedicationRouteId = order.MedicationRouteId,
                MedicationUnitId = order.MedicationUnitId,
                Ndc = order.Ndc,
                OrderPhysicianUserId = null,
                OrderStatus = null,
                ParentDrugId = null,
                ParentDrugName = null,
                PointInTime = order.PointInTime,
                Priority = order.Priority,
                Prn = order.Prn,
                Reaction = null,
                Schedule = null,
                Severity = null,

                Name = null,
                AllergyDrugId = null,
                InformationSource = null,
                PersonNumber = null,
                AccountNumber = null
            };
        }
    }
}