using System;
using System.Linq;
using Emar.Core.Helpers;
using Emar.Core.Medications.Model;
using Emar.Core.Medications.Model.Mappings;
using Emar.Core.Orders.Service;
using Emar.Core.Users.Model.Mappings;
using Emar.Data.Entities;

namespace Emar.Core.Orders.Model.Mappings
{
    public static class OrderMapper
    {
        public static PatientOrderDto MapOrder(PatientOrder patientOrder, string dateFormat, string drugDbVendor, string orderBase,
            string adminBase)
        {
            if (patientOrder == null)
            {
                return null;
            }

            PatientOrderDto patientOrderDto = new PatientOrderDto
            {
                DateFormat = dateFormat,
                Id = patientOrder.Id,
                PatientId = patientOrder.PatientId,
                AddUserId = patientOrder.AddUserId,
                AddUser = UserMapper.MapUser(patientOrder.AddUser),
                AddDatetime = patientOrder.AddDatetime,
                OrderingPhysicianId = patientOrder.OrderingPhysicianId,
                OrderingPhysicianUser = UserMapper.MapUser(patientOrder.OrderPhysicianUser),
                MedicationId = patientOrder.MedicationId,
                Medication = MedicationMapper.MapMedication(patientOrder.Medication),
                Dose = patientOrder.Dose,
                DoseUnit = MedicationMapper.MapMedicationUnit(patientOrder.MedicationUnit),
                MedicationRoute = MedicationMapper.MapMedicationRoute(patientOrder.MedicationRoute),
                ////Priority = (OrderPriorities)Enum.Parse(typeof(OrderPriorities), patientOrder.Priority),
                FrequencyId = patientOrder.FrequencyScheduleId,
                FrequencySchedule = MedicationMapper.MapMedicationFrequency(patientOrder.FrequencySchedule),
                Prn = patientOrder.Prn,
                PointInTime = patientOrder.PointInTime,
                OrderStatus = patientOrder.OrderStatus,
                //OrderStatusCode = (OrderStatuses)Enum.Parse(typeof(OrderStatuses), patientOrder.OrderStatus),
                BeginDatetime = patientOrder.BeginDatetime,
                //BeginDate = DateTimeHelper.GetDate(patientOrder.BeginDatetime, dateFormat),
                //BeginTime = DateTimeHelper.GetTime(patientOrder.BeginDatetime),
                EndDatetime = patientOrder.EndDateTime,
                //EndDate = DateTimeHelper.GetDate(patientOrder.EndDateTime, dateFormat),
                //EndTime = DateTimeHelper.GetTime(patientOrder.EndDateTime),
                OrderNotes = patientOrder.OrderNotes,
                OrderAdministrations = patientOrder.OrderAdministrations?.Select(admin => MapOrderAdministration(admin, dateFormat, OrderStatuses.Pending, null)).ToList(),
                OrderInteractions = patientOrder.OrderInteractions?.Select(interaction => MedicationMapper.MapOrderInteraction(interaction, drugDbVendor)).ToList(),
                AllergyReactions = patientOrder.AllergyReactionsView?.Select(MedicationMapper.MapAllergyReactionView).ToList()
                ////OrderEvents = patientOrder.OrderEvents?.Select(OrderMapper.MapOrderEvent).Where(@event => @event.AdministrationId == null).ToList()
                ////OrderEvents = patientOrder.OrderEvents?.Select(ev => OrderMapper.MapOrderEvent(ev)).ToList()
            };

            patientOrderDto.OrderAdministrations =
                patientOrder.OrderAdministrations?
                    .Select(admin =>
                        MapOrderAdministration(admin, dateFormat, patientOrderDto.OrderStatusCode, adminBase))
                    .ToList();

            if (!Enum.TryParse(patientOrder.OrderStatus, out OrderStatuses orderStatus))
                orderStatus = OrderStatuses.Pending;
            else
                patientOrderDto.OrderStatusCode = orderStatus;

            if (!string.IsNullOrWhiteSpace(orderBase) && !string.IsNullOrWhiteSpace(adminBase))
                patientOrderDto.AvailableActions = ActionService.AvailableOrderActions(patientOrderDto, orderBase);

            patientOrderDto.NextActionTime = null;
            if (patientOrderDto.OrderAdministrations == null) return patientOrderDto;

            foreach (var admin in patientOrderDto.OrderAdministrations.Where(admin =>
                    admin.TimeNeedingAction.HasValue)
                .Where(admin => !patientOrderDto.NextActionTime.HasValue
                                || patientOrderDto.NextActionTime > admin.TimeNeedingAction))
                patientOrderDto.NextActionTime = admin.TimeNeedingAction;

            return patientOrderDto;
        }

