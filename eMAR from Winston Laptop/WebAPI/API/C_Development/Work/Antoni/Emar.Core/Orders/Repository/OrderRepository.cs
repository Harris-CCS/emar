using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Emar.Core.Helpers;
using Emar.Core.Medications.Model;
using Emar.Core.Options.Model;
using Emar.Core.Options.Repository;
using Emar.Core.Orders.Model;
using Emar.Core.ResourceParameters;
using Emar.Core.Templates.Model;
using Emar.Data;
using Emar.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Caching.Memory;

namespace Emar.Core.Orders.Repository
{
    public class OrderRepository : IOrderRepository
    {
        private readonly EmarContext _context;
        private readonly IPropertyMappingService _propertyMappingService;
        private readonly MemoryCache _cache;
        private readonly IOptionRepository _optionRepository;

        public OrderRepository(IOptionRepository optionRepository)
        {
            _optionRepository = optionRepository;
        }

        public OrderRepository(EmarContext emarContext, IPropertyMappingService propertyMappingService, EmarMemoryCache cache, IOptionRepository optionRepository)
        {
            _context = emarContext ?? throw new ArgumentNullException(nameof(emarContext));
            _propertyMappingService = propertyMappingService ?? throw new ArgumentNullException(nameof(propertyMappingService));
            _optionRepository = optionRepository ?? throw new ArgumentNullException(nameof(optionRepository));
            _cache = cache.Cache;
        }

        public PagedList<PatientOrder> GetOrders(BaseLinkResource resource)
        {
            var orders = GetOrders(order => order.PatientId == resource.PatientId);

            if (resource.OrderBy != null)
            {
                //get property mapping dictionary
                var propertyMappingDictionary = _propertyMappingService.GetPropertyMapping<PatientOrderDto, PatientOrder>();

                orders = orders.AsQueryable().ApplySort(resource.OrderBy, propertyMappingDictionary);
            }

            return PagedList<PatientOrder>.Create(orders.AsQueryable(), resource.PageNumber, resource.PageSize);
        }

        public IEnumerable<PatientOrder> GetOrders(long patientId)
        {
            return GetOrders(order => order.PatientId == patientId);
        }

        public PatientOrder GetOrder(long orderId)
        {
            return GetOrders(order => order.Id == orderId)
                .FirstOrDefault();
        }

        private IEnumerable<PatientOrder> GetOrders(Expression<Func<PatientOrder, bool>> wherePredicate)
        {
            var orders = _context.PatientOrders
                    .Include(order => order.OrderAdministrations)
                        .ThenInclude(a => a.AcknowledgeUser)
                    .Include(order => order.OrderAdministrations)
                        .ThenInclude(a => a.StopUser)
                    .Include(order => order.OrderAdministrations)
                        .ThenInclude(a => a.AdministeringUser)
                    .Include(i => i.Medication)
                        .ThenInclude(m => m.MedicationDetails)
                            .ThenInclude(md => md.MedicationUnit)
                    .Include(order => order.OrderEvents)
                        .ThenInclude(e => e.Action)
                    .Include(order => order.MedicationRoute)
                    .Include(order => order.MedicationUnit)
                    .Include(order => order.AddUser)
                    .Include(order => order.OrderPhysicianUser)
                    .Include(order => order.FrequencySchedule)
                        .ThenInclude(f => f.FrequencyType)
                    .Include(order => order.OrderInteractions)
                        .ThenInclude(interaction => interaction.DrugInteractionView)
                    .Include(order => order.AllergyReactionsView)
                    .Where(wherePredicate)
                    .ToList();

            return orders;
        }

