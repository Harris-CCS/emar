using System;
using System.Collections.Generic;
using System.Linq;
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
                           && patientOrder.Medication.DrugId != "COMBO"
                           //////&& patientOrder.Medication.SiteId != -1
                           && patientOrder.MedicationUnit.SiteId == codeShareSites
                               .FirstOrDefault(c =>
                                   c.Entity == OrderRepository.CodeShareEntity.MedicationUnit)?
                               .SharedSiteId
                    ? MapMedicationUnit(patientOrder.MedicationUnit)
                    : null,
                MedicationRoute = patientOrder.MedicationRoute != null
                                  && patientOrder.Medication.DrugId != "COMBO"
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
                                    && patientOrder.Medication.DrugId != "COMBO"
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
                PatientProblemId = patientOrder.PatientProblemId,
                PatientProblem = PatientMapper.MapPatientProblem(patientOrder.PatientProblem),
                Duration = patientOrder.Duration,
                DurationUnitId = patientOrder.DurationUnitId,
                DurationUnit = MapDurationUnit(patientOrder.DurationUnit),
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
                                         || @event.ActionId == (int)ActionEnum.UnHold))
                    .ToList()
            };

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
                OrderReactions = cartOrder.OrderReactions
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
                        || e.ActionId == (int)ActionEnum.UnHold)
                    .Select(MapOrderEvent)
                    .ToList().OrderBy(e => e.EventDatetime)
            };

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
                OrderNotes = dbObj.OrderNotes
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
                OrderNotes = dbObj.OrderNotes
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
                OrderNotes = dbObj.OrderNotes//,
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
                DurationUnitId = dbObj.DurationUnitId
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
                OrderNotes = dbObj.OrderNotes
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
                DurationUnitId = dbObj.DurationUnitId
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
                OrderNotes = dbObj.OrderNotes
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
                DurationUnitId = dbObj.DurationUnitId
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
                AccountNumber = null
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
                AccountNumber = null
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
                AccountNumber = null
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
                AccountNumber = null
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
                AccountNumber = null
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
                AccountNumber = null
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
                AccountNumber = null
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
            IEnumerable<CodeSharedId> codeShareSites)
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
                                            .SharedSiteId))
                        })
                    .ToList(),
                Administrations = administrations?.Select(MapFrequencyScheduleAdministration).ToList(),
                AdministrationInstructions = orderInstructions?.Select(MapOrderInstructions).ToList()
            };

            return ret;
        }

        private static IEnumerable<PreferredDoseDto> MapPreferredMedicationDose(IEnumerable<PreferredMedicationDose> preferredDoses)
        {
            return preferredDoses?
                .Select(p =>
                    p == null
                        ? null
                        : new PreferredDoseDto
                        {
                            Dose = p.Dose,
                            DoseUnit = MapMedicationUnit(p.MedicationUnit)
                        })
                .ToList();
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
                            SiteId = p.MedicationRoute.SiteId
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
    }
}