        public static PatientOrder MapCartOrderToOrder(PatientCartOrder cartOrder)
        {
            if (cartOrder == null)
            {
                return null;
            }

            PatientOrder patientOrder = new PatientOrder
            {
                PatientId = cartOrder.PatientId,
                AddUserId = cartOrder.UserId,
                AddDatetime = cartOrder.AddDatetime,
                //OrderingPhysicianId = cartOrder.OrderingPhysicianId,
                MedicationId = cartOrder.MedicationId,
                Dose = cartOrder.Dose,
                MedicationUnitId = cartOrder.MedicationUnitId,
                MedicationRouteId = cartOrder.MedicationRouteId,
                Priority = cartOrder.Priority,
                FrequencyScheduleId = cartOrder.FrequencyScheduleId,
                Prn = cartOrder.Prn,
                PointInTime = cartOrder.PointInTime,
                OrderStatus = OrderStatuses.Pending.ToString(),
                BeginDatetime = cartOrder.BeginDatetime,
                EndDateTime = cartOrder.EndDatetime,
                OrderNotes = cartOrder.OrderNotes,
                OrderAdministrations = cartOrder.CartOrderAdministrations?.Select(MapCartToOrderAdministration).ToList(),
                OrderInteractions = cartOrder.OrderInteractions,
                OrderReactions = cartOrder.OrderReactions
            };

            return patientOrder;
        }

        public static OrderAdministration MapCartToOrderAdministration(CartOrderAdministration cartOrderAdministration)
        {
            if (cartOrderAdministration == null)
            {
                return null;
            }

            OrderAdministration administration = new OrderAdministration
            {
                AdministrationScheduledDatetime = cartOrderAdministration.AdministrationScheduledDatetime,
                StopScheduledDatetime = cartOrderAdministration.StopScheduledDatetime,
                PointInTime = cartOrderAdministration.PointInTime
            };

            return administration;
        }

        public static OrderAdministrationDto MapOrderAdministration(OrderAdministration administration,
            string dateFormat, OrderStatuses orderStatusCode, string adminBase)
        {
            if (administration == null)
            {
                return null;
            }

            OrderAdministrationDto administrationDto = new OrderAdministrationDto
            {
                DateFormat = dateFormat,
                Id = administration.Id,
                OrderId = administration.PatientOrderId,
                AdministrationScheduledDatetime = administration.AdministrationScheduledDatetime,
                AdministrationSystemDatetime = administration.AdministrationSystemDatetime,
                AdministrationDatetime = administration.AdministrationDatetime,
                AdministeringUserId = administration.AdministeringUserId,
                StopScheduledDatetime = administration.StopScheduledDatetime,
                StopInputDatetime = administration.StopInputDatetime,
                StopDatetime = administration.StopDatetime,
                StopUserId = administration.StopUserId,
                AcknowledgeUserId = administration.AcknowledgeUserId,
                AcknowledgeDatetime = administration.AcknowledgeDatetime,
                PointInTime = administration.PointInTime,
                OnHold = administration.OnHold,
                MissedDose = administration.MissedDose
                ////AdministrationEvents = administration.OrderEvents?.Select(OrderMapper.MapOrderEvent).Where(@event => @event.AdministrationId == null).ToList()
                ////AdministrationEvents = administration.OrderEvents?.Select(MapOrderEvent).ToList()
            };

            if (!string.IsNullOrWhiteSpace(adminBase))
                administrationDto.AvailableActions =
                    ActionService.AvailableAdministrationActions(administrationDto, orderStatusCode, adminBase);

            return administrationDto;
        }

        public static OrderEventDto MapOrderEvent(OrderEvent @event, string dateFormat)
        {
            if (@event == null)
            {
                return null;
            }

            OrderEventDto eventDto = new OrderEventDto
            {
                Id = @event.Id,
                DateFormat = dateFormat,
                OrderId = @event.PatientOrderId,
                AdministrationId = @event.OrderAdministrationId,
                EventDateTime = @event.EventDateTime,
                //EventDate = DateTimeHelper.GetDate(@event.EventDateTime, dateFormat),
                //EventTime = DateTimeHelper.GetTime(@event.EventDateTime),
                SystemDateTime = @event.AddDatetime,
                //SystemDate = DateTimeHelper.GetDate(@event.AddDatetime, dateFormat),
                //SystemTime = DateTimeHelper.GetTime(@event.AddDatetime),
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
                MedicationId = dbObj.MedicationId,
                Medication = MedicationMapper.MapMedication(dbObj.Medication),
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
                //FrequencySchedule = MedicationMapper.MapMedicationFrequency(dbObj.FrequencySchedule),
                //OrderNotes = dbObj.OrderNotes
            };

            return ret;
        }