        public IEnumerable<PatientOrder> GetPatientOrders(Expression<Func<PatientOrder, bool>> wherePredicate)
        {
            IEnumerable<PatientOrder> orders;

            if (wherePredicate == null)
            {
                orders = _context.PatientOrders
                    .Include(i => i.Medication)
                        .ThenInclude(m => m.MedicationDetails)
                            .ThenInclude(d => d.FdbBrandName)
                    .Include(i => i.Medication)
                        .ThenInclude(m => m.MedicationDetails)
                            .ThenInclude(md => md.MedicationUnit)
                    .ToList();
            }
            else
            {
                orders = _context.PatientOrders
                    .Include(i => i.Medication)
                        .ThenInclude(m => m.MedicationDetails)
                            .ThenInclude(d => d.FdbBrandName)
                    .Include(i => i.Medication)
                        .ThenInclude(m => m.MedicationDetails)
                            .ThenInclude(md => md.MedicationUnit)
                    .Where(wherePredicate)
                    .ToList();
            }

            return orders.AsEnumerable();
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
            return _context.OrderEvents
                .Include(e => e.User)
                .Include(e => e.Action)
                .FirstOrDefault(o => o.Id == eventId);
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
        /// <param name="resource"></param>
        /// <returns></returns>
        public IEnumerable<UserQuickListItem> GetUserQuickListMostUsed(BaseLinkResource resource)
        {
            Expression<Func<UserQuickListItem, bool>> whereExpression;

            if (resource.SiteId == 0)
            {
                whereExpression = i =>
                    i.UserId == resource.UserId
                    && i.WeeklyUsageRollingAverage > -1;
            }
            else
            {
                whereExpression = i =>
                    i.UserId == resource.UserId
                    && i.SiteId == resource.SiteId
                    && i.WeeklyUsageRollingAverage > -1;
            }

            return _context.UserQuickListItems
                    .Where(whereExpression)
                    .Include(i => i.Medication)
                        .ThenInclude(m => m.MedicationDetails)
                            .ThenInclude(d => d.FdbBrandName)
                    .Include(i => i.Medication)
                        .ThenInclude(m => m.MedicationDetails)
                            .ThenInclude(md => md.MedicationUnit)
                    .Include(i => i.MedicationRoute)
                    .Include(i => i.MedicationUnit)
                    .Include(i => i.FrequencySchedule)
                        .ThenInclude(f => f.FrequencyType)
                    .OrderByDescending(i => i.WeeklyUsageRollingAverage)
                    .Take(80)
                    .ToList();
        }

        public Dictionary<string, int> GetUserQuickListTabs(BaseLinkResource resource)
        {
            Expression<Func<UserQuickListItem, bool>> whereExpression;
            if (resource.SiteId == 0)
            {
                whereExpression = i =>
                    i.UserId == resource.UserId;
            }
            else
            {
                whereExpression = i =>
                    i.UserId == resource.UserId
                    && i.SiteId == resource.SiteId;
            }

            var stuff = _context.UserQuickListItems
                .Include(i => i.Medication)
                .Where(whereExpression)
                .GroupBy(i => i.Medication.DisplayName.Substring(0, 1).ToUpper())
                .Select(i => new { name = i.Key, count = i.Count() }).ToList();

            return stuff.ToDictionary(s => s.name, s => s.count);
        }

        IEnumerable<UserQuickListItem> IOrderRepository.GetUserQuickListTabItems(string tab, BaseLinkResource resource)
        {
            Expression<Func<UserQuickListItem, bool>> whereExpression;

            if (tab == "#")
            {
                if (resource.SiteId == 0)
                {
                    whereExpression = i =>
                        i.UserId == resource.UserId
                        && !EF.Functions.Like(i.Medication.DisplayName, "[a-zA-Z]%");
                }
                else
                {
                    whereExpression = i =>
                        i.UserId == resource.UserId
                        && i.SiteId == resource.SiteId
                        && !EF.Functions.Like(i.Medication.DisplayName, "[a-zA-Z]%");
                }
            }
            else
            {
                if (resource.SiteId == 0)
                {
                    whereExpression = i =>
                        i.UserId == resource.UserId
                        && EF.Functions.Like(i.Medication.DisplayName, $"[{tab.ToLower()}{tab.ToUpper()}]%");
                }
                else
                {
                    whereExpression = i =>
                        i.UserId == resource.UserId
                        && i.SiteId == resource.SiteId
                        && EF.Functions.Like(i.Medication.DisplayName, $"[{tab.ToLower()}{tab.ToUpper()}]%");
                }
            }

            return _context.UserQuickListItems
                    .Where(whereExpression)
                    .Include(i => i.Medication)
                        .ThenInclude(m => m.MedicationDetails)
                            .ThenInclude(d => d.FdbBrandName)
                    .Include(i => i.Medication)
                        .ThenInclude(m => m.MedicationDetails)
                            .ThenInclude(md => md.MedicationUnit)
                    .Include(i => i.Medication)
                        .ThenInclude(m => m.Site)
                    .Include(i => i.MedicationRoute)
                    .Include(i => i.MedicationUnit)
                    .Include(i => i.FrequencySchedule)
                        .ThenInclude(f => f.FrequencyType)
                    .ToList();
        }

        public UserQuickListItem GetUserQuickListItem(int quickListItemId)
        {
            var item = _context.UserQuickListItems
                .Include(i => i.Medication)
                    .ThenInclude(m => m.MedicationDetails)
                        .ThenInclude(d => d.FdbBrandName)
                .Include(i => i.Medication)
                    .ThenInclude(m => m.MedicationDetails)
                        .ThenInclude(md => md.MedicationUnit)
                .Include(i => i.Medication)
                    .ThenInclude(m => m.Site)
                .Include(i => i.MedicationRoute)
                .Include(i => i.MedicationUnit)
                .Include(i => i.FrequencySchedule)
                    .ThenInclude(f => f.FrequencyType)
                .FirstOrDefault(i => i.Id == quickListItemId);

            return item;
        }

        public UserQuickListItem AddQuickListItem(UserQuickListItem item)
        {
            using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    _context.UserQuickListItems.Add(item);
                    _context.SaveChanges();
                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    throw;
                }

                return GetUserQuickListItem(item.Id);
            }
        }

