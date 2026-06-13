using Emar.Core.Medications.Model;
using Emar.Core.Medications.Model.Mappings;
using Emar.Core.Orders.Model;
using Emar.Core.Orders.Model.Mappings;
using Emar.Core.Orders.Repository;
using Emar.Core.Patients.Model.Mappings;
using Emar.Core.ResourceParameters;
using Emar.Core.Users.Model.Mappings;
using Emar.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Emar.Core.Carts.Model.Mappings
{
    public static class CartOrderMapper
    {
        public static CartOrderDto MapCartOrder(PatientCartOrder order, string drugDbVendor,
            List<CodeSharedId> codeShareSites, BaseLinkResource resource = null,
            string? TabToLoad = null, string? pathwayName = null)
        {
            if (order == null)
                return null;

            var orderDto = new CartOrderDto
            {
                Id = order.Id,
                PatientId = order.PatientId,
                UserId = order.UserId,
                User = UserMapper.MapUser(order.User),
                AddDatetime = order.AddDatetime,
                MedicationId = order.MedicationId,
                Medication = MedicationMapper.MapMedication(
                    order.Medication,
                    codeShareSites
                        .FirstOrDefault(c =>
                            c.Entity == OrderRepository.CodeShareEntity.MedicationUnit)?
                        .SharedSiteId),
                Dose = order.Dose,
                // 20201217 BRM: Taking out the "gate-displaying" - because Romel said so.
                DoseUnit =  OrderMapper.MapMedicationUnit(order.MedicationUnit),
                MedicationRoute = OrderMapper.MapMedicationRoute(order.MedicationRoute),
                Priority = (OrderPriorities)order.Priority,
                FrequencyId = order.FrequencyScheduleId,
                // 20201217 BRM: Taking out the "gate-displaying" - because Romel said so.
                FrequencySchedule = OrderMapper.MapFrequencySchedule(order.FrequencySchedule),
                Prn = order.Prn,
                PointInTime = order.PointInTime,
                BeginDatetime = order.BeginDatetime,
                EndDatetime = order.EndDatetime,
                UserQuickListItemId = order.UserQuickListItemId,
                OrderNotes = order.OrderNotes,
                AntimicrobialIndicationId = order.AntimicrobialIndicationId,
                AntimicrobialIndication = MedicationMapper.MapAntimicrobial(order.AntimicrobialIndication),
                AntimicrobialIndicationText = order.AntimicrobialIndicationText,
                PatientProblemId = order.PatientProblemId,
                PatientProblem = PatientMapper.MapPatientProblem(order.PatientProblem),
                Duration = order.Duration,
                DurationUnitId = order.DurationUnitId,
                DurationUnit = OrderMapper.MapDurationUnit(order.DurationUnit),
                // 20201217 BRM: Adding administration sorting for Mario
                CartOrderAdministrations = order.CartOrderAdministrations
                    .Select(MapCartOrderAdministration)
                    .ToList()
                    .OrderBy(a => a.AdministrationScheduledDatetime),
                OrderInteractions = order.OrderInteractions?.Select(interaction => MedicationMapper.MapOrderInteraction(interaction, drugDbVendor, resource)).ToList(),
                AllergyReactions = order.AllergyReactionsView?.Select(MedicationMapper.MapAllergyReactionView).ToList(),

                Ndc = order.Ndc,
                PrnIndication = order.PrnIndication,
                TabToLoad = TabToLoad,
                PathwayName = pathwayName
            };

            return orderDto;
        }

        public static PatientCartOrder MapCartOrderDto(CartOrderIuDto orderDto, string drugDbVendor)
        {
            if (orderDto == null)
                return null;

            //If the root has Prn set to true, then use that.
            //If the root has Prn set to false, then look at the frequency schedule and use that value.
            //If the root has Prn set to false and the frequency schedule doesn't have Prn set, then use false.
            //Winston Murdock, 08/10/2021.  EMAR-1110.
            bool tempPrn = orderDto.FrequencySchedule?.FrequencyType.Name.ToUpper().Equals("PRN") ?? false;
            if (!orderDto.Prn)
            {
                orderDto.Prn = tempPrn;
            } //end if

            //If the root has PointInTime set to true, then use that.
            //If the root has PointInTime set to false, then look at the frequency schedule and use that value.
            //If the root has PointInTime set to false and the frequency schedule doesn't have PointInTime set, then use false.
            //Winston Murdock, 08/10/2021.  EMAR-1119.
            bool tempPointInTime = orderDto.FrequencySchedule?.PointInTime ?? false;
            if (!orderDto.PointInTime)
            {
                orderDto.PointInTime = tempPointInTime;
            } //end if

            var order = new PatientCartOrder
            {
                Id = orderDto.Id,
                PatientId = orderDto.PatientId,
                UserId = orderDto.UserId,
                AddDatetime = orderDto.AddDatetime,
                MedicationId = orderDto.MedicationId,
                Dose = orderDto.Dose,
                MedicationUnitId = orderDto.MedicationUnitId,
                MedicationRouteId = orderDto.MedicationRouteId,
                Priority = Convert.ToByte(orderDto.Priority),
                FrequencyScheduleId = orderDto.FrequencyId,
                //Use the Prn value we set above.
                //Winston Murdock, 08/10/2021.  EMAR-1110.
                //Prn = orderDto.FrequencySchedule?.FrequencyType.Name.ToUpper().Equals("PRN") ?? false,
                Prn = orderDto.Prn,
                //Use the PointInTime value we set above.
                //Winston Murdock, 08/10/2021.  EMAR-1119.
                //PointInTime = orderDto.FrequencySchedule?.PointInTime ?? false,
                PointInTime = orderDto.PointInTime,
                BeginDatetime = orderDto.BeginDatetime,
                //If we have a value for EndDateTime, pass it along.
                //If we don't, this will pass along null.
                //Before this change, we were relying on the code below to calculate
                //it based on the scheduled time of the last administration.
                //But we are only doing that if we do not have a value for EndDateTime
                //and if the user did select a Duration.
                //So we must accept the user's entry here.
                //Winston Murdock, 02/16/2022.  PC-27021
                EndDatetime = orderDto.EndDatetime,
                OrderNotes = orderDto.OrderNotes,
                UserQuickListItemId = (int?)orderDto.UserQuickListItemId,
                AntimicrobialIndicationId = orderDto.AntimicrobialIndicationId,
                Duration = orderDto.Duration,
                DurationUnitId = orderDto.DurationUnitId,
                PatientProblemId = orderDto.PatientProblemId,
                AntimicrobialIndicationText = orderDto.AntimicrobialIndicationText,
                CartOrderAdministrations = orderDto.CartOrderAdministrations?
                    //Use the PointInTime value we set above.
                    //Winston Murdock, 08/10/2021.  EMAR-1119.
                    //.Select(a => MapCartOrderAdministrationToDto(a, orderDto.FrequencySchedule?.PointInTime ?? false))
                    .Select(a => MapCartOrderAdministrationToDto(a, orderDto.PointInTime))
                    .ToList(),
                OrderInteractions = orderDto.OrderInteractions?.Select(
                    o => MedicationMapper.MapOrderInteractionDto(o, drugDbVendor)
                ).ToList(),
                AllergyReactionsView = orderDto.AllergyReactions?.Select(MedicationMapper.MapAllergyReactionViewDto).ToList(),
                
                Ndc = orderDto.Ndc,
                PrnIndication = orderDto.PrnIndication
            };

            //Do this big line twice.
            //Once to calculate the scheduled stop time for the last administration.
            //The second time to calculate the order's end date time if we need to calculate it.
            //Winston Murdock, 02/09/2022.  PC-26986

            //Calculate the scheduled stop time for the last administration
            var lastAdministrationScheduledStopTime = orderDto.EndDatetime
                                // duration and duration unit have been set
                                ?? (orderDto.Duration != null && orderDto.DurationUnit != null
                                    // duration unit is not "dose"
                                    ? orderDto.DurationUnit.DurationInMinutes != 0
                                        // calculate total number of minutes from selected duration
                                        // and duration unit and add to order's begin datetime
                                        ? orderDto.BeginDatetime.AddMinutes(
                                            (double) orderDto.Duration * orderDto.DurationUnit.DurationInMinutes)
                                        : (DateTimeOffset?) null
                                          // duration unit is "dose" and there are administrations
                                          ?? (order.CartOrderAdministrations.Any()
                                              // calculate total number of minutes from order's begin datetime
                                              // until selected (via duration) administration's scheduled start datetime
                                              // and add to order's begin datetime
                                              ? orderDto.BeginDatetime
                                                  .AddMinutes(order.CartOrderAdministrations.ToList()
                                                      [(int) orderDto.Duration > order.CartOrderAdministrations.Count
                                                          ? order.CartOrderAdministrations.Count - 1
                                                          : (int) orderDto.Duration - 1]
                                                      .AdministrationScheduledDatetime.Subtract(orderDto.BeginDatetime)
                                                      .TotalMinutes)
                                              : (DateTimeOffset?) null)
                                    : order.CartOrderAdministrations.Any()
                                        // calculate total number of minutes from order's begin datetime
                                        // until last administration's scheduled start datetime
                                        // and add to order's begin datetime
                                        ? orderDto.BeginDatetime.AddMinutes(order.CartOrderAdministrations
                                            .OrderBy(a => a.AdministrationScheduledDatetime).Last()
                                            .AdministrationScheduledDatetime.Subtract(orderDto.BeginDatetime)
                                            .TotalMinutes)
                                        : (DateTimeOffset?) null);

            //Calculate the end datetime for the order.
            //If the user selected one in the UI, then we don't need to calculate it and update the value.
            //If the user did not select one but they did select a duration (x doses, minutes, hours, days, etc...),
            //then calculate it based on how long the duration is.
            //If the user did not select one and did not select a duration, then
            //we don't want to have an end time listed.
            //Winston Murdock, 02/09/2022.  PC-26986

            //We only want to calculate and set the EndDateTime if we don't have a value for it and if we do have a duration.
            //If we do have an EndDatetime, then don't change it.
            //If we don't have an EndDateTime and we don't have a duration, then it's appropriate for us not to have an EndDateTime.
            //Winston Murdock, 02/16/2022.  PC-27021
            //if (!order.EndDatetime.HasValue)
            if ((!order.EndDatetime.HasValue) && (order.Duration != null) && (order.DurationUnit != null))
                {
                order.EndDatetime = orderDto.EndDatetime
                                    // duration and duration unit have been set
                                    ?? (orderDto.Duration != null && orderDto.DurationUnit != null
                                        // duration unit is not "dose"
                                        ? orderDto.DurationUnit.DurationInMinutes != 0
                                            // calculate total number of minutes from selected duration
                                            // and duration unit and add to order's begin datetime
                                            ? orderDto.BeginDatetime.AddMinutes(
                                                (double)orderDto.Duration * orderDto.DurationUnit.DurationInMinutes)
                                            : (DateTimeOffset?)null
                                              // duration unit is "dose" and there are administrations
                                              ?? (order.CartOrderAdministrations.Any()
                                                  // calculate total number of minutes from order's begin datetime
                                                  // until selected (via duration) administration's scheduled start datetime
                                                  // and add to order's begin datetime
                                                  ? orderDto.BeginDatetime
                                                      .AddMinutes(order.CartOrderAdministrations.ToList()
                                                          [(int)orderDto.Duration > order.CartOrderAdministrations.Count
                                                              ? order.CartOrderAdministrations.Count - 1
                                                              : (int)orderDto.Duration - 1]
                                                          .AdministrationScheduledDatetime.Subtract(orderDto.BeginDatetime)
                                                          .TotalMinutes)
                                                  : (DateTimeOffset?)null)
                                        : (DateTimeOffset?)null);
            } //end if

            if (lastAdministrationScheduledStopTime != null)
            {
                order.CartOrderAdministrations =
                    order.CartOrderAdministrations
                        .Where(a =>
                            a.AdministrationScheduledDatetime == lastAdministrationScheduledStopTime
                            || a.AdministrationScheduledDatetime < lastAdministrationScheduledStopTime)
                        .ToList();

                // 20201217 BRM: Adding the below .Any() check so we don't crap out on PRN orders
                if (order.CartOrderAdministrations.Any() &&
                    !order.CartOrderAdministrations.Last().PointInTime)
                {
                    order.CartOrderAdministrations.Last().StopScheduledDatetime = lastAdministrationScheduledStopTime;
                }
            }

            return order;
        }

        private static CartOrderAdministrationDto MapCartOrderAdministration(CartOrderAdministration administration)
        {
            if (administration == null)
                return null;

            var administrationDto = new CartOrderAdministrationDto
            {
                Id = administration.Id,
                PatientCartOrderId = administration.PatientCartOrderId,
                AdministrationScheduledDatetime = administration.AdministrationScheduledDatetime,
                StopScheduledDatetime = administration.StopScheduledDatetime,
                PointInTime = administration.PointInTime
            };

            return administrationDto;
        }

        private static CartOrderAdministration MapCartOrderAdministrationToDto(CartOrderAdministrationDto adminDto,
            bool frequencySchedulePointInTime)
        {
            if (adminDto == null)
                return null;

            var admin = new CartOrderAdministration
            {
                Id = adminDto.Id,
                PatientCartOrderId = adminDto.PatientCartOrderId,
                AdministrationScheduledDatetime = adminDto.AdministrationScheduledDatetime,
                StopScheduledDatetime = adminDto.StopScheduledDatetime,
                PointInTime = frequencySchedulePointInTime
            };

            return admin;
        }

        public static CartOrderAdministration MapFrequencyScheduleAdminToCartOrderAdmin(FrequencyScheduleAdministration admin)
        {
            if (admin == null)
                return null;

            var administration = new CartOrderAdministration
            {
                AdministrationScheduledDatetime = admin.ScheduleDateTime,
                StopScheduledDatetime = admin.StopDateTime,
                PointInTime = admin.PointInTime
            };

            return administration;
        }

        public static MedicationModel MapPatientCartOrderToModel(PatientCartOrder order, int userId, int siteId, int? codeShareSiteMedicationUnit)
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
                Category = null,
                ChangeDatetime = null,
                ChangeUserId = null,
                Class = null,
                Comment = order.OrderNotes,
                Dose = order.Dose,
                EndDatetime = null,
                FrequencyScheduleId = order.FrequencyScheduleId,
                InternalDrugId = null,
                IsActive = null,
                MedicationId = order.MedicationId,
                Medication = MedicationMapper.MapMedication(order.Medication, codeShareSiteMedicationUnit),
                MedicationDrugId = null,
                MedicationRouteId = order.MedicationRouteId,
                MedicationUnitId = order.MedicationUnitId,
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
                AccountNumber = null,

                Ndc = order.Ndc,
                PrnIndication = order.PrnIndication
            };
        }

        public static OverrideReasonDto MapOverrideReason(OverrideReason reason)
        {
            if (reason == null)
            {
                return null;
            }

            return new OverrideReasonDto
            {
                Id = reason.Id,
                SiteId = reason.SiteId,
                IsMedication = reason.IsMedication,
                Description = reason.Description
            };
        }
    }
}