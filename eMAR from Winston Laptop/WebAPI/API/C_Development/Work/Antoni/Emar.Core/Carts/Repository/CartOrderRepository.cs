using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using Emar.Core.Carts.Model;
using Emar.Core.Helpers;
using Emar.Core.Orders.Model.Mappings;
using Emar.Core.ResourceParameters;
using Emar.Data;
using Emar.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Caching.Memory;

namespace Emar.Core.Carts.Repository
{
    public class CartOrderRepository : ICartOrderRepository
    {
        private readonly EmarContext _context;
        private readonly IPropertyMappingService _propertyMappingService;
        private readonly MemoryCache _cache;

        public CartOrderRepository(EmarContext emarContext, IPropertyMappingService propertyMappingService, EmarMemoryCache cache)
        {
            _context = emarContext ?? throw new ArgumentNullException(nameof(emarContext));
            _propertyMappingService = propertyMappingService ?? throw new ArgumentNullException(nameof(propertyMappingService));
            _cache = cache.Cache;
        }

        public PagedList<PatientCartOrder> GetOrders(BaseLinkResource resource)
        {
            var orders = GetCartOrders(order =>
                order.UserId == resource.UserId &&
                order.PatientId == resource.PatientId);

            if (resource.OrderBy != null)
            {
                //get property mapping dictionary
                var propertyMappingDictionary = _propertyMappingService.GetPropertyMapping<CartOrderDto, PatientCartOrder>();

                orders = orders.AsQueryable().ApplySort(resource.OrderBy, propertyMappingDictionary);
            }

            return PagedList<PatientCartOrder>.Create(orders.AsQueryable(), resource.PageNumber, resource.PageSize);
        }

        private IEnumerable<PatientCartOrder> GetCartOrders(Expression<Func<PatientCartOrder, bool>> wherePredicate)
        {
            return _context.PatientCartOrders
                .Include(order => order.CartOrderAdministrations)
                .Include(order => order.Medication)
                    .ThenInclude(med => med.MedicationDetails)
                        .ThenInclude(md => md.MedicationUnit)
                .Include(order => order.MedicationRoute)
                .Include(order => order.FrequencySchedule)
                    .ThenInclude(f => f.FrequencyType)
                .Include(order => order.MedicationUnit)
                .Include(order => order.User)
                .Include(order => order.OrderInteractions)
                    .ThenInclude(interaction => interaction.DrugInteractionView)
                .Include(order => order.AllergyReactionsView)
                .Where(wherePredicate)
                .AsEnumerable();
        }

        public IEnumerable<PatientCartOrder> GetPatientCartOrders(Expression<Func<PatientCartOrder, bool>> wherePredicate = null, bool forOverrideReasons = false)
        {
            if (wherePredicate == null)
            {
                return _context.PatientCartOrders
                    .Include(i => i.Medication)
                        .ThenInclude(m => m.MedicationDetails)
                            .ThenInclude(d => d.FdbBrandName)
                    .Include(i => i.Medication)
                        .ThenInclude(m => m.MedicationDetails)
                            .ThenInclude(md => md.MedicationUnit)
                    .ToList();
            }

            if (forOverrideReasons)
            {
                return _context.PatientCartOrders
                    .Include(order => order.OrderInteractions)
                        .ThenInclude(interaction => interaction.MedicationInteraction)
                    .Include(order => order.OrderInteractions)
                        .ThenInclude(interaction => interaction.DrugInteractionView)
                    .Include(order => order.OrderReactions)
                    .Include(order => order.AllergyReactionsView)
                    .Where(wherePredicate)
                    .ToList();
            }

            return _context.PatientCartOrders
                .Include(i => i.Medication)
                    .ThenInclude(m => m.MedicationDetails)
                        .ThenInclude(d => d.FdbBrandName)
                .Include(i => i.Medication)
                    .ThenInclude(m => m.MedicationDetails)
                        .ThenInclude(md => md.MedicationUnit)
                .Where(wherePredicate)
                .ToList();
        }