        public UserQuickListItem GetUserQuickListTabItem(long itemId, int userId)
        {
            Expression<Func<UserQuickListItem, bool>> whereLambda = i => i.Id == itemId;

            if (userId != 0)
            {
                whereLambda = whereLambda.And(i => i.UserId == userId);
            }

            return _context.UserQuickListItems
                .Where(whereLambda)
                .FirstOrDefault();
        }

        public FdbBrandName GetUserQuickListItemFdbBrandName(long itemId)
        {
            var query =
                from p in (from p in _context.UserQuickListItems select p).Where(u => u.Id == itemId)
                join s in _context.FdbBrandName on p.Medication.DrugId equals s.MedidString
                select s;

            return query.FirstOrDefault();
        }
        #endregion

        #region Department Preferred List Section
        public IEnumerable<DepartmentPreferredListItem> GetDepartmentPreferredList(string departmentCode, BaseLinkResource resource)
        {
            Expression<Func<DepartmentPreferredListItem, bool>> whereLambda = s => s.SiteId == resource.SiteId;
            if (!string.IsNullOrWhiteSpace(departmentCode))
                whereLambda = s => s.SiteId == resource.SiteId && s.DepartmentCode == departmentCode;

            var items = _context.DepartmentPreferredListItems.Where(whereLambda)
                .Include(i => i.Medication)
                    .ThenInclude(m => m.MedicationDetails)
                        .ThenInclude(d => d.FdbBrandName)
                .Include(i => i.Medication)
                    .ThenInclude(m => m.MedicationDetails)
                        .ThenInclude(md => md.MedicationUnit)
                .Include(i => i.Medication)
                    .ThenInclude(m => m.Site)
                .Include(g => g.MedicationUnit)
                .Include(g => g.MedicationRoute)
                .Include(g => g.FrequencySchedule)
                    .ThenInclude(f => f.FrequencyType)
                .ToList();

            return items;
        }

        public DepartmentPreferredListItem GetDepartmentPreferredItem(long itemId)
        {
            Expression<Func<DepartmentPreferredListItem, bool>> whereLambda = i => i.Id == itemId;

            return _context.DepartmentPreferredListItems
                .Include(i => i.Medication)
                    .ThenInclude(m => m.MedicationDetails)
                        .ThenInclude(d => d.FdbBrandName)
                .Include(i => i.Medication)
                    .ThenInclude(m => m.MedicationDetails)
                        .ThenInclude(md => md.MedicationUnit)
                .Include(i => i.Medication)
                    .ThenInclude(m => m.Site)
                .Include(i => i.MedicationRoute)
                .Include(i => i.MedicationUnit)
                .Include(i => i.FrequencySchedule)
                    .ThenInclude(f => f.FrequencyType)
                .FirstOrDefault(whereLambda);
        }

