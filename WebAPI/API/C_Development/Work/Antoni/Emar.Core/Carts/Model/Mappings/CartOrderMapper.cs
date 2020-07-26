using System.Linq;
using Emar.Data.Entities;

namespace Emar.Core.Carts.Model.Mappings
{
    public static class CartOrderMapper
    {
        public static CartOrderDto MapCartOrder(PatientCartOrder order)
        {
            if (order == null)
            {
                return null;
            }

            CartOrderDto orderDto = new CartOrderDto
            {
                Id = order.Id,
                PatientId = order.PatientId,
                UserId = order.UserId,
                User = order.User,
                AddDatetime = order.AddDatetime,
                Ndc = order.Ndc,
                DrugId = order.DrugId,
                BrandName = order.BrandName,
                Dose = order.Dose,
                DoseUnit = order.DoseUnit,
                MedicationRouteId = order.MedicationRouteId,
                MedicationRoute = order.MedicationRoute,
                ////Priority = (OrderPriorities)Enum.Parse(typeof(OrderPriorities), order.Priority),
                FrequencyId = order.FrequencyId,
                Prn = order.Prn,
                PointInTime = order.PointInTime,
                BeginDatetime = order.BeginDatetime,
                EndDatetime = order.EndDatetime,
                OrderNotes = order.OrderNotes,
                CartOrderAdministrations = order.CartOrderAdministrations
            };

            return orderDto;
        }

        public static PatientCartOrder MapCartOrderDto(CartOrderDto orderDto)
        {
            if (orderDto == null)
            {
                return null;
            }

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
                DoseUnit = orderDto.DoseUnit,
                MedicationRouteId = orderDto.MedicationRouteId,
                MedicationRoute = orderDto.MedicationRoute,
                ////Priority = (OrderPriorities)Enum.Parse(typeof(OrderPriorities), order.Priority),
                FrequencyId = orderDto.FrequencyId,
                Prn = orderDto.Prn,
                PointInTime = orderDto.PointInTime,
                BeginDatetime = orderDto.BeginDatetime,
                EndDatetime = orderDto.EndDatetime,
                OrderNotes = orderDto.OrderNotes,
                CartOrderAdministrations = orderDto.CartOrderAdministrations.ToList()
            };

            return order;
        }

        public static CartOrderAdministrationDto MapCartOrderAdministration(CartOrderAdministration administration)
        {
            if (administration == null)
            {
                return null;
            }

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
    }
}
