using System.Linq;
using Emar.Core.Helpers;
using Emar.Core.Medications.Model.Mappings;
using Emar.Core.Users.Model.Mappings;
using Emar.Data.Entities;

namespace Emar.Core.Carts.Model.Mappings
{
    public static class CartOrderMapper
    {
        public static CartOrderDto MapCartOrder(PatientCartOrder order)
        {
            if (order == null)
                return null;

            var dateFormat = order.Patient.Site.SiteOptions.FirstOrDefault(si => si.Option.Name == @"LONG_DATE_FORMAT").OptionValue;

            CartOrderDto orderDto = new CartOrderDto
            {
                Id = order.Id,
                PatientId = order.PatientId,
                UserId = order.UserId,
                User = UserMapper.MapUser(order.User),
                AddDatetime = order.AddDatetime,
                AddDate = DateTimeHelper.GetDate(order.AddDatetime, dateFormat),
                AddTime = DateTimeHelper.GetTime(order.AddDatetime),
                Ndc = order.Ndc,
                DrugId = order.DrugId,
                BrandName = order.BrandName,
                Dose = order.Dose,
                DoseUnit = MedicationMapper.MapMedicationUnit(order.MedicationUnit),
                MedicationRoute = MedicationMapper.MapMedicationRoute(order.MedicationRoute),
                //Priority = (OrderPriorities)Enum.Parse(typeof(OrderPriorities), order.Priority),
                FrequencyId = order.FrequencyId,
                Prn = order.Prn,
                PointInTime = order.PointInTime,
                BeginDatetime = order.BeginDatetime,
                BeginDate = DateTimeHelper.GetDate(order.BeginDatetime, dateFormat),
                BeginTime = DateTimeHelper.GetTime(order.BeginDatetime),
                EndDatetime = order.EndDatetime,
                EndDate = DateTimeHelper.GetDate(order.EndDatetime, dateFormat),
                EndTime = DateTimeHelper.GetTime(order.EndDatetime),
                OrderNotes = order.OrderNotes,
                CartOrderAdministrations = order.CartOrderAdministrations.Select(MapCartOrderAdministration).ToList()
            };

            return orderDto;
        }

        public static PatientCartOrder MapCartOrderDto(CartOrderDto orderDto)
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
                MedicationUnitId = orderDto.DoseUnit?.Id,
                MedicationRouteId = orderDto.MedicationRoute?.Id,
                //MedicationRoute = orderDto.MedicationRoute,
                ////Priority = (OrderPriorities)Enum.Parse(typeof(OrderPriorities), order.Priority),
                FrequencyId = orderDto.FrequencyId,
                Prn = orderDto.Prn,
                PointInTime = orderDto.PointInTime,
                BeginDatetime = orderDto.BeginDatetime,
                EndDatetime = orderDto.EndDatetime,
                OrderNotes = orderDto.OrderNotes,
                CartOrderAdministrations = orderDto.CartOrderAdministrations?.Select(MapCartOrderAdministrationToDto).ToList()
            };

            return order;
        }

        public static CartOrderAdministrationDto MapCartOrderAdministration(CartOrderAdministration administration)
        {
            if (administration == null)
                return null;

            var dateFormat = administration.PatientCartOrder.Patient.Site.SiteOptions.FirstOrDefault(si => si.Option.Name == @"LONG_DATE_FORMAT").OptionValue;

            CartOrderAdministrationDto administrationDto = new CartOrderAdministrationDto
            {
                Id = administration.Id,
                PatientCartOrderId = administration.PatientCartOrderId,
                AdministrationScheduledDatetime = administration.AdministrationScheduledDatetime,
                AdministrationScheduledDate = DateTimeHelper.GetDate(administration.AdministrationScheduledDatetime, dateFormat),
                AdministrationScheduledTime = DateTimeHelper.GetTime(administration.AdministrationScheduledDatetime),
                StopScheduledDatetime = administration.StopScheduledDatetime,
                StopScheduledDate = DateTimeHelper.GetDate(administration.StopScheduledDatetime, dateFormat),
                StopScheduledTime = DateTimeHelper.GetTime(administration.StopScheduledDatetime),
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
    }
}