        public FdbBrandName GetDepartmentPreferredListItemFdbBrandName(long itemId)
        {
            var query =
                from p in (from p in _context.DepartmentPreferredListItems select p).Where(u => u.Id == itemId)
                join s in _context.FdbBrandName on p.Medication.DrugId equals s.MedidString
                select s;

            return query.FirstOrDefault();
        }
        #endregion

        #region Groups Remembered Orders Section
        public IEnumerable<GroupListItem> GetGroupRememberedOrderItems(string departmentCode, BaseLinkResource resource)
        {
            Expression<Func<GroupListItem, bool>> whereLambda;
            if (string.IsNullOrWhiteSpace(departmentCode))
                whereLambda = s => s.SiteId == resource.SiteId;
            else
                whereLambda = s => s.SiteId == resource.SiteId && s.DepartmentCode == departmentCode;

            var items = _context.GroupListItems
                .Where(whereLambda)
                .Include(i => i.Medication)
                    .ThenInclude(m => m.MedicationDetails)
                        .ThenInclude(d => d.FdbBrandName)
                .Include(i => i.Medication)
                    .ThenInclude(m => m.MedicationDetails)
                        .ThenInclude(md => md.MedicationUnit)
                .Include(i => i.Medication)
                    .ThenInclude(m => m.Site)
                .Include(g => g.MedicationUnit)
                .Include(g => g.MedicationRoute)
                .Include(g => g.FrequencySchedule)
                    .ThenInclude(f => f.FrequencyType)
                .ToList();

            return items;
        }

        public GroupListItem GetGroupRememberedOrderItem(long itemId)
        {
            Expression<Func<GroupListItem, bool>> whereLambda = i => i.Id == itemId;

            return _context.GroupListItems
                .Include(i => i.Medication)
                    .ThenInclude(m => m.MedicationDetails)
                        .ThenInclude(d => d.FdbBrandName)
                .Include(i => i.Medication)
                    .ThenInclude(m => m.MedicationDetails)
                        .ThenInclude(md => md.MedicationUnit)
                .Include(i => i.Medication)
                    .ThenInclude(m => m.Site)
                .Include(i => i.MedicationRoute)
                .Include(i => i.MedicationUnit)
                .Include(i => i.FrequencySchedule)
                    .ThenInclude(f => f.FrequencyType)
                .FirstOrDefault(whereLambda);
        }

        public FdbBrandName GetGroupRememberedOrderItemFdbBrandName(long itemId)
        {
            var query =
                from p in (from p in _context.GroupListItems select p).Where(u => u.Id == itemId)
                join s in _context.FdbBrandName on p.Medication.DrugId equals s.MedidString
                select s;

            return query.FirstOrDefault();
        }
        #endregion

        #region Allergies Section
        public IEnumerable<PatientAllergy> GetAllergies(Func<PatientAllergy, bool> wherePredicate)
        {
            return _context.PatientAllergies
                    .Where(wherePredicate)
                    .AsEnumerable();
        }
        #endregion

        #region Scheduler Support Methods

        public enum CodeShareEntity
        {
            MedicationUnit,
            MedicationRoute,
            FrequencySchedule,
            OrderInstruction
        }

        public List<CodeSharedId> GetCodeShareSites(int siteId)
        {
            var medicationUnitSiteId = _context.GetCodeShareSiteViewMedicationUnits
                .FirstOrDefault(g => g.Id == siteId)?
                .SiteId;

            var medicationRouteSiteId = _context.GetCodeShareSiteViewMedicationRoutes
                .FirstOrDefault(g => g.Id == siteId)?
                .SiteId;

            var frequencySiteId = _context.GetCodeShareSiteViewFrequencySchedules
                .FirstOrDefault(g => g.Id == siteId)?
                .SiteId;

            var orderInstructionSiteId = _context.GetCodeShareSiteViewOrderInstructions
                .FirstOrDefault(g => g.Id == siteId)?
                .SiteId;

            var codeSharedIds = new List<CodeSharedId>();

            if (medicationUnitSiteId != null)
            {
                codeSharedIds.Add(
                    new CodeSharedId
                    {
                        SiteId = siteId,
                        Entity = CodeShareEntity.MedicationUnit,
                        SharedSiteId = medicationUnitSiteId.Value
                    });
            }

            if (medicationRouteSiteId != null)
            {
                codeSharedIds.Add(
                    new CodeSharedId
                    {
                        SiteId = siteId,
                        Entity = CodeShareEntity.MedicationRoute,
                        SharedSiteId = medicationRouteSiteId.Value
                    });
            }

            if (frequencySiteId != null)
            {
                codeSharedIds.Add(
                    new CodeSharedId
                    {
                        SiteId = siteId,
                        Entity = CodeShareEntity.FrequencySchedule,
                        SharedSiteId = frequencySiteId.Value
                    });
            }

            if (orderInstructionSiteId != null)
            {
                codeSharedIds.Add(
                    new CodeSharedId
                    {
                        SiteId = siteId,
                        Entity = CodeShareEntity.OrderInstruction,
                        SharedSiteId = orderInstructionSiteId.Value
                    });
            }

            return codeSharedIds;
        }

