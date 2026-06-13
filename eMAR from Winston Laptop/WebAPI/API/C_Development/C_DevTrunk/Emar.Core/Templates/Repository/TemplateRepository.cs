using Emar.Core.Carts.Repository;
using Emar.Core.Helpers;
using Emar.Core.Notifications.Model;
using Emar.Core.Orders.Model;
using Emar.Core.OutboundChart.Model;
using Emar.Core.OutboundChart.Service;
using Emar.Core.OutboundData.Model;
using Emar.Core.OutboundData.Repository;
using Emar.Core.OutboundData.Service;
using Emar.Core.Sites.Repository;
using Emar.Core.Templates.Model;
using Emar.Data;
using Emar.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Linq.Dynamic.Core;

namespace Emar.Core.Templates.Repository
{
    public class TemplateRepository : ITemplateRepository
    {
        private readonly EmarContext _context;
        private readonly ISiteRepository _siteRepository;
        private readonly IEmarOutboundDataRepository _emarOutboundDataRepository;
        private readonly IOdsEmarOutboundService _odsEmarOutboundService;
        private readonly IOcsEmarOutboundService _ocsEmarOutboundService;
        private readonly MemoryCache _cache;

        public TemplateRepository(EmarContext context, EmarMemoryCache cache, ISiteRepository siteRepository, IOdsEmarOutboundService odsEmarOutboundService,
                                  IOcsEmarOutboundService ocsEmarOutboundService, IEmarOutboundDataRepository emarOutboundDataRepository)
        {
            _context = context;
            _siteRepository = siteRepository;
            _odsEmarOutboundService = odsEmarOutboundService ?? throw new ArgumentNullException(nameof(odsEmarOutboundService));
            _ocsEmarOutboundService = ocsEmarOutboundService ?? throw new ArgumentNullException(nameof(ocsEmarOutboundService));
            _emarOutboundDataRepository = emarOutboundDataRepository ?? throw new ArgumentNullException(nameof(emarOutboundDataRepository));
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
            var orderQuery = _context.PatientOrders
                .Include(o => o.OrderAdministrations)
                .Where(oa => oa.Id == orderId)
                .AsQueryable();

            PatientOrder order = orderQuery.FirstOrDefault();
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

                    //Use a helper function to delete the administrations in the list and their children.
                    //This lets us call the same logic in multiple places (cancel, order discontinue, etc...).
                    //Winston Murdock, 05/26/2022.  PC-27264
                    DeleteAdministrationsAndChildren(order.OrderAdministrations.ToList());

                    //Set the status of the order to cancelled.
                    order.OrderStatus = OrderStatus.Cancelled.ToString();

                    //If the pharmacy verification status is 2, flip it back to 1
                    //so that a pharmacist has to verify the order again.
                    //Winston Murdock, 07/09/2021.  EMAR-1085
                    //if (order.PharmacyVerificationStatus == 2)
                    //{
                    //    //Reset the status from 2 to 1.
                    //    order.PharmacyVerificationStatus = 1;
                    //} //end if

                    //Also need to delete the order interactions (and medication interactinos) for this order (if any).
                    //Use a helper function do this since both Cancel and Delete need to call it.
                    //Winston Murdock, 05/05/2022.  PC-27153
                    DeleteInteractionsForOrder(order);

                    break;
                case ActionEnum.Delete:
                    if (ValidateOrderCancelOrDelete(order, action))
                    {
                        // If validation passes, include the extra details we'll need.
                        order = orderQuery
                            .Include(o => o.OrderEvents)
                            .Include(o => o.OrderAdministrations)
                                .ThenInclude(a => a.OrderEvents)
                                    .ThenInclude(e => e.OrderEventDetails)
                            .Include(o => o.OrderAdministrations)
                                .ThenInclude(n => n.OrderAdministrationNotifications)
                            .First();

                        foreach (var @event in order.OrderEvents.Where(e => e.OrderAdministrationId != null))
                        {
                            _context.RemoveRange(@event.OrderEventDetails);
                            _context.Remove(@event);
                        }
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

                    //Also need to delete the order interactions (and medication interactinos) for this order (if any).
                    //Use a helper function do this since both Cancel and Delete need to call it.
                    //Winston Murdock, 05/05/2022.  PC-27153
                    DeleteInteractionsForOrder(order);


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
                        Utilities.LogException(e);
                        throw new Exception($"Error when saving '{action}' action to the database.", e);
                    }

                    break;
                case ActionEnum.Hold:
                    //If the pharmacy verification status is 2, flip it back to 1
                    //so that a pharmacist has to verify the order again.
                    //Winston Murdock, 07/09/2021.  EMAR-1085
                    //if (order.PharmacyVerificationStatus == 2)
                    //{
                    //    //Reset the status from 2 to 1.
                    //    order.PharmacyVerificationStatus = 1;
                    //} //end if

                    //Change the status to "OnHold"
                    //Winston Murdock, 07/20/2021.  Emar-1071.
                    order.OrderStatus = OrderStatus.OnHold.ToString();

                    //Set the OnHold flag to true for future administrations of this order.
                    //Winston Murdock, 07/21/2021.  EMAR-1071.
                    eventTime = GetEventDatetimeFromTemplateResponses(templateId, templateResponses, eventTime);

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
                    //order.OrderStatus = order.PointInTime
                    //    ? OrderStatus.OnGoing.ToString()
                    //    : OrderStatus.Pending.ToString();

                    //If the order for this administration has any administrations that
                    //are given (i.e. they have a value for AdministrationSystemDateTime),
                    //then set set the order's status to Ongoing.
                    //Else, set the order's status to Pending.
                    //Winston Murdock, 09/09/2021.  EMAR-1186.
                    if (order.OrderAdministrations.Any(oa => oa.AdministrationSystemDatetime != null))
                    {
                        //At least one administration has been given - Ongoing.
                        order.OrderStatus = OrderStatus.OnGoing.ToString();
                    }
                    else
                    {
                        //No administrations have been given - Pending.
                        order.OrderStatus = OrderStatus.Pending.ToString();
                    } //end if

                    //Set the OnHold flag to false for all future administrations of this order.
                    //Winston Murdock, 07/21/2021.  EMAR-1071.
                    eventTime = GetEventDatetimeFromTemplateResponses(templateId, templateResponses, eventTime);

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
                    //Moving this logic into a section at the bottom where we check the .ini file just
                    //like we do for actions that create a notification.
                    //We still need to have this section uncommented so that we don't
                    //hit the case below where we passed in an action we aren't handling yet.
                    //But we don't do anything special for pharmacy verification beyond the notification logic.
                    //Winston Murdock, 11/05/2021.  PC-26778
                    break;

                //// The following actions are Administration-only actions
                //case ActionEnum.Acknowledge:
                //    break;
                case ActionEnum.OrderDiscontinue:
                    //Set the order's status to pending discontinue.
                    order.OrderStatus = OrderStatus.PendingDiscontinue.ToString();

                    //Remove all future administration beyond the user-selected discontinue time.
                    //Winston Murdock, 02/20/2021.  EMAR-716

                    eventTime = GetEventDatetimeFromTemplateResponses(templateId, templateResponses, eventTime);

                    //Get the list of all administrations that are in the future
                    //i.e. after the entered discontinue time.
                    var administrationsToCancel = order.OrderAdministrations.Where
                    (
                        a => a.AdministrationScheduledDatetime > eventTime
                    );

                    //Use a helper function to delete the administrations in the list and their children.
                    //This lets us call the same logic in multiple places (cancel, order discontinue, etc...).
                    //Winston Murdock, 05/26/2022.  PC-27264
                    DeleteAdministrationsAndChildren(administrationsToCancel.ToList());

                    //If the pharmacy verification status is 2, flip it back to 1
                    //so that a pharmacist has to verify the order again.
                    ////Winston Murdock, 07/09/2021.  EMAR-1085
                    //if (order.PharmacyVerificationStatus == 2)
                    //{
                    //    //Reset the status from 2 to 1.
                    //    order.PharmacyVerificationStatus = 1;
                    //} //end if

                    break;
                case ActionEnum.CompleteDiscontinue:
                    //Update the order's status to Discontinued.
                    //Winston Murdockm 02/20/2021.
                    order.OrderStatus = OrderStatus.Discontinued.ToString();

                    //If the pharmacy verification status is 2, flip it back to 1
                    //so that a pharmacist has to verify the order again.
                    ////Winston Murdock, 07/09/2021.  EMAR-1085
                    //if (order.PharmacyVerificationStatus == 2)
                    //{
                    //    //Reset the status from 2 to 1.
                    //    order.PharmacyVerificationStatus = 1;
                    //} //end if

                    // Update start/stop times associated with a continuous order
                    UpdateContinuousOrderTimes(order.OrderAdministrations.First(), action, siteId, templateId, templateResponses);

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

            //If this is an action that triggers deleting the notification for an order
            //(and setting the phamracy verification status to 2), then do so.
            //This used to be part of the PharmVerification case above.
            //We added a key to the .ini file for the actions that trigger deleting a notification.
            //So we'll see if the current action is one of those rather than hardcoding this.
            //That lets us manage the list of actions in the .ini file rather than having
            //to recompile the API should this list change (or should one client want it set
            //differently from another client).
            //Winston Murdock, 11/05/2021.  PC-26778

            //Also need to delete any "administration" notifications for this order.
            //We initially only did order administrations, even for events on administrations.
            //Btu we have been asked by Emerus to implement administration notifications too.
            //Romel and Merrily have blessed deleting the notification for all administrations.
            //Winston Murdock, 02/01/2022.  PC=26975
            if (CheckActionIsInIni(action, "deletenotification"))
            {
                //If there's a row in pharmacy_notification_orders for this patient order,
                //grab the notification_id into a variable, then delete the row from the details table.
                //If there are no more rows in pharmacy_notifications_orders for this notification_id,
                //then delete the row from pharmacy_notifications too.
                //Lastly, flip the pharmacy verification status from 1 to 2.
                //Winston Murdock, 09/30/2021.  EMAR-1223
                long notificationId;

                // Get a list of all pharmacy notification administrations for the
                // order we're working with and that have a notification row.
                var adminsWithNotifications =
                (
                    from pna in _context.PharmacyNotificationAdministrations
                    join oa in _context.OrderAdministrations on pna.OrderAdministrationId equals oa.Id
                    where oa.PatientOrderId == order.Id
                    select pna
                ).ToList();

                //Loop through all administrations for this order that have a notification.
                foreach (PharmacyNotificationAdministration pna in adminsWithNotifications)
                {
                    //Get the pharmacy notification for this administration.
                    var pn = _context.PharmacyNotifications.Find(pna.PharmacyNotificationId);

                    //Delete this row from the notification administration table.
                    _context.PharmacyNotificationAdministrations.Remove(pna);

                    //Then delete this row from the notifications table.
                    _context.PharmacyNotifications.Remove(pn);
                } //end foreach

                if (_context.PharmacyNotificationOrders.Any(x => x.PatientOrderId == order.Id))
                {
                    //This one does exist in pharmacy_notification_orders.
                    //Grab the notification_id value, then delete it.
                    var orderDetail = _context.PharmacyNotificationOrders.Where(x => x.PatientOrderId == order.Id).FirstOrDefault();
                    notificationId = orderDetail.PharmacyNotificationId;
                    _context.PharmacyNotificationOrders.Remove(orderDetail);

                    //See if there are any order detail rows for this notification apart from the one we just deleted.
                    //If there are not any other order detail rows, then we'll need to also delete the notification.
                    if (!_context.PharmacyNotificationOrders.Any(x => x.PharmacyNotificationId == notificationId && x.Id != orderDetail.Id))
                    {
                        PharmacyNotification notification = _context.PharmacyNotifications.Find(notificationId);
                        _context.PharmacyNotifications.Remove(notification);
                    } //end if (are there any other orders for this notification in the order details table apart from the one we just deleted?)
                } //end if (is this order in the notification order details table?)

                //Update the Pharmacy Verification Status field from 1 to 2.
                //Winston Murdock, 04/02/2021.  EMAR-795
                order.PharmacyVerificationStatus = 2;
            } //end if

            //Need to look at the .ini file for Ray's service.
            //If the action the user just took on this order is one of the actions listed in the .ini file,
            //then we'll need to create a notification for this order and flip the pharmacy verification status to 1.
            //Winston Murdock, EMAR-1224.
            if (CheckActionIsInIni(action, "order"))
            {
                //If this is an "ER" patient, the order has a "stat" priority, and
                //there is only one administration, then we don't want to do any notification stuff on it.
                //Basically, treat this just like we treat new orders.
                //Winston Murdock, 11/16/2021.  PC-26780
                //Get the patient's disposition type code.
                string dispositionTypeCode = _context.Patients.Find(order.PatientId).DispositionTypeCode;
                Data.Entities.Patient thisPatient = _context.Patients.Find(order.PatientId);
                byte isInPatient = CartOrderRepository.SetPharmacyVerificationStatusByDispositionTypeCode(dispositionTypeCode);
                bool bContinue = true;

                //Also need to check the patient's custom indicators to see
                //if one of them is "EDHOLD" or not.
                //If we've already got isInPatient set to 1, then we don't
                //need to do this check since we know that this
                //is an "inpatient" patient
                //Winston Murdock, 01/06/2022.  PC=26905
                if (isInPatient == 0)
                {
                    isInPatient = CheckPatientIndicators(thisPatient.Id);
                } //end if

                //If this is an "ER" patient)
                if (isInPatient == 0)
                {
                    //If this is an "ER" patient, then dn't create notifications.
                    //by setting bcontinue to false, we won't create a pharmacy notification for this order.
                    //Winston Murdock, 01/06/2022.  PC-26905
                    ////If this order is "stat" priority and has only one administration, then don't continue.
                    //if (order.Priority == (byte)OrderPriorities.Stat && order.OrderAdministrations.Count < 2)
                    //{
                    //    //This is a stat order with only one administration.
                    //    //Do not do any of the notifications logic.
                    bContinue = false;
                    //} //end if
                } //end if

                //If this is an stat order with one administration for an "er" patient, then don't continue.
                if (bContinue)
                {
                    //If there's not already a notification for this order, then create one.
                    //If there's already a notification, then we just need to update the timestamp.
                    if (!_context.PharmacyNotificationOrders.Any(x => x.PatientOrderId == order.Id))
                    {
                        //No notification exists.  Create one.

                        //Create the notification.
                        PharmacyNotification notification = new PharmacyNotification
                        {
                            PatientId = order.PatientId,
                            Type = "Order",
                            EnteredDatetime = siteNow
                        };

                        //Create the notification order detail.
                        PharmacyNotificationOrder notificationOrder = new PharmacyNotificationOrder
                        {
                            PatientOrderId = order.Id
                        };
                        notification.PharmacyNotificationOrders.Add(notificationOrder);

                        //Add the newly created PharmacyNotification (and its InpatientNotificationOrder children) to the context.
                        _context.PharmacyNotifications.Add(notification);

                        //Since we triggered a notification for this order,
                        //Also set it's pharmacy verification status flag to 1
                        //which signifies that a pharmacist needs to verify this.
                        order.PharmacyVerificationStatus = 1;
                    }
                    else
                    {
                        //There already is a notification.
                        //Update the timestamp.
                        //Get the pharmacy notification entity.
                        //Winston Murdock, 11/12/2021.  PC-26781
                        PharmacyNotification existingNotification = _context.PharmacyNotifications.Where(pn => pn.PharmacyNotificationOrders.Any(pno => pno.PatientOrderId == order.Id)).FirstOrDefault();

                        //Now update the timestamp.
                        existingNotification.EnteredDatetime = siteNow;
                    } //end if (Is there already a notification for this order?)
                } //end if
            } //end if

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
                    // Update start/stop times associated with a continuous order
                    UpdateContinuousOrderTimes(admin, action, siteId, templateId, templateResponses);

                    // Remove any notifications associated with this administration
                    foreach (var notification in admin.OrderAdministrationNotifications)
                    {
                        _context.Remove(notification);
                    }
                    break;
                case ActionEnum.Give:
                    if (ValidateAdminGive(admin))
                    {
                        // If this order is currently "Pending Discontinue", then we will need to keep that status even though the user
                        // is performing a Give now.
                        bool revertToPendingDiscontinue = (admin.PatientOrder.OrderStatus == OrderStatus.PendingDiscontinue.ToString());
    
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

                                // Update start/stop times associated with a continuous order
                                UpdateContinuousOrderTimes(admin, action, siteId, templateId, templateResponses);
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

                        // Revert order to "Pending Discontinue" status if necessary
                        if (revertToPendingDiscontinue)
                        {
                            admin.PatientOrder.OrderStatus = OrderStatus.PendingDiscontinue.ToString();
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
                        
                        //When holding an administration, do not hold the order.
                        //Winston Murdock, 01/31/2022.  PC-26974
                        //admin.PatientOrder.OrderStatus = OrderStatus.OnHold.ToString();

                        //We only want to hold this one administration, not all future administrations.
                        //Winston Murdock, 01/31/2022.  PC-26974
                        //foreach (var orderAdministration in admin.PatientOrder.OrderAdministrations
                        //    .Where(a => a.AdministrationScheduledDatetime > admin.AdministrationScheduledDatetime))
                        //{
                        //    if (orderAdministration.OnHold == false
                        //        && orderAdministration.MissedDose == false
                        //        // not Non-Point-In-Time Given
                        //        && ((orderAdministration.PointInTime && orderAdministration.StopDatetime == null)
                        //            // not Point-In-Time Given
                        //            || (orderAdministration.PointInTime == false &&
                        //                orderAdministration.AdministrationDatetime == null)))
                        //    {
                        //        orderAdministration.OnHold = true;
                        //    }
                        //}

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
                    //if (admin.PatientOrder.PharmacyVerificationStatus == 2)
                    //{
                    //    //Reset the status from 2 to 1.
                    //    admin.PatientOrder.PharmacyVerificationStatus = 1;
                    //} //end if

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
                    //if (admin.PatientOrder.PharmacyVerificationStatus == 2)
                    //{
                    //    //Reset the status from 2 to 1.
                    //    admin.PatientOrder.PharmacyVerificationStatus = 1;
                    //} //end if

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

                    eventTime = GetEventDatetimeFromTemplateResponses(templateId, templateResponses, eventTime);

                    //Strip off the seconds and milliseconds from eventTime.
                    //We always save them with no seconds or milliseconds.
                    //But rescheduling (with the default time of now), will have seconds and milliseconds.
                    //This prevents us from using the seconds and milliseconds when rescheduling all future administrations
                    //but not using it (which is correct) when the nightly job creates new administrations.
                    //Winston Murdock, 03/07/2022.  Found as part of the research into PC-27067.
                    eventTime = new DateTimeOffset(eventTime.Year, eventTime.Month, eventTime.Day, eventTime.Hour, eventTime.Minute, 0, 0, eventTime.Offset);

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

                    // Update start/stop times associated with a continuous order
                    UpdateContinuousOrderTimes(admin, action, siteId, templateId, templateResponses);

                    //Calculate the difference between the time we are rescheduling this administration to
                    //and the time it is currently scheduled for (as a number of minutes).
                    //1) If this order is a daily or weekly frequency.
                    //2) Calculate the number of minutes between the new time for this administration
                    //      and the current time for it.
                    //3) If there's an existing offset, grab the value and add (or subtract) the difference
                    //      between the new time and the previously scheduled time to (or from) it.
                    //      Then update the existing row in the DB.
                    //4) If there's not an existing offset, insert a new one with the value from step 2.
                    //If it's a positive value, then we've moved the administration out into the future.
                    //if it's a negative value, then we've moved the administration back into the past.
                    //Winston Murdock, 04/01/2022.  PC-27098

                    //We don't have the frequency schedule info here.
                    //I need to go out and get it.
                    //Use the patient order's frequency schedule ID to get that.
                    //Then use the frequency schedule's frequency type ID to get that.
                    //Then looka t the name of the frequency type.
                    var frequencySchedule = _context.FrequencySchedules.Find(admin.PatientOrder.FrequencyScheduleId);
                    var frequencyType = _context.FrequencyTypes.Find(frequencySchedule.FrequencyTypeId);

                    //Check if it's a daily or weekly frequency.
                    if ((frequencyType.Name == "Daily") || (frequencyType.Name == "Weekly"))
                    {
                        //Calculate the time difference between the administration's currently scheduled time
                        //and the new time we want to reschedule it to.
                        //Then Grab the minutes from that.
                        //TimeSpan.TotalMinutes is a double.
                        //We have to convert that to an int since that's what the DB needs.
                        var newMinutesOffset = Convert.ToInt32(timeOffset.TotalMinutes);


                        //If there is already an offset for this order...
                        if ((_context.FutureAdministrationsReschedules.Any(fra => fra.PatientOrderId == admin.PatientOrderId)))
                        {
                            //We have rescheduled this administration before.

                            //Get the offset for this order.
                            FutureAdministrationsReschedule futureAdministrationsReschedule = _context.FutureAdministrationsReschedules.Where(fra => fra.PatientOrderId == admin.PatientOrderId).FirstOrDefault();

                            //We need to grab the current offset value and
                            //add (or subtract) the new offset to (or from) it.
                            var existingMinutesOffset = futureAdministrationsReschedule.TimeOffsetMinutes;

                            //Total offset is the existing offset plus the new offset.
                            var totalMinutesOffset = existingMinutesOffset + newMinutesOffset;

                            //Now update the offset to the newly calculated offset.
                            futureAdministrationsReschedule.TimeOffsetMinutes = totalMinutesOffset;
                        }
                        else
                        {
                            //This is the first time we are rescheduling this order.
                            //Create a new offset.
                            FutureAdministrationsReschedule futureAdministrationReschedule = new FutureAdministrationsReschedule
                            {
                                PatientOrderId = admin.PatientOrderId,
                                TimeOffsetMinutes = newMinutesOffset
                            };

                            _context.FutureAdministrationsReschedules.Add(futureAdministrationReschedule);
                        } //end if (does this order already have an offset row?)
                    } //end if (is this order a daily or weekly frequency?)

                    //No nweed to save things back to the DB here.
                    //We do that in CreateOrderEvent down below.

                    //If the pharmacy verification status is 2 for this
                    //administration's patient order, flip it back to 1
                    //so that a pharmacist has to verify the order again.
                    //Winston Murdock, 07/09/2021.  EMAR-1085
                    ////if (admin.PatientOrder.PharmacyVerificationStatus == 2)
                    ////{
                    ////    //Reset the status from 2 to 1.
                    ////    admin.PatientOrder.PharmacyVerificationStatus = 1;
                    ////} //end if

                    break;
                case ActionEnum.UnHold:
                    if (admin.OnHold == false)
                    {
                        throw new ArgumentException(
                            $"Unable to perform 'UnHold' action.  Administration is not currently not on hold.  (administration: {admin.Id}, order {admin.PatientOrderId})");
                    }

                    admin.OnHold = false;

                    //Change the status to "OnGoing" if this is a point in time order.
                    //Else, change the status to "Pending."
                    //Winston Murdock, 07/20/2021.  Emar-1071.
                    //admin.PatientOrder.OrderStatus = admin.PatientOrder.PointInTime
                    //    ? OrderStatus.OnGoing.ToString()
                    //    : OrderStatus.Pending.ToString();

                    //If the order for this administration has any administrations that
                    //are given (i.e. they have a value for AdministrationSystemDateTime),
                    //then set set the order's status to Ongoing.
                    //Else, set the order's status to Pending.
                    //Winston Murdock, 09/09/2021.  EMAR-1186.
                    if (admin.PatientOrder.OrderAdministrations.Any(oa => oa.AdministrationSystemDatetime != null))
                    {
                        //At least one administration has been given - Ongoing.
                        admin.PatientOrder.OrderStatus = OrderStatus.OnGoing.ToString();
                    }
                    else
                    {
                        //No administrations have been given - Pending.
                        admin.PatientOrder.OrderStatus = OrderStatus.Pending.ToString();
                    } //end if


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

            //Need to look at the .ini file for Ray's service.
            //If the action the user just took on this administration is one of the actions listed in the .ini file,
            //then we'll need to create a notification for this administration's order and flip the pharmacy verification status to 1.
            //Winston Murdock, EMAR-1224.
            if (CheckActionIsInIni(action, "administration"))
            {
                //We need to actually create an administration notification here, not an order administration.
                //Winston Murdock, 01/31/2022.  PC-26975

                //If this is an "ER" patient, the order has a "stat" priority, and
                //there is only one administration, then we don't want to do any notification stuff on it.
                //Basically, treat this just like we treat new orders.
                //Winston Murdock, 11/16/2021.  PC-26780
                //Get the patient's disposition type code.
                string dispositionTypeCode = _context.Patients.Find(admin.PatientOrder.PatientId).DispositionTypeCode;
                Data.Entities.Patient thisPatient = _context.Patients.Find(admin.PatientOrder.PatientId);
                byte isInPatient = CartOrderRepository.SetPharmacyVerificationStatusByDispositionTypeCode(dispositionTypeCode);
                bool bContinue = true;

                //Also need to check the patient's custom indicators to see
                //if one of them is "EDHOLD" or not.
                //If we've already got isInPatient set to 1, then we don't
                //need to do this check since we know that this
                //is an "inpatient" patient
                //Winston Murdock, 01/06/2022.  PC=26905
                if (isInPatient == 0)
                {
                    isInPatient = CheckPatientIndicators(thisPatient.Id);
                } //end if

                //If this is an "ER" patient)
                if (isInPatient == 0)
                {
                    //If this is an "ER" patient, then don't create notifications.
                    //by setting bContinue to false, we won't create a pharmacy notification for this order.
                    //Winston Murdock, 01/06/2022.  PC-26905
                    ////If this order is "stat" priority and has only one administration, then don't continue.
                    //if (admin.PatientOrder.Priority == (byte)OrderPriorities.Stat && admin.PatientOrder.OrderAdministrations.Count < 2)
                    //{
                    //    //This is a stat order with only one administration.
                    //    //Do not do any of the notifications logic.
                        bContinue = false;
                    //} //end if
                } //end if
                 
                //If the flag says to make a notification for this administration...
                if (bContinue)
                {
                    //If there's not already an administration notification for this order, then create one.
                    //If there's already an administration notification, then we need to update the Administration Id and the time stamp.
                    if (!_context.PharmacyNotificationAdministrations.Any(x => x.OrderAdministrationId == admin.Id))
                    {
                        //Create the notification.
                        PharmacyNotification notification = new PharmacyNotification
                        {
                            PatientId = admin.PatientOrder.PatientId,
                            //Type = "Order",
                            Type = "Administration",
                            EnteredDatetime = siteNow
                        };

                        //Create the notification order detail.
                        PharmacyNotificationAdministration notificationAdministration = new PharmacyNotificationAdministration
                        {
                            OrderAdministrationId = admin.Id
                        };
                        notification.PharmacyNotificationAdministrations.Add(notificationAdministration);

                        //Add the newly created PharmacyNotification (and its InpatientNotificationAdministration children) to the context.
                        _context.PharmacyNotifications.Add(notification);

                        //Since we triggered a notification for this order,
                        //Also set it's pharmacy verification status flag to 1
                        //which signifies that a pharmacist needs to verify this.
                        admin.PatientOrder.PharmacyVerificationStatus = 1;
                    }
                    else
                    {
                        //There already is a notification.
                        //Update the timestamp.
                        //Get the pharmacy notification entity.
                        //Winston Murdock, 11/12/2021.  PC-26781
                        PharmacyNotification existingNotification = _context.PharmacyNotifications.Where(pn => pn.PharmacyNotificationAdministrations.Any(pna => pna.OrderAdministrationId == admin.Id)).FirstOrDefault();

                        //Now update the timestamp.
                        existingNotification.EnteredDatetime = siteNow;
                    } //end if (Is there already a notification for this order?)
                } //end if
            } //end if

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
            DateTimeOffset eventTime = GetEventDatetimeFromTemplateResponses(templateId, templateResponses, siteNow);

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
                        eventDateTime = DateTimeOffsetExtensions.TimeAdjustedForTimeZone(timeZoneName, offset);
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
                            ivStopDateTime = DateTimeOffsetExtensions.TimeAdjustedForTimeZone(timeZoneName, offset);
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
                    else if (templateName.Equals(OdsConstants.Intravenous) && promptText.ToLower().Equals(OdsConstants.IVNumber.ToLower()) &&
                             !string.IsNullOrEmpty(enteredText))
                    {
                        siteNumber = int.TryParse(enteredText.Substring(0, 1), out int siteInt) ? siteInt : (int?)null;
                    }
                    else if (templateName.Equals(OdsConstants.IntravenousInI) && promptText.ToLower().Equals(OdsConstants.IVNumber.ToLower()) &&
                             !string.IsNullOrEmpty(enteredText))
                    {
                        siteNumber = int.TryParse(enteredText.Substring(0, 1), out int siteInt) ? siteInt : (int?)null;
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

            var changeCount = 0;
            try
            {
                changeCount = _context.SaveChanges();
            }
            catch (Exception e)
            {
                Utilities.LogException(e);
                throw new Exception($"Error when saving '{action}' action to the database.", e);
            }

            if (changeCount > 0)
            {
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
            else
            {
                // get the patient id from the patient order id
                var query = from p in _context.PatientOrders
                            where p.Id == orderId
                            select p.PatientId;
                var patId = query.FirstOrDefault();

                var patIbex = _emarOutboundDataRepository.GetExternalPatientId(patId);
                var externalSiteId = (byte)_emarOutboundDataRepository.GetExternalSiteId(siteId);
                var externalUserId = _emarOutboundDataRepository.GetExternalUserId(userId);

                // failed to save any changes so log error
                var errMsg = "SaveChanges returned no changes - action not saved.";
                Utilities.LogError(patIbex, externalSiteId, orderId, adminId ?? 0, errMsg, action.ToString());

                throw new Exception(errMsg);
            }
        }

        private DateTimeOffset GetEventDatetimeFromTemplateResponses(int? templateId, Dictionary<string, string> templateResponses, DateTimeOffset defaultEventDatetime)
        {
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
                            defaultEventDatetime = potentialEventTime;
                        } //end if
                    } //end if
                } //end if
            } //end if

            return defaultEventDatetime;
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

        private void UpdateContinuousOrderTimes(OrderAdministration admin, ActionEnum action, int siteId, int? templateId, Dictionary<string, string> templateResponses)
        {
            if (admin == null)
            {
                return;
            }

            var frequencySchedule = _context.FrequencySchedules.Find(admin.PatientOrder.FrequencyScheduleId);
            var frequencyType = _context.FrequencyTypes.Find(frequencySchedule.FrequencyTypeId);

            // If this is not a continuous order, we don't have anything else to do.
            if (!frequencyType.Name.ToLowerInvariant().Equals("continuous"))
            {
                return;
            }

            var siteNow = _siteRepository.GetSiteTimeZone(siteId).NowWithTimeZoneOffset();
            var eventTime = GetEventDatetimeFromTemplateResponses(templateId, templateResponses, siteNow);

            // Adjust start and end times on give or reschedule
            if (action == ActionEnum.Give || 
                action == ActionEnum.Reschedule)
            {
                admin.PatientOrder.BeginDatetime = eventTime;
                if (admin.PatientOrder.Duration != null && admin.PatientOrder.DurationUnitId != null)
                {
                    var durationUnit = _context.DurationUnits.Find(admin.PatientOrder.DurationUnitId);
                    if (durationUnit != null)
                    {
                        admin.PatientOrder.EndDateTime =
                            admin.PatientOrder.BeginDatetime.AddMinutes((double)(admin.PatientOrder.Duration * durationUnit.DurationInMinutes));
                    }
                }
            }
            
            // Adjust end time on cancel or complete discontinue
            else if (action == ActionEnum.Cancel ||
                     action == ActionEnum.CompleteDiscontinue)
            {
                admin.PatientOrder.EndDateTime = eventTime;
            }
        }

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
        /// <param name="adminId">Order administration Id</param>
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
                                                                            || action == ActionEnum.Hold
                                                                            || action == ActionEnum.UnHold)))
                return false;

            // other actions w/o an adminId are handled afterwards (that's the intent, anyhow)
            if (adminId == null)
                return true;

            // get all order admin Ids in the order_events table
            var adminIds = (
                             from oe in _context.OrderEvents
                             where oe.PatientOrderId == orderId
                             select oe.OrderAdministrationId
                           ).ToList();

            var foundNonNull = false;
            // check each admin id in the list against the input adminId value
            foreach (var id in adminIds)
            {
                if (id != null)
                    foundNonNull = true;

                // if value is null or not equal to the adminId, then try next value in list
                if (id == null || id != adminId)
                {
                    continue;
                }
                else
                {
                    // match is found, return false 
                    return false;
                }
            }

            // did not find a match
            // if found a non null value then return true, else return false.
            return foundNonNull ? true : false;
        }

        public bool CheckActionIsInIni(ActionEnum action, string key)
        {
            //If the action is in the ini file setting for order, administration, or deletenotification (based on key),
            //return true so that the calling method generates a notification.
            //Else return false so that that the calling method does not generate a notification.
            //The .ini file is at inetpub/wwwroot/eMARAPI/pharmacy_notification/pharmacy_notifications.ini.
            
            //I changed the second param from a boolean to a string so that we could also pull the list
            //of actions that trigger deleting a notification.
            //Winston Murdock, 11/05/2021.  PC-26778.

            bool bRet = false;

            try
            {
                IniFile iniFile = new IniFile();
                string[] sValues;

                //See if this action was fired on an order or an administration.
                if (key == "order")
                {
                    //Order

                    //Get the order actions that fire off a notification into an array.
                    sValues = iniFile.Read("Order", "Actions").Split(",");
                }
                else if (key == "administration")
                {
                    //Administration

                    //Get the administration actions that fire off a notification into an array.
                    sValues = iniFile.Read("Administration", "Actions").Split(",");
                }
                else
                {
                    //Delete Notification

                    //Get the order actions that delete a notification into an array.
                    sValues = iniFile.Read("DeleteNotification", "Actions").Split(",");
                } //end if

                //See if the passed in action is in sValues.
                if (sValues.Any(x => x == action.ToString()))
                {
                    bRet = true;
                } //end if (is the action in the value from the ini file?)
            }
            catch (Exception e)
            {
                //We couldn't find the ini file or something like that.
                //Return false.
                bRet = false;
            }
            return bRet;
        } //end CheckActionIsInIni

        public byte CheckPatientIndicators(long patientId)
        {
            //If the existing check for disposition statue didn't tell us
            //that this patient is an "inpatient" patient, then we need
            //to check the patient's custom indicators.
            //If one, or more, of them is "EDHOLD", then we'll return 1.
            //Else, we'll return 0.
            //Winston Murdock, 01/06/2021.  PC-26905
            byte bRet = 0;

            //List of codes to check against.
            //TODO: Future enhancement: have this check the ini file or site options
            //table rather than hardcoding the value.
            List<string> sList = new List<string>();
            sList.Add("EDHOLD");
            sList.Add("ED_HOLD");

            var patientIndicators = _context.PatientIndicators.Where(x => x.PatientId == patientId);

            //Check the patient's custom indicators to see if at least
            //one is in the list of indicators we want to check.
            if (patientIndicators.Any(x => sList.Contains(x.Code)))
            {
                //Set the indicator to 1.
                bRet = 1;
            } //end if

            return bRet;
        } //end CheckPatientIndicators

        public void DeleteInteractionsForOrder(PatientOrder order)
        {
            //Get all of the interactions for this order.
            //order.OrderInteractions only has where DrugNum = 1 due to the changes we made
            //for the return to the UI to handle Tylenol interacting to Tylenol.
            //We need to go get all order interactions here.
            //Then we'll ue that list throughout here instead of the one we already got.
            //IF I don't call .ToList at the end of this line, then I'll keep the datareader open and hit
            //issues later on with attempting to open a datareader while another one is already open.
            //https://stackoverflow.com/a/10498776
            //Winston Murdock, 05/05/2022.  PC-27153
            //var orderInteractions = _context.OrderInteractions.Where(oi => oi.PatientOrderId == order.Id).ToList();
            var orderInteractions = _context.OrderInteractions
                .Include(oi => oi.MedicationInteraction)
                .Where(oi => oi.PatientOrderId == order.Id)
                .ToList();

            if (orderInteractions != null)
            {
                var medicationInteractions = new Collection<MedicationInteraction>();

                try
                {
                    //Since we're deleting this cart order,
                    //delete both sides of the interactions for it
                    //We've already got this cart order's side of the interaction, but we need to go
                    //get the other side for each interaction.
                    //If it's an interaction to an actual order, then there will not be a second
                    //side to this interaction (since we won't calculate that until this order
                    //is checked out from the cart).

                    //foreach (var orderInteraction in cartOrder.OrderInteractions)
                    foreach (var orderInteraction in orderInteractions)
                    {
                        if (orderInteraction.MedicationInteraction != null)
                        {
                            medicationInteractions.Add(orderInteraction.MedicationInteraction);
                        } //end if
                    } //end foreach

                    //foreach (var interaction in cartOrder.OrderInteractions)
                    foreach (var interaction in orderInteractions)
                    {
                        //Also get the other side of the order interaction.
                        //Jim says that each MedicationInteraction is for one pair of rows in OrderInteractions
                        //(order to order, cart order to cart order, cart order to order, etc...).
                        //So I can just grab the second row where the MedicationInteractionId matches and then delete it.
                        //That will prevent us from hitting a foreign key issue below when deleting the medication interaction.
                        var otherSideOrderInteractions =
                        (
                                from oi in _context.OrderInteractions
                                where oi.MedicationInteractionId == interaction.MedicationInteractionId
                                && oi.Id != interaction.Id
                                select oi
                        ).ToList();

                        //This should be either one entry or zero entries.
                        //If we have anything in there, then delete it.
                        //If we don't have any, then we don't have any to delete.
                        if (otherSideOrderInteractions.Any())
                        {
                            foreach (var osoi in otherSideOrderInteractions)
                            {
                                _context.OrderInteractions.Remove(osoi);
                            } //end foreach
                        } //end if

                        //Now that we've deleted the other side of the order interaction,
                        //delete this side of it.
                        _context.OrderInteractions.Remove(interaction);
                    } //end foreach order interaction

                    //Don't save things back to the DB context yet.
                    //CreateOrderEvent will do that for us.
                    //Save things back to the DB context.
                    //_context.SaveChanges(true);
                }
                catch (Exception ex)
                {
                    //Log any error.
                    //Not sure what we could do here.
                    //We can't delete an order if we run into an issue
                    //deleting its order interactions.
                    //But at least we'll log this to the event viewer.
                    Utilities.LogException(ex);
                } //end try/catch.

                //If we added any MedicationInteractions to the list, delete them.
                //Since we have already deleted both of the order interaction children of this
                //medication interaction, we won't run into an error trying to delete this.
                if (medicationInteractions.Count > 0)
                {
                    try
                    {
                        foreach (var interaction in medicationInteractions)
                        {
                            if (interaction != null)
                            {
                                _context.MedicationInteractions.Remove(interaction);
                            }
                        } //end foreach medication interaction

                        //Don't save things back to the DB context yet.
                        //CreateOrderEvent will do that for us.
                        //Save things back to the DB context.
                        //_context.SaveChanges(true);
                    }
                    catch (Exception ex)
                    {
                        //Log any error.
                        //Not sure what we could do here.
                        //We can't delete an order if we run into an issue
                        //deleting its order interactions.
                        //But at least we'll log this to the evnet viewer.
                        Utilities.LogException(ex);
                    } //end try/catch.
                } //end if (do we have any medication interactions for this order interaction?)
            } //end if
        } //end DeleteInteractionsForOrder

        public void DeleteAdministrationsAndChildren(List<OrderAdministration> adminsToDelete)
        {
            //Before we delete the future administrations, we need to delete their children.
            //It's possible that we could already have a notification or order event on
            //a future administration.
            //Winston Murdock, 05/26/2022.  PC-27264

            //Save the ID of each future administration for this order into a list.
            List<long> adminIdsToDelete = new List<long>();
            foreach (var adminToGetId in adminsToDelete)
            {
                adminIdsToDelete.Add(adminToGetId.Id);
            } //end foreach

            //Make one DB hit to get the list of order administrations we are deleting
            //and all of their children (notifications, order events, and order event details).
            //By getting the Id for each adminsitration we're going to delete into a list
            //we can call .Contains on that list.
            //The alternative is making a DB hit for each administration, which likely takes longer.
            var adminstoDeleteWithChildren = _context.OrderAdministrations
                    .Include(oa => oa.OrderAdministrationNotifications)
                    .Include(oa => oa.OrderEvents)
                        .ThenInclude(oe => oe.OrderEventDetails)
                    .Include(oa => oa.PharmacyNotificationAdministrations)
                        .ThenInclude(pna => pna.PharmacyNotification)
                    .Where(oa => adminIdsToDelete.Contains(oa.Id))
                    .ToList();

            //For each future administration, delete the children.
            //Then delete the administration itself.
            foreach (var administration in adminstoDeleteWithChildren)
            {
                //Loop through each administration and delete their notification.
                foreach (PharmacyNotificationAdministration pna in administration.PharmacyNotificationAdministrations)
                {
                    //Since pharmacy notification administration is a child of
                    //pharmacy notification, we must delete it first.
                    //Then we delete the pharmacy notification.
                    _context.PharmacyNotificationAdministrations.Remove(pna);

                    _context.PharmacyNotifications.Remove(pna.PharmacyNotification);

                } //end foreach pharmacy notification administration

                //For each order event, delete the order event details.
                foreach (var orderEvent in administration.OrderEvents)
                {
                    _context.OrderEventDetails.RemoveRange(orderEvent.OrderEventDetails);
                } //end foreach orderEvent

                //Delete the order events.
                _context.OrderEvents.RemoveRange(administration.OrderEvents);

                //Delete the notifications for this administration.
                _context.Notifications.RemoveRange(administration.OrderAdministrationNotifications);

                //The line below will delete all adminstrations rather than
                //my deleting them one by one.
                //Yes, it will likely result in one delete from query for each one.
                //But it's fewer executions in C# and should be a little faster.
                //Delete this administration.
                //_context.OrderAdministrations.Remove(administration);
            } //end foreach (administration we want to delete).

            //Delete all administrations.
            _context.OrderAdministrations.RemoveRange(adminstoDeleteWithChildren);
        } //end DeleteAdministrationsAndChildren
        #endregion
    }
}