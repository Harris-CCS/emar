using System.Linq;
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

            CartOrderDto orderDto = new CartOrderDto
            {
                Id = order.Id,
                PatientId = order.PatientId,
                UserId = order.UserId,
                User = UserMapper.MapUser(order.User),
                AddDatetime = order.AddDatetime,
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
                EndDatetime = order.EndDatetime,
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
                MedicationUnitId = orderDto.DoseUnit.Id,
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

            CartOrderAdministrationDto administrationDto = new CartOrderAdministrationDto
            {
                Id = administration.Id,
                PatientCartOrderId = administration.PatientCartOrderId,
                AdministrationScheduledDatetime = administration.AdministrationScheduledDatetime,
                StopScheduledDatetime = administration.StopScheduledDatetime,
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