        private Expression<Func<Medication, bool>> GetSchedulerSetupDataWhereExpression(int siteId, Expression<Func<Medication, bool>> baseExpression)
        {
            var drugDbVendor = _optionRepository.GetOption(siteId, OptionNames.DRUG_DB_VENDOR).ToUpper();
            var medInpat = _optionRepository.GetOption(siteId, OptionNames.MEDINPAT).ToUpper() == "Y";
            var medOutpat = _optionRepository.GetOption(siteId, OptionNames.MEDOUTPAT).ToUpper() == "Y";
            var medPyxis = _optionRepository.GetOption(siteId, OptionNames.MEDPYXIS).ToUpper() == "Y";

            Expression<Func<Medication, bool>> whereExpression = m =>
                m.DrugVendor == drugDbVendor;

            whereExpression = whereExpression.And(baseExpression);

            if (medInpat && medOutpat && medPyxis)
            {
                whereExpression = whereExpression.And(m =>
                    m.SiteFormularys.Any(f =>
                        f.IsInpatient || f.IsOutpatient || f.IsPyxis));
            }
            else if (medInpat && medOutpat)
            {
                whereExpression = whereExpression.And(m =>
                    m.SiteFormularys.Any(f =>
                        f.IsInpatient || f.IsOutpatient));
            }
            else if (medInpat && medPyxis)
            {
                whereExpression = whereExpression.And(m =>
                    m.SiteFormularys.Any(f =>
                        f.IsInpatient || f.IsPyxis));
            }
            else if (medOutpat && medPyxis)
            {
                whereExpression = whereExpression.And(m =>
                    m.SiteFormularys.Any(f =>
                        f.IsOutpatient || f.IsPyxis));
            }
            else if (medInpat)
            {
                whereExpression = whereExpression.And(m =>
                    m.SiteFormularys.Any(f =>
                        f.IsInpatient));
            }
            else if (medOutpat)
            {
                whereExpression = whereExpression.And(m =>
                    m.SiteFormularys.Any(f =>
                        f.IsOutpatient));
            }
            else if (medPyxis)
            {
                whereExpression = whereExpression.And(m =>
                    m.SiteFormularys.Any(f =>
                        f.IsPyxis));
            }

            return whereExpression;
        }

        public IEnumerable<Medication> GetSchedulerSetupData(int siteId, string brandName)
        {
            var query = new List<Medication>();

            var whereExpression = GetSchedulerSetupDataWhereExpression(siteId,
                m =>
                    m.MedicationDetails.Any(md => md.BrandName == brandName)
                    && m.SiteId.ToString().ToUpper() != "COMBO");
            //////&& m.SiteId == -1);

            query.AddRange(_context.Medications
                .Include(m => m.MedicationDetails)
                    .ThenInclude(md => md.MedicationUnit)
                .Include(m => m.PreferredMedicationDoses)
                    .ThenInclude(p => p.MedicationUnit)
                .Include(m => m.PreferredMedicationRoutes)
                    .ThenInclude(p => p.MedicationRoute)
                .Include(m => m.PreferredFrequencySchedules)
                    .ThenInclude(p => p.FrequencySchedule)
                        .ThenInclude(f => f.FrequencyType)
                .Where(whereExpression)
                .ToList());

            if (!query.Any() || query.All(m => m == null))
            {
                whereExpression = GetSchedulerSetupDataWhereExpression(siteId,
                    m =>
                        m.DisplayName == brandName
                        && m.SiteId == siteId);

                query.Add(_context.Medications
                    .Include(m => m.MedicationDetails)
                        .ThenInclude(md => md.MedicationUnit)
                    .Include(m => m.PreferredMedicationDoses)
                        .ThenInclude(p => p.MedicationUnit)
                    .Include(m => m.PreferredMedicationRoutes)
                        .ThenInclude(p => p.MedicationRoute)
                    .Include(m => m.PreferredFrequencySchedules)
                        .ThenInclude(p => p.FrequencySchedule)
                            .ThenInclude(f => f.FrequencyType)
                    .FirstOrDefault(whereExpression));
            }

            return query;
        }

