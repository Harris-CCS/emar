using Emar.Core.Helpers;
using Emar.Core.Notifications.Model;
using Emar.Core.Orders.Model;
using Emar.Core.OutboundChart.Model;
using Emar.Core.OutboundChart.Service;
using Emar.Core.OutboundData.Model;
using Emar.Core.OutboundData.Service;
using Emar.Core.Sites.Repository;
using Emar.Core.Templates.Model;
using Emar.Data;
using Emar.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Dynamic.Core;

namespace Emar.Core.Templates.Repository
{
    public class TemplateRepository : ITemplateRepository
    {
        private readonly EmarContext _context;
        private readonly ISiteRepository _siteRepository;
        private readonly IOdsEmarOutboundService _odsEmarOutboundService;
        private readonly IOcsEmarOutboundService _ocsEmarOutboundService;
        private readonly MemoryCache _cache;

        public TemplateRepository(EmarContext context, EmarMemoryCache cache, ISiteRepository siteRepository, IOdsEmarOutboundService odsEmarOutboundService,
                                  IOcsEmarOutboundService ocsEmarOutboundService)
        {
            _context = context;
            _siteRepository = siteRepository;
            _odsEmarOutboundService = odsEmarOutboundService ?? throw new ArgumentNullException(nameof(odsEmarOutboundService));
            _ocsEmarOutboundService = ocsEmarOutboundService ?? throw new ArgumentNullException(nameof(ocsEmarOutboundService));
            _cache = cache.Cache;
        }

        public Template GetTemplate(int templateId)
        {
            return _context.Templates
                .Include(t => t.TemplatePromptGroups)
                    .ThenInclude(tp => tp.PromptGroup)
                        .ThenInclude(pg => pg.Prompts)
                            .ThenInclude(p => p.PromptChoices)
                .FirstOrDefault(t => t.Id == templateId);
        }

        public Template GetTemplateForOrderAction(long orderId, ActionEnum action, int siteId)
        {
            var order = _context.PatientOrders.FirstOrDefault(o => o.Id == orderId);

            if (order == null)
                throw new ArgumentException($"No patient_orders exists for order id '{orderId}'.",
                    nameof(orderId));

            Debug.Assert(order.MedicationRouteId != null, "order.MedicationRouteId != null");
            var templateId = GetTemplateId(action, siteId, order.MedicationRouteId.Value);

            return templateId.HasValue ? GetTemplate(templateId.Value) : null;
        }

        public Template GetTemplateForAdministrationAction(long adminId, ActionEnum action, int siteId)
        {
            var info = _context.OrderAdministrations
                .Select(a => new
                {
                    a.Id,
                    a.PatientOrder.MedicationRouteId
                })
                .FirstOrDefault(a => a.Id == adminId);

            if (info == null)
                throw new ArgumentException($"No administrations for administration id '{adminId}'.", nameof(adminId));

            Debug.Assert(info.MedicationRouteId != null, "info.MedicationRouteId != null");
            var templateId = GetTemplateId(action, siteId, info.MedicationRouteId.Value);

            return templateId.HasValue ? GetTemplate(templateId.Value) : null;
        }

        private int? GetTemplateId(ActionEnum action, int siteId, int routeId)
        {
            var templateMappers = GetActionRouteTemplates().ToList();

            var templateId = ((templateMappers
                                   .FirstOrDefault(a =>
                                       a.ActionId == (int)action
                                       && a.MedicationRouteId == routeId
                                       && a.SiteId == siteId)?.TemplateId
                               ?? templateMappers
                                   .FirstOrDefault(a =>
                                       a.ActionId == (int)action
                                       && a.MedicationRouteId == null
                                       && a.SiteId == siteId)?.TemplateId)
                              ?? templateMappers
                                  .FirstOrDefault(a =>
                                      a.ActionId == (int)action
                                      && a.MedicationRouteId == routeId
                                      && a.SiteId == null)?.TemplateId)
                             ?? templateMappers
                                 .FirstOrDefault(a =>
                                     a.ActionId == (int)action
                                     && a.MedicationRouteId == null
                                     && a.SiteId == null)?.TemplateId;
            return templateId;
        }

        private IEnumerable<ActionRouteTemplate> GetActionRouteTemplates()
        {
            return _cache.GetOrCreate("All" + CacheKeys.ActionRouteTemplates, entry =>
            {
                entry.SlidingExpiration = TimeSpan.FromMinutes(30);
                var ret =
                    _context.ActionRouteTemplates.ToList();
                entry.Size = ret.Count;
                return ret;
            });
        }

