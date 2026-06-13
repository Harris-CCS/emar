using System;
using System.Collections.Generic;
using System.Linq;
using Emar.Core.Carts.Model;
using Emar.Core.Helpers;
using Emar.Core.Medications.Model;
using Emar.Core.Medications.Model.Mappings;
using Emar.Core.Orders.Repository;
using Emar.Core.Patients.Model.Mappings;
using Emar.Core.ResourceParameters;
using Emar.Core.Templates.Model;
using Emar.Core.Templates.Model.Mappings;
using Emar.Core.Users.Model.Mappings;
using Emar.Data.Entities;
using System.Diagnostics;

namespace Emar.Core.Orders.Model.Mappings
{
    public static class OrderMapper
    {
        public static PatientOrderDto MapOrder(PatientOrder patientOrder, string drugDbVendor,
            OrderActionMapperHelper orderActionMapperHelper, List<CodeSharedId> codeShareSites, BaseLinkResource resource = null)
        {
            if (patientOrder == null)
            {
                return null;
            }

            var patientOrderDto = new PatientOrderDto
            {
                Id = patientOrder.Id,
                PatientId = patientOrder.PatientId,
                AddUserId = patientOrder.AddUserId,
                AddUser = UserMapper.MapUser(patientOrder.AddUser),
                AddDatetime = patientOrder.AddDatetime,
                OrderingPhysicianId = patientOrder.OrderingPhysicianId,
                OrderingPhysicianUser = UserMapper.MapUser(patientOrder.OrderPhysicianUser),
                MedicationId = patientOrder.MedicationId,
                Medication = MedicationMapper.MapMedication(
                    patientOrder.Medication,
                    codeShareSites
                        .FirstOrDefault(c =>
                            c.Entity == OrderRepository.CodeShareEntity.MedicationUnit)?
                        .SharedSiteId),
                Dose = patientOrder.Dose,
                DoseUnit = patientOrder.MedicationUnit != null
                           //Need to return dose units for combo meds
                           //Winston Murdock, 04/13/2021.
                           //&& patientOrder.Medication.DrugId != "COMBO"
                           //////&& patientOrder.Medication.SiteId != -1
                           && patientOrder.MedicationUnit.SiteId == codeShareSites
                               .FirstOrDefault(c =>
                                   c.Entity == OrderRepository.CodeShareEntity.MedicationUnit)?
                               .SharedSiteId
                    ? MapMedicationUnit(patientOrder.MedicationUnit)
                    : null,
                MedicationRoute = patientOrder.MedicationRoute != null
                                  //Need to return dose units for combo meds
                                  //Winston Murdock, 04/13/2021.
                                  //&& patientOrder.Medication.DrugId != "COMBO"
                                  //////&& patientOrder.Medication.SiteId != -1
                                  && patientOrder.MedicationRoute.SiteId == codeShareSites
                                      .FirstOrDefault(c =>
                                          c.Entity == OrderRepository.CodeShareEntity.MedicationRoute)?
                                      .SharedSiteId
                    ? MapMedicationRoute(patientOrder.MedicationRoute)
                    : null,
                Priority = (OrderPriorities)patientOrder.Priority,
                FrequencyId = patientOrder.FrequencyScheduleId,
                FrequencySchedule = patientOrder.FrequencySchedule != null
                                    //Need to return dose units for combo meds
                                    //Winston Murdock, 04/13/2021.
                                    //&& patientOrder.Medication.DrugId != "COMBO"
                                    //////&& patientOrder.Medication.SiteId != -1
                                    && patientOrder.FrequencySchedule.SiteId == codeShareSites
                                        .FirstOrDefault(c =>
                                            c.Entity == OrderRepository.CodeShareEntity.FrequencySchedule)?
                                        .SharedSiteId
                    ? MapFrequencySchedule(patientOrder.FrequencySchedule)
                    : null,
                Prn = patientOrder.Prn,
                PointInTime = patientOrder.PointInTime,
                OrderStatus = patientOrder.OrderStatus,
                BeginDatetime = patientOrder.BeginDatetime,
                EndDatetime = patientOrder.EndDateTime,
                OrderNotes = patientOrder.OrderNotes,
                AntimicrobialIndicationId = patientOrder.AntimicrobialIndicationId,
                AntimicrobialIndication = MedicationMapper.MapAntimicrobial(patientOrder.AntimicrobialIndication),
                AntimicrobialIndicationText = patientOrder.AntimicrobialIndicationText,
                PharmacyVerificationStatus = patientOrder.PharmacyVerificationStatus,
                PatientProblemId = patientOrder.PatientProblemId,
                PatientProblem = PatientMapper.MapPatientProblem(patientOrder.PatientProblem),
                Duration = patientOrder.Duration,
                DurationUnitId = patientOrder.DurationUnitId,
                Ndc = patientOrder.Ndc,
                PrnIndication = patientOrder.PrnIndication,
                DurationUnit = MapDurationUnit(patientOrder.DurationUnit),
                CosignNeeded = MapCosignNeeded(patientOrder),
                OrderInteractions = patientOrder.OrderInteractions?
                    .Select(interaction =>
                        MedicationMapper.MapOrderInteraction(interaction, drugDbVendor, resource))
                    .ToList(),
                AllergyReactions = patientOrder.AllergyReactionsView?
                    .Select(MedicationMapper.MapAllergyReactionView)
                    .ToList(),
                OrderStatusCode = !Enum.TryParse(patientOrder.OrderStatus, out OrderStatus orderStatus)
                    ? OrderStatus.Pending
                    : orderStatus,
                OrderEvents = patientOrder.OrderEvents?.Select(MapOrderEvent)
                    .Where(@event => @event.AdministrationId == null
                                     && (@event.ActionId == (int)ActionEnum.CoSign
                                         || @event.ActionId == (int)ActionEnum.Hold
                                         || @event.ActionId == (int)ActionEnum.UnHold
                                         || @event.ActionId == (int)ActionEnum.OrderDiscontinue
                                         || @event.ActionId == (int)ActionEnum.Cancel
                                         || @event.ActionId == (int)ActionEnum.Delete
                                         || @event.ActionId == (int)ActionEnum.CompleteDiscontinue
                                         || @event.ActionId == (int)ActionEnum.PharmVerification
                                         ))
                    .ToList()
            };


            //Added OrderDiscontinue, Cancel, Delete, and CompleteDiscontinue to the filter above.
            //Couldn't find a good place to put this comment above, so I put it here.
            //Winston Murdock, 02/20/2021.  EMAR-716.


            patientOrderDto.OrderAdministrations =
                patientOrder.OrderAdministrations?
                    .Select(admin =>
                        MapOrderAdministration(admin, patientOrderDto.OrderStatusCode,
                            orderActionMapperHelper)
                    ).ToList().OrderBy(a => a.AdministrationScheduledDatetime);

            if (orderActionMapperHelper != null)
                patientOrderDto.AvailableActions = orderActionMapperHelper.AvailableOrderActions(patientOrderDto);

            patientOrderDto.NextActionTime = null;
            if (patientOrderDto.OrderAdministrations == null) return patientOrderDto;

            foreach (var admin in patientOrderDto.OrderAdministrations
                .Where(admin =>
                    admin.TimeNeedingAction.HasValue)
                .Where(admin => !patientOrderDto.NextActionTime.HasValue
                                || patientOrderDto.NextActionTime > admin.TimeNeedingAction))
                patientOrderDto.NextActionTime = admin.TimeNeedingAction;

            //Figure out which filter criteria this order will fall under.
            //EMAR-425.  Winston Murdock/Bradley Marshall, 01/26/2021.
            List<string> applicables = new List<string>();

            //If priority is stat, then this is a stat order.
            if (patientOrderDto.Priority == OrderPriorities.Stat)
            {
                applicables.Add("stat");
            } //end if

            //If Prn = true, then this is a prn order.
            if (patientOrderDto.Prn)
            {
                applicables.Add("prn");
            } //end if

            //If beginTime is in the future, then this is a scheduled order.
            if (patientOrderDto.BeginDatetime > DateTimeOffset.Now)
            {
                applicables.Add("scheduled");
            } //end if

            //If point in time is true, then this is a timed order.
            if (patientOrderDto.PointInTime)
            {
                applicables.Add("timed");
            } //end if

            //If this order is not a point in time order
            //and the begin time for this order is in the past
            //and either the end time for this order is in the future
            //or this order does not have an end time listed,
            //then this is a continuous order.
            if
            (
                (!patientOrderDto.PointInTime)
                && (patientOrderDto.BeginDatetime <= DateTimeOffset.Now)
                &&
                (
                    patientOrderDto.EndDatetime > DateTimeOffset.Now
                    ||
                    !patientOrderDto.EndDatetime.HasValue
                )
            )
            {
                applicables.Add("continuous");
            } //end if

            //Add the list of applicable filters to the ApplicableFilters property.
            patientOrderDto.ApplicableFilters = applicables;

            return patientOrderDto;
        }