        public IEnumerable<Medication> GetSchedulerSetupData(int siteId, EmarOrderType itemType, int itemId)
        {
            var medicationId = itemType switch
            {
                EmarOrderType.UserQuickListItem => _context.UserQuickListItems.Find(itemId).MedicationId,
                EmarOrderType.DepartmentPreferredListItem => _context.DepartmentPreferredListItems.Find(itemId).MedicationId,
                EmarOrderType.GroupRememberedOrder => _context.GroupListItems.Find(itemId).MedicationId,
                EmarOrderType.PatientCartOrder => _context.PatientCartOrders.Find((long)itemId).MedicationId,
                EmarOrderType.MedicationItem => itemId,
                _ => -1
            };

            if (medicationId == -1)
            {
                return null;
            }

            var query = new List<Medication>();

            var whereExpression = GetSchedulerSetupDataWhereExpression(siteId,
                m =>
                    m.Id == medicationId
                    && m.SiteId.ToString().ToUpper() != "COMBO");
            //&& m.SiteId == -1);

            query.AddRange(_context.Medications
                .Include(m => m.MedicationDetails)
                    .ThenInclude(md => md.MedicationUnit)
                .Include(m => m.PreferredMedicationDoses)
                    .ThenInclude(p => p.MedicationUnit)
                .Include(m => m.PreferredMedicationRoutes)
                    .ThenInclude(p => p.MedicationRoute)
                .Include(m => m.PreferredFrequencySchedules)
                    .ThenInclude(p => p.FrequencySchedule)
                        .ThenInclude(f => f.FrequencyType)
                .Where(whereExpression)
                .ToList());

            if (!query.Any() || query.All(m => m == null))
            {
                whereExpression = GetSchedulerSetupDataWhereExpression(siteId,
                    m =>
                        m.Id == medicationId
                        && m.SiteId == siteId);

                query.Add(_context.Medications
                    .Include(m => m.MedicationDetails)
                        .ThenInclude(md => md.MedicationUnit)
                    .Include(m => m.PreferredMedicationDoses)
                        .ThenInclude(p => p.MedicationUnit)
                    .Include(m => m.PreferredMedicationRoutes)
                        .ThenInclude(p => p.MedicationRoute)
                    .Include(m => m.PreferredFrequencySchedules)
                        .ThenInclude(p => p.FrequencySchedule)
                            .ThenInclude(f => f.FrequencyType)
                    .FirstOrDefault(whereExpression));
            }

            return query;
        }

        public List<AntimicrobialRequiredIndicator> GetAntimicrobialRequiredIndicators(int siteId, List<Medication> medications)
        {
            var query = new List<AntimicrobialRequiredIndicator>();

            query.AddRange(from medication in medications.Where(m => m != null)
                           let results = _context
                               .GetAntimicrobialRequiredFdbFunction
                               .FromSqlInterpolated(
                                   $"SELECT [antimicrobial_required] FROM get_antimicrobial_required_fdb({siteId},{medication.Id})")
                               .ToList()
                           select new AntimicrobialRequiredIndicator
                           {
                               MedicationId = medication.Id,
                               AntimicrobialRequired = results.Any(i => i.AntimicrobialRequired)
                           });

            return query;
        }

        public List<FrequencyScheduleAdministration> GetSchedulerAdministrations(int siteId, EmarOrderType itemType, int itemId, DateTimeOffset start, DateTimeOffset? stop)
        {
            var frequencyId = itemType switch
            {
                EmarOrderType.UserQuickListItem => _context.UserQuickListItems.Find(itemId).FrequencyScheduleId,
                EmarOrderType.DepartmentPreferredListItem => _context.DepartmentPreferredListItems.Find(itemId).FrequencyScheduleId,
                EmarOrderType.GroupRememberedOrder => _context.GroupListItems.Find(itemId).FrequencyScheduleId,
                _ => null
            };

            return frequencyId == null
                ? null
                : GetNewAdministrations(siteId, frequencyId.Value, _context.Sites.Find(siteId).TimeZoneName.NowWithTimeZoneOffset(), null)
                    .ToList();
        }

