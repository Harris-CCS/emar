using System;
using System.Collections.Generic;
using System.Linq;
using Emar.Core.Helpers;
using Emar.Core.Orders.Model;
using Emar.Core.ResourceParameters;
using Emar.Data;
using Emar.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Emar.Core.Orders.Repository
{
    public class OrderRepository : IOrderRepository
    {
        private readonly EmarContext _context;
        private readonly IPropertyMappingService _propertyMappingService;

        public OrderRepository()
        {

        }

        public OrderRepository(EmarContext emarContext, IPropertyMappingService propertyMappingService)
        {
            _context = emarContext ?? throw new ArgumentNullException(nameof(emarContext));
            _propertyMappingService = propertyMappingService ?? throw new ArgumentNullException(nameof(propertyMappingService));
        }

        public PagedList<PatientOrder> GetOrders(long? patientId, OrdersResourceParameters resourceParameters)
        {
            patientId ??= resourceParameters.PatientId;

            var orders = GetOrders();

            if ((patientId != null) &&
                (patientId != -1))
            {
                orders = orders
                    .Where(order => order.PatientId == patientId);
            }

            if (resourceParameters.OrderBy != null)
            {
                //get property mapping dictionary
                var propertyMappingDictionary = _propertyMappingService.GetPropertyMapping<PatientOrderDto, PatientOrder>();

                orders = orders.AsQueryable().ApplySort(resourceParameters.OrderBy, propertyMappingDictionary);
            }

            return PagedList<PatientOrder>.Create(orders.AsQueryable(), resourceParameters.PageNumber, resourceParameters.PageSize);
        }

        IEnumerable<PatientOrder> GetOrders()
        {
            return _context.PatientOrders
                    .Include(order => order.OrderAdministrations)
                    .Include(order => order.MedicationRoute)
                    .Include(order => order.MedicationUnit)
                    .Include(order => order.AddUser)
                    .Include(order => order.OrderPhysicianUser)
                    .Include(order => order.Patient)
                        .ThenInclude(patient => patient.Site)
                            .ThenInclude(site => site.SiteOptions)
                                .ThenInclude(siteOptions => siteOptions.Option)
                    .AsEnumerable();
        }

        public PatientOrder GetOrder(long orderId, OrdersResourceParameters resourceParameters)
        {
            return GetOrders()
                    .FirstOrDefault(order => order.Id == orderId);
        }

        public IEnumerable<OrderAdministration> GetAdministrations(long orderId)
        {
            return _context.OrderAdministrations
                    .Where(administration => administration.PatientOrderId == orderId)
                    .AsEnumerable();
        }

        public OrderAdministration GetAdministration(long administrationId)
        {
            return _context.OrderAdministrations
                    .FirstOrDefault(administration => administration.Id == administrationId);
        }

        public IEnumerable<OrderEvent> GetEvents(long orderId)
        {
            return _context.OrderEvents
                    .Where(@event => @event.PatientOrderId == orderId)
                    .AsEnumerable();
        }

        public OrderEvent GetEvent(long eventId)
        {
            return _context.OrderEvents.Find(eventId);
        }

        public IEnumerable<OrderEvent> GetAdministrationEvents(long administrationId)
        {
            return _context.OrderEvents
                    .Where(@event => @event.OrderAdministrationId == administrationId)
                    .AsEnumerable();
        }


        #region UserQuickList Section

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="siteId"></param>
        /// <returns></returns>
        public IEnumerable<UserQuickListItem> GetUserQuickListMostUsed(int userId, int? siteId)
        {
            if (siteId == null)
                return _context.UserQuickListItems
                    .Where(i => i.UserId == userId && i.WeeklyUsageRollingAverage > -1)
                    .Include(i => i.MedicationRoute)
                    .OrderByDescending(i => i.WeeklyUsageRollingAverage)
                    .Take(80)
                    .ToList();

            return _context.UserQuickListItems
                .Where(i => i.UserId == userId && i.SiteId == siteId)
                .Include(i => i.MedicationRoute)
                .OrderByDescending(i => i.WeeklyUsageRollingAverage)
                .Take(80)
                .ToList();
        }

        public List<string> GetUserQuickListTabs(int userId, int? siteId)
        {
            if (siteId == null)
                return _context.UserQuickListItems
                    .Where(i => i.UserId == userId)
                    .GroupBy(i => i.BrandName.Substring(0, 1).ToUpper())
                    .Select(i => i.Key)
                    .ToList();

            return _context.UserQuickListItems
                .Where(i => i.UserId == userId && i.SiteId == siteId)
                .GroupBy(i => i.BrandName.Substring(0, 1).ToUpper())
                .Select(i => i.Key)
                .ToList();
        }

        IEnumerable<UserQuickListItem> IOrderRepository.GetUserQuickListTabItems(int userId, int? siteId, string tab)
        {
            if (tab == "#")
            {
                if (siteId == null)
                    return _context.UserQuickListItems
                        .Where(i => i.UserId == userId)
                        .Include(i => i.MedicationRoute)
                        .ToList()
                        .Where(i => !char.IsLetter(i.BrandName.Substring(0, 1).ToCharArray()[0]));

                return _context.UserQuickListItems
                    .Where(i => i.UserId == userId
                                && i.SiteId == siteId)
                    .Include(i => i.MedicationRoute)
                    .ToList()
                    .Where(i => !char.IsLetter(i.BrandName.Substring(0, 1).ToCharArray()[0]));
            }

            if (siteId == null)
                return _context.UserQuickListItems
                    .Where(i => i.UserId == userId
                                && i.BrandName.Substring(0, 1).ToUpper() == tab)
                    .Include(i => i.MedicationRoute)
                    .ToList();

            return _context.UserQuickListItems
                .Where(i => i.UserId == userId
                            && i.SiteId == siteId
                            && i.BrandName.Substring(0, 1).ToUpper() == tab)
                .Include(i => i.MedicationRoute)
                .ToList();
        }
        #endregion
    }
}