        public static PatientOrder MapCartOrderToOrder(PatientCartOrder cartOrder, DateTimeOffset addDatetime, int? orderingPhysicianId = null)
        {
            if (cartOrder == null)
            {
                return null;
            }

            var patientOrder = new PatientOrder
            {
                PatientId = cartOrder.PatientId,
                AddUserId = cartOrder.UserId,
                AddDatetime = DateTimeOffset.Parse(addDatetime.ToString("yyyy-MM-dd HH:mm:ss zz")),
                MedicationId = cartOrder.MedicationId,
                Dose = cartOrder.Dose,
                MedicationUnitId = cartOrder.MedicationUnitId,
                MedicationRouteId = cartOrder.MedicationRouteId,
                Priority = cartOrder.Priority,
                FrequencyScheduleId = cartOrder.FrequencyScheduleId,
                Prn = cartOrder.Prn,
                PointInTime = cartOrder.PointInTime,
                OrderStatus = OrderStatus.Pending.ToString(),
                BeginDatetime = cartOrder.BeginDatetime,
                EndDateTime = cartOrder.EndDatetime,
                OrderNotes = cartOrder.OrderNotes,
                AntimicrobialIndicationId = cartOrder.AntimicrobialIndicationId,
                AntimicrobialIndicationText = cartOrder.AntimicrobialIndicationText,
                PatientProblemId = cartOrder.PatientProblemId,
                Duration = cartOrder.Duration,
                DurationUnitId = cartOrder.DurationUnitId,
                OrderAdministrations = cartOrder.CartOrderAdministrations?.Select(MapCartToOrderAdministration).ToList(),
                OrderInteractions = cartOrder.OrderInteractions,
                OrderReactions = cartOrder.OrderReactions,
                PharmacyVerificationStatus = SetPharmacyVerificationStatusByDispositionTypeCode(cartOrder.Patient.DispositionTypeCode),
                Ndc = cartOrder.Ndc,
                PrnIndication = cartOrder.PrnIndication
            };

            if (orderingPhysicianId != null)
            {
                patientOrder.OrderingPhysicianId = (int)orderingPhysicianId;
            }

            return patientOrder;
        }

        private static OrderAdministration MapCartToOrderAdministration(CartOrderAdministration cartOrderAdministration)
        {
            if (cartOrderAdministration == null)
            {
                return null;
            }

            var administration = new OrderAdministration
            {
                AdministrationScheduledDatetime = cartOrderAdministration.AdministrationScheduledDatetime,
                StopScheduledDatetime = cartOrderAdministration.StopScheduledDatetime,
                PointInTime = cartOrderAdministration.PointInTime
            };

            return administration;
        }

        public static OrderAdministrationDto MapOrderAdministration(OrderAdministration administration,
            OrderStatus orderStatusCode, OrderActionMapperHelper orderActionMapperHelper)
        {
            if (administration == null)
            {
                return null;
            }

            var administrationDto = new OrderAdministrationDto
            {
                Id = administration.Id,
                OrderId = administration.PatientOrderId,
                AdministrationScheduledDatetime = administration.AdministrationScheduledDatetime,
                AdministrationSystemDatetime = administration.AdministrationSystemDatetime,
                AdministrationDatetime = administration.AdministrationDatetime,
                AdministeringUserId = administration.AdministeringUserId,
                AdministeringUser = UserMapper.MapUser(administration.AdministeringUser),
                StopScheduledDatetime = administration.StopScheduledDatetime,
                StopInputDatetime = administration.StopInputDatetime,
                StopDatetime = administration.StopDatetime,
                StopUserId = administration.StopUserId,
                StopUser = UserMapper.MapUser(administration.StopUser),
                AcknowledgeUserId = administration.AcknowledgeUserId,
                AcknowledgeUser = UserMapper.MapUser(administration.AcknowledgeUser),
                AcknowledgeDatetime = administration.AcknowledgeDatetime,
                PointInTime = administration.PointInTime,
                OnHold = administration.OnHold,
                MissedDose = administration.MissedDose,
                AdministrationEvents = administration.OrderEvents?
                    .Where(e =>
                        e.ActionId == (int)ActionEnum.CoSign
                        || e.ActionId == (int)ActionEnum.Hold
                        || e.ActionId == (int)ActionEnum.UnHold
                        || e.ActionId == (int)ActionEnum.FollowUp)
                    .Select(MapOrderEvent)
                    .ToList().OrderBy(e => e.EventDatetime)
            };

            //Added FollowUp to the filter above.
            //Couldn't find a good place to put this comment above, so I put it here.
            //Winston Murdock, 02/25/2021.  EMAR-732.


            if (orderActionMapperHelper?.AdminLinkBaseExists == true)
                administrationDto.AvailableActions =
                    orderActionMapperHelper.AvailableAdministrationActions(administrationDto, orderStatusCode);

            return administrationDto; 
        }

        public static OrderEventDto MapOrderEvent(OrderEvent @event)
        {
            if (@event == null)
            {
                return null;
            }

            var eventDto = new OrderEventDto
            {
                Id = @event.Id,
                OrderId = @event.PatientOrderId,
                AdministrationId = @event.OrderAdministrationId,
                EventDatetime = @event.EventDateTime,
                SystemDatetime = @event.AddDatetime,
                UserId = @event.AddUserId,
                User = UserMapper.MapUser(@event.User),
                ActionId = @event.ActionId,
                Action = TemplateMapper.MapAction(@event.Action),
                TemplateId = @event.TemplateId,
                TemplateResponses = @event.OrderEventDetails?.Select(TemplateMapper.MapOrderEventDetail)
            };

            return eventDto;
        }

        public static UserQuickListItemDto MapUserQuickListItem(UserQuickListItem dbObj, List<CodeSharedId> codeShareSites, BaseLinkResource resource = null)
        {
            if (dbObj == null)
            {
                return null;
            }

            var ret = new UserQuickListItemDto
            {
                UserId = dbObj.UserId,
                SiteId = dbObj.SiteId,
                Id = dbObj.Id,
                MedicationId = dbObj.MedicationId,
                Medication = MedicationMapper.MapMedication(dbObj.Medication,
                    codeShareSites
                        .FirstOrDefault(c =>
                            c.Entity == OrderRepository.CodeShareEntity.MedicationUnit)?
                        .SharedSiteId),
                Dose = dbObj.Dose,
                DoseUnit = dbObj.MedicationUnit != null
                           && dbObj.Medication.DrugId != "COMBO"
                           //////&& dbObj.Medication.SiteId != -1
                           && dbObj.MedicationUnit.SiteId == codeShareSites
                               .FirstOrDefault(c =>
                                   c.Entity == OrderRepository.CodeShareEntity.MedicationUnit)?
                               .SharedSiteId
                    ? MapMedicationUnit(dbObj.MedicationUnit)
                    : null,
                MedicationRoute = dbObj.MedicationRoute != null
                                  && dbObj.Medication.DrugId != "COMBO"
                                  //////&& dbObj.Medication.SiteId != -1
                                  && dbObj.MedicationRoute.SiteId == codeShareSites
                                      .FirstOrDefault(c =>
                                          c.Entity == OrderRepository.CodeShareEntity.MedicationRoute)?
                                      .SharedSiteId
                    ? MapMedicationRoute(dbObj.MedicationRoute)
                    : null,
                FrequencySchedule = dbObj.FrequencySchedule != null
                                    && dbObj.Medication.DrugId != "COMBO"
                                    //////&& dbObj.Medication.SiteId != -1
                                    && dbObj.FrequencySchedule.SiteId == codeShareSites
                                        .FirstOrDefault(c =>
                                            c.Entity == OrderRepository.CodeShareEntity.FrequencySchedule)?
                                        .SharedSiteId
                    ? MapFrequencySchedule(dbObj.FrequencySchedule)
                    : null,
                DurationInMinutes = dbObj.DurationInMinutes,
                Priority = dbObj.Priority ?? Convert.ToByte(OrderPriorities.Stat),
                OrderNotes = dbObj.OrderNotes,
                Ndc = dbObj.Ndc,
                PrnIndication = dbObj.PrnIndication,

                //Need to pull these in when editting a cart order that came from a quick list item.
                //Winston Murdock, 08/20/2021.  EMAR-1164.
                Duration = dbObj.Duration,
                DurationUnitId = dbObj.DurationUnitId,
                DurationUnit = MapDurationUnit(dbObj.DurationUnit)
            };

            ret.PointInTime = ret.FrequencySchedule?.PointInTime ?? true;

            if (resource != null && resource.PatientId != 0)
            {
                ret.Links = ListItemHateOasLinks(dbObj.Id, resource);
            }

            return ret;
        }