        public IEnumerable<OrderInstruction> GetOrderInstructions(int siteId)
        {
            return _cache.GetOrCreate(siteId == 0 ? "All" : siteId + CacheKeys.OrderInstructions, entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30);

                var ret = siteId == 0
                    ? _context.OrderInstructions.ToList()
                    : _context.OrderInstructions.Where(s => s.SiteId == siteId).ToList();

                entry.Size = ret.Count;

                return ret;
            });
        }

        public IEnumerable<FrequencySchedule> GetScheduleFrequencies(int siteId)
        {
            return _cache.GetOrCreate(siteId == 0 ? "All" : siteId + CacheKeys.FrequencySchedules, entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30);

                var ret = siteId == 0
                    ? _context.FrequencySchedules.ToList()
                    : _context.FrequencySchedules
                        .Include(f => f.FrequencyType)
                        .Where(s => s.SiteId == siteId).ToList();

                entry.Size = ret.Count;

                return ret;
            });
        }

        public IEnumerable<MedicationRoute> GetRoutes(int siteId)
        {
            return _cache.GetOrCreate(siteId == 0 ? "All" : siteId + CacheKeys.Routes, entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30);

                var ret = siteId == 0
                    ? _context.MedicationRoutes.ToList()
                    : _context.MedicationRoutes.Where(s => s.SiteId == siteId).ToList();

                entry.Size = ret.Count;

                return ret;
            });
        }

        public IEnumerable<MedicationUnit> GetUnits(int siteId)
        {
            return _cache.GetOrCreate(siteId == 0 ? "All" : siteId + CacheKeys.Units, entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30);

                var ret = siteId == 0
                    ? _context.MedicationUnits.ToList()
                    : _context.MedicationUnits.Where(s => s.SiteId == siteId).ToList();

                entry.Size = ret.Count;

                return ret;
            });
        }

        public IEnumerable<DurationUnit> GetDurationUnits()
        {
            return _cache.GetOrCreate("All" + CacheKeys.DurationUnits, entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30);

                var ret = _context.DurationUnits.ToList();

                entry.Size = ret.Count;

                return ret;
            });
        }
        #endregion

        #region Utility Methods
        public int GetSiteForOrder(long orderId)
        {
            var x = _context.PatientOrders.Where(o => o.Id == orderId)
                .Select(o => o.Patient.SiteId)
                .FirstOrDefault();

            return x;
        }

        public int GetSiteForAdministration(long adminId)
        {
            var x = _context.OrderAdministrations.Where(a => a.Id == adminId)
                .Select(a => a.PatientOrder.Patient.SiteId)
                .FirstOrDefault();

            return x;
        }

        public List<FrequencyScheduleAdministration> GetNewAdministrations(int siteId, int frequencyId, DateTimeOffset start, DateTimeOffset? stop)
        {
            _context.Sites.Find(siteId).TimeZoneName.NowWithTimeZoneOffset();


            var administrations = new List<FrequencyScheduleAdministration>();

            if (frequencyId == 0)
                // no frequency has been set
                return administrations;

            if (frequencyId < 0)
                throw new ArgumentException("Negative Frequency Schedule IDs not allowable.",
                    nameof(frequencyId));

            if (stop != null && stop.Value < start)
                throw new ArgumentOutOfRangeException(nameof(stop), stop,
                    "Stop time must be after start time.");

            stop ??= DateTimeOffset.Now.AddDays(int.TryParse(_optionRepository.GetOption(siteId, OptionNames.SCHEDULE_FUTURE_ITEMS), out int number) ? number : 7);

            administrations = _context.FrequencyScheduleAdministrations
                .FromSqlInterpolated(
                    $"EXEC [dbo].[get_frequency_schedule_items] {frequencyId}, {start}, {stop}")
                .ToList();

            return administrations;
        }
        #endregion
    }
}