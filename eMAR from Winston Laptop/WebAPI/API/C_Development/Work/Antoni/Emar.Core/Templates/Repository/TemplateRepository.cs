using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Dynamic.Core;
using Emar.Core.Helpers;
using Emar.Core.Orders.Model;
using Emar.Core.Sites.Repository;
using Emar.Core.Templates.Model;
using Emar.Data;
using Emar.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace Emar.Core.Templates.Repository
{
    public class TemplateRepository : ITemplateRepository
    {
        private readonly EmarContext _context;
        private readonly ISiteRepository _siteRepository;
        private readonly MemoryCache _cache;

        public TemplateRepository(EmarContext context, EmarMemoryCache cache, ISiteRepository siteRepository)
        {
            _context = context;
            _siteRepository = siteRepository;
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
                .FirstOrDefault(oa => oa.Id == orderId);

            if (order == null)
                throw new ArgumentException($"No order with id '{orderId}'.", nameof(orderId));

            switch (action)
            {
                //case ActionEnum.Acknowledge:
                //    break;
                case ActionEnum.CoSign:
                    // CoSign has no other actions than writing to the Events
                    break;
                case ActionEnum.Cancel:
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
                    }

                    break;
                //case ActionEnum.CompleteDiscontinue:
                //    break;
                //case ActionEnum.Give:
                //    break;
                //case ActionEnum.Hold:
                //    break;
                //case ActionEnum.MissedDose:
                //    break;
                //case ActionEnum.OrderDiscontinue:
                //    break;
                //case ActionEnum.Repeat:
                //    break;
                //case ActionEnum.Reschedule:
                //    break;
                //case ActionEnum.UnHold:
                //    break;
                //case ActionEnum.FollowUp:
                //    break;
                //case ActionEnum.Complete:
                //    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(action), action, $"From '{nameof(FileOrderEvent)}'.");
            }

            var orderEventId = CreateOrderEvent(userId, siteId, action, templateId, templateResponses, order.Id);

            return orderEventId;
        }

        public long FileAdminEvent(in int userId, long adminId, ActionEnum action, int siteId, int? templateId = null,
            Dictionary<string, string> templateResponses = null)
        {
            var admin = _context.OrderAdministrations
                .Include(oa => oa.PatientOrder)
                    .ThenInclude(o => o.OrderAdministrations)
                .FirstOrDefault(oa => oa.Id == adminId);

            if (admin == null)
                throw new ArgumentException($"No administration with id '{adminId}'.", nameof(adminId));

            var siteNow = _siteRepository.GetSiteTimeZone(siteId).NowWithTimeZoneOffset();

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
                //case ActionEnum.Cancel:
                //    break;
                //case ActionEnum.CompleteDiscontinue:
                //    break;
                //case ActionEnum.Delete:
                //    break;
                case ActionEnum.Give:
                    if (ValidateAdminGive(admin))
                    {
                        if (!admin.PointInTime)
                        {
                            if (admin.PatientOrder.OrderStatus == OrderStatus.Pending.ToString())
                            {
                                admin.AdministeringUserId = userId;
                                admin.AdministrationDatetime = siteNow;
                                admin.AdministrationSystemDatetime = siteNow;
                                admin.PatientOrder.OrderStatus = OrderStatus.OnGoing.ToString();
                            }
                        }
                        else if (admin.PointInTime)
                        {
                            admin.AdministeringUserId = userId;
                            admin.AdministrationDatetime = siteNow;
                            admin.AdministrationSystemDatetime = siteNow;

                            // all administrations are "accounted for", order "Completed" else order "OnGoing"
                            admin.PatientOrder.OrderStatus = admin.PatientOrder.OrderAdministrations.All(a =>
                                a.MissedDose
                                || a.AdministrationDatetime != null)
                                ? OrderStatus.Completed.ToString()
                                : OrderStatus.OnGoing.ToString();
                        }

                        if (admin.OnHold)
                        {
                            admin.OnHold = false;

                            foreach (var orderAdministration in admin.PatientOrder.OrderAdministrations
                                .Where(a =>
                                    a.AdministrationDatetime > admin.AdministrationDatetime
                                    && a.OnHold))
                            {
                                orderAdministration.OnHold = false;
                            }

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
                    }

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

                    break;
                //case ActionEnum.OrderDiscontinue:
                //    break;
                //case ActionEnum.Repeat:
                //    break;
                //case ActionEnum.Reschedule:
                //    break;
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
                    // FollowUp has no other actions than writing to the Events
                    break;
                //case ActionEnum.Complete:
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
            //int? eventTimePromptId = null;
            //if (templateId.HasValue)
            //{
            //    // Go to the template definition and get the PromptId of the Prompt that has the actual event time
            //}  

            // Create the new Event
            var orderEvent = new OrderEvent
            {
                PatientOrderId = orderId,
                OrderAdministrationId = adminId,
                //EventDateTime = siteNow,  --  Filled out below, possibly with a user response
                AddUserId = userId,
                AddDatetime = siteNow,
                ActionId = (int)action,
                TemplateId = templateId
            };

            var eventDateTime = siteNow;
            if (templateResponses != null)
            {
                foreach (var response in templateResponses)
                {
                    if (!int.TryParse(response.Key, out int promptId))
                        throw new ArgumentException(
                            $"Found Body Key in JSON body ({response.Key}) that isn't an integer.",
                            nameof(templateResponses));

                    orderEvent.OrderEventDetails.Add(new OrderEventDetail
                    {
                        PromptText = GetPromptText(promptId),
                        EnteredText = GetResponseValue(response.Value, promptId),
                        PromptId = promptId
                    });

                    //if (eventTimePromptId.HasValue && promptId == eventTimePromptId)
                    //{
                    //    // Set eventDateTime to the value returned for the prompt, if any
                    //    try
                    //    {

                    //    }
                    //    catch (Exception ex)
                    //    {
                    //        throw new Exception(
                    //            $"Unable to parse the value {response.Value}, the response to {GetPromptText(promptId)}, into a DateTimeOffset", ex);
                    //    }
                    //    // Else make sure the eventDateTime is left as siteNow
                    //}

                    //if(prompt.PromptType == "Notify")
                }
            }

            // Assign the EventDateTime with either the value entered by the user (if harvested above) or siteNow
            orderEvent.EventDateTime = eventDateTime;

            _context.Add(orderEvent);

            try
            {
                _context.SaveChanges();
            }
            catch (Exception e)
            {
                throw new Exception($"Error when saving '{action}' action to the database.", e);
            }

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

        private Dictionary<int,string> GetPromptDict()
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

        private static bool ValidateOrderCancelOrDelete(PatientOrder order, ActionEnum action)
        {
            // Status must be pending
            if (!order.OrderStatus.Equals(OrderStatus.Pending.ToString(), StringComparison.InvariantCultureIgnoreCase))
            {
                throw new ArgumentException(
                    $"Unable to perform '{action}' action.  Order is not in 'Pending' status.  (order {order.Id})",
                    nameof(order.Id));
            }

            if (order.OrderAdministrations.Any(a =>
                a.MissedDose))
            {
                throw new ArgumentException(
                    $"Unable to perform '{action}' action.  One or more administrations have 'Missed Dose' status.  (order {order.Id})");
            }

            if (order.OrderAdministrations.Any(a =>
                a.OnHold))
            {
                throw new ArgumentException(
                    $"Unable to perform '{action}' action.  One or more administrations have 'On Hold' status.  (order {order.Id})");
            }

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
            var id = _context.Templates.FirstOrDefault( t => t.Name == templateName)?.Id;
            if(!id.HasValue)
                throw new ArgumentException($"Template Name \"{templateName}\" doesn't exist in the database.",
                    nameof(templateName));
            return id.Value;
        }

        #endregion
    }
}