        public static UserQuickListItem MapUserQuickListItemAddDto(UserQuickListItemAddDto dbObj, int siteId, int userId)
        {
            if (dbObj == null)
            {
                return null;
            }

            
            return new UserQuickListItem
            {
                SiteId = siteId,
                UserId = userId,
                MedicationId = dbObj.MedicationId,
                Dose = dbObj.Dose,
                MedicationUnitId = dbObj.MedicationUnitId,
                MedicationRouteId = dbObj.MedicationRouteId,
                FrequencyScheduleId = dbObj.FrequencyId,
                OrderNotes = dbObj.OrderNotes,
                
                //Added Priority, Duration, and DurationUnitId.
                //These need to be pulled from the form on the page, but we
                //did not pull them because they did not exist in the object.
                //Winston Murdock, 03/01/2021.  EMAR-582
                //Priority = dbObj.Priority,
                //Priority comes in as an OrderPriorities enumeration value.
                //We need to convert from the enumeration to a byte here.
                //Call a helper method to do that.
                //Winston Murdock, 03/11/2021.  EMAR-582
                Priority = ConvertPriorityToByte(dbObj.Priority),
                Duration = dbObj.Duration,
                DurationUnitId = dbObj.DurationUnitId,

                Ndc = dbObj.Ndc,
                PrnIndication = dbObj.PrnIndication
            };
        }

        public static UserQuickListItem MapUserQuickListItemDto(UserQuickListItemDto dbObj)
        {
            if (dbObj == null)
                return null;

            var ret = new UserQuickListItem
            {
                Id = (int)dbObj.Id,
                SiteId = dbObj.SiteId,
                UserId = dbObj.UserId,
                MedicationId = dbObj.MedicationId,
                Dose = dbObj.Dose,
                MedicationUnitId = dbObj.MedicationUnitId,
                MedicationRouteId = dbObj.MedicationRouteId,
                FrequencyScheduleId = dbObj.FrequencyId,
                OrderNotes = dbObj.OrderNotes,
                Ndc = dbObj.Ndc,
                PrnIndication = dbObj.PrnIndication//,
                ////UsagesThisWeek = dbObj.UsagesThisWeek,
                ////WeeklyUsageRollingAverage = dbObj.WeeklyUsageRollingAverage,
                ////FrequencySchedule = dbObj.FrequencySchedule,
                ////MedicationRoute = dbObj.MedicationRoute,
                ////MedicationUnit = dbObj.MedicationUnit,
                ////Site = dbObj.Site,
                ////User = dbObj.User,
                ////PatientCartOrders = dbObj.PatientCartOrders


                //UserId = dbObj.UserId,
                //SiteId = dbObj.SiteId,
                //Id = dbObj.Id,
                //Ndc = dbObj.Ndc,
                //DrugId = dbObj.DrugId,
                //BrandName = dbObj.BrandName,
                //Dose = dbObj.Dose,
                //DoseUnit = MedicationMapper.MapMedicationUnit(dbObj.MedicationUnit),
                //MedicationRoute = MedicationMapper.MapMedicationRoute(dbObj.MedicationRoute),
                //FrequencySchedule = MedicationMapper.MapFrequencySchedule(dbObj.FrequencySchedule),
                //OrderNotes = dbObj.OrderNotes
            };

            return ret;
        }

        internal static PatientCartOrder MapUserQuickListItemToPatientCartOrder(int userId, long patientId, UserQuickListItem dbObj)
        {
            if (dbObj == null)
                return null;

            var ret = new PatientCartOrder
            {
                // Properties From the OrderBase
                MedicationId = dbObj.MedicationId,
                Dose = dbObj.Dose,
                MedicationUnitId = dbObj.MedicationUnitId,
                MedicationRouteId = dbObj.MedicationRouteId,
                FrequencyScheduleId = dbObj.FrequencyScheduleId,
                PointInTime = dbObj.FrequencySchedule.PointInTime,
                OrderNotes = dbObj.OrderNotes,

                // Properties from CartOrderDto
                PatientId = patientId,
                UserId = userId,
                AddDatetime = dbObj.Medication.Site.TimeZoneName.NowWithTimeZoneOffset(),
                Priority = dbObj.Priority ?? Convert.ToByte(OrderPriorities.Stat),
                Prn = dbObj.FrequencySchedule.FrequencyType.Name.ToUpper().Equals("PRN"),
                BeginDatetime = dbObj.Medication.Site.TimeZoneName.NowWithTimeZoneOffset(),
                // EndDatetime: done later taking into account administrations and duration
                UserQuickListItemId = dbObj.Id,
                Duration = dbObj.Duration,
                DurationUnitId = dbObj.DurationUnitId,
                Ndc = dbObj.Ndc,
                PrnIndication = dbObj.PrnIndication
            };

            return ret;
        }

        public static DepartmentPreferredItemDto MapDepartmentPreferredListItem(DepartmentPreferredListItem dbObj, BaseLinkResource resource, List<CodeSharedId> codeShareSites)
        {
            if (dbObj == null)
                return null;

            var ret = new DepartmentPreferredItemDto
            {
                DepartmentCode = dbObj.DepartmentCode,
                SiteId = dbObj.SiteId,
                Id = dbObj.Id,
                MedicationId = dbObj.MedicationId,
                Medication = MedicationMapper.MapMedication(dbObj.Medication,
                    codeShareSites
                        .FirstOrDefault(c =>
                            c.Entity == OrderRepository.CodeShareEntity.MedicationUnit)?
                        .SharedSiteId),
                Dose = dbObj.Dose,
                DoseUnit = dbObj.MedicationUnit != null
                           && dbObj.Medication.DrugId != "COMBO"
                           //////&& dbObj.Medication.SiteId != -1
                           && dbObj.MedicationUnit.SiteId == codeShareSites
                               .FirstOrDefault(c =>
                                   c.Entity == OrderRepository.CodeShareEntity.MedicationUnit)?
                               .SharedSiteId
                    ? MapMedicationUnit(dbObj.MedicationUnit)
                    : null,
                MedicationRoute = dbObj.MedicationRoute != null
                                  && dbObj.Medication.DrugId != "COMBO"
                                  //////&& dbObj.Medication.SiteId != -1
                                  && dbObj.MedicationRoute.SiteId == codeShareSites
                                      .FirstOrDefault(c =>
                                          c.Entity == OrderRepository.CodeShareEntity.MedicationRoute)?
                                      .SharedSiteId
                    ? MapMedicationRoute(dbObj.MedicationRoute)
                    : null,
                FrequencySchedule = dbObj.FrequencySchedule != null
                                    && dbObj.Medication.DrugId != "COMBO"
                                    //////&& dbObj.Medication.SiteId != -1
                                    && dbObj.FrequencySchedule.SiteId == codeShareSites
                                        .FirstOrDefault(c =>
                                            c.Entity == OrderRepository.CodeShareEntity.FrequencySchedule)?
                                        .SharedSiteId
                    ? MapFrequencySchedule(dbObj.FrequencySchedule)
                    : null,
                DurationInMinutes = dbObj.DurationInMinutes,
                Priority = dbObj.Priority ?? Convert.ToByte(OrderPriorities.Stat),
                OrderNotes = dbObj.OrderNotes,
                Ndc = dbObj.Ndc
            };

            ret.PointInTime = ret.FrequencySchedule?.PointInTime ?? true;

            if (resource.PatientId != 0)
            {
                ret.Links = ListItemHateOasLinks(dbObj.Id, resource);
            }

            return ret;
        }

