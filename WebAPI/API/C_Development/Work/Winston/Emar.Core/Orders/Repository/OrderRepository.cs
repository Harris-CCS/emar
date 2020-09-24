using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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
            var orders = GetOrders(order => order.PatientId == ((patientId ?? resourceParameters.PatientId) ?? -1));

            if (resourceParameters.OrderBy != null)
            {
                //get property mapping dictionary
                var propertyMappingDictionary = _propertyMappingService.GetPropertyMapping<PatientOrderDto, PatientOrder>();

                orders = orders.AsQueryable().ApplySort(resourceParameters.OrderBy, propertyMappingDictionary);
            }

            return PagedList<PatientOrder>.Create(orders.AsQueryable(), resourceParameters.PageNumber, resourceParameters.PageSize);
        }

        public PatientOrder GetOrder(long orderId, OrdersResourceParameters resourceParameters)
        {
            return GetOrders(order => order.Id == orderId)
                    .FirstOrDefault();
        }

        IEnumerable<PatientOrder> GetOrders(Expression<Func<PatientOrder, bool>> wherePredicate)
        {
            return _context.PatientOrders
                    .Include(order => order.OrderAdministrations)
                    .Include(order => order.MedicationRoute)
                    .Include(order => order.MedicationUnit)
                    .Include(order => order.AddUser)
                    .Include(order => order.OrderPhysicianUser)
                    .Include(order => order.FrequencySchedule)
                    .Include(order => order.Patient)
                    //.Include(order => order.Patient)
                    //    .ThenInclude(patient => patient.Site)
                    //        .ThenInclude(site => site.SiteOptions)
                    //            .ThenInclude(siteOptions => siteOptions.Option)
                    .Where(wherePredicate)
                    .AsEnumerable();
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
            Expression<Func<UserQuickListItem, bool>> whereExpression;
            if (siteId == null)
                whereExpression = i => i.UserId == userId && i.WeeklyUsageRollingAverage > -1;
            else
                whereExpression = i => i.UserId == userId && i.SiteId == siteId && i.WeeklyUsageRollingAverage > -1;

            return _context.UserQuickListItems
                    .Where(whereExpression)
                    .Include(i => i.MedicationRoute)
                    .Include(i => i.MedicationUnit)
                    .Include(i => i.FrequencySchedule)
                    .OrderByDescending(i => i.WeeklyUsageRollingAverage)
                    .Take(80)
                    .ToList();
        }

        public List<string> GetUserQuickListTabs(int userId, int? siteId)
        {
            Expression<Func<UserQuickListItem, bool>> whereExpression;
            if (siteId == null)
                whereExpression = i => i.UserId == userId;
            else
                whereExpression = i => i.UserId == userId && i.SiteId == siteId;
             
            //TODO
            //return _context.UserQuickListItems
            //    .Where(whereExpression)
            //    .GroupBy(i => i.BrandName.Substring(0, 1).ToUpper())
            //    .Select(i => i.Key)
            //    .ToList();
            return null;
        }

        IEnumerable<UserQuickListItem> IOrderRepository.GetUserQuickListTabItems(int userId, int? siteId, string tab)
        {
            //todo
            return null;

            Expression<Func<UserQuickListItem, bool>> whereExpression;

            if (tab == "#")
            {
                if (siteId == null)
                    //whereExpression = i => i.UserId == userId && !EF.Functions.Like(i.Medication.MedicationDetails, "[a-z]%");
                    throw new System.NotImplementedException();
                else
                    whereExpression = i => i.UserId == userId && i.SiteId == siteId;

                return _context.UserQuickListItems
                    .Where(whereExpression)
                    .Include(i => i.MedicationRoute)
                    .Include(i => i.MedicationUnit)
                    .Include(i => i.FrequencySchedule)
                    .ToList();


                //if (siteId == null)
                //    whereExpression = i => i.UserId == userId;
                //else
                //    whereExpression = i => i.UserId == userId && i.SiteId == siteId;

                //return _context.UserQuickListItems
                //    .Where(whereExpression)
                //    .Include(i => i.MedicationRoute)
                //    .Include(i => i.MedicationUnit)
                //    .Include(i => i.FrequencySchedule)
                //    .ToList()
                //    .Where(i => !char.IsLetter(i.Medication.DisplayName.Substring(0, 1).ToCharArray()[0]));
            }

            //if (siteId == null)
                //whereExpression = i => i.UserId == userId
                                       //todo
                                       //&& i.BrandName.Substring(0, 1).ToUpper() == tab;
            //else
            //    whereExpression = i => i.UserId == userId && i.SiteId == siteId
                                                          //TODO
                                                          //&& i.BrandName.Substring(0, 1).ToUpper() == tab;

            //TODO
            //return _context.UserQuickListItems
            //    .Where(whereExpression)
            //    .Include(i => i.MedicationRoute)
            //    .Include(i => i.MedicationUnit)
            //    .Include(i => i.FrequencySchedule)
            //    //.Include(i => i.Medication)
            //    //.ThenInclude(i => i.MedicationDetails)
            //    .ToList();
        }

        #endregion

        #region Department Preferred List Section

        public List<DepartmentPreferredListItem> GetDepartmentPreferredList(int siteId, string departmentCode, string linkBase)
        {
            Expression<Func<DepartmentPreferredListItem, bool>> whereLambda = s => s.SiteId == siteId;
            if (!string.IsNullOrWhiteSpace(departmentCode))
                whereLambda = s => s.SiteId == siteId && s.DepartmentCode == departmentCode;

            return _context.DepartmentPreferredListItems.Where(whereLambda)
                    .Include(g => g.MedicationUnit)
                    .Include(g => g.MedicationRoute)
                    .Include(g => g.FrequencySchedule).ToList();
        }

        #endregion

        #region Groups Remembered Orders Section

        public List<GroupListItem> GetGroupRememberedOrderItems(int siteId, string departmentCode, string linkBase)
        {
            Expression<Func<GroupListItem, bool>> whereLambda;
            if (string.IsNullOrWhiteSpace(departmentCode))
                whereLambda = s => s.SiteId == siteId;
            else
                whereLambda = s => s.SiteId == siteId && s.DepartmentCode == departmentCode;

            return _context.GroupListItems.Where(whereLambda)
                .Include(g => g.MedicationUnit)
                .Include(g => g.MedicationRoute)
                .Include(g => g.FrequencySchedule).ToList();
        }

        public int GetSiteForOrder(long orderId)
        {
            var x = _context.PatientOrders.Where(o => o.Id == orderId)
                .Include(o => o.Patient)
                .Select(o => o.Patient.SiteId)
                .FirstOrDefault();

            return x;
        }

        public IEnumerable<PatientOrder> GetOrders(long patientId)
        {
            throw new NotImplementedException();
        }

        Dictionary<string, int> IOrderRepository.GetUserQuickListTabs(int userId, int? siteId)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