        public PatientCartOrder GetOrder(long orderId)
        {
            return GetCartOrders(order => order.Id == orderId)
                    .FirstOrDefault();
        }

        public PatientCartOrder AddCartOrder(PatientCartOrder cartOrder)
        {
            try
            {
                _context.PatientCartOrders.Add(cartOrder);
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                throw;
            }

            return GetOrder(cartOrder.Id);
        }

        public bool UpdateCartOrder(PatientCartOrder cartOrder)
        {
            int i;

            try
            {
                var order = _context.PatientCartOrders.First(g => g.Id == cartOrder.Id);
                _context.Entry(order).CurrentValues.SetValues(cartOrder);

                foreach (var administration in order.CartOrderAdministrations)
                {
                    _context.CartOrderAdministrations.Remove(administration);
                }

                order.CartOrderAdministrations = cartOrder.CartOrderAdministrations;

                i = _context.SaveChanges();
            }
            catch (Exception ex)
            {
                i = 0;
            }

            return i > 0;
        }

        public bool DeleteCartOrder(long cartOrderId)
        {
            var cartOrder = _context.PatientCartOrders
                .Include(order => order.CartOrderAdministrations)
                .Include(order => order.OrderInteractions)
                    .ThenInclude(interaction => interaction.MedicationInteraction)
                .Include(order => order.OrderReactions)
                .FirstOrDefault(order => order.Id == cartOrderId);

            return DeleteCartOrder(cartOrder);
        }

        public bool DeleteCartOrders(int userId, long patientId)
        {
            var cartOrders = _context.PatientCartOrders
                .Where(order => order.UserId == userId)
                .Where(order => order.PatientId == patientId)
                .Include(order => order.CartOrderAdministrations)
                .Include(order => order.OrderInteractions)
                    .ThenInclude(interaction => interaction.MedicationInteraction)
                .Include(order => order.OrderReactions)
                .ToList();

            bool success = true;

            foreach (var cartOrder in cartOrders)
            {
                if (!DeleteCartOrder(cartOrder))
                {
                    success = false;
                }
            }

            return success;
        }

        private bool DeleteCartOrder(PatientCartOrder cartOrder)
        {
            bool success = true;
            int i = 0;

            if (cartOrder != null)
            {
                if (cartOrder.OrderInteractions != null)
                {
                    var medicationInteractions = new Collection<MedicationInteraction>();

                    foreach (var orderInteraction in cartOrder.OrderInteractions)
                    {
                        if (orderInteraction.MedicationInteraction != null)
                        {
                            medicationInteractions.Add(orderInteraction.MedicationInteraction);
                        }
                    }

                    if (cartOrder.OrderInteractions != null)
                    {
                        try
                        {
                            foreach (var interaction in cartOrder.OrderInteractions)
                            {
                                var orderInteractions = _context.OrderInteractions
                                    .Where(x => x.MedicationInteractionId == interaction.MedicationInteractionId);

                                _context.OrderInteractions.RemoveRange(orderInteractions);
                            }

                            _context.SaveChanges(true);
                        }
                        catch (Exception ex)
                        {
                            success = false;
                        }
                    }

                    if (success && medicationInteractions.Count > 0)
                    {
                        try
                        {
                            foreach (var interaction in medicationInteractions)
                            {
                                if (interaction != null)
                                {
                                    _context.MedicationInteractions.Remove(interaction);
                                }
                            }

                            _context.SaveChanges(true);
                        }
                        catch (Exception ex)
                        {
                            success = false;
                        }
                    }
                }

                try
                {
                    if (cartOrder.CartOrderAdministrations != null)
                    {
                        _context.CartOrderAdministrations.RemoveRange(cartOrder.CartOrderAdministrations);
                    }

                    if (cartOrder.OrderReactions != null)
                    {
                        _context.OrderReactions.RemoveRange(cartOrder.OrderReactions);
                    }

                    _context.PatientCartOrders.Remove(cartOrder);
                    i = _context.SaveChanges(true);

                }
                catch (Exception ex)
                {
                    i = 0;
                }
            }

            return success && i > 0;
        }