        internal static PatientCartOrder MapDepartmentPreferredListItemToPatientCartOrder(int userId, long patientId, DepartmentPreferredListItem dbObj)
        {
            if (dbObj == null)
                return null;

            var ret = new PatientCartOrder
            {
                // Properties From the OrderBase
                MedicationId = dbObj.MedicationId,
                Dose = dbObj.Dose,
                MedicationUnitId = dbObj.MedicationUnitId,
                MedicationRouteId = dbObj.MedicationRouteId,
                FrequencyScheduleId = dbObj.FrequencyScheduleId,
                PointInTime = dbObj.FrequencySchedule.PointInTime,
                OrderNotes = dbObj.OrderNotes,

                // Properties from CartOrderDto
                PatientId = patientId,
                UserId = userId,
                AddDatetime = dbObj.Medication.Site.TimeZoneName.NowWithTimeZoneOffset(),
                Priority = dbObj.Priority ?? Convert.ToByte(OrderPriorities.Stat),
                Prn = dbObj.FrequencySchedule.FrequencyType.Name.ToUpper().Equals("PRN"),
                BeginDatetime = dbObj.Medication.Site.TimeZoneName.NowWithTimeZoneOffset(),
                // EndDatetime: done later taking into account administrations and duration
                UserQuickListItemId = null,
                Duration = dbObj.Duration,
                DurationUnitId = dbObj.DurationUnitId,
                Ndc = dbObj.Ndc
            };

            return ret;
        }

        public static GroupListItemDto MapGroupListItem(GroupListItem dbObj, BaseLinkResource resource, List<CodeSharedId> codeShareSites)
        {
            if (dbObj == null)
                return null;

            var ret = new GroupListItemDto
            {
                DepartmentCode = dbObj.DepartmentCode,
                GroupName = dbObj.GroupName,
                SiteId = dbObj.SiteId,
                Id = dbObj.Id,
                MedicationId = dbObj.MedicationId,
                Medication = MedicationMapper.MapMedication(dbObj.Medication,
                    codeShareSites
                        .FirstOrDefault(c =>
                            c.Entity == OrderRepository.CodeShareEntity.MedicationUnit)?
                        .SharedSiteId),
                Dose = dbObj.Dose,
                DoseUnit = dbObj.MedicationUnit != null
                           && dbObj.Medication.DrugId != "COMBO"
                           //////&& dbObj.Medication.SiteId != -1
                           && dbObj.MedicationUnit.SiteId == codeShareSites
                               .FirstOrDefault(c =>
                                   c.Entity == OrderRepository.CodeShareEntity.MedicationUnit)?
                               .SharedSiteId
                    ? MapMedicationUnit(dbObj.MedicationUnit)
                    : null,
                MedicationRoute = dbObj.MedicationRoute != null
                                  && dbObj.Medication.DrugId != "COMBO"
                                  //////&& dbObj.Medication.SiteId != -1
                                  && dbObj.MedicationRoute.SiteId == codeShareSites
                                      .FirstOrDefault(c =>
                                          c.Entity == OrderRepository.CodeShareEntity.MedicationRoute)?
                                      .SharedSiteId
                    ? MapMedicationRoute(dbObj.MedicationRoute)
                    : null,
                FrequencySchedule = dbObj.FrequencySchedule != null
                                    && dbObj.Medication.DrugId != "COMBO"
                                    //////&& dbObj.Medication.SiteId != -1
                                    && dbObj.FrequencySchedule.SiteId == codeShareSites
                                        .FirstOrDefault(c =>
                                            c.Entity == OrderRepository.CodeShareEntity.FrequencySchedule)?
                                        .SharedSiteId
                    ? MapFrequencySchedule(dbObj.FrequencySchedule)
                    : null,
                DurationInMinutes = dbObj.DurationInMinutes,
                Priority = dbObj.Priority ?? Convert.ToByte(OrderPriorities.Stat),
                OrderNotes = dbObj.OrderNotes,
                Ndc = dbObj.Ndc
            };

            ret.PointInTime = ret.FrequencySchedule?.PointInTime ?? true;

            if (resource.PatientId != 0)
            {
                ret.Links = ListItemHateOasLinks(dbObj.Id, resource);
            }

            return ret;
        }

        internal static PatientCartOrder MapGroupListItemToPatientCartOrder(int userId, long patientId, GroupListItem dbObj)
        {
            if (dbObj == null)
                return null;

            var ret = new PatientCartOrder
            {
                // Properties From the OrderBase
                MedicationId = dbObj.MedicationId,
                Dose = dbObj.Dose,
                MedicationUnitId = dbObj.MedicationUnitId,
                MedicationRouteId = dbObj.MedicationRouteId,
                FrequencyScheduleId = dbObj.FrequencyScheduleId,
                PointInTime = dbObj.FrequencySchedule.PointInTime,
                OrderNotes = dbObj.OrderNotes,

                // Properties from CartOrderDto
                PatientId = patientId,
                UserId = userId,
                AddDatetime = dbObj.Medication.Site.TimeZoneName.NowWithTimeZoneOffset(),
                Priority = dbObj.Priority ?? Convert.ToByte(OrderPriorities.Stat),
                Prn = dbObj.FrequencySchedule.FrequencyType.Name.ToUpper().Equals("PRN"),
                BeginDatetime = dbObj.Medication.Site.TimeZoneName.NowWithTimeZoneOffset(),
                // EndDatetime: done later taking into account administrations and duration
                UserQuickListItemId = null,
                Duration = dbObj.Duration,
                DurationUnitId = dbObj.DurationUnitId,
                Ndc = dbObj.Ndc
            };

            return ret;
        }

        private static IEnumerable<HateOasLinkDto> ListItemHateOasLinks(int id, BaseLinkResource resource)
        {
            var links = new List<HateOasLinkDto>();

            if (!string.IsNullOrEmpty(resource.LinkCopyItemToCart) ||
                !string.IsNullOrEmpty(resource.LinkGetSchedulerOptionsListItem))
            {
                if (!string.IsNullOrEmpty(resource.LinkCopyItemToCart))
                {
                    links.Add(new HateOasLinkDto(resource.LinkCopyItemToCart.Replace("/-99/", string.Concat("/", id, "/")),
                        "add_list_item_to_cart",
                        "POST"));
                }

                if (!string.IsNullOrEmpty(resource.LinkGetSchedulerOptionsListItem))
                {
                    links.Add(new HateOasLinkDto(
                        resource.LinkGetSchedulerOptionsListItem.Replace("/-99", string.Concat("/", id)),
                        "get_scheduler_options",
                        "GET"));
                }
            }

            return links;
        }

        public static FrequencyScheduleDto MapFrequencySchedule(FrequencySchedule dbObj)
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
                Prn = dbObj.FrequencyType.Name.ToUpper().Equals("PRN"),
                FrequencyType = MapFrequencyType(dbObj.FrequencyType),
                Notes = dbObj.Notes
            };

            return ret;
        }

        public static FrequencyTypeDto MapFrequencyType(FrequencyType dbObj)
        {
            if (dbObj == null)
            {
                return null;
            }

            var ret = new FrequencyTypeDto
            {
                Id = dbObj.Id,
                Name = dbObj.Name
            };

            return ret;
        }

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
                SiteId = medicationRoute.SiteId,
                //Since priority allows nulls in the DB, handle that.
                //If it's null , then set the value to 0.
                //Else, return the actual value.
                //Since we're sorting by Priority in the service, this will cause the
                //entries with a null priority to sort first (which is correct per Jim Hoos).
                //Winston Murdock, 02/25/2021. EMAR-779
                Priority = String.IsNullOrEmpty(medicationRoute.Priority.ToString()) ? 0 : medicationRoute.Priority
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
                Active = medicationUnit.IsActive,
                //Since priority allows nulls in the DB, handle that here.
                //If it's null , then set the value to 0.
                //Else, return the actual value.
                //Since we're sorting by Priority in the service, this will cause the
                //entries with a null priority to sort first (which is correct per Jim Hoos).
                //Winston Murdock, 02/25/2021. EMAR-779
                Priority = String.IsNullOrEmpty(medicationUnit.Priority.ToString()) ? 0 : medicationUnit.Priority
            };

            return ret;
        }

        public static DurationUnitDto MapDurationUnit(DurationUnit durationUnit)
        {
            if (durationUnit == null)
            {
                return null;
            }

            var ret = new DurationUnitDto
            {
                Id = durationUnit.Id,
                DurationInMinutes = durationUnit.DurationInMinutes,
                Name = durationUnit.Name
            };

            return ret;
        }
        
        public static FrequencyScheduleAdministrationDto MapFrequencyScheduleAdministration(FrequencyScheduleAdministration administration)
        {
            if (administration == null)
            {
                return null;
            }

            var ret = new FrequencyScheduleAdministrationDto
            {
                ScheduleDateTime = administration.ScheduleDateTime,
                StopDateTime = administration.StopDateTime,
                PointInTime = administration.PointInTime
            };

            return ret;
        }

        #region Model mappers
        public static MedicationModel MapOrderItemToModel(EmarOrderType orderType, object item, long patientId, int userId, int? codeShareSiteMedicationUnit)
        {
            return
                orderType switch
                {
                    EmarOrderType.UserQuickListItem => OrderMapper.MapUserQuickListItemToModel((UserQuickListItem)item, patientId, codeShareSiteMedicationUnit),
                    EmarOrderType.DepartmentPreferredListItem => OrderMapper.MapDepartmentPreferredListItemToModel((DepartmentPreferredListItem)item, patientId, userId, codeShareSiteMedicationUnit),
                    EmarOrderType.GroupRememberedOrder => OrderMapper.MapGroupListItemToModel((GroupListItem)item, patientId, userId, codeShareSiteMedicationUnit),
                    EmarOrderType.MedicationItem => OrderMapper.MapMedicationToModel((Medication)item, patientId, userId, codeShareSiteMedicationUnit),
                    //ToDo: Confirm this works.
                    //Winston Murdock, 01/06/2021.
                    EmarOrderType.PatientOrder => OrderMapper.MapPatientOrderToModel((PatientOrder)item, userId, userId, codeShareSiteMedicationUnit),
                    EmarOrderType.PatientCartOrder => OrderMapper.MapPatientCartOrderToModel((PatientCartOrder)item, userId, userId, codeShareSiteMedicationUnit),
                    _ => null
                };
        }

        public static MedicationModel MapOrderItemDtoToModel(EmarOrderType orderType, object item, long patientId, int userId)
        {
            return
                orderType switch
                {
                    EmarOrderType.UserQuickListItem => OrderMapper.MapUserQuickListItemDtoToModel((UserQuickListItemDto)item, patientId),
                    EmarOrderType.DepartmentPreferredListItem => OrderMapper.MapDepartmentPreferredListItemDtoToModel((DepartmentPreferredItemDto)item, patientId, userId),
                    EmarOrderType.GroupRememberedOrder => OrderMapper.MapGroupListItemDtoToModel((GroupListItemDto)item, patientId, userId),
                    _ => null
                };
        }

        private static MedicationModel MapUserQuickListItemToModel(UserQuickListItem item, long patientId, int? codeShareSiteMedicationUnit)
        {
            if (item == null)
            {
                return null;
            }

            return new MedicationModel
            {
                SiteId = item.SiteId,
                PatientId = patientId,
                UserId = item.UserId,
                SourceTable = SourceTables.UserQuickListItems,
                SourceTableId = item.Id,
                Type = EmarOrderType.UserQuickListItem,
                ActionStatus = null,
                AddDatetime = null,
                AddUserId = null,
                AlternateName = null,
                BeginDatetime = null,
                Category = null,
                ChangeDatetime = null,
                ChangeUserId = null,
                Class = null,
                Comment = item.OrderNotes,
                Dose = item.Dose,
                EndDatetime = null,
                FrequencyScheduleId = item.FrequencyScheduleId,
                InternalDrugId = null,
                IsActive = null,
                MedicationId = item.MedicationId,
                Medication = MedicationMapper.MapMedication(item.Medication, codeShareSiteMedicationUnit),
                MedicationDrugId = null,
                MedicationRouteId = null,
                MedicationUnitId = null,
                OrderPhysicianUserId = null,
                OrderStatus = null,
                ParentDrugId = null,
                ParentDrugName = null,
                PointInTime = item.FrequencySchedule.PointInTime,
                Priority = item.Priority,
                Prn = item.FrequencySchedule.FrequencyType.Name.ToUpper().Equals("PRN"),
                Reaction = null,
                Schedule = null,
                Severity = null,

                Name = null,
                AllergyDrugId = null,
                InformationSource = null,
                PersonNumber = null,
                AccountNumber = null,

                Ndc = item.Ndc
            };
        }

        private static MedicationModel MapUserQuickListItemDtoToModel(UserQuickListItemDto item, long patientId)
        {
            if (item == null)
            {
                return null;
            }

            return new MedicationModel
            {
                SiteId = item.SiteId,
                PatientId = patientId,
                UserId = item.UserId,
                SourceTable = SourceTables.UserQuickListItems,
                SourceTableId = item.Id,
                Type = EmarOrderType.UserQuickListItem,
                ActionStatus = null,
                AddDatetime = null,
                AddUserId = null,
                AlternateName = null,
                BeginDatetime = null,
                Category = null,
                ChangeDatetime = null,
                ChangeUserId = null,
                Class = null,
                Comment = item.OrderNotes,
                Dose = item.Dose,
                EndDatetime = null,
                FrequencyScheduleId = item.FrequencySchedule?.Id,
                InternalDrugId = null,
                IsActive = null,
                MedicationId = item.MedicationId,
                Medication = item.Medication,
                MedicationDrugId = null,
                MedicationRouteId = null,
                MedicationUnitId = null,
                OrderPhysicianUserId = null,
                OrderStatus = null,
                ParentDrugId = null,
                ParentDrugName = null,
                PointInTime = item.FrequencySchedule?.PointInTime,
                Priority = item.Priority,
                Prn = item.FrequencySchedule?.Prn,
                Reaction = null,
                Schedule = null,
                Severity = null,

                Name = null,
                AllergyDrugId = null,
                InformationSource = null,
                PersonNumber = null,
                AccountNumber = null,

                Ndc = item.Ndc,
                PrnIndication = item.PrnIndication
            };
        }

        private static MedicationModel MapDepartmentPreferredListItemToModel(DepartmentPreferredListItem item, long patientId, int userId, int? codeShareSiteMedicationUnit)
        {
            if (item == null)
            {
                return null;
            }

            return new MedicationModel
            {
                SiteId = item.SiteId,
                PatientId = patientId,
                UserId = userId,
                SourceTable = SourceTables.DepartmentPreferredListItems,
                SourceTableId = item.Id,
                Type = EmarOrderType.DepartmentPreferredListItem,
                ActionStatus = null,
                AddDatetime = null,
                AddUserId = null,
                AlternateName = null,
                BeginDatetime = null,
                Category = null,
                ChangeDatetime = null,
                ChangeUserId = null,
                Class = null,
                Comment = item.OrderNotes,
                Dose = item.Dose,
                EndDatetime = null,
                FrequencyScheduleId = item.FrequencyScheduleId,
                InternalDrugId = null,
                IsActive = null,
                MedicationId = item.MedicationId,
                Medication = MedicationMapper.MapMedication(item.Medication, codeShareSiteMedicationUnit),
                MedicationDrugId = null,
                MedicationRouteId = null,
                MedicationUnitId = null,
                OrderPhysicianUserId = null,
                OrderStatus = null,
                ParentDrugId = null,
                ParentDrugName = null,
                PointInTime = item.FrequencySchedule.PointInTime,
                Priority = item.Priority,
                Prn = item.FrequencySchedule.FrequencyType.Name.ToUpper().Equals("PRN"),
                Reaction = null,
                Schedule = null,
                Severity = null,

                Name = null,
                AllergyDrugId = null,
                InformationSource = null,
                PersonNumber = null,
                AccountNumber = null,

                Ndc = item.Ndc
            };
        }

        private static MedicationModel MapDepartmentPreferredListItemDtoToModel(DepartmentPreferredItemDto item, long patientId, int userId)
        {
            if (item == null)
            {
                return null;
            }

            return new MedicationModel
            {
                SiteId = item.SiteId,
                PatientId = patientId,
                UserId = userId,
                SourceTable = SourceTables.DepartmentPreferredListItems,
                SourceTableId = item.Id,
                Type = EmarOrderType.DepartmentPreferredListItem,
                ActionStatus = null,
                AddDatetime = null,
                AddUserId = null,
                AlternateName = null,
                BeginDatetime = null,
                Category = null,
                ChangeDatetime = null,
                ChangeUserId = null,
                Class = null,
                Comment = item.OrderNotes,
                Dose = item.Dose,
                EndDatetime = null,
                FrequencyScheduleId = item.FrequencySchedule?.Id,
                InternalDrugId = null,
                IsActive = null,
                MedicationId = item.MedicationId,
                Medication = item.Medication,
                MedicationDrugId = null,
                MedicationRouteId = null,
                MedicationUnitId = null,
                OrderPhysicianUserId = null,
                OrderStatus = null,
                ParentDrugId = null,
                ParentDrugName = null,
                PointInTime = item.FrequencySchedule?.PointInTime,
                Priority = item.Priority,
                Prn = item.FrequencySchedule?.Prn,
                Reaction = null,
                Schedule = null,
                Severity = null,

                Name = null,
                AllergyDrugId = null,
                InformationSource = null,
                PersonNumber = null,
                AccountNumber = null,

                Ndc = item.Ndc
            };
        }

        private static MedicationModel MapGroupListItemToModel(GroupListItem item, long patientId, int userId, int? codeShareSiteMedicationUnit)
        {
            if (item == null)
            {
                return null;
            }

            return new MedicationModel
            {
                SiteId = item.SiteId,
                PatientId = patientId,
                UserId = userId,
                SourceTable = SourceTables.GroupListItems,
                SourceTableId = item.Id,
                Type = EmarOrderType.GroupRememberedOrder,
                ActionStatus = null,
                AddDatetime = null,
                AddUserId = null,
                AlternateName = null,
                BeginDatetime = null,
                Category = null,
                ChangeDatetime = null,
                ChangeUserId = null,
                Class = null,
                Comment = item.OrderNotes,
                Dose = item.Dose,
                EndDatetime = null,
                FrequencyScheduleId = item.FrequencyScheduleId,
                InternalDrugId = null,
                IsActive = null,
                MedicationId = item.MedicationId,
                Medication = MedicationMapper.MapMedication(item.Medication, codeShareSiteMedicationUnit),
                MedicationDrugId = null,
                MedicationRouteId = null,
                MedicationUnitId = null,
                OrderPhysicianUserId = null,
                OrderStatus = null,
                ParentDrugId = null,
                ParentDrugName = null,
                PointInTime = item.FrequencySchedule.PointInTime,
                Priority = item.Priority,
                Prn = item.FrequencySchedule.FrequencyType.Name.ToUpper().Equals("PRN"),
                Reaction = null,
                Schedule = null,
                Severity = null,

                Name = null,
                AllergyDrugId = null,
                InformationSource = null,
                PersonNumber = null,
                AccountNumber = null,

                Ndc = item.Ndc
            };
        }

        private static MedicationModel MapMedicationToModel(Medication item, long patientId, int userId, int? codeShareSiteMedicationUnit)
        {
            if (item == null)
            {
                return null;
            }

            return new MedicationModel
            {
                SiteId = 0,
                PatientId = patientId,
                UserId = userId,
                SourceTable = SourceTables.Medications,
                SourceTableId = item.Id,
                Type = EmarOrderType.MedicationItem,
                ActionStatus = null,
                AddDatetime = null,
                AddUserId = null,
                AlternateName = null,
                BeginDatetime = null,
                Category = null,
                ChangeDatetime = null,
                ChangeUserId = null,
                Class = null,
                Comment = null,
                Dose = null,
                EndDatetime = null,
                FrequencyScheduleId = null,
                InternalDrugId = null,
                IsActive = null,
                MedicationId = item.Id,
                Medication = MedicationMapper.MapMedication(item, codeShareSiteMedicationUnit),
                MedicationDrugId = null,
                MedicationRouteId = null,
                MedicationUnitId = null,
                OrderPhysicianUserId = null,
                OrderStatus = null,
                ParentDrugId = null,
                ParentDrugName = null,
                PointInTime = null,
                Priority = null,
                Prn = null,
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

        private static MedicationModel MapGroupListItemDtoToModel(GroupListItemDto item, long patientId, int userId)
        {
            if (item == null)
            {
                return null;
            }

            return new MedicationModel
            {
                SiteId = item.SiteId,
                PatientId = patientId,
                UserId = userId,
                SourceTable = SourceTables.GroupListItems,
                SourceTableId = item.Id,
                Type = EmarOrderType.GroupRememberedOrder,
                ActionStatus = null,
                AddDatetime = null,
                AddUserId = null,
                AlternateName = null,
                BeginDatetime = null,
                Category = null,
                ChangeDatetime = null,
                ChangeUserId = null,
                Class = null,
                Comment = item.OrderNotes,
                Dose = item.Dose,
                EndDatetime = null,
                FrequencyScheduleId = item.FrequencySchedule?.Id,
                InternalDrugId = null,
                IsActive = null,
                MedicationId = item.MedicationId,
                Medication = item.Medication,
                MedicationDrugId = null,
                MedicationRouteId = null,
                MedicationUnitId = null,
                OrderPhysicianUserId = null,
                OrderStatus = null,
                ParentDrugId = null,
                ParentDrugName = null,
                PointInTime = item.FrequencySchedule?.PointInTime,
                Priority = item.Priority,
                Prn = item.FrequencySchedule?.Prn,
                Reaction = null,
                Schedule = null,
                Severity = null,

                Name = null,
                AllergyDrugId = null,
                InformationSource = null,
                PersonNumber = null,
                AccountNumber = null,

                Ndc = item.Ndc
            };
        }

        public static MedicationModel MapPatientCartOrderToModel(PatientCartOrder order, int? userId, int? siteId, int? codeShareSiteMedicationUnit)
        {
            if (order == null)
            {
                return null;
            }

            return new MedicationModel
            {
                Id = order.Id,
                SiteId = siteId ?? -1,
                PatientId = order.PatientId,
                UserId = userId ?? -1,
                SourceTable = SourceTables.PatientOrders,
                SourceTableId = order.Id,
                Type = EmarOrderType.PatientOrder,
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
                FrequencyScheduleId = null,
                IsActive = null,
                MedicationId = order.MedicationId,

                //When editting a cart order, the reactions/interactions were not showing.
                //I had to add this (copied from MapPatientOrderToModel) to map the Medication too.
                //We need the Medication, MedicationDetails, and FdbBrandName at the very end.
                //Winston Murdock, 03/01/2022.  PC-27061
                Medication = MedicationMapper.MapMedication(order.Medication, codeShareSiteMedicationUnit),
                
                MedicationDrugId = null,
                MedicationRouteId = order.MedicationRouteId,
                MedicationUnitId = order.MedicationUnitId,
                OrderPhysicianUserId = null,
                OrderStatus = null,
                ParentDrugId = null,
                ParentDrugName = null,
                PointInTime = order.PointInTime,
                Prn = order.Prn,
                Reaction = null,
                Schedule = null,
                Severity = null,

                Name = null,
                AllergyDrugId = null,
                InformationSource = null,
                PersonNumber = null,
                AccountNumber = null,
                Interactions = new List<Dictionary<string, object>>(),
                Reactions = new List<Dictionary<string, object>>(),

                Ndc = order.Ndc,
                PrnIndication = order.PrnIndication,
                
            };
        }

        public static MedicationModel MapPatientOrderToModel(PatientOrder order, int? userId, int? siteId, int? codeShareSiteMedicationUnit)
        {
            if (order == null)
            {
                return null;
            }

            return new MedicationModel
            {
                Id = order.Id,
                SiteId = siteId ?? -1,
                PatientId = order.PatientId,
                UserId = userId ?? -1,
                SourceTable = SourceTables.PatientOrders,
                SourceTableId = order.Id,
                Type = EmarOrderType.PatientOrder,
                ActionStatus = null,
                AddDatetime = order.AddDatetime,
                AddUserId = order.AddUserId,
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
                //////////InternalDrugId = order.FdbBrandName?.PcRoutedGenId,
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

        public static MedicationInteractionReaction MedicationInteractionsReactions(MedicationModel medication)
        {
            if (medication == null)
            {
                return null;
            }

            return new MedicationInteractionReaction
            {
                SiteId = medication.SiteId,
                PatientId = medication.PatientId,
                UserId = medication.UserId,
                SourceTable = medication.SourceTable,
                SourceTableId = medication.SourceTableId,
                Type = medication.Type,
                BrandName = medication.Medication?.MedicationDetails?.FirstOrDefault()?.FdbBrandName?.BrandName,
                ActiveName = medication.Medication?.MedicationDetails?.FirstOrDefault()?.FdbBrandName?.Active,
                ActiveId = medication.Medication?.MedicationDetails?.FirstOrDefault()?.FdbBrandName?.PcRoutedGenId,
                Interactions = medication.Interactions,
                Reactions = medication.Reactions
            };
        }
        #endregion

        public static SchedulerOptionsDto MapSchedulerSetupData(
            string brandName,
            List<Medication> medications,
            List<AntimicrobialRequiredIndicator> antimicrobialRequiredIndicators,
            List<FrequencyScheduleAdministration> administrations,
            IEnumerable<OrderInstruction> orderInstructions,
            IEnumerable<CodeSharedId> codeShareSites,
            DateTimeOffset? endDateTime = null)
        {
            if (medications.All(m => m == null))
            {
                return null;
            }
            
            var ret = new SchedulerOptionsDto
            {
                BrandName = brandName,
                AvailableFormStrength = medications
                    .Where(medication => medication != null)
                    .Select(m =>
                        new FormStrengthDto
                        {
                            Combo = m.SiteId > -1,
                            MedicationDetails = m.MedicationDetails.Select(d =>
                                MedicationMapper.MapMedicationDetail(d,
                                    codeShareSites
                                        .FirstOrDefault(c =>
                                            c.Entity == OrderRepository.CodeShareEntity.MedicationUnit)?
                                        .SharedSiteId)),
                            MedicationId = m.Id,
                            AntimicrobialRequiredIndicator = antimicrobialRequiredIndicators.FirstOrDefault(a => a.MedicationId == m.Id).AntimicrobialRequired,
                            FormStrengthName = m.DisplayName,
                            PreferredDoses = MapPreferredMedicationDose(
                                m.PreferredMedicationDoses
                                    .Where(p =>
                                        p.MedicationId == m.Id
                                        && p.MedicationUnit != null
                                        && p.Medication.DrugId != "COMBO"
                                        //////&& p.Medication.SiteId != -1
                                        && p.MedicationUnit.SiteId == codeShareSites
                                            .FirstOrDefault(c =>
                                                c.Entity == OrderRepository.CodeShareEntity.MedicationUnit)?
                                            .SharedSiteId)),
                            PreferredRoutes = MapPreferredMedicationRoute(
                                m.PreferredMedicationRoutes
                                    .Where(p =>
                                        p.MedicationId == m.Id
                                        && p.MedicationRoute != null
                                        && p.Medication.DrugId != "COMBO"
                                        //////&& p.Medication.SiteId != -1
                                        && p.MedicationRoute.SiteId == codeShareSites
                                            .FirstOrDefault(c =>
                                                c.Entity == OrderRepository.CodeShareEntity.MedicationRoute)?
                                            .SharedSiteId)),
                            PreferredFrequencies = MapPreferredFrequencySchedule(
                                m.PreferredFrequencySchedules
                                    .Where(p =>
                                        p.MedicationId == m.Id
                                        && p.Medication.DrugId != "COMBO"
                                        //////&& p.Medication.SiteId != -1
                                        && p.FrequencySchedule != null
                                        && p.FrequencySchedule.SiteId == codeShareSites
                                            .FirstOrDefault(c =>
                                                c.Entity == OrderRepository.CodeShareEntity.FrequencySchedule)?
                                            .SharedSiteId)),
                            //Map the vendor-specific lists here.
                            //For the one we're using, we'll actually have something to map.
                            //For the ones we're not using, this will return a null list in the DTO.
                            FdbNdcInfos = MapFdbNdcInfo(m.FdbNdcInfos)
                        })
                    .ToList(),
                Administrations = administrations?.Select(MapFrequencyScheduleAdministration).ToList(),
                AdministrationInstructions = orderInstructions?.Select(MapOrderInstructions).ToList()
            };

            //We'll always have one medication, so this should always resolve.
            //And if there aren't any cart orders, then this should be null.
            //Winston Murdock, 07/30/2021.
            if (medications.Any())
            {
                //Make a patient cart orders DTO and return that.
                //Getting a 500 error when I just return the entity.
                //And we're not "supposed" to return an entity to the UI.
                //So I'll stop being lazy...
                //ret.PatientCartOrders = medications[0].PatientCartOrders.ToList();
                ret.PatientCartOrders = MapPatientCartOrders(medications[0].PatientCartOrders.ToList());

                //Also map the user quick list items (which we will have if the user got her from a quick list item).
                //Winston Murdock, 08/20/2021.  EMAR-1164.
                UserQuickListItemDto userQuickListItemDto = MapUserQuickListItem(medications[0].UserQuickListItems.FirstOrDefault(), codeShareSites.ToList());
                ret.UserQuickListItems = YieldUserQuickListItemDto(userQuickListItemDto).ToList();

                //Map the endDateTime if we have one.
                //Modifying a patient orders and editting a patient cart order will cause us to have one.
                //Winston Murdock, 02/16/2022.  PC-27021
                if (endDateTime.HasValue)
                {
                    ret.EndDateTime = endDateTime;
                } //end if
            } //end if

            return ret;
        }

        private static IEnumerable<PreferredDoseDto> MapPreferredMedicationDose(IEnumerable<PreferredMedicationDose> preferredDoses)
        {
            //return preferredDoses?
            //    .Select(p =>
            //        p == null
            //            ? null
            //            : new PreferredDoseDto
            //            {
            //                Dose = p.Dose,
            //                DoseUnit = MapMedicationUnit(p.MedicationUnit),
            //                //Combine the dose and name into one field so that we can do a GroupBy and then FirstOrDefault on it.
            //                //This simulates doing a select distinct in a SQL query.
            //                //https://stackoverflow.com/a/14321048
            //                //Winston Murdock, 03/03/2021.  EMAR-824
            //                DosePlusUnit = p.Dose + p.MedicationUnit.Name
            //            })
            //    //Simulate a select distinct by grouping by a field and then selecting the first of that field.
            //    //Winston Murdock, 03/03/2021.  EMAR-824
            //    //https://stackoverflow.com/a/14321048
            //    .GroupBy(x => x.DosePlusUnit).Select(x => x.FirstOrDefault())
            //    .ToList();
            
            //Adding a try/catch block since this was being screwy on 57c.
            //And since this section is the only thing that's changed.
            //We added the DosePlusUnit field to the DTO and return it here.
            //We also group by it and then grab the first one to avoid duplicates on the happy buttons.
            try
            {
                return preferredDoses?
                    .Select(p =>
                        p == null
                            ? null
                            : new PreferredDoseDto
                            {
                                Dose = p.Dose,
                                DoseUnit = MapMedicationUnit(p.MedicationUnit),
                                //Combine the dose and name into one field so that we can do a GroupBy and then FirstOrDefault on it.
                                //This simulates doing a select distinct in a SQL query.
                                //https://stackoverflow.com/a/14321048
                                //Winston Murdock, 03/03/2021.  EMAR-824
                                DosePlusUnit = p.Dose + p.MedicationUnit.Name
                            })
                    //Simulate a select distinct by grouping by a field and then selecting the first of that field.
                    //Winston Murdock, 03/03/2021.  EMAR-824
                    //https://stackoverflow.com/a/14321048
                    .GroupBy(x => x.DosePlusUnit).Select(x => x.FirstOrDefault())
                    .ToList();
            }
            catch (Exception ex)
            {
                using (EventLog eventLog = new EventLog("Application"))
                {
                    string sException = ex.Message + "\n";
                    sException += "source = " + ex.Source + "\n";
                    sException += ex.StackTrace + "\n";

                    eventLog.Source = "PulseCheck EMAR API";
                    eventLog.WriteEntry(sException, EventLogEntryType.Error, 101, 1);
                } //end using.

                throw new Exception(ex.Message);
            } //end try/catch

        }

        private static IEnumerable<MedicationRouteDto> MapPreferredMedicationRoute(IEnumerable<PreferredMedicationRoute> preferredRoutes)
        {
            return preferredRoutes?
                .Select(p =>
                    p == null
                        ? null
                        : new MedicationRouteDto
                        {
                            Id = p.MedicationRoute.Id,
                            RouteName = p.MedicationRoute.Name,
                            SiteId = p.MedicationRoute.SiteId,
                            Priority = p.MedicationRoute.Priority
                        })
                .ToList();
        }

        private static IEnumerable<FrequencyScheduleDto> MapPreferredFrequencySchedule(IEnumerable<PreferredFrequencySchedule> preferredFrequencySchedules)
        {
            return preferredFrequencySchedules?
                .Select(p =>
                    p == null
                        ? null
                        : new FrequencyScheduleDto
                        {
                            Id = p.FrequencySchedule.Id,
                            SiteId = p.SiteId,
                            ScheduleName = p.FrequencySchedule.Name,
                            PointInTime = p.FrequencySchedule.PointInTime,
                            Prn = p.FrequencySchedule.FrequencyType.Name.ToUpper().Equals("PRN"),
                            FrequencyType = MapFrequencyType(p.FrequencySchedule.FrequencyType),
                            Notes = p.FrequencySchedule.Notes
                        })
                .ToList();
        }

        private static OrderInstructionDto MapOrderInstructions(OrderInstruction orderInstruction)
        {
            if (orderInstruction == null)
            {
                return null;
            }

            return new OrderInstructionDto
            {
                Id = orderInstruction.Id,
                SiteId = orderInstruction.SiteId,
                Description = orderInstruction.Description,
                IsActive = orderInstruction.IsActive
            };
        }

        public static byte ConvertPriorityToByte(OrderPriorities? oPriority)
        {
            //Take in an OrderPriorities object (which can be null)
            //and convert it to a byte.
            //Winston Murdock, 03/11/2021.  EMAR-582

            //Default the return to 0.
            byte byteReturn = 0;
            
            //Try to convert from the priority object to a byte.
            //If the priority is actually in the enumeration (stat and routine right now), then this will work.
            //If it failes, then we'll fall to the catch block and use stat.
            try
            {
                byteReturn = Convert.ToByte(oPriority);
            }
            catch (Exception ex)
            {
                //Not doing anything in this catch.
                //We handle a default case below.
            } //end try/catch

            //If we evaluated to 0, then we'll use stat.
            //Either the try/catch hit an error, or this is null and evaluated to 0.
            if (byteReturn == 0)
            {
                byteReturn = Convert.ToByte(OrderPriorities.Stat);
            } //end if

            //Return.
            return byteReturn;
        } //end ConvertPRiorityStringToByte

        public static byte SetPharmacyVerificationStatusByDispositionTypeCode(string? dispositionTypeCode)
        {
            //Use the disposition type code to see if this order needs pharmacy verification or not.
            //Winston Murdock, 04/29/2021.

            //Default the return to 0.
            //Only set it to 1 if we meet the criteria.
            byte ret = 0;

            //If the parameter is null, then don't check its value.
            if (!string.IsNullOrEmpty(dispositionTypeCode))
            {
                //If the disposition type code is either "INP" or "INPT" or "OBS" then we'll return 1.
                //Else, we'll return 0.
                if (dispositionTypeCode.ToUpper() == "INP" || dispositionTypeCode.ToUpper() == "INPT" || dispositionTypeCode.ToUpper() == "OBS")
                {
                    ret = 1;
                } //end if (value is "INP" or "INPT" or "OBS")
            } //end if (param is not null?)

            // Return.
            return ret;
        } //end SetPharmacyVerificationStatusByDispositionTypeCode

        public static List<FdbNdcInfoDto>? MapFdbNdcInfo(List<FdbNdcInfo>? fdbNdcInfos)
        {
            //If the list is empty, return null.
            if (fdbNdcInfos == null)
            {
                return null;
            }

            //For each entity in the list, map to the DTO.
            return fdbNdcInfos?
                .Select(p =>
                    p == null
                        ? null
                        : new FdbNdcInfoDto
                        {
                            Ndc = p.Ndc,
                            BaseNdc = p.BaseNdc,
                            Repackaged = p.Repackaged,
                            Medid = p.Medid,
                            MedidString = p.MedidString,
                            Packaging = p.Packaging,
                            DaysObsolete = p.DaysObsolete,
                            GcnSeqno = p.GcnSeqno,
                            HiclSeqno = p.HiclSeqno,
                            RoutedGenId = p.RoutedGenId,
                            DoseForm = p.DoseForm,
                            Route = p.Route,
                            DrugCat = p.DrugCat
                        })
                .ToList();

            //throw new NotImplementedException();
        } //end MapFdbNdcInfo

        public static List<CartOrderDto>? MapPatientCartOrders(List<PatientCartOrder>? patientCartOrders)
        {
            //If the list is empty, return null.
            if (patientCartOrders == null)
            {
                return null;
            }

            //For each entity in the list, map to the DTO.
            return patientCartOrders?
                .Select(p =>
                    p == null
                        ? null
                        : new CartOrderDto
                        {
                            PatientId = p.PatientId,
                            UserId = p.UserId,
                            AddDatetime = p.AddDatetime,
                            Priority = (OrderPriorities)p.Priority == OrderPriorities.Stat ? OrderPriorities.Stat : OrderPriorities.Routine,
                            Prn = p.Prn,
                            BeginDatetime = p.BeginDatetime,
                            EndDatetime = p.EndDatetime,
                            UserQuickListItemId = p.UserQuickListItemId,
                            //CartOrderAdministrations = p.CartOrderAdministrations,
                            //User = p.User,
                            Ndc = p.Ndc,
                            PrnIndication = p.PrnIndication,
                            PointInTime = p.PointInTime,
                            MedicationId = p.MedicationId,
                            MedicationRouteId = p.MedicationRouteId,
                            OrderNotes = p.OrderNotes,
                            AntimicrobialIndicationId = p.AntimicrobialIndicationId,
                            AntimicrobialIndicationText = p.AntimicrobialIndicationText,
                            PatientProblemId = p.PatientProblemId,
                            DurationUnitId = p.DurationUnitId,
                            Duration = p.Duration,
                            FrequencyId = p.FrequencyScheduleId
                        })
                .ToList();

            //throw new NotImplementedException();
        } //end MapPatientCartOrders

        public static IEnumerable<UserQuickListItemDto> YieldUserQuickListItemDto(UserQuickListItemDto item)
        {
            //This takes one UserQuickListItem and converts it ao an IEnumerable so that we can call .ToList() on it.
            //Winston Murdock, 08/20/2021.  EMAR-1164.
            yield return item;
        }

        public static bool MapCosignNeeded(PatientOrder patientOrder)
        {

            //If the patient is not a pediatric patient, and the medication
            //is not a "high risk" medication then return false.
            //If the patient is a pediatric patient or the medication
            //is a "high risk" medication then return true.
            //Winston Murdock, 11/05/2021.  PC-26735, PC-26739

            bool bRet = false;

            //If the patient is a pediatric patient or the medication is a "high risk" medication
            //then cosign is required.  If both of those are false, then it's not required.

            //Emerus decided that they don't want all orders on
            //pediatric/no DOB patients to require a cosignature.
            //Eventually, we'll have this controlled by an ini file setting.
            //For now, just comment this out.
            //The only thing that triggers an order as requiring cosignature
            //is its being "high risk."
            //Winston Murdock, 11/29/2021. PC-26828

            //See if this patient is younger than 13.
            //if (patientOrder.IsPedatric)
            //{
            //    //This a pediatric patient.
            //    //Cosign is needed.
            //    bRet = true;
            //}
            //else
            //{

            //This is not a pediatric patient.

            //If this is a "high risk" medication, then cosign is required.
            if (patientOrder.IsHighRisk)
            {
                //Is high risk.
                //Cosign is needed.
                bRet = true;
            }
            else
            {
                //Not high risk.
                //Cosign is not needed.
                bRet = false;
            } //end if (Is this medication high risk?)
            
            //} //end if (Is this patient younger than 13?)

            return bRet;
        } //end MapCosignNeeded
    }
}