        public IEnumerable<OrderAvailableAction> GetSiteOrderActions(int siteId)
        {
            return _cache.GetOrCreate(siteId + CacheKeys.OrderActions, entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30);
                var ret =
                    _context.OrderAvailableActions
                        .Include(oa => oa.Action)
                        .Where(o => o.SiteId == siteId).ToList();
                entry.Size = ret.Count;
                return ret;
            });
        }

        public IEnumerable<OrderAdministrationAvailableAction> GetSiteOrderAdministrationActions(int siteId)
        {
            return _cache.GetOrCreate(siteId + CacheKeys.OrderAdministrationActions, entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30);
                var ret =
                    _context.OrderAdministrationAvailableActions
                        .Include(oa => oa.Action)
                        .Where(o => o.SiteId == siteId).ToList();
                entry.Size = ret.Count;
                return ret;
            });
        }

        #region Filing Actions Methods

        public long FileOrderEvent(in int userId, long orderId, ActionEnum action, int siteId, int? templateId = null,
            Dictionary<string, string> templateResponses = null)
        {
            var order = _context.PatientOrders
                .Include(o => o.OrderEvents)
                .Include(o => o.OrderAdministrations)
                    .ThenInclude(a => a.OrderEvents)
                        .ThenInclude(e => e.OrderEventDetails)
                .Include(o => o.OrderAdministrations)
                    .ThenInclude(n => n.OrderAdministrationNotifications)
                .FirstOrDefault(oa => oa.Id == orderId);

            if (order == null)
                throw new ArgumentException($"No order with id '{orderId}'.", nameof(orderId));

            long? adminId = null;

            //Get the current time for the site.
            var siteNow = _siteRepository.GetSiteTimeZone(siteId).NowWithTimeZoneOffset();
            DateTimeOffset eventTime = siteNow;

            switch (action)
            {
                case ActionEnum.CoSign:
                    // CoSign has no other actions than writing to the Events

                    break;
                case ActionEnum.Cancel:
                    //We need to delete any administrations for this order before canceling it.
                    //Romel confirmed this understanding.
                    //Winston Murdock, 02/19/2021.  EMAR-717.

                    //Loop through all of the administrations for this order.
                    //Again, the magic of EntityFramework makes it so that I don't
                    //have to re-query the DB to get them.
                    foreach (var administration in order.OrderAdministrations)
                    {
                        //Delete this administration.
                        _context.Remove(administration);
                    } //end foreach.

                    //Set the status of the order to cancelled.
                    order.OrderStatus = OrderStatus.Cancelled.ToString();

                    //If the pharmacy verification status is 2, flip it back to 1
                    //so that a pharmacist has to verify the order again.
                    //Winston Murdock, 07/09/2021.  EMAR-1085
                    if (order.PharmacyVerificationStatus == 2)
                    {
                        //Reset the status from 2 to 1.
                        order.PharmacyVerificationStatus = 1;
                    } //end if
                    
                    break;
                case ActionEnum.Delete:
                    if (ValidateOrderCancelOrDelete(order, action))
                    {
                        foreach (var @event in order.OrderEvents.Where(e => e.OrderAdministrationId != null))
                        {
                            _context.RemoveRange(@event.OrderEventDetails);
                            _context.Remove(@event);
                        }
                        //////_context.RemoveRange(order.OrderEvents.Where(e => e.OrderAdministrationId != null));
                        _context.RemoveRange(order.OrderAdministrations);

                        order.OrderStatus = action == ActionEnum.Cancel
                            ? OrderStatus.Cancelled.ToString()
                            : OrderStatus.Deleted.ToString();

                        foreach (var @administration in order.OrderAdministrations)
                        {
                            foreach (var @notification in @administration.OrderAdministrationNotifications)
                            {
                                _context.Remove(@notification);
                            }
                        }
                    }

                    break;
                case ActionEnum.Give:
                    //Can't get here unless this is a PRN order.
                    //The "give" link won't be shown in the UI for non PRN orders.
                    //Thusly, we don't need any logic to check if this is a PRN order or not.
                    
                    //Create the order administration event.
                    OrderAdministration admin;
                    order.OrderAdministrations.Add(admin = new OrderAdministration
                    {
                        AdministeringUserId = userId,
                        AdministrationDatetime = siteNow,
                        AdministrationSystemDatetime = siteNow,
                        AdministrationScheduledDatetime = siteNow,
                        PointInTime = order.PointInTime
                    });

                    //Check if this order is point in time or not.
                    if (!order.PointInTime)
                    {
                        //If not point in time, mark it as completed.
                        //Else, set it to ongoing.
                        order.OrderStatus = OrderStatus.Completed.ToString();
                    }
                    {
                        //Set it to ongoing.
                        order.OrderStatus = OrderStatus.OnGoing.ToString();
                    } //end if
                    
                    //Save changes so that the administration child of the order has an ID.
                    //Per Brad, Entity Framework handles the logic of getting us the ID for
                    //the entry that we just saved.
                    try
                    {
                        _context.SaveChanges();

                        //Get the ID so that we can pass it along to CreateOrderEvent.
                        adminId = admin.Id;
                    }
                    catch (Exception e)
                    {
                        throw new Exception($"Error when saving '{action}' action to the database.", e);
                    }

                    break;
                case ActionEnum.Hold:
                    //If the pharmacy verification status is 2, flip it back to 1
                    //so that a pharmacist has to verify the order again.
                    //Winston Murdock, 07/09/2021.  EMAR-1085
                    if (order.PharmacyVerificationStatus == 2)
                    {
                        //Reset the status from 2 to 1.
                        order.PharmacyVerificationStatus = 1;
                    } //end if

                    //Change the status to "OnHold"
                    //Winston Murdock, 07/20/2021.  Emar-1071.
                    order.OrderStatus = OrderStatus.OnHold.ToString();

                    //Set the OnHold flag to true for future administrations of this order.
                    //Winston Murdock, 07/21/2021.  EMAR-1071.
                    //If we have a template ID...
                    if (templateId.HasValue)
                    {
                        //Get the value of templates.event_datetime_prompt_id.
                        int? eventDateTimePromptId = GetEventDatetimePromptId((int)templateId);

                        //If this field was not null...
                        if (eventDateTimePromptId.HasValue)
                        {
                            //If the user selected a date/time...
                            if (templateResponses.TryGetValue(eventDateTimePromptId.ToString(), out var userEnteredDateTime))
                            {
                                //We know we have a value.
                                //If that value can be converted to a date/time offset....
                                if (DateTimeOffset.TryParse(userEnteredDateTime, out var potentialEventTime))
                                {
                                    //We could convert that value to a date time.
                                    //Set the eventTime variable to it.
                                    eventTime = potentialEventTime;
                                } //end if
                            } //end if
                        } //end if
                    } //end if

                    foreach (var orderAdministration in order.OrderAdministrations
                        .Where(a => a.AdministrationScheduledDatetime > eventTime))
                    {
                        if (orderAdministration.OnHold == false
                            && orderAdministration.MissedDose == false
                            // not Non-Point-In-Time Given
                            && ((orderAdministration.PointInTime && orderAdministration.StopDatetime == null)
                                // not Point-In-Time Given
                                || (orderAdministration.PointInTime == false &&
                                    orderAdministration.AdministrationDatetime == null)))
                        {
                            orderAdministration.OnHold = true;
                        }
                    }

                    break;
                //case ActionEnum.Repeat:
                //    break;
                //case ActionEnum.Reschedule:
                //    break;
                case ActionEnum.UnHold:
                    //Change the status to "OnGoing" if this is a point in time order.
                    //Else, change the status to "Pending."
                    //Winston Murdock, 07/20/2021.  Emar-1071.
                    order.OrderStatus = order.PointInTime
                        ? OrderStatus.OnGoing.ToString()
                        : OrderStatus.Pending.ToString();

                    //Set the OnHold flag to false for all future administrations of this order.
                    //Winston Murdock, 07/21/2021.  EMAR-1071.
                    //If we have a template ID...
                    if (templateId.HasValue)
                    {
                        //Get the value of templates.event_datetime_prompt_id.
                        int? eventDateTimePromptId = GetEventDatetimePromptId((int)templateId);

                        //If this field was not null...
                        if (eventDateTimePromptId.HasValue)
                        {
                            //If the user selected a date/time...
                            if (templateResponses.TryGetValue(eventDateTimePromptId.ToString(), out var userEnteredDateTime))
                            {
                                //We know we have a value.
                                //If that value can be converted to a date/time offset....
                                if (DateTimeOffset.TryParse(userEnteredDateTime, out var potentialEventTime))
                                {
                                    //We could convert that value to a date time.
                                    //Set the eventTime variable to it.
                                    eventTime = potentialEventTime;
                                } //end if
                            } //end if
                        } //end if
                    } //end if

                    foreach (var orderAdministration in order.OrderAdministrations
                        .Where(a => a.AdministrationScheduledDatetime > eventTime
                                    && a.OnHold))
                    {
                        orderAdministration.OnHold = false;
                    }

                    break;
                //case ActionEnum.Modify:
                //    break;
                case ActionEnum.PharmVerification:
                    //Update the Pharmacy Verification Status field from 1 to 2.
                    //Winston Murdock, 04/02/2021.  EMAR-795
                    order.PharmacyVerificationStatus = 2;
                    break;

                //// The following actions are Administration-only actions
                //case ActionEnum.Acknowledge:
                //    break;
                case ActionEnum.OrderDiscontinue:
                    //Set the order's status to pending discontinue.
                    order.OrderStatus = OrderStatus.PendingDiscontinue.ToString();

                    //Remove all future administration beyond the user-selected discontinue time.
                    //Winston Murdock, 02/20/2021.  EMAR-716

                    //If we have a template ID...
                    if (templateId.HasValue)
                    {
                        //Get the value of templates.event_datetime_prompt_id.
                        int? eventDateTimePromptId = GetEventDatetimePromptId((int)templateId);

                        //If this field was not null...
                        if (eventDateTimePromptId.HasValue)
                        {
                            //If the user selected a date/time...
                            if (templateResponses.TryGetValue(eventDateTimePromptId.ToString(), out var userEnteredDateTime))
                            {
                                //We know we have a value.
                                //If that value can be converted to a date/time offset....
                                if (DateTimeOffset.TryParse(userEnteredDateTime, out var potentialEventTime))
                                {
                                    //We could convert that value to a date time.
                                    //Set the eventTime variable to it.
                                    eventTime = potentialEventTime;
                                } //end if
                            } //end if
                        } //end if
                    } //end if

                    //Get the list of all administrations that are in the future
                    //i.e. after the entered discontinue time.
                    var administrationsToCancel = order.OrderAdministrations.Where
                    (
                        a => a.AdministrationScheduledDatetime > eventTime
                    );

                    //Now delete all of the future administrations.
                    foreach (var administration in administrationsToCancel)
                    {
                        //Delete this administration.
                        _context.Remove(administration);
                    } //end foreach.

                    //If the pharmacy verification status is 2, flip it back to 1
                    //so that a pharmacist has to verify the order again.
                    //Winston Murdock, 07/09/2021.  EMAR-1085
                    if (order.PharmacyVerificationStatus == 2)
                    {
                        //Reset the status from 2 to 1.
                        order.PharmacyVerificationStatus = 1;
                    } //end if

                    break;
                case ActionEnum.CompleteDiscontinue:
                    //Update the order's status to Discontinued.
                    //Winston Murdockm 02/20/2021.
                    order.OrderStatus = OrderStatus.Discontinued.ToString();

                    //If the pharmacy verification status is 2, flip it back to 1
                    //so that a pharmacist has to verify the order again.
                    //Winston Murdock, 07/09/2021.  EMAR-1085
                    if (order.PharmacyVerificationStatus == 2)
                    {
                        //Reset the status from 2 to 1.
                        order.PharmacyVerificationStatus = 1;
                    } //end if

                    break;
                //case ActionEnum.MissedDose:
                //    break;
                case ActionEnum.FollowUp:
                    //This case being empty is intentional.
                    //We were hitting an exception here when this was commented out
                    //because we were faling into the "default" case.
                    //This was blessed by Colin, Debi, and Merrily at 10:18 PM on 02/20/2021.
                    //Winston Murdock, 02/20/2021.  Winston Murdock
                    break;
                //case ActionEnum.Complete:
                //    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(action), action, $"From '{nameof(FileOrderEvent)}'.");
            }

            var orderEventId = CreateOrderEvent(userId, siteId, action, templateId, templateResponses, order.Id, adminId);

            return orderEventId;
        }

        public long FileAdminEvent(in int userId, long adminId, ActionEnum action, int siteId, int? templateId = null,
            Dictionary<string, string> templateResponses = null)
        {
            var admin = _context.OrderAdministrations
                .Include(oa => oa.PatientOrder)
                    .ThenInclude(o => o.OrderAdministrations)
                    .ThenInclude(n => n.OrderAdministrationNotifications)
                .FirstOrDefault(oa => oa.Id == adminId);

            if (admin == null)
                throw new ArgumentException($"No administration with id '{adminId}'.", nameof(adminId));

            var siteNow = _siteRepository.GetSiteTimeZone(siteId).NowWithTimeZoneOffset();

            //For anywhere that we'll pull the 
            DateTimeOffset eventTime = siteNow;
            
            switch (action)
            {
                case ActionEnum.Acknowledge:
                    if (admin.AcknowledgeUserId != null)
                        throw new ArgumentException(
                            $"Unable to perform 'Acknowledge' action.  Administration has already been acknowledged.  (administration: {admin.Id}, order {admin.PatientOrderId})");

                    // Update the field on the administration
                    admin.AcknowledgeUserId = userId;
                    admin.AcknowledgeDatetime = siteNow;
                    break;
                case ActionEnum.CoSign:
                    // CoSign has no other actions than writing to the Events
                    break;
                case ActionEnum.Cancel:
                case ActionEnum.CompleteDiscontinue:
                case ActionEnum.Delete:
                    // Remove any notifications associated with this administration
                    foreach (var notification in admin.OrderAdministrationNotifications)
                    {
                        _context.Remove(notification);
                    }
                    break;
                case ActionEnum.Give:
                    if (ValidateAdminGive(admin))
                    {
                        if (!admin.PointInTime)
                        {
                            if (admin.PatientOrder.OrderStatus == OrderStatus.Pending.ToString())
                            {
                                admin.AdministeringUserId = userId;
                                //We might need to use the prompt id to get
                                //the prompt object and then use its timestamp here
                                //instead of using siteNow.
                                //Winston Murdock, 01/26/2021.
                                admin.AdministrationDatetime = siteNow;
                                admin.AdministrationSystemDatetime = siteNow;
                                admin.PatientOrder.OrderStatus = OrderStatus.OnGoing.ToString();
                            }
                        }
                        else if (admin.PointInTime)
                        {
                            admin.AdministeringUserId = userId;
                            //We might need to use the prompt id to get
                            //the prompt object and then use its timestamp here
                            //instead of using siteNow.
                            //Winston Murdock, 01/26/2021.
                            admin.AdministrationDatetime = siteNow;
                            admin.AdministrationSystemDatetime = siteNow;

                            // all administrations are "accounted for", order "Completed" else order "OnGoing"
                            admin.PatientOrder.OrderStatus = admin.PatientOrder.OrderAdministrations.All(a =>
                                a.MissedDose
                                || a.AdministrationDatetime != null)
                                ? OrderStatus.Completed.ToString()
                                : OrderStatus.OnGoing.ToString();
                        }

                        //If the user is giving an administration where OnHold = true
                        //Flip the OnHold flag to false for all administrations of this patient order...
                        //I changed this from only updating future administrations to updating all
                        //administrations (both past, present, and future).
                        //This understanding was confirmed with Debi Vose via phone 10:38 PM, 01/19/2021.
                        //Winston Murdock 02/19/2021.  EMAR-721
                        //if (admin.OnHold)
                        if (admin.OnHold)
                        {
                            //Get all administrations for this order.
                            //Due to the magic of EntityFramework, I don't have to go out to the context and get them.
                            //This administration knows about its patient order parent.
                            //And the patient order knows about all of its administration children.
                            foreach (var orderAdministration in admin.PatientOrder.OrderAdministrations)
                            {
                                //Set the OnHold flag to false for this administration.
                                orderAdministration.OnHold = false;
                            } //end foreach.
                        } //end if (administration or order is OnHold?)
                        
                        // Acknowledge any notifications that were waiting for this administration.
                        foreach (var notification in admin.OrderAdministrationNotifications
                            .Where(n => (
                                n.CategoryCode == NotificationCategoryEnum.Pending.Value ||
                                n.CategoryCode == NotificationCategoryEnum.PossibleOverdue.Value
                            )))
                        {
                            notification.AcknowledgedDateTime = siteNow;
                        }
                    }

                    break;
                case ActionEnum.Hold:
                    if (ValidateAdminHold(admin))
                    {
                        admin.OnHold = true;
                        admin.PatientOrder.OrderStatus = OrderStatus.OnHold.ToString();

                        foreach (var orderAdministration in admin.PatientOrder.OrderAdministrations
                            .Where(a => a.AdministrationScheduledDatetime > admin.AdministrationScheduledDatetime))
                        {
                            if (orderAdministration.OnHold == false
                                && orderAdministration.MissedDose == false
                                // not Non-Point-In-Time Given
                                && ((orderAdministration.PointInTime && orderAdministration.StopDatetime == null)
                                    // not Point-In-Time Given
                                    || (orderAdministration.PointInTime == false &&
                                        orderAdministration.AdministrationDatetime == null)))
                            {
                                orderAdministration.OnHold = true;
                            }
                        }

                        // Acknowledge any notifications associated with this admin that required a follow up.
                        foreach (var notification in admin.OrderAdministrationNotifications
                            .Where(n => n.CategoryCode == NotificationCategoryEnum.FollowUp.Value))
                        {
                            notification.AcknowledgedDateTime = siteNow;
                        }
                    }

                    //If the pharmacy verification status is 2 for this
                    //administration's patient order, flip it back to 1
                    //so that a pharmacist has to verify the order again.
                    //Winston Murdock, 07/09/2021.  EMAR-1085
                    if (admin.PatientOrder.PharmacyVerificationStatus == 2)
                    {
                        //Reset the status from 2 to 1.
                        admin.PatientOrder.PharmacyVerificationStatus = 1;
                    } //end if

                    break;
                case ActionEnum.MissedDose:
                    if (ValidateMissedDose(admin, siteNow))
                    {
                        admin.MissedDose = true;
                        admin.OnHold = false;

                        admin.PatientOrder.OrderStatus = admin.PatientOrder.OrderAdministrations.Any(a =>
                            !a.MissedDose
                            && a.AdministrationDatetime == null)
                            ? OrderStatus.OnGoing.ToString()
                            : OrderStatus.Completed.ToString();
                    }

                    //If the pharmacy verification status is 2 for this
                    //administration's patient order, flip it back to 1
                    //so that a pharmacist has to verify the order again.
                    //Winston Murdock, 07/09/2021.  EMAR-1085
                    if (admin.PatientOrder.PharmacyVerificationStatus == 2)
                    {
                        //Reset the status from 2 to 1.
                        admin.PatientOrder.PharmacyVerificationStatus = 1;
                    } //end if

                    break;
                // TODO: Winston - code out the Administration actions here - refer to the Workflow doc in the Team api files
                //case ActionEnum.OrderDiscontinue:
                //    break;
                //case ActionEnum.Repeat:
                //    break;
                case ActionEnum.Reschedule:
                    //********************************************************************************
                    //For administrations only (i.e. not for orders themselves).
                    //
                    //1) Calculate the difference between the time the user entered on the template
                    //   and the scheduled time for this administration.
                    //2) Set the scheduled time for this administration to the time the user entered
                    //   on the template.
                    //3) Update the scheduled time for all future administrations by the difference
                    //   between the time the user enetered and the scheduled time of the administration.
                    //
                    //Note: This requires that the "reschedule" template (currently id = 3) have the
                    //      event_datetime_prompt_id field filled out (current that prompt is 39).
                    //      Else, it will always use siteNow instead of the time the user entered.
                    //Note 2: The UI prevents the user from selecting a time in the past.
                    //        So we don't have to handle that case here.
                    //
                    //Scenario 1: It's currently 1:30 PM, the patient has an administration scheduled for
                    //            2 PM, and they will be in XRAY at 2 PM.  This would let them move this
                    //            administration to say 3 PM (and then bump the future ones by 1 hour).
                    //Scenario 2: It's 2:30 PM, and you're just now getting to the 2 PM administration.
                    //            This will move the 2 PM administration to 2:30 PM and then will push
                    //            out any future administrations by 30 minutes.
                    //Scenario 3: It's currently 1:30 PM, and this administration is scheduled for 3 PM.
                    //            I want to move this administration from 3 PM to 2 PM (which is earlier
                    //            than the currently scheduled time but later than siteNow).  We will
                    //            update this administration's scheduled time to 2 PM and then move all
                    //            future administions to be 1 hour earlier.
                    //Scenario 4: The first administration is 9 PM.  It's 8:30 PM now.  Then 9:30 PM,
                    //            10 PM, 10:30 PM, etc....
                    //            What if we reschedule administration 2 to be an earlier time than
                    //            administration 1?
                    //            TBD later with product
                    //
                    //This understanding was signed off on by Colin, Debi, Romel, Sylvie, and Hsi-An.
                    //Winston Murdock, 02/19/2021.  EMAR-264/EMAR-715.
                    //********************************************************************************

                    //Note: scenario 3 isn't working on 57c.  I'll revisit this after we get MVP out the door.
                    
                    //If we have a template ID...
                    if (templateId.HasValue)
                    {
                        //Get the value of templates.event_datetime_prompt_id.
                        int? eventDateTimePromptId = GetEventDatetimePromptId((int)templateId);

                        //If this field was not null...
                        if (eventDateTimePromptId.HasValue)
                        {
                            //If the user selected a date/time...
                            if (templateResponses.TryGetValue(eventDateTimePromptId.ToString(), out var userEnteredDateTime))
                            {
                                //We know we have a value.
                                //If that value can be converted to a date/time offset....
                                if (DateTimeOffset.TryParse(userEnteredDateTime, out var potentialEventTime))
                                {
                                    //We could convert that value to a date time.
                                    //Set the eventTime variable to it.
                                    eventTime = potentialEventTime;
                                } //end if
                            } //end if
                        } //end if
                    } //end if
                    
                    //Calculate the difference between the time the user entered and the
                    //time this administration is currently scheduled for.
                    //If we're moving the administration to an earlier time (that is still
                    //later than right now), this will be a negative value.
                    //If we're pushing the administration out to a future time, this will
                    //be a positive value.
                    var timeOffset = eventTime - admin.AdministrationScheduledDatetime;

                    //Get the list of all future administration events
                    //Defined as the scheduled date time being after the time
                    //of the administration being rescheduled.
                    //Use EF to get this administration's order and then to get all of its administration children
                    //Rather than querying the db context again (i.e. we save a DB hit).
                    var futureAdmins = admin.PatientOrder.OrderAdministrations.Where
                    (
                        //Only grab the administration where the 
                        //scheduled date time is after the scheduled time
                        //for this administration.
                        //Also exclude this administration.
                        a => a.AdministrationScheduledDatetime > admin.AdministrationScheduledDatetime
                        && a.Id != admin.Id
                    );

                    //Loop through the list of administrations and add the timeOffset to the scheduled datetime.
                    //If timeOffset is a positive value, then we'll move the future administrations forward by that offest.
                    //If timeOffset is a negative value, then we'll move the future administrations backward by that offset.
                    foreach (var futureAdmin in futureAdmins)
                    {
                        futureAdmin.AdministrationScheduledDatetime = futureAdmin.AdministrationScheduledDatetime.Add(timeOffset);
                    } //end foreach loop.

                    //Update the time of this one administration to the chosen time.
                    //Have to do this after grabbing/updating the future administrations.
                    admin.AdministrationScheduledDatetime = eventTime;

                    //If the pharmacy verification status is 2 for this
                    //administration's patient order, flip it back to 1
                    //so that a pharmacist has to verify the order again.
                    //Winston Murdock, 07/09/2021.  EMAR-1085
                    if (admin.PatientOrder.PharmacyVerificationStatus == 2)
                    {
                        //Reset the status from 2 to 1.
                        admin.PatientOrder.PharmacyVerificationStatus = 1;
                    } //end if

                    break;
                case ActionEnum.UnHold:
                    if (admin.OnHold == false)
                    {
                        throw new ArgumentException(
                            $"Unable to perform 'UnHold' action.  Administration is not currently not on hold.  (administration: {admin.Id}, order {admin.PatientOrderId})");
                    }

                    admin.OnHold = false;
                    admin.PatientOrder.OrderStatus = admin.PatientOrder.PointInTime
                        ? OrderStatus.OnGoing.ToString()
                        : OrderStatus.Pending.ToString();

                    foreach (var orderAdministration in admin.PatientOrder.OrderAdministrations
                        .Where(a => a.AdministrationScheduledDatetime > admin.AdministrationScheduledDatetime
                                    && a.OnHold))
                    {
                        orderAdministration.OnHold = false;
                    }
                    break;
                case ActionEnum.FollowUp:
                    // Acknowledge any notifications associated with this admin that required a follow up.
                    foreach (var notification in admin.OrderAdministrationNotifications
                        .Where(n => n.CategoryCode == NotificationCategoryEnum.FollowUp.Value))
                    {
                        notification.AcknowledgedDateTime = siteNow;
                    }
                    break;
                //case ActionEnum.Complete:
                //    break;


                //// The following actions are at the Order Level only
                //case ActionEnum.Repeat:
                //    break;
                //case ActionEnum.Cancel:
                //    break;
                //case ActionEnum.Delete:
                //    break;
                //case ActionEnum.Modify:
                //    break;
                //case ActionEnum.PharmVerification:
                //    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(action), action, $"From '{nameof(FileAdminEvent)}'.");
            }

            return CreateOrderEvent(userId, siteId, action, templateId, templateResponses, admin.PatientOrderId, admin.Id);
        }

        private long CreateOrderEvent(int userId, int siteId, ActionEnum action, int? templateId, Dictionary<string, string> templateResponses,
            long orderId, long? adminId = null)
        {
            var siteNow = _siteRepository.GetSiteTimeZone(siteId).NowWithTimeZoneOffset();
            var timeZoneName = _siteRepository.GetSiteTimeZone(siteId);

            //Use the template ID to get the ID of the prompt for the date/time entry field.
            //Then grab the value of that field.
            //Winston Murdock, Bradley Marshall, 01/27/2021.  EMAR-452
            //Default to now..
            DateTimeOffset eventTime = siteNow;

            //If we have a template ID...
            if (templateId.HasValue)
            {
                //Get the value of templates.event_datetime_prompt_id.
                int? eventDateTimePromptId = GetEventDatetimePromptId((int)templateId);

                //If this field was not null...
                if (eventDateTimePromptId.HasValue)
                {
                    //If the user selected a date/time...
                    if (templateResponses.TryGetValue(eventDateTimePromptId.ToString(), out var userEnteredDateTime))
                    {
                        //We know we have a value.
                        //If that value can be converted to a date/time offset....
                        if (DateTimeOffset.TryParse(userEnteredDateTime, out var potentialEventTime))
                        {
                            //We could convert that value to a date time.
                            //Set the eventTime variable to it.
                            eventTime = potentialEventTime;
                        } //end if
                    } //end if
                } //end if
            } //end if

            // call the GetOrderAdminStatus() method to determine the new order administration status
            var newOrderAdmin = GetOrderAdminStatus(orderId, adminId, action);
            // Create the new Event
            var orderEvent = new OrderEvent
            {
                PatientOrderId = orderId,
                OrderAdministrationId = adminId,
                //If the user selected a date/time, this will have that value.
                //IF they didn't select one, this will have Now.
                EventDateTime = eventTime,
                AddUserId = userId,
                AddDatetime = siteNow,
                ActionId = (int)action,
                TemplateId = templateId
            };

            DateTimeOffset? eventDateTime = null;
            // ODS specific variables
            var medicationRouteType = (string)null;
            DateTimeOffset? ivStopDateTime = null;
            var stopTimeUnknown = (string)null;
            var siteLocation = (string)null;
            int? siteNumber = null;
            char[] IVSites = { '1', '2', '3', '4' };
            var orderedPromptList = new List<OcsPromptParameters>();

            if (templateResponses != null)
            {
                var templateName = templateId != null ? GetTemplateName((int)templateId) : null;
                // get the map for prompt IDs to ordinal position for a particular template
                var promptSequenceMap = GetPromptSequenceFromTemplate(templateId);
                // this 'for' loop is far from ideal but decided to use a list to map the prompts into
                // this approach seems to work but could be rewritten when time permits
                for (int i=0; i <= promptSequenceMap.Count; i++) { orderedPromptList.Add(null); }

                // loop through each prompt
                foreach (var response in templateResponses)
                {
                    if (!int.TryParse(response.Key, out int promptId))
                        throw new ArgumentException(
                            $"Found Body Key in JSON body ({response.Key}) that isn't an integer.",
                            nameof(templateResponses));

                    var enteredText = GetResponseValue(response.Value, promptId);
                    orderEvent.OrderEventDetails.Add(new OrderEventDetail
                    {
                        PromptText = GetPromptText(promptId),
                        EnteredText = enteredText,
                        PromptId = promptId,

                        //Pull the Chart Markup.
                        //Per Debi Vose, here is what we need to do here.
                        //If prompts.prompt_type = 'DropDownListBox' or 'threeStateButton' Then
                        //    Get the chart_markup for the select choice.
                        //Else
                        //    Get chart markup from prompt.
                        //End If

                        //Winston Murdock, 02/09/2021. EMAR-649
                        ChartMarkup = GetChartMarkup(promptId, enteredText)
                    });

                    //TODO: If they click a notify on the template, we need to send a notification to someone.
                    //if(prompt.PromptType == "Notify")

                    // ODS specific code 
                    var promptText = GetPromptText(promptId);
                    // TODO: move validity check at bottom of this loop up here to help performance 
                    // may want to change if/else to switch/case on template name or id/enum
                    if (GetPromptType(promptId) == PromptType.DateTime &&
                        (promptText.Equals(OdsConstants.GivenAt) || promptText.Equals(OdsConstants.At) || promptText.Equals(OdsConstants.DocumentedAt)))
                    {
                        DateTimeOffset offset;
                        if (!DateTimeOffset.TryParse(enteredText, out offset))
                        {
                            offset = siteNow; // error handling here?
                        }
                        eventDateTime = TimeAdjustedForTimeZone(timeZoneName, offset);
                        enteredText = eventDateTime?.ToString("yyyyMMddHHmmss");
                    }
                    else if (templateName.Equals(OdsConstants.FollowUp) && (promptText.Equals(OdsConstants.IVDiscontinued) ||
                             promptText.Equals(OdsConstants.IVContinuedUponTransfer)) &&
                             !string.IsNullOrEmpty(enteredText))
                    {
                        // if both have a value, then last one processed is used. Revisit this code.
                        if (!DateTimeOffset.TryParse(enteredText, out var offset))
                        {
                            ivStopDateTime = null; // error handling here?
                        }
                        else
                        {
                            ivStopDateTime = TimeAdjustedForTimeZone(timeZoneName, offset);
                            enteredText = ivStopDateTime?.ToString();
                        }
                    }
                    else if (templateName.Equals(OdsConstants.FollowUp) && promptText.Equals(OdsConstants.IVStopTimeUnknown) &&
                             enteredText.Equals(OdsConstants.True))
                    {
                        stopTimeUnknown = OdsConstants.Unknown;
                    }
                    else if ((templateName.Equals(OdsConstants.Intravenous) || templateName.Equals(OdsConstants.IntravenousInI)) &&
                             (promptText.Equals(OdsConstants.IVLocation) || promptText.Equals(OdsConstants.OtherIVSite)) &&
                             !string.IsNullOrEmpty(enteredText))
                    {
                        // if both have a value, then last one processed is used. Revisit this code.
                        siteLocation = enteredText;
                    }
                    else if (templateName.Equals(OdsConstants.Intravenous) && promptText.ToLower().Equals(OdsConstants.SiteNumber.ToLower()) &&
                             !string.IsNullOrEmpty(enteredText))
                    {
                        siteNumber = int.TryParse(enteredText.Substring(0, 1), out int siteInt) ? siteInt : (int?)null;
                    }
                    else if (templateName.Equals(OdsConstants.IntravenousInI) && GetPromptType(promptId) == PromptType.CheckBox
                             && promptText.IndexOfAny(IVSites, 0) == 0 && enteredText.Equals(OdsConstants.True))
                    {
                        siteNumber = int.TryParse(promptText.Substring(0, 1), out int siteInt) ? siteInt : (int?)null;
                    }
                    // perform validity check and add to prompt list if valid
                    if ((!string.IsNullOrEmpty(enteredText) && GetPromptType(promptId) != PromptType.CheckBox)
                     || (GetPromptType(promptId) == PromptType.CheckBox && enteredText.Equals(OdsConstants.True)))
                    {
                        // place entered prompt text in proper position in the orderedPromptList
                        orderedPromptList[promptSequenceMap[promptId]] = new OcsPromptParameters
                        {
                            promptId = promptId,
                            promptType = GetPromptType(promptId),
                            promptLabel = promptText,
                            promptValue = enteredText,
                            chartMarkup = GetChartMarkup(promptId, enteredText)
                        };
                    }
                }

                medicationRouteType = GetMedicationRouteType(orderId);
            }

            _context.Add(orderEvent);

            //If this is a give action and we have an administration ID, then
            //we need to update the timestamp for the administration as well.
            if (action == ActionEnum.Give && adminId.HasValue)
            {
                //Give wth an admin ID.
                //Update the related administration with the event time.
                var admin = _context.OrderAdministrations.FirstOrDefault(a => a.Id == adminId);
                admin.AdministrationDatetime = eventTime;
            } //end if

            //_hostService.RecordCosignAction(Patient, User, orderId, datetime);

            try
            {
                _context.SaveChanges();
            }
            catch (Exception e)
            {
                throw new Exception($"Error when saving '{action}' action to the database.", e);
            }
            // Ods administration event code
            var stopTimeEntered = stopTimeUnknown ?? (ivStopDateTime?.ToString("yyyyMMddHHmm"));
            var parameter = new OdsAdministrationParameters
            {
                OrderId = orderId,
                AdministrationId = adminId ?? 0,
                Action = action.ToString(),
                AddUserId = userId,
                SiteId = siteId,
                AddDatetime = siteNow,
                EventDateTime = eventDateTime ?? siteNow,
                IVEdit = action == ActionEnum.Give ? "G" : (action == ActionEnum.FollowUp && stopTimeEntered != null) ? "S" : null,
                IVLocation = siteLocation,
                IVSite = siteNumber,
                IVType = medicationRouteType,
                StopDate = stopTimeEntered,
                NewOrderAdmin = newOrderAdmin,
            };
            _odsEmarOutboundService.SendAdministrationAction(parameter);
            _ocsEmarOutboundService.SendChartTemplateMarkup(orderedPromptList, siteId, orderId, userId, action, adminId ?? 0, newOrderAdmin);

            return orderEvent.Id;
        }

        private string GetResponseValue(string responseValue, in int promptId)
        {
            switch (GetPromptType(promptId))
            {
                case PromptType.CheckBox:
                case PromptType.CheckBoxCheckChildren:
                case PromptType.CheckBoxShowChildren:
                    if (responseValue == "null" || responseValue == null)
                        return "false";
                    return responseValue;
                default:
                    return responseValue;
            }
        }

        private PromptType GetPromptType(in int promptId)
        {
            var dict = GetPromptDict();

            if (!dict.ContainsKey(promptId))
                throw new ArgumentException($"Requested prompt with Id {promptId}, which does not exist.", nameof(promptId));

            var typeString = dict[promptId].Split("|")[0];
            if (!Enum.TryParse(typeString, out PromptType type))
                throw new ArgumentException(
                    $"Found the PromptID #{promptId} points to a prompt type (\"{typeString}\") which doesn't exist in the PromptType Enum.",
                    nameof(promptId));

            return type;
        }

        private string GetPromptText(in int promptId)
        {
            var dict = GetPromptDict();

            if (!dict.ContainsKey(promptId))
                throw new ArgumentException($"Requested prompt with Id {promptId}, which does not exist.", nameof(promptId));

            return dict[promptId].Split("|")[1];
        }

        private Dictionary<int, string> GetPromptDict()
        {
            return _cache.GetOrCreate("All" + CacheKeys.Prompts, entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30);
                var prompts = _context.Prompts.Where(p => p.IsActive);
                var ret = new Dictionary<int, string>();
                foreach (var p in prompts)
                    ret.Add(p.Id, p.PromptType + "|" + p.PromptText);
                entry.Size = ret.Count;
                return ret;
            });
        }

        private string GetChartMarkup(int promptId, string enteredValue)
        {
            //If prompts.prompt_type = 'DropDownListBox' or 'threeStateButton' Then
            //	Get the chart_markup for the selected value in the prompt choice.
            //Else
            //	Get the chart markup from the prompt itself.
            //End If
            //Winston Murdock, 02/09/2021. EMAR-649

            //First, grab the Prompt Type for the prompt.
            var promptType = GetPromptType(promptId);
            
            //Chart Markup string that we'll return.
            string chartMarkup = "";

            //If this prompt is either a DropDownListBox or threeStateButton, then
            //grab the chart markup for the prompt choice that the user selected on the screen.
            //If the prompt is any other type, then grab the chart markup from the prompt itself.
            switch (promptType)
            {
                case PromptType.DropDownListBox: case PromptType.threeStateButton:
                    //Grab the list of prompt choices that match the prompt ID and entered value.
                    var promptChoices = _context.PromptChoices.Where(b => b.PromptId == promptId && b.ChoiceText == enteredValue);
                    
                    //If we have at least one prompt choice, then grab the chart markup.
                    //Else, we'll leave the chart markup as an empty string.
                    if (promptChoices.Count() > 0)
                    {
                        chartMarkup = promptChoices.FirstOrDefault().ChartMarkup;
                    } //end if
                    
                    break;
                default:
                    //Grab the list of prompts that match the prompt ID.
                    var prompts = _context.Prompts.Where(a => a.Id == promptId);

                    //If we have at least one prompt, then grab the chart markup.
                    //Else, we'll leave the chart markup as an empty string.
                    if (prompts.Count() > 0)
                    {
                        chartMarkup = prompts.FirstOrDefault().ChartMarkup;
                    } //end if

                    break;
            } //end switch case on prompt type
            
            //Return the chart markup for the passed in prompt id.
            return chartMarkup;
        } //end GetChartMarkup

        private static bool ValidateOrderCancelOrDelete(PatientOrder order, ActionEnum action)
        {
            // Status must be pending
            // or OnHold.  Winston Murdock, 02/16/2021.  EMAR-691.
            if (!order.OrderStatus.Equals(OrderStatus.Pending.ToString(), StringComparison.InvariantCultureIgnoreCase) && !order.OrderStatus.Equals(OrderStatus.OnHold.ToString(), StringComparison.InvariantCultureIgnoreCase))
            {
                throw new ArgumentException(
                    $"Unable to perform '{action}' action.  Order is not in 'Pending' or 'OnHold' status.  (order {order.Id})",
                    nameof(order.Id));
            }

            if (order.OrderAdministrations.Any(a =>
                a.MissedDose))
            {
                throw new ArgumentException(
                    $"Unable to perform '{action}' action.  One or more administrations have 'Missed Dose' status.  (order {order.Id})");
            }

            //We do want to allow canceling orders with OnHold administrations.
            //Thusly, I'm commenting out this check.
            //Romel confirmed this understanding.
            //Winston Murdock, 02/19/2021.  EMAR-717.
            //if (order.OrderAdministrations.Any(a =>
            //    a.OnHold))
            //{
            //    throw new ArgumentException(
            //        $"Unable to perform '{action}' action.  One or more administrations have 'On Hold' status.  (order {order.Id})");
            //}

            if (order.OrderAdministrations.Any(admin => admin.AdministrationDatetime != null))
            {
                throw new ArgumentException(
                    $"Unable to perform '{action}' action.  One or more administrations have already been given.  (order {order.Id})");
            }

            return true;
        }

        private static bool ValidateAdminGive(OrderAdministration admin)
        {
            // Non-Point-In-Time Given
            if ((admin.PointInTime == false && admin.StopDatetime != null)
                // Point-In-Time Given
                || (admin.PointInTime && admin.AdministrationDatetime != null))
            {
                throw new ArgumentException(
                    $"Unable to perform 'Give' action.  Administration has already been given.  (administration: {admin.Id}, order {admin.PatientOrderId})");
            }

            if (admin.MissedDose)
            {
                throw new ArgumentException(
                    $"Unable to perform 'Give' action.  Administration is in 'Missed Dose' status.  (administration: {admin.Id}, order {admin.PatientOrderId})");
            }

            if (!admin.PointInTime)
            {
                if (admin.PatientOrder.OrderStatus != OrderStatus.Pending.ToString())
                {
                    throw new ArgumentException(
                        $"Unable to perform 'Give' action.  Order not in 'Pending' status.  (administration: {admin.Id}, order {admin.PatientOrderId})");
                }
            }

            return true;
        }

        private static bool ValidateAdminHold(OrderAdministration admin)
        {
            if (admin.OnHold)
            {
                throw new ArgumentException(
                    $"Unable to perform 'Hold' action.  Administration is already 'On Hold'.  (administration: {admin.Id}, order {admin.PatientOrderId})");
            }

            // Not Pending or Late
            if (admin.AdministrationDatetime != null)
            {
                throw new ArgumentException(
                    $"Unable to perform 'Hold' action.  Administration is not 'Pending' nor 'Late'.  (administration: {admin.Id}, order {admin.PatientOrderId})");
            }

            if (admin.MissedDose)
            {
                throw new ArgumentException(
                    $"Unable to perform 'Hold' action.  Administration is in 'MissedDose' status.  (administration: {admin.Id}, order {admin.PatientOrderId})");
            }

            return true;
        }

        private static bool ValidateMissedDose(OrderAdministration admin, DateTimeOffset siteNow)
        {
            if (admin.PointInTime == false)
            {
                throw new ArgumentException(
                    $"Unable to perform 'MissedDose' action.  Not a 'Point-In-Time' order.  (administration: {admin.Id}, order {admin.PatientOrderId})");
            }

            if (admin.MissedDose)
            {
                throw new ArgumentException(
                    $"Unable to perform 'MissedDose' action.  Administration is already in 'Missed Dose' status.  (administration: {admin.Id}, order {admin.PatientOrderId})");
            }

            if (admin.AdministrationScheduledDatetime >= siteNow)
            {
                throw new ArgumentException(
                    $"Unable to perform 'MissedDose' action.  Administration is scheduled in the future.  (administration: {admin.Id}, order {admin.PatientOrderId})");
            }

            return true;
        }
        #endregion

        #region Utility methods

        public int GetTemplateId(string templateName)
        {
            var id = _context.Templates.FirstOrDefault(t => t.Name == templateName)?.Id;
            if (!id.HasValue)
                throw new ArgumentException($"Template Name \"{templateName}\" doesn't exist in the database.",
                    nameof(templateName));
            return id.Value;
        }

        public string GetTemplateName(int templateId)
        {
            var name = _context.Templates.FirstOrDefault(t => t.Id == templateId)?.Name;
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException($"Template Id \"{templateId}\" doesn't exist in the database.",
                    nameof(templateId));
            return name;
        }

        public int? GetEventDatetimePromptId(int templateId)
        {
            //EMAR-626.
            //Winston Murdock, 01/27/2021.

            //var list = _context.Templates.Where(i => i.Id == templateId);
            //var ret = list.FirstOrDefault().EventDatetimePromptId;
            //return ret;

            //Grab the Datetime Prompt Id for the passed in template Id.
            //Look for it in the cache.  If it's not there, grab it from the DB instead.
            return _cache.GetOrCreate(templateId + CacheKeys.TemplateDatetimePromptIds, entry =>
            {
                //Using a sliding expiration instead of an absolute expiration.
                //This value is fairly stable and likely won't change.
                //So as long as at least one hit is made to this cache every 30 minutes, then it won't expire.
                //Else, if this were a template that is only accessed infrequently, it will fall off the cache at 30 mintues.
                entry.SlidingExpiration = TimeSpan.FromMinutes(30);
                var ret = _context.Templates.FirstOrDefault(i => i.Id == templateId).EventDatetimePromptId;
                entry.Size = 1;
                return ret;
            });
        } //end GetEventDatetimePromptId

        public string GetMedicationRouteType(long orderId)
        {
            var query = from po in _context.PatientOrders
                        join mr in _context.MedicationRoutes on po.MedicationRouteId equals mr.Id
                        where po.Id == orderId
                        select mr.Type;
            var type = query.FirstOrDefault();

            // do a sanity check that type has a proper value
            if (!string.IsNullOrEmpty(type) && !type.Equals(OdsConstants.Injection)
                && !type.Equals(OdsConstants.Infusion) && !type.Equals(OdsConstants.Hydration))
                type = "";
            return type;
        }

        public Dictionary<int,int> GetPromptSequenceFromTemplate(int? templateId)
        {
            var prompts = _context.GetPromptSequenceFromTemplateFunctions.FromSqlInterpolated(
                          $"SELECT [prompt_id],[row_num] FROM get_prompt_sequence_from_template({templateId})"
                          ).ToList();

            Dictionary<int, int> promptDict = new Dictionary<int, int>();
            foreach (var promptRow in prompts)
            {
                promptDict.Add(promptRow.prompt_id, (int)promptRow.row_num);
            }

            return promptDict;
        }

        /// <summary>
        /// Get the status of whether or not this is a new order administration
        /// Should be called before the order event row is inserted 
        /// </summary>
        /// <param name="orderId">Patient order Id</param>
        /// <param name="adminId">Administration Id</param>
        /// <param name="action">Action enumeration</param>
        /// <returns>Bool type</returns>
        public bool GetOrderAdminStatus(long orderId, long? adminId, ActionEnum action)
        {
            // pharmacy verification action won't cause a new order admin
            // other actions with null adminIds also won't cause a new order admin - need to adjust?
            if (action == ActionEnum.PharmVerification || (adminId == null && (action == ActionEnum.CoSign
                                                                            || action == ActionEnum.Cancel
                                                                            || action == ActionEnum.Delete
                                                                            || action == ActionEnum.OrderDiscontinue
                                                                            || action == ActionEnum.CompleteDiscontinue
                                                                            || action == ActionEnum.Hold)))
                return false;

            // other actions w/o an adminId are handled afterwards (that's the intent, anyhow)
            if (adminId == null)
                return true;

            // get all order admin Ids
            var query = from oe in _context.OrderEvents
                        join oa in _context.OrderAdministrations on oe.OrderAdministrationId equals oa.Id
                        where oe.PatientOrderId == orderId
                        orderby oa.AdministrationScheduledDatetime descending
                        select oe.OrderAdministrationId;
            //            var orderAdminId = query.FirstOrDefault();
            //            return orderAdminId > 0 && orderAdminId != adminId;
            var orderEventPresent = query.FirstOrDefault() != null && query.FirstOrDefault() != 0;
            var foundAdminId = query.FirstOrDefault(e => e == adminId);

            // if there is at least one order event for the patient order and there is no matching admin Id,
            //   then return true. Else return false.
            return orderEventPresent && foundAdminId != adminId;
        }

        public DateTimeOffset TimeAdjustedForTimeZone(string siteTimeZone, DateTimeOffset tzo)
        {
            var tz = TimeZoneInfo.GetSystemTimeZones().FirstOrDefault(z =>
                z.DisplayName == siteTimeZone
                || z.DaylightName == siteTimeZone
                || z.StandardName == siteTimeZone);
            if (tz == null)
                throw new ArgumentException(
                    "Invalid Timezone passed to Emar.Core.Templates.Repository.TimeAdjustedForTimeZone()",
                    nameof(siteTimeZone));

            var siteTzOffset = tz.BaseUtcOffset;
            if (tz.IsDaylightSavingTime(tzo))
                siteTzOffset = siteTzOffset.Add(new TimeSpan(0, 60, 0));
            return (int)(siteTzOffset - tzo.Offset).TotalMinutes == 0 ? tzo : tzo.ToOffset(siteTzOffset);
        }
        #endregion
    }
}