        internal static PatientCartOrder MapUserQuickListItemToPatientCartOrder(UserQuickListItem dbObj)
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
                //PointInTime = dbObj.point
                OrderNotes = dbObj.OrderNotes,

                // Properties from CartOrderDto
                PatientId = -1,
                UserId = -1,
                AddDatetime = DateTimeOffset.Now,
                Priority = 1,
                Prn = false,
                BeginDatetime = DateTimeOffset.Now,
                //EndDatetime = ??? -- need to look at duration
                UserQuickListItemId = dbObj.Id
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
                MedicationId = dbObj.MedicationId,
                Medication = MedicationMapper.MapMedication(dbObj.Medication),
                Dose = dbObj.Dose,
                DoseUnit = MedicationMapper.MapMedicationUnit(dbObj.MedicationUnit),
                MedicationRoute = MedicationMapper.MapMedicationRoute(dbObj.MedicationRoute),
                FrequencySchedule = MedicationMapper.MapMedicationFrequency(dbObj.FrequencySchedule),
                DurationInMinutes = dbObj.DurationInMinutes,
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
                MedicationId = dbObj.MedicationId,
                Medication = MedicationMapper.MapMedication(dbObj.Medication),
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

        #region Model mappers

        public static MedicationModel MapOrderItemToModel(EmarOrderType orderType, object item, long patientId)
        {
            return MapOrderItemToModel(orderType, item, patientId, -99);
        }

        private static MedicationModel MapOrderItemToModel(EmarOrderType orderType, object item, long patientId, int userId)
        {
            return
                orderType == EmarOrderType.UserQuickListItem ? OrderMapper.MapUserQuickListItemToModel((UserQuickListItem)item, patientId) :
                orderType == EmarOrderType.DepartmentPreferredListItem ? OrderMapper.MapDepartmentPreferredListItemToModel((DepartmentPreferredListItem)item, patientId, userId) :
                orderType == EmarOrderType.GroupRememberedOrder ? OrderMapper.MapGroupListItemToModel((GroupListItem)item, patientId, userId) :
                null;
        }

        public static MedicationModel MapOrderItemDtoToModel(EmarOrderType orderType, object item, long patientId, int userId)
        {
            return
                orderType == EmarOrderType.UserQuickListItem ? OrderMapper.MapUserQuickListItemDtoToModel((UserQuickListItemDto)item, patientId) :
                orderType == EmarOrderType.DepartmentPreferredListItem ? OrderMapper.MapDepartmentPreferredListItemDtoToModel((DepartmentPreferredItemDto)item, patientId, userId) :
                orderType == EmarOrderType.GroupRememberedOrder ? OrderMapper.MapGroupListItemDtoToModel((GroupListItemDto)item, patientId, userId) :
                null;
        }

        private static MedicationModel MapUserQuickListItemToModel(UserQuickListItem item, long patientId)
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
                Medication = MedicationMapper.MapMedication(item.Medication),
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
                FrequencyScheduleId = item.FrequencySchedule.Id,
                InternalDrugId = null,
                IsActive = null,
                MedicationId = item.MedicationId,
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

        private static MedicationModel MapDepartmentPreferredListItemToModel(DepartmentPreferredListItem item, long patientId, int userId)
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
                Medication = MedicationMapper.MapMedication(item.Medication),
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
                FrequencyScheduleId = item.FrequencySchedule.Id,
                InternalDrugId = null,
                IsActive = null,
                MedicationId = item.MedicationId,
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

        private static MedicationModel MapGroupListItemToModel(GroupListItem item, long patientId, int userId)
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
                Medication = MedicationMapper.MapMedication(item.Medication),
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
                FrequencyScheduleId = item.FrequencySchedule.Id,
                InternalDrugId = null,
                IsActive = null,
                MedicationId = item.MedicationId,
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

        public static MedicationModel MapPatientOrderToModel(PatientOrder order, int? userId, int? siteId)
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
                Medication = MedicationMapper.MapMedication(order.Medication),
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
    }
}