        public IEnumerable<OverrideReason> GetOverrideReasons(int siteId)
        {
            return _context.OverrideReasons
                .Where(reason => reason.SiteId == siteId)
                .AsEnumerable();
        }

        public bool CheckoutOrders(CartPreCheckoutResponseDataDto cartPreCheckoutResponseData, int userId, long patientId)
        {
            int i;

            var cartOrders = _context.PatientCartOrders
                .Where(order => order.UserId == userId)
                .Where(order => order.PatientId == patientId)
                .Include(order => order.Patient)
                    .ThenInclude(patient => patient.Site)
                .Include(order => order.CartOrderAdministrations)
                .Include(order => order.OrderInteractions)
                    .ThenInclude(interaction => interaction.MedicationInteraction)
                .Include(order => order.OrderReactions)
                .ToList();

            if (!cartOrders.Any())
            {
                return false;
            }

            try
            {
                var secs = 0;
                var addDatetime = cartOrders.First().Patient.Site.TimeZoneName.NowWithTimeZoneOffset();

                foreach (var cartOrder in cartOrders)
                {
                    if (cartOrder.UserQuickListItemId != null)
                    {
                        _context.UserQuickListItems.Find(cartOrder.UserQuickListItemId).UsagesThisWeek += 1;
                    }

                    _context.PatientOrders.Add(OrderMapper.MapCartOrderToOrder(
                        cartOrder,
                        addDatetime.AddSeconds(secs),
                        int.TryParse(cartPreCheckoutResponseData.OrderingPhysicianUserId, out int number)
                            ? number
                            : (int?)null));

                    _context.CartOrderAdministrations.RemoveRange(cartOrder.CartOrderAdministrations);
                    _context.PatientCartOrders.Remove(cartOrder);

                    secs++;
                }

                foreach (var rationalia in cartPreCheckoutResponseData.DrugInteractionOverrideRationalia)
                {
                    if (!string.IsNullOrEmpty(rationalia.MedicationInteractionId) &&
                        !string.IsNullOrEmpty(rationalia.OverrideReasonId))
                    {
                        if ((int.TryParse(rationalia.MedicationInteractionId,
                                out int medicationInteractionId)) &&
                            (int.TryParse(rationalia.OverrideReasonId, out int overrideReasonId)))
                        {
                            var interaction = _context.MedicationInteractions
                                .First(m => m.Id == medicationInteractionId);

                            interaction.OverrideReasonId = overrideReasonId;
                        }
                    }
                }

                foreach (var rationalia in cartPreCheckoutResponseData.AllergyReactionOverrideRationalia)
                {
                    if (!string.IsNullOrEmpty(rationalia.OrderReactionId) &&
                        !string.IsNullOrEmpty(rationalia.OverrideReasonId))
                    {
                        if ((int.TryParse(rationalia.OrderReactionId, out int orderReactionId)) &&
                            (int.TryParse(rationalia.OverrideReasonId, out int overrideReasonId)))
                        {
                            var reaction = _context.OrderReactions
                                .First(m => m.Id == orderReactionId);

                            reaction.OverrideReasonId = overrideReasonId;
                        }
                    }
                }

                i = _context.SaveChanges(true);
            }
            catch (Exception ex)
            {
                i = 0;
            }

            return i > 0;
        }

        public IEnumerable<CartOrderAdministration> GetAdministrations(long orderId)
        {
            return _context.CartOrderAdministrations
                    .Where(administration => administration.PatientCartOrderId == orderId)
                    .AsEnumerable();
        }

        public CartOrderAdministration GetAdministration(long administrationId)
        {
            return _context.CartOrderAdministrations.Find(administrationId);
        }

        public FrequencySchedule GetFrequency(int frequencyId)
        {
            return _cache.GetOrCreate(frequencyId + CacheKeys.OrderInstructions, entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30);

                var ret = _context.FrequencySchedules
                    .Include(f => f.FrequencyType)
                    .FirstOrDefault(f => f.Id == frequencyId);

                entry.Size = 1;

                return ret;
            });
        }
    }
}