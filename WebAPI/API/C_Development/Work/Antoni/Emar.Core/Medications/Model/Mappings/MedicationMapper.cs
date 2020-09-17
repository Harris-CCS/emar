using System;
using System.Linq;
using Emar.Core.Users.Model.Mappings;
using Emar.Data.Entities;

namespace Emar.Core.Medications.Model.Mappings
{
    public static class MedicationMapper
    {
        public static MedicationRouteDto MapMedicationRoute(MedicationRoute medicationRoute)
        {
            if (medicationRoute == null)
            {
                return null;
            }

            var ret = new MedicationRouteDto
            {
                Id = medicationRoute.Id,
                RouteName = medicationRoute.Name,
                SiteId = medicationRoute.SiteId
            };

            return ret;
        }

        public static MedicationUnitDto MapMedicationUnit(MedicationUnit medicationUnit)
        {
            if (medicationUnit == null)
            {
                return null;
            }

            var ret = new MedicationUnitDto
            {
                Id = medicationUnit.Id,
                UnitName = medicationUnit.Name,
                SiteId = medicationUnit.SiteId,
                Code = medicationUnit.Code,
                PrintName = medicationUnit.PrintName,
                Active = medicationUnit.IsActive
            };

            return ret;
        }

        public static FrequencyScheduleDto MapMedicationFrequency(FrequencySchedule dbObj)
        {
            if (dbObj == null)
            {
                return null;
            }

            var ret = new FrequencyScheduleDto
            {
                Id = dbObj.Id,
                ScheduleName = dbObj.Name,
                SiteId = dbObj.SiteId,
                PointInTime = dbObj.PointInTime,
                Notes = dbObj.Notes
                //int FrequencyTypeId { get; set; }
                //int FrequencyTypeRecurring { get; set; }
                //int FrequencyInterval { get; set; }
                //int FrequencyIntervalUnitId { get; set; }
                //TimeSpan IntervalStartTime { get; set; }
                //short IntervalEndMinutes { get; set; }
            };

            return ret;
        }

        public static DrugInteractionViewDto MapDrugInteractionsView(DrugInteractionView drugInteractionsView, string drugDBVendor)
        {
            if (drugInteractionsView == null)
            {
                return null;
            }

            DrugInteractionViewDto drugInteractionViewDto = new DrugInteractionViewDto
            {
                Id = drugInteractionsView.Id,
                InteractionDrug1 = drugInteractionsView.InteractionDrug1,
                InteractionDrug2 = drugInteractionsView.InteractionDrug2,
                Severity = drugDBVendor == DrugDBVendors.FDB ? ((FDBInteractionSeverity)drugInteractionsView.Severity).ToString() :
                           drugDBVendor == DrugDBVendors.Multum ? ((MULTUMInteractionSeverity)drugInteractionsView.Severity).ToString() :
                           "",
                OverrideReasonDatetime = drugInteractionsView.OverrideReasonDatetime,
                OverrideReasonUser = UserMapper.MapUser(drugInteractionsView.OverrideReasonUser),
                OverrideReason = MapOverrideReason(drugInteractionsView.OverrideReason),
                OrderId1 = drugInteractionsView.OrderId1,
                OrderTable1 = drugInteractionsView.OrderTable1,
                OrderName1 = drugInteractionsView.OrderName1,
                OrderId2 = drugInteractionsView.OrderId2,
                OrderTable2 = drugInteractionsView.OrderTable2,
                OrderName2 = drugInteractionsView.OrderName2
            };

            return drugInteractionViewDto;
        }

        public static AllergyReactionView MapAllergyReactionViewDto(AllergyReactionViewDto allergyReactionViewDto)
        {
            if (allergyReactionViewDto == null)
            {
                return null;
            }

            AllergyReactionView allergyReactionView = new AllergyReactionView
            {
                Id = allergyReactionViewDto.Id,
                PatientAllergyId = allergyReactionViewDto.PatientAllergyId,
                OverrideReasonId = allergyReactionViewDto.OverrideReasonId,
                OverrideReasonUserId = allergyReactionViewDto.OverrideReasonUserId,
                OverrideReasonDatetime = allergyReactionViewDto.OverrideReasonDatetime,
                OrderId = allergyReactionViewDto.OrderId,
                OrderTable = allergyReactionViewDto.OrderTable,
                BrandName = allergyReactionViewDto.BrandName,
                AllergyName = allergyReactionViewDto.AllergyName
            };

            return allergyReactionView;
        }

        public static AllergyReactionViewDto MapAllergyReactionView(AllergyReactionView allergyReactionView, string drugDBVendor)
        {
            if (allergyReactionView == null)
            {
                return null;
            }

            AllergyReactionViewDto allergyReactionViewDto = new AllergyReactionViewDto
            {
                Id = allergyReactionView.Id,
                PatientAllergyId = allergyReactionView.PatientAllergyId,
                OverrideReasonDatetime = allergyReactionView.OverrideReasonDatetime,
                OverrideReasonUser = UserMapper.MapUser(allergyReactionView.OverrideReasonUser),
                OverrideReason = MapOverrideReason(allergyReactionView.OverrideReason),
                OrderId = allergyReactionView.OrderId,
                OrderTable = allergyReactionView.OrderTable,
                BrandName = allergyReactionView.BrandName,
                AllergyName = allergyReactionView.AllergyName
            };

            return allergyReactionViewDto;
        }

        public static DrugInteractionView MapDrugInteractionsViewDto(DrugInteractionViewDto drugInteractionsViewDto)
        {
            if (drugInteractionsViewDto == null)
            {
                return null;
            }

            DrugInteractionView drugInteractionView = new DrugInteractionView
            {
                Id = drugInteractionsViewDto.Id,
                InteractionDrug1 = drugInteractionsViewDto.InteractionDrug1,
                InteractionDrug2 = drugInteractionsViewDto.InteractionDrug2,
                Severity = Enum.TryParse(drugInteractionsViewDto.Severity, out byte severity) ? severity : (byte)0,
                OverrideReasonDatetime = drugInteractionsViewDto.OverrideReasonDatetime,
                OverrideReasonUserId = drugInteractionsViewDto.OverrideReasonUserId,
                OverrideReasonId = drugInteractionsViewDto.OverrideReasonId,
                OrderId1 = drugInteractionsViewDto.OrderId1,
                OrderTable1 = drugInteractionsViewDto.OrderTable1,
                OrderName1 = drugInteractionsViewDto.OrderName1,
                OrderId2 = drugInteractionsViewDto.OrderId2,
                OrderTable2 = drugInteractionsViewDto.OrderTable2,
                OrderName2 = drugInteractionsViewDto.OrderName2
            };

            return drugInteractionView;
        }

        public static OrderReactionDto MapOrderReaction(OrderReaction orderReaction, string drugDBVendor)
        {
            if (orderReaction == null)
            {
                return null;
            }

            OrderReactionDto orderReactionDto = new OrderReactionDto
            {
                Id = orderReaction.Id,
                PatientAllergyId = orderReaction.PatientAllergyId,
                PatientOrderId = orderReaction.PatientOrderId,
                PatientCartOrderId = orderReaction.PatientCartOrderId,
                OverrideReasonDatetime = orderReaction.OverrideReasonDatetime,
                OverrideReasonUser = UserMapper.MapUser(orderReaction.OverrideReasonUser),
                OverrideReason = MapOverrideReason(orderReaction.OverrideReason)
            };

            return orderReactionDto;
        }

        public static OrderReaction MapOrderReactionDto(OrderReactionDto orderReactionDto)
        {
            if (orderReactionDto == null)
            {
                return null;
            }

            OrderReaction orderReaction = new OrderReaction
            {
                Id = orderReactionDto.Id,
                PatientAllergyId = orderReactionDto.PatientAllergyId,
                PatientOrderId = orderReactionDto.PatientOrderId,
                PatientCartOrderId = orderReactionDto.PatientCartOrderId,
                OverrideReasonDatetime = orderReactionDto.OverrideReasonDatetime,
                OverrideReasonUserId = orderReactionDto.OverrideReasonUserId,
                OverrideReasonId = orderReactionDto.OverrideReasonId
            };

            return orderReaction;
        }

        public static MedicationInteractionDto MapMedicationInteraction(MedicationInteraction medicationInteraction, string drugDBVendor)
        {
            if (medicationInteraction == null)
            {
                return null;
            }

            MedicationInteractionDto medicationInteractionDto = new MedicationInteractionDto
            {
                Id = medicationInteraction.Id,
                InteractionDrug1 = medicationInteraction.InteractionDrug1,
                InteractionDrug2 = medicationInteraction.InteractionDrug2,
                InteractionDrugName2 = medicationInteraction.InteractionDrugName2,
                Severity = drugDBVendor == DrugDBVendors.FDB ? ((FDBInteractionSeverity)medicationInteraction.Severity).ToString() :
                           drugDBVendor == DrugDBVendors.Multum ? ((MULTUMInteractionSeverity)medicationInteraction.Severity).ToString() :
                           "",
                OverrideReasonDatetime = medicationInteraction.OverrideReasonDatetime,
                OverrideReasonUser = UserMapper.MapUser(medicationInteraction.OverrideReasonUser),
                OverrideReason = MapOverrideReason(medicationInteraction.OverrideReason),
                OrderInteractions = medicationInteraction.OrderInteractions?.Select(interaction => MapOrderInteraction(interaction, drugDBVendor)).ToList(),
            };

            return medicationInteractionDto;
        }

        public static MedicationInteraction MapMedicationInteractionDto(MedicationInteractionDto medicationInteractionDto)
        {
            if (medicationInteractionDto == null)
            {
                return null;
            }

            MedicationInteraction medicationInteraction = new MedicationInteraction
            {
                Id = medicationInteractionDto.Id,
                InteractionDrug1 = medicationInteractionDto.InteractionDrug1,
                InteractionDrug2 = medicationInteractionDto.InteractionDrug2,
                InteractionDrugName2 = medicationInteractionDto.InteractionDrugName2,
                Severity = Enum.TryParse(medicationInteractionDto.Severity, out byte severity) ? severity : (byte)0,
                OverrideReasonDatetime = medicationInteractionDto.OverrideReasonDatetime,
                OverrideReasonUserId = medicationInteractionDto.OverrideReasonUserId,
                OverrideReasonId = medicationInteractionDto.OverrideReasonId,
                OrderInteractions = medicationInteractionDto.OrderInteractions?.Select(interactionDto => MapOrderInteractionDto(interactionDto)).ToList(),
            };

            return medicationInteraction;
        }

        public static OrderInteractionDto MapOrderInteraction(OrderInteraction orderInteraction, string drugDBVendor)
        {
            if (orderInteraction == null)
            {
                return null;
            }

            OrderInteractionDto orderInteractionDto = new OrderInteractionDto
            {
                Id = orderInteraction.Id,
                MedicationInteractionId = orderInteraction.MedicationInteractionId,
                //DrugNum = orderInteraction.DrugNum,
                PatientOrderId = orderInteraction.PatientOrderId,
                PatientCartOrderId = orderInteraction.PatientCartOrderId,
                PatientHomeMedicationId = orderInteraction.PatientHomeMedicationId,
                DrugInteraction = MapDrugInteractionsView(orderInteraction.DrugInteractionView, drugDBVendor),
            };

            return orderInteractionDto;
        }

        public static OrderInteraction MapOrderInteractionDto(OrderInteractionDto orderInteractionDto)
        {
            if (orderInteractionDto == null)
            {
                return null;
            }

            OrderInteraction orderInteraction = new OrderInteraction
            {
                Id = orderInteractionDto.Id,
                MedicationInteractionId = orderInteractionDto.MedicationInteractionId,
                PatientOrderId = orderInteractionDto.PatientOrderId,
                PatientCartOrderId = orderInteractionDto.PatientCartOrderId,
                PatientHomeMedicationId = orderInteractionDto.PatientHomeMedicationId,
                DrugInteractionView = MapDrugInteractionsViewDto(orderInteractionDto.DrugInteraction)
            };

            return orderInteraction;
        }

        public static OverrideReasonDto MapOverrideReason(OverrideReason overrideReason)
        {
            if (overrideReason == null)
            {
                return null;
            }

            OverrideReasonDto overrideReasonDto = new OverrideReasonDto
            {
                Id = overrideReason.Id,

                SiteId = overrideReason.SiteId,
                IsMedication = overrideReason.IsMedication,
                Description = overrideReason.Description
            };

            return overrideReasonDto;
        }

        public static OverrideReason MapOverrideReasonDto(OverrideReasonDto overrideReasonDto)
        {
            if (overrideReasonDto == null)
            {
                return null;
            }

            OverrideReason overrideReason = new OverrideReason
            {
                Id = overrideReasonDto.Id,

                SiteId = overrideReasonDto.SiteId,
                IsMedication = overrideReasonDto.IsMedication,
                Description = overrideReasonDto.Description
            };

            return overrideReason;
        }
    }
}