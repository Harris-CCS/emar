using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Emar.Core.Helpers;
using Emar.Core.Medications.Model;
using Emar.Core.Medications.Repository;
using Emar.Core.Options.Model;
using Emar.Core.Options.Repository;
using Emar.Core.Orders.Model;
using Emar.Core.ResourceParameters;
using Emar.Core.Sites.Repository;
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
        private readonly ISiteRepository _siteRepository;
        private readonly IPropertyMappingService _propertyMappingService;
        private readonly MemoryCache _cache;
        private readonly IOptionRepository _optionRepository;
        private IDrugDbRepository _vendorRepository;

        public OrderRepository(IOptionRepository optionRepository)
        {
            _optionRepository = optionRepository;
        }

        public OrderRepository(EmarContext emarContext, ISiteRepository siteRepository, IPropertyMappingService propertyMappingService, 
            EmarMemoryCache cache, IOptionRepository optionRepository)
        {
            _context = emarContext ?? throw new ArgumentNullException(nameof(emarContext));
            _siteRepository = siteRepository;
            _propertyMappingService = propertyMappingService ?? throw new ArgumentNullException(nameof(propertyMappingService));
            _optionRepository = optionRepository ?? throw new ArgumentNullException(nameof(optionRepository));
            _cache = cache.Cache;
            _vendorRepository = new DrugDbRepositoryFdb(_context, _optionRepository);
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
            // In order for the two separate calls to populate the OrderAdministrations in the context (but still be much
            // more efficient than doing it in one call - like 30s --> 2s - .AsNoTracking() must not be used on either call

            //Moved the administrations out to their own query/DB hit to make this query faster.
            //Bradley Marshall, 06/14/2022.  PC-27309

            //Moved the interactions out to their own query/DB hit to make this query faster.
            //Winston Murdock, 06/20/2022.  PC-27309

            //For the problem patient on Emerus Prod, this query takes less than one second.
            //And the JSON comes back in exactly the same structure as it was previously.
            //Thusly, no UI changes are needed.
            var orders = _context.PatientOrders
                    //.Include(order => order.OrderAdministrations)
                    //    .ThenInclude(a => a.AcknowledgeUser)
                    //.Include(order => order.OrderAdministrations)
                    //    .ThenInclude(a => a.StopUser)
                    //.Include(order => order.OrderAdministrations)
                    //    .ThenInclude(a => a.AdministeringUser)
                    .Include(i => i.Medication)
                        .ThenInclude(m => m.MedicationDetails)
                            .ThenInclude(md => md.MedicationUnit)
                    .Include(order => order.OrderEvents)
                        .ThenInclude(e => e.Action)
                    .Include(order => order.MedicationRoute)
                    .Include(order => order.MedicationUnit)
                    .Include(order => order.AddUser)
                    .Include(order => order.OrderPhysicianUser)
                    .Include(order => order.DurationUnit)
                    .Include(order => order.FrequencySchedule)
                        .ThenInclude(f => f.FrequencyType)
                    //.Include(order => order.OrderInteractions)
                    //    .ThenInclude(interaction => interaction.DrugInteractionView)
                    .Include(order => order.AllergyReactionsView)
                    .Where(wherePredicate)
                    //.AsNoTracking()
                    .ToList();

            //Break administrations out into its own query.
            //We end up getting three rows for each administration (one for each of the users listed).
            //And moving this to a separate query had a hugely positive impact on the time
            //it takes to run this query.
            //For the problem patient on Emerus Prod, this query takes less than one second.
            //Bradley Marshall, 06/15/2022.
            _context.PatientOrders
                    .Include(order => order.OrderAdministrations)
                       .ThenInclude(a => a.AcknowledgeUser)
                    .Include(order => order.OrderAdministrations)
                        .ThenInclude(a => a.StopUser)
                    .Include(order => order.OrderAdministrations)
                        .ThenInclude(a => a.AdministeringUser)
                    .Where(wherePredicate)
                    //.AsNoTracking()
                    .ToList();

            //Break interactions out into their own query to avoid long execution times
            //when the patient has a bunch of interactions.
            //For the problem patient on Emerus Prod, this query takes less than one second.
            //Winston Murdock, 06/20/2022.
            _context.PatientOrders
                .Include(order => order.OrderInteractions)
                    .ThenInclude(interaction => interaction.DrugInteractionView)
                .Where(wherePredicate)
                .ToList();

            //We need some way to pull the users for the order events.
            //When I just .included them, I ended up with a cartesian product that resulted in
            //a patient with 7 orders on 57c having over 5,000 hits coming from the DB.
            //this resulted in timeouts and issues at Emerus prod until we rolled it back.
            //I'll probably do similar to how I do the NDC info fro the medications on the
            //scheduler options page.
            //Loop through each patient order.
            //Then loop through each order events
            //Then set the user for each order event.
            //We might need to do something similar on the mar department page when we get all orders for a patient.
            //EMAR-1080.

            // Found that for every order, we were getting a siteId for the patient by making a DB call
            // This method is called from 3 places, twice for the patient, and once for the order.  In all
            // cases, the siteId will be the same for all orders, so just get it once.
            //Use this initial value of -99 for site id to see if we're on the first iteration or not.
            int siteId = -99;
            string drugDbVendor = "";
            bool isPediatric = true;

            //Confirm that we have orders.
            if (orders.Count() > 0)
            {
                //Now confirm that the first item is not null.
                if (orders[0] != null)
                {
                    //Loop through the list of patient orders.
                    foreach (PatientOrder order in orders)
                    {
                        //Confirm that this order has events.
                        if (order.OrderEvents.Count() > 0)
                        {
                            //Now confirm that the first item is not null.
                            if (order.OrderEvents.ToList()[0] != null)
                            {
                                //Loop through each order event for this patient order.
                                foreach (OrderEvent orderEvent in order.OrderEvents)
                                {
                                    //Get the user entity for this order event.
                                    orderEvent.User = GetLocalCachedUser(orderEvent.AddUserId);
                                } //end foreach
                            } //end if
                        } //end if

                        //We need to get the FdbBrandName for the order.
                        //order.Medication.MedicationDetails[i].FdbBraneName
                        //Only do this if we're on FDB.
                        //Reaction checking when repeating an existing patient order
                        //was not working because we didn't have the FdbBrandName populated.
                        //Winston Murdock, 03/01/2022.  PC-27061
                        //Use this initial value of -99 for site id to see if we're on the first iteration or not.
                        if (siteId == -99)
                        {
                            siteId = _context.Patients.Find(order.PatientId).SiteId;
                            drugDbVendor = _optionRepository.GetOption(siteId, OptionNames.DRUG_DB_VENDOR).ToUpper();

                            var dateOfBirth = _context.Patients.Find(order.PatientId).DateOfBirth;
                            // isPediatric is initialized to true above - will be true unless there is a birthday and age > 12
                            //If there's no DOB, then don't check the patients age.
                            //Per conversation with Merrily on 11/11/2021.  PC-26739
                            if (dateOfBirth.HasValue)
                            {
                                //Cast dateOfBirth to a DateTime since it comes back as a nullable DateTime from the DB context.
                                if (Years((DateTime)dateOfBirth, DateTime.Now) > 12)
                                {
                                    isPediatric = false;
                                } //end if
                            }
                        }

                        if (drugDbVendor == "F")
                        {
                            //FDB.
                            if (order.Medication.MedicationDetails.Count() > 0)
                            {
                                foreach (MedicationDetail md in order.Medication.MedicationDetails)
                                {
                                    md.FdbBrandName = GetLocalCachedFdbBrandName(md.DrugId);
                                } //end foreach
                            } //end if
                        } //end if

                        //See if the medication is a high risk medication or not.
                        //Winston Murdock, 11/10/2021.  PC-26735
                        order.IsHighRisk = GetLocalCachedHighRiskMed(order.MedicationId);

                        //Get the patient's data of birth.
                        //If the patient is 12 or younger, then this is a pediatric patient.
                        //The PatientOrder entity doesn't have the Patient child populated.
                        //So go get that one field from EF.
                        //We've got an index on patients.id, so this will be a super fast query.
                        //TODO: Make the age buffer for pediatric patients a site setting rather than hardcoding it here.
                        order.IsPedatric = isPediatric;

                        //See if we have any OrderInteractions.
                        if (order.OrderInteractions.Any())
                        {
                            //The order_interactions table has two rows for each interactions.
                            //If this order is Tylenol, and it interacts with a Warfarin order
                            //for the same patient, then it will have a row for the
                            //Tylenol -> Warfarin interaction with drug_num = 1
                            //and a row for the Warfarin -> Tylenol interaction with drug_num = 2.
                            //We only want the row for drug_num = 1.
                            //To accomplish this...
                            //Set OrderInteractions equal to a filtered copy of OrderInteractions
                            //filtered to only have entries where DrugNum is 1.
                            //I tried writing the filter clause for the big query above to do this
                            //but gave up after an hour of failure.
                            //The following code accomplishes the same thing without any extra DB hits.
                            //Winston Murdock, 03/21/2022.  PC-27067
                            order.OrderInteractions = order.OrderInteractions.Where(oi => oi.DrugNum == 1).ToList();
                        } //end if (any OrderInteractions?)

                        //If we have an AntimicrobialIndicationId, then get
                        //the AntimicrobialIndication entity for this order.
                        //Winston Murdock, 04/11/2022.  PC-27140
                        if (order.AntimicrobialIndicationId.HasValue)
                        {
                            order.AntimicrobialIndication = _context.AntimicrobialIndications.Find(order.AntimicrobialIndicationId);
                        } //end if
                    } //end foreach
                } //end if
            } //end if 

            return orders;
        }

        readonly Dictionary<int, User> _localUserCache = new Dictionary<int, User>();
        readonly Dictionary<string, FdbBrandName> _localFdbBrandNameCache = new Dictionary<string, FdbBrandName>();
        private Dictionary<int, bool> _localHighRiskCache = new Dictionary<int, bool>();

        private User GetLocalCachedUser(int orderEventAddUserId)
        {
            if (_localUserCache.TryGetValue(orderEventAddUserId, out var user))
                return user;

            user =
                (from row in _context.Users
                 where row.Id == orderEventAddUserId
                 select row
                ).FirstOrDefault();

            _localUserCache.Add(orderEventAddUserId, user);
            return user;
        }

        private FdbBrandName GetLocalCachedFdbBrandName(string mdDrugId)
        {
            if (_localFdbBrandNameCache.TryGetValue(mdDrugId, out var brandName))
                return brandName;

            brandName = _context.FdbBrandName.FirstOrDefault(x => x.MedidString == mdDrugId);

            _localFdbBrandNameCache.Add(mdDrugId, brandName);
            return brandName;
        }

        private bool GetLocalCachedHighRiskMed(int orderMedicationId)
        {
            if (_localHighRiskCache.TryGetValue(orderMedicationId, out var isHighRisk))
                return isHighRisk;

            isHighRisk = _context.MedicationsHighRisk.Any(mhr => mhr.MedicationId == orderMedicationId);

            _localHighRiskCache.Add(orderMedicationId, isHighRisk);
            return isHighRisk;
        }

        public int Years(DateTime start, DateTime end)
        {
            //https://stackoverflow.com/a/4127477
            return (end.Year - start.Year - 1) +
                (((end.Month > start.Month) ||
                ((end.Month == start.Month) && (end.Day >= start.Day))) ? 1 : 0);
        } // end Years

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
            //If all three of the filters are N (I/O/P), then proceed normally.
            //Else, join out to site_formulary_match (which we can do since we pre-calculate the match levels now)
            //and only return ones that are greater than or equal to the needed match level.
            //Winston Murdock, 03/26/2021.  EMAR-351

            //TODO: Move the vendor-specific logic to a vendor-specific controller once we start handling multiple vendors.
            //This joins out to fdb_brand_name, so it will need to reside in a vendor-specific repository.
            //We've already got IDrugDbRepository and DrugDbRepositoryFDB setup for vendor-specific things.
            //We'll need to add Multum, Medispan, etc... in the future.

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

            //Get the vendor and formulary Y/N settings from the DB.
            var drugDbVendor = _optionRepository.GetOption(resource.SiteId, OptionNames.DRUG_DB_VENDOR).ToUpper();
            var medInpat = _optionRepository.GetOptionBool(resource.SiteId, OptionNames.MEDINPAT, false);
            var medOutpat = _optionRepository.GetOptionBool(resource.SiteId, OptionNames.MEDOUTPAT, false);
            var medPyxis = _optionRepository.GetOptionBool(resource.SiteId, OptionNames.MEDPYXIS, false);
            var exactMatch = _optionRepository.GetOptionBool(resource.SiteId, OptionNames.MEDEXACTMATCH, false);

            // Figure out if we're doing exact match or not and set matchLevel accordingly.
            int matchLevel = exactMatch ? 3 : 1;

            //Eventually, we'll check the vendor.  For now, just assume we're on American FDB.
            //Also, this duplicates the brand name search logic of only applying the formulary filters
            //if the I/O/P site options are set.
            //Per Romel, we could have a site with I/O/P set to N but that still has entries in their formulary table.
            //So we would need to also see if there are any rows in the formulary table with I, O< or P set to Y
            //to deteremind whether or not to formulary filter.
            //This will be done after the Emerus go live.

            // If this site is not doing formulary filtering (I, O< and P are all N), then return the user's quick list as we are currently doing.
            // If one, or more, of them are Y, then we need to apply the filtering logic to it.
            if (!medInpat && !medOutpat && !medPyxis)
            {
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
            else
            {
                //We are doing at least one formulary filter.
                //Need to join to site_formulary_match where we match on medication_id and site_id.
                //Then only grab the ones that are higher than that the match level.
                //This is the same query as above, except we include SiteFormularyMatchs.
                var uqliItems = _context.UserQuickListItems
                    .Where(whereExpression)
                    .Include(i => i.Medication)
                        .ThenInclude(m => m.MedicationDetails)
                            .ThenInclude(d => d.FdbBrandName)
                    .Include(i => i.Medication)
                        .ThenInclude(m => m.MedicationDetails)
                            .ThenInclude(md => md.MedicationUnit)
                    .Include(i => i.Medication)
                        .ThenInclude(m => m.SiteFormularyMatchs)
                    .Include(i => i.MedicationRoute)
                    .Include(i => i.MedicationUnit)
                    .Include(i => i.FrequencySchedule)
                        .ThenInclude(f => f.FrequencyType)
                    .OrderByDescending(i => i.WeeklyUsageRollingAverage)
                    .Take(80)
                    .ToList();

                //Now filter the list to only have items that match on site id.
                //Since Medication.SiteFormularyMatchs is a list, we cannot directly access the SiteId field.
                //Do .Any(lambda) to only return SiteFormularyMatch entities that match on the SiteId.
                var siteUqliItems = uqliItems
               .Where(s => s.Medication.SiteFormularyMatchs
                     .Any(x => x.SiteId == resource.SiteId)).ToList();

                var returnUqliItems = new List<UserQuickListItem>();

                //Now that we've filtered siteUqliItems to only have match rows that match on the siteId,
                //we need to apply the formulary filtering logic.
                //For each of I/O/P
                //If this one is on, return all rows that have that match value greater than or equal to the match level we calculated.
                //If exact match is enabled, then matchLevel will be 3.
                //Else, matchLevel will be 1.
                if (medInpat)
                {
                    //Add any where InpatientMatch is greater than or equal to match Level
                    returnUqliItems.AddRange(
                        siteUqliItems.Where(a => a.Medication.SiteFormularyMatchs
                            .Any(b => b.InpatientMatch >= matchLevel))
                        );
                } //end if (medInpat = Y?)

                if (medOutpat)
                {
                    //Add any where IutpatientMatch is greater than or equal to match Level
                    returnUqliItems.AddRange(
                        siteUqliItems.Where(a => a.Medication.SiteFormularyMatchs
                            .Any(b => b.OutpatientMatch >= matchLevel))
                        );
                } //end if (medOutpat = Y?)

                if (medPyxis)
                {
                    //Add any where PyxisMatch is greater than or equal to match Level
                    returnUqliItems.AddRange(
                        siteUqliItems.Where(a => a.Medication.SiteFormularyMatchs
                            .Any(b => b.PyxisMatch >= matchLevel))
                        );
                } //end if (medPyxis = Y?)

                //Could have duplicate return rows here.
                //Group by the medication id and then so a first or default.
                //So that we only return each medication once.
                //Group by uqli.id so that we don't include the same item multiple times.
                //We can have the same medication in the list multiple times with different doses, etc...
                //So we cannot group by medication id.
                return returnUqliItems.GroupBy(x => x.Id).Select(x => x.FirstOrDefault());
            } //end if (all of I/O/P equal N?)
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

            //Get the vendor and formulary Y/N settings from the DB.
            var drugDbVendor = _optionRepository.GetOption(resource.SiteId, OptionNames.DRUG_DB_VENDOR).ToUpper();
            var medInpat = _optionRepository.GetOptionBool(resource.SiteId, OptionNames.MEDINPAT, false);
            var medOutpat = _optionRepository.GetOptionBool(resource.SiteId, OptionNames.MEDOUTPAT, false);
            var medPyxis = _optionRepository.GetOptionBool(resource.SiteId, OptionNames.MEDPYXIS, false);
            var exactMatch = _optionRepository.GetOptionBool(resource.SiteId, OptionNames.MEDEXACTMATCH, false);

            // Figure out if we're doing exact match or not and set matchLevel accordingly.
            int matchLevel = exactMatch ? 3 : 1;

            //TODO: Eventually, we'll check the vendor.  For now, just assume we're on American FDB.
            //Also, this duplicates the brand name search logic of only applying the formulary filters
            //if the I/O/P site options are set.
            //Per Romel, we could have a site with I/O/P set to N but that still has entries in their formulary table.
            //So we would need to also see if there are any rows in the formulary table with I, O< or P set to Y
            //to deteremind whether or not to formulary filter.
            //This will be done after the Emerus go live.

            // If this site is not doing formulary filtering (I, O< and P are all N), then return the user's quick list as we are currently doing.
            // If one, or more, of them are Y, then we need to apply the filtering logic to it.
            if (!medInpat && !medOutpat && !medPyxis)
            {
                //Not doing any formulary filtering.
                //Return as normal.
                var stuff = _context.UserQuickListItems
                .Include(i => i.Medication)
                .Where(whereExpression)
                .GroupBy(i => i.Medication.DisplayName.Substring(0, 1).ToUpper())
                .Select(i => new { name = i.Key, count = i.Count() }).ToList();

                return stuff.ToDictionary(s => s.name, s => s.count);
            }
            else
            {
                //We are doing at least one formulary filter.
                //Need to join to site_formulary_match where we match on medication_id and site_id.
                //Then only grab the ones that are higher than that the match level.
                //This is the same query as above, except we include SiteFormularyMatchs.
                var uqliItems = _context.UserQuickListItems
                .Include(i => i.Medication)
                    .ThenInclude(m => m.SiteFormularyMatchs)
                .Where(whereExpression)
                //Instead of selecting into the dictionary here, we'll do it later on.
                //.GroupBy(i => i.Medication.DisplayName.Substring(0, 1).ToUpper())
                //.Select(i => new { name = i.Key, count = i.Count() }).ToList();
                .ToList();

                //Now filter the list to only have items that match on site id.
                //Since Medication.SiteFormularyMatchs is a list, we cannot directly access the SiteId field.
                //Do .Any(lambda) to only return SiteFormularyMatch entities that match on the SiteId.
                var siteUqliItems = uqliItems
               .Where(s => s.Medication.SiteFormularyMatchs
                     .Any(x => x.SiteId == resource.SiteId)).ToList();


                var returnUqliItems = new List<UserQuickListItem>();

                //Now that we've filtered siteUqliItems to only have match rows that match on the siteId,
                //we need to apply the formulary filtering logic.
                //For each of I/O/P
                //If this one is on, add all rows that have that match value greater than or equal to the match level we calculated.
                //If exact match is enabled, then matchLevel will be 3.
                //Else, matchLevel will be 1.
                if (medInpat)
                {
                    //Add any where InpatientMatch is greater than or equal to match Level
                    returnUqliItems.AddRange(
                        siteUqliItems.Where(a => a.Medication.SiteFormularyMatchs
                            .Any(b => b.InpatientMatch >= matchLevel))
                        );
                } //end if (medInpat = Y?)

                if (medOutpat)
                {
                    //Add any where IutpatientMatch is greater than or equal to match Level
                    returnUqliItems.AddRange(
                        siteUqliItems.Where(a => a.Medication.SiteFormularyMatchs
                            .Any(b => b.OutpatientMatch >= matchLevel))
                        );
                } //end if (medOutpat = Y?)

                if (medPyxis)
                {
                    //Add any where PyxisMatch is greater than or equal to match Level
                    returnUqliItems.AddRange(
                        siteUqliItems.Where(a => a.Medication.SiteFormularyMatchs
                            .Any(b => b.PyxisMatch >= matchLevel))
                        );
                } //end if (medPyxis = Y?)

                //Could have duplicate return rows here.
                //Group by the medication id and then so a first or default.
                //So that we only return each medication once.
                //Group by uqli.id so that we don't include the same item multiple times.
                //We can have the same medication in the list multiple times with different doses, etc...
                //So we cannot group by medication id.
                var returnCandidates = returnUqliItems.GroupBy(x => x.Id).Select(x => x.FirstOrDefault()).ToList();

                //Now create a dictionary with the counts for each letter.
                var returnCandidatesDistinct = returnCandidates.GroupBy(i => i.Medication.DisplayName.Substring(0, 1).ToUpper()).ToList();
                var returnDictionary = returnCandidatesDistinct.Select(i => new { name = i.Key, count = i.Count() }).ToList();

                //Retuen the dictionary.
                return returnDictionary.ToDictionary(s => s.name, s => s.count);
            } //end if (I/O/P = N?)
        }

        IEnumerable<UserQuickListItem> IOrderRepository.GetUserQuickListTabItems(string tab, BaseLinkResource resource)
        {
            //If all three of the filters are N (I/O/P), then proceed normally.
            //Else, join out to site_formulary_match (which we can do since we pre-calculate the match levels now)
            //and only return ones that are greater than or equal to the needed match level.
            //Winston Murdock, 03/26/2021.  EMAR-351

            //TODO: Move the vendor-specific logic to a vendor-specific controller once we start handling multiple vendors.
            //This joins out to fdb_brand_name, so it will need to reside in a vendor-specific repository.
            //We've already got IDrugDbRepository and DrugDbRepositoryFDB setup for vendor-specific things.
            //We'll need to add Multum, Medispan, etc... in the future.

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

            //Get the vendor and formulary Y/N settings from the DB.
            var drugDbVendor = _optionRepository.GetOption(resource.SiteId, OptionNames.DRUG_DB_VENDOR).ToUpper();
            var medInpat = _optionRepository.GetOptionBool(resource.SiteId, OptionNames.MEDINPAT, false);
            var medOutpat = _optionRepository.GetOptionBool(resource.SiteId, OptionNames.MEDOUTPAT, false);
            var medPyxis = _optionRepository.GetOptionBool(resource.SiteId, OptionNames.MEDPYXIS, false);
            var exactMatch = _optionRepository.GetOptionBool(resource.SiteId, OptionNames.MEDEXACTMATCH, false);

            // Figure out if we're doing exact match or not and set matchLevel accordingly.
            int matchLevel = exactMatch ? 3 : 1;

            //TODO: Eventually, we'll check the vendor.  For now, just assume we're on American FDB.
            //Also, this duplicates the brand name search logic of only applying the formulary filters
            //if the I/O/P site options are set.
            //Per Romel, we could have a site with I/O/P set to N but that still has entries in their formulary table.
            //So we would need to also see if there are any rows in the formulary table with I, O< or P set to Y
            //to deteremind whether or not to formulary filter.
            //This will be done after the Emerus go live.

            // If this site is not doing formulary filtering (I, O< and P are all N), then return the user's quick list as we are currently doing.
            // If one, or more, of them are Y, then we need to apply the filtering logic to it.
            if (!medInpat && !medOutpat && !medPyxis)
            {
                //Not doing any formulary filtering.
                //Return as normal.
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
                            .ThenInclude(i => i.FrequencyType)
                        .ToList();
            }
            else
            {
                //We are doing at least one formulary filter.
                //Need to join to site_formulary_match where we match on medication_id and site_id.
                //Then only grab the ones that are higher than that the match level.
                //This is the same query as above, except we include SiteFormularyMatchs.
                var uqliItems = _context.UserQuickListItems
                        .Where(whereExpression)
                        .Include(i => i.Medication)
                            .ThenInclude(m => m.MedicationDetails)
                                .ThenInclude(d => d.FdbBrandName)
                        .Include(i => i.Medication)
                            .ThenInclude(m => m.MedicationDetails)
                                .ThenInclude(md => md.MedicationUnit)
                        .Include(i => i.Medication)
                            .ThenInclude(m => m.SiteFormularyMatchs)
                        .Include(i => i.MedicationRoute)
                        .Include(i => i.MedicationUnit)
                        .Include(i => i.FrequencySchedule)
                            .ThenInclude(i => i.FrequencyType)
                        .ToList();

                //Now filter the list to only have items that match on site id.
                //Since Medication.SiteFormularyMatchs is a list, we cannot directly access the SiteId field.
                //Do .Any(lambda) to only return SiteFormularyMatch entities that match on the SiteId.
                var siteUqliItems = uqliItems
               .Where(s => s.Medication.SiteFormularyMatchs
                     .Any(x => x.SiteId == resource.SiteId)).ToList();


                var returnUqliItems = new List<UserQuickListItem>();

                //Now that we've filtered siteUqliItems to only have match rows that match on the siteId,
                //we need to apply the formulary filtering logic.
                //For each of I/O/P
                //If this one is on, add all rows that have that match value greater than or equal to the match level we calculated.
                //If exact match is enabled, then matchLevel will be 3.
                //Else, matchLevel will be 1.
                if (medInpat)
                {
                    //Add any where InpatientMatch is greater than or equal to match Level
                    returnUqliItems.AddRange(
                        siteUqliItems.Where(a => a.Medication.SiteFormularyMatchs
                            .Any(b => b.InpatientMatch >= matchLevel))
                        );
                } //end if (medInpat = Y?)

                if (medOutpat)
                {
                    //Add any where IutpatientMatch is greater than or equal to match Level
                    returnUqliItems.AddRange(
                        siteUqliItems.Where(a => a.Medication.SiteFormularyMatchs
                            .Any(b => b.OutpatientMatch >= matchLevel))
                        );
                } //end if (medOutpat = Y?)

                if (medPyxis)
                {
                    //Add any where PyxisMatch is greater than or equal to match Level
                    returnUqliItems.AddRange(
                        siteUqliItems.Where(a => a.Medication.SiteFormularyMatchs
                            .Any(b => b.PyxisMatch >= matchLevel))
                        );
                } //end if (medPyxis = Y?)

                //Could have duplicate return rows here.
                //Group by the medication id and then so a first or default.
                //So that we only return each medication once.
                //Group by uqli.id so that we don't include the same item multiple times.
                //We can have the same medication in the list multiple times with different doses, etc...
                //So we cannot group by medication id.
                return returnUqliItems.GroupBy(x => x.Id).Select(x => x.FirstOrDefault());
            } //end if (I/O/P all equal N?)
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

        public bool DeleteQuickListItem(int quickListItemId)
        {
            //Get the quick list item.
            //Since we've already got a method that does that, use it.
            var quickListItem = GetUserQuickListItem(quickListItemId);

            //Now call a helper function to delete the item from the DB context
            //and return true or false based on the success or failure of the delete operation.
            return DeleteQuickListItem(quickListItem);
        } //end DeleteQuickListItem

        private bool DeleteQuickListItem(UserQuickListItem quickListItem)
        {
            bool success = true;
            int i = 0;

            if (quickListItem != null)
            {
                try
                {
                    _context.UserQuickListItems.Remove(quickListItem);
                    i = _context.SaveChanges(true);

                }
                catch (Exception ex)
                {
                    i = 0;
                }
            }

            return success && i > 0;
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

        public IEnumerable<DepartmentPreferredListItem> GetDepartmentPreferredListByTab(string tab, BaseLinkResource resource, string departmentCode)
        {
            //Setup the filter.
            //Account for whether the tab is "#" or a letter,
            //whether we have a department code or not,
            //and whether we have a site id or not.
            //Thus, there are eight potential paths (2^3) here.
            Expression<Func<DepartmentPreferredListItem, bool>> whereExpression;

            if (tab == "#")
            {
                if (resource.SiteId == 0)
                {
                    if (!string.IsNullOrWhiteSpace(departmentCode))
                    {
                        whereExpression = i =>
                            !EF.Functions.Like(i.Medication.DisplayName, "[a-zA-Z]%")
                            && i.DepartmentCode == departmentCode;
                    }
                    else
                    {
                        whereExpression = i =>
                            !EF.Functions.Like(i.Medication.DisplayName, "[a-zA-Z]%");
                    } //end if
                }
                else
                {
                    if (!string.IsNullOrWhiteSpace(departmentCode))
                    {
                        whereExpression = i =>
                            i.SiteId == resource.SiteId
                            && !EF.Functions.Like(i.Medication.DisplayName, "[a-zA-Z]%")
                            && i.DepartmentCode == departmentCode;
                    }
                    else
                    {
                        whereExpression = i =>
                            i.SiteId == resource.SiteId
                            && !EF.Functions.Like(i.Medication.DisplayName, "[a-zA-Z]%");
                    } //end if
                }
            }
            else
            {
                if (resource.SiteId == 0)
                {
                    if (!string.IsNullOrWhiteSpace(departmentCode))
                    {
                        whereExpression = i =>
                            EF.Functions.Like(i.Medication.DisplayName, $"[{tab.ToLower()}{tab.ToUpper()}]%")
                            && i.DepartmentCode == departmentCode;
                    }
                    else
                    {
                        whereExpression = i =>
                            EF.Functions.Like(i.Medication.DisplayName, $"[{tab.ToLower()}{tab.ToUpper()}]%");
                    } //end if
                }
                else
                {
                    if (!string.IsNullOrWhiteSpace(departmentCode))
                    {
                        whereExpression = i =>
                            i.SiteId == resource.SiteId
                            && EF.Functions.Like(i.Medication.DisplayName, $"[{tab.ToLower()}{tab.ToUpper()}]%")
                            && i.DepartmentCode == departmentCode;
                    }
                    else
                    {
                        whereExpression = i =>
                            i.SiteId == resource.SiteId
                            && EF.Functions.Like(i.Medication.DisplayName, $"[{tab.ToLower()}{tab.ToUpper()}]%");
                    } //end if
                }
            }

            var items = _context.DepartmentPreferredListItems.Where(whereExpression)
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
        } //end GetDepartmentPreferredListTabItems

        public Dictionary<string, int> GetDepartmentPreferredListTabs(string departmentCode, BaseLinkResource resource)
        {
            //Build up the where clause.
            //Account for department code and site id.
            Expression<Func<DepartmentPreferredListItem, bool>> whereExpression;
            if (resource.SiteId == 0)
            {
                if (!string.IsNullOrWhiteSpace(departmentCode))
                {
                    whereExpression = i =>
                        i.DepartmentCode == departmentCode;
                }
                else
                {
                    whereExpression = null;
                } //end if
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(departmentCode))
                {
                    whereExpression = i =>
                        i.SiteId == resource.SiteId
                        && i.DepartmentCode == departmentCode;
                }
                else
                {
                    whereExpression = i =>
                        i.SiteId == resource.SiteId;

                } //end if
            }

            //Not doing any formulary filtering.
            //Return as normal.
            var stuff = _context.DepartmentPreferredListItems
            .Include(i => i.Medication)
            .Where(whereExpression)
            .GroupBy(i => i.Medication.DisplayName.Substring(0, 1).ToUpper())
            .Select(i => new { name = i.Key, count = i.Count() }).ToList();

            return stuff.ToDictionary(s => s.name, s => s.count);
        } //end GetDepartmentPreferredListTabs

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
            OrderInstruction,
            OverrideReason,
            AntimicrobialIndicationReason,
            Service,
            Formulary,
            VitalSign
        }

        private CodeShareEntity? GetCodeShareEnum(string val)
        {
            return val switch
            {
                "antimicrobial_indications" => CodeShareEntity.AntimicrobialIndicationReason,
                "formulary" => CodeShareEntity.Formulary,
                "frequency_schedules" => CodeShareEntity.FrequencySchedule,
                "interaction_overrides" => CodeShareEntity.OverrideReason,
                "medication_routes" => CodeShareEntity.MedicationRoute,
                "medication_units" => CodeShareEntity.MedicationUnit,
                "order_instructions" => CodeShareEntity.OrderInstruction,
                "services" => CodeShareEntity.Service,
                "vital_signs" => CodeShareEntity.VitalSign,
                _ => null,
            };
        }

        public List<CodeSharedId> GetCodeShareSites(int siteId)
        {
            //Go out to site_code_shares to find the site we're pulling each item (units, routes, schedules, etc...) from.
            //Grab the row where source_site_id = the site the user is on and the entity matches the one we want.
            //Then pull the target_site_id (which is the site we're pulilng things from).
            //Winston Murdockm 04/21/2021.  EMAR-811.
            var medicationUnitSiteId = _context.SiteCodeShares
                .FirstOrDefault(g => g.SourceSiteId == siteId && g.Entity == "medication_units")?
                .TargetSiteId;

            var medicationRouteSiteId = _context.SiteCodeShares
                .FirstOrDefault(g => g.SourceSiteId == siteId && g.Entity == "medication_routes")?
                .TargetSiteId;

            var frequencySiteId = _context.SiteCodeShares
                .FirstOrDefault(g => g.SourceSiteId == siteId && g.Entity == "frequency_schedules")?
                .TargetSiteId;

            var orderInstructionSiteId = _context.SiteCodeShares
                .FirstOrDefault(g => g.SourceSiteId == siteId && g.Entity == "order_instructions")?
                .TargetSiteId;

            //Adding override reasons, antimicrobial indications, and services.
            //Even though we don't have a ticket for them yet, adding formulary and vital signs too.
            //Whenever we're ready to start code sharing those, we'll already have this scaffolding setup.
            //Winston Murdock, 04/21/2021.  EMAR-811 and EMAR-812.
            var overrideReasonsSiteId = _context.SiteCodeShares
                .FirstOrDefault(g => g.SourceSiteId == siteId && g.Entity == "interaction_overrides")?
                .TargetSiteId;

            var antimicrobialIndicationsSiteId = _context.SiteCodeShares
                .FirstOrDefault(g => g.SourceSiteId == siteId && g.Entity == "antimicrobial_indications")?
                .TargetSiteId;

            var servicesSiteId = _context.SiteCodeShares
                .FirstOrDefault(g => g.SourceSiteId == siteId && g.Entity == "services")?
                .TargetSiteId;

            var formularySiteId = _context.SiteCodeShares
                .FirstOrDefault(g => g.SourceSiteId == siteId && g.Entity == "formulary")?
                .TargetSiteId;

            var vitalSignsSiteId = _context.SiteCodeShares
                .FirstOrDefault(g => g.SourceSiteId == siteId && g.Entity == "vital_signs")?
                .TargetSiteId;



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

            //Adding override reasons, antimicrobial indications, and services.
            //Even thoughw e don't have a ticket for them yet, adding formulary and vital signs too.
            //Whenever we're ready to start code sharing those, we'll already have this scaffolding setup.
            //Winston Murdock, 04/21/2021.  EMAR-811 and EMAR-812.
            if (overrideReasonsSiteId != null)
            {
                codeSharedIds.Add(
                    new CodeSharedId
                    {
                        SiteId = siteId,
                        Entity = CodeShareEntity.OverrideReason,
                        SharedSiteId = overrideReasonsSiteId.Value
                    });
            }

            if (antimicrobialIndicationsSiteId != null)
            {
                codeSharedIds.Add(
                    new CodeSharedId
                    {
                        SiteId = siteId,
                        Entity = CodeShareEntity.AntimicrobialIndicationReason,
                        SharedSiteId = antimicrobialIndicationsSiteId.Value
                    });
            }

            if (servicesSiteId != null)
            {
                codeSharedIds.Add(
                    new CodeSharedId
                    {
                        SiteId = siteId,
                        Entity = CodeShareEntity.Service,
                        SharedSiteId = servicesSiteId.Value
                    });
            }

            if (formularySiteId != null)
            {
                codeSharedIds.Add(
                    new CodeSharedId
                    {
                        SiteId = siteId,
                        Entity = CodeShareEntity.Formulary,
                        SharedSiteId = formularySiteId.Value
                    });
            }

            if (vitalSignsSiteId != null)
            {
                codeSharedIds.Add(
                    new CodeSharedId
                    {
                        SiteId = siteId,
                        Entity = CodeShareEntity.VitalSign,
                        SharedSiteId = vitalSignsSiteId.Value
                    });
            }

            return codeSharedIds;
        }

        /// <summary>
        /// Proposed replacement for the combination of GetCodeShareSites() and then querying the returned list in the client methods
        /// This will replace 9 un-cached DB hits with one cached hit per site
        /// </summary>
        /// <param name="sourceSiteId">The site you are working with</param>
        /// <param name="entity">The entity you want the code share site for</param>
        /// <returns>The site you should use for the codes for the passed entity</returns>
        public int GetCodeShareSites(int sourceSiteId, CodeShareEntity entity)
        {
            var codeShares = _cache.GetOrCreate($"{sourceSiteId}{CacheKeys.CodeShares}", entry =>
                        {
                            entry.SlidingExpiration = TimeSpan.FromMinutes(30);

                            var codeShares = _context.SiteCodeShares
                                    .Where(s => s.SourceSiteId == sourceSiteId)
                                    .ToList();

                            Dictionary<CodeShareEntity, int> ret = new Dictionary<CodeShareEntity, int>();
                            foreach (var codeShare in codeShares)
                            {
                                var cs = GetCodeShareEnum(codeShare.Entity);
                                if (cs != null)
                                    ret.Add((CodeShareEntity)cs, codeShare.TargetSiteId);
                            }

                            entry.Size = ret.Count;

                            return ret;
                        });

            return codeShares.ContainsKey(entity) ? codeShares[entity] : sourceSiteId;
        }

        private Expression<Func<Medication, bool>> GetSchedulerSetupDataWhereExpression(int siteId, Expression<Func<Medication, bool>> baseExpression, string drugId = null)
        {
            //Apply the formulary criteria to the list of medications that match the passed in name.
            //Prior to my changes, this was only seeing if the drug was in the formulary table and
            //was not looking at the match table at all.
            //Now we correctly check the site settings to see which of I/O/P we're using.
            //And we also check the exact match setting to see if pull ones and up or threes and up.
            //Then we craft our where clause using the appropriate criteria.
            //Winston Murdock, 06/03/2021.  EMAR-1026.

            //Get the drug vendor, and the formulary settings.
            var drugDbVendor = _optionRepository.GetOption(siteId, OptionNames.DRUG_DB_VENDOR).ToUpper();
            var medInpat = _optionRepository.GetOption(siteId, OptionNames.MEDINPAT).ToUpper() == "Y";
            var medOutpat = _optionRepository.GetOption(siteId, OptionNames.MEDOUTPAT).ToUpper() == "Y";
            var medPyxis = _optionRepository.GetOption(siteId, OptionNames.MEDPYXIS).ToUpper() == "Y";
            var exactMatch = _optionRepository.GetOptionBool(siteId, OptionNames.MEDEXACTMATCH, false);


            //If exact match is on, then the match level must be three or higher.
            //Else the match level must be 1 or higher.
            int matchLevel = exactMatch ? 3 : 1;

            Expression<Func<Medication, bool>> whereExpression = m =>
                m.DrugVendor == drugDbVendor;

            whereExpression = whereExpression.And(baseExpression);

            //Do not apply any formulary criteria for a combo med (i.e. drugId = "COMBO").
            if (drugId.ToUpper() != "COMBO")
            {
                //Not a combo med.  Apply the filtering criteria.

                //I messed up and did greater than match level instead of greater than or equals to match level.
                //Thusly, this would only pull in fours when exact match was on (which improperly excludes threes)
                //and would only pull in twos, threes, and fours when exact match is off (improperly excluding ones).
                //This has been fixed.
                //Thanks to Romel for catching this.
                //Winston Murdock, 06/23/2021.  EMAR-1026

                if (medInpat && medOutpat && medPyxis)
                {
                    //Grab where at least one of I, O, or P is greater than the match level.
                    whereExpression = whereExpression.And(m =>
                        m.SiteFormularyMatchs.Any(f =>
                            (f.InpatientMatch >= matchLevel || f.OutpatientMatch >= matchLevel || f.PyxisMatch >= matchLevel)
                            && f.SiteId == siteId));
                }
                else if (medInpat && medOutpat)
                {
                    //Grab wehre at least one of I or O is greater than the match level.
                    whereExpression = whereExpression.And(m =>
                        m.SiteFormularyMatchs.Any(f =>
                            (f.InpatientMatch >= matchLevel || f.OutpatientMatch >= matchLevel)
                            && f.SiteId == siteId));
                }
                else if (medInpat && medPyxis)
                {
                    //Grab where at least one of I or P is greater than the match level.
                    whereExpression = whereExpression.And(m =>
                        m.SiteFormularyMatchs.Any(f =>
                            (f.InpatientMatch >= matchLevel || f.PyxisMatch >= matchLevel)
                            && f.SiteId == siteId));
                }
                else if (medOutpat && medPyxis)
                {
                    //Grab where at least one of O or P is greater than the match level.
                    whereExpression = whereExpression.And(m =>
                        m.SiteFormularyMatchs.Any(f =>
                            (f.InpatientMatch >= matchLevel || f.OutpatientMatch >= matchLevel)
                            && f.SiteId == siteId));
                }
                else if (medInpat)
                {
                    //Grab where I is greater than the match level.
                    whereExpression = whereExpression.And(m =>
                        m.SiteFormularyMatchs.Any(f =>
                            (f.InpatientMatch >= matchLevel)
                            && f.SiteId == siteId));
                }
                else if (medOutpat)
                {
                    //Grab where O is greater than the match level.
                    whereExpression = whereExpression.And(m =>
                        m.SiteFormularyMatchs.Any(f =>
                            (f.OutpatientMatch >= matchLevel)
                            && f.SiteId == siteId));
                }
                else if (medPyxis)
                {
                    //Grab where P is greater than the match level.
                    whereExpression = whereExpression.And(m =>
                        m.SiteFormularyMatchs.Any(f =>
                            (f.PyxisMatch >= matchLevel)
                            && f.SiteId == siteId));
                } //end if

                //No else case here.
                //If all three are N, then we don't need to bother with the formulary filtering.
            } //end if (combo med?)

            return whereExpression;
        }

        public IEnumerable<Medication> GetSchedulerSetupData(int siteId, string brandName, bool bAll)
        {
            var query = new List<Medication>();

            //We don't want to do formulary calculations when this is a combo med.
            //So we need to go get the medication's type from the DB and then pass that along.
            //Use the brand name to get the medication object (join to medication details)
            //then pull the type.
            //Winston Murdock, 04/13/2021.
            var med = _context.Medications.FirstOrDefault(m => m.MedicationDetails.Any(md => md.BrandName == brandName));
            var drugId = med.DrugId;

            Expression<Func<Medication, bool>> whereExpression;

            //Only call the helper function to check the formulary if we are not doing an "all" lookup.
            //Winston Murdock, 06/02/2021.  EMAR-1044
            if (!bAll)
            {
                //bAll = false.
                //Apply formulary filtering to the list.
                whereExpression = GetSchedulerSetupDataWhereExpression(siteId,
                    m =>
                        m.MedicationDetails.Any(md => md.BrandName == brandName)
                        && m.DrugId.ToUpper() != "COMBO",
                    drugId);
            }
            else
            {
                //bAll = true.
                //Do not apply formulary filtering to the list.
                whereExpression = m =>
                    m.MedicationDetails.Any(md => md.BrandName == brandName)
                    && m.DrugId.ToUpper() != "COMBO";
            } //end if (bAll = false?)

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

            //We "should" always find a match on the brand name in the medication details table
            //since that is the field being shown in the brand name search.
            //But, just in case we ddn't find one, try to get a match on
            //the display name column in the medications table.
            if (!query.Any() || query.All(m => m == null))
            {
                //Only call the helper function to check the formulary if we are not doing an "all" lookup.
                //Winston Murdock, 06/02/2021.  EMAR-1044
                if (!bAll)
                {
                    //bAll = false.
                    //Apply formulary filtering to the list.
                    whereExpression = GetSchedulerSetupDataWhereExpression(siteId,
                    m =>
                        m.DisplayName == brandName,
                    //&& m.SiteId == siteId,
                    drugId);
                }
                else
                {
                    //bAll = true.
                    //Do not apply formulary filtering to the list.
                    whereExpression = m =>
                        m.DisplayName == brandName;
                    //&& m.SiteId == siteId;
                } //end if (bAll = false?)

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


            //Before we grab the NDC values, handle a null medication list.
            //Check that the count is greater than 0.
            if (query.Count() > 0)
            {
                //Now confirm that the first item is not null.
                if (query[0] != null)
                {
                    //Loop through the list of medications and get their rows in fdb_ndc_info.
                    //Only do this for FDB American for now.  Handle other drug vendors later.
                    //In that world, the lists for the vendors we're not using will be null
                    //and will map to a null list in the DTO.
                    //Winston Murdock, 05/13/2021.  EMAR-932.

                    //Per conversation with Romel, we only want to return NDCs
                    //that are in formulary when exact match is on.
                    //When it's off (or we're doing an all/non-formulary search),
                    //we'll return all NDCs.
                    //Here's how I handled this here.
                    //When exact match is on, we are already filtering to only pull
                    //medications that have a 3 or 4 as the match level.
                    //Below, we will only pull rows from FdbNdcInfo where the BaseNdc
                    //matches the NDC in the Siteformulary for this medication.
                    //When exact match is off, we are already filtering to only pull
                    //medications that have a 1, 2, 3, or 4 as the match level.
                    //Below, we will pull all matching rows from FdbNdcInfo for this medication.
                    //When we're doing an "all" search (not searching the formulary),
                    //we aren't doing any filtering based on the match level.
                    //Below, we will pull all matching rows from FdbNdcInfo for this medication.
                    //Winston Murdock, 94/21/2022.  PC-27188
                    string vendor = _optionRepository.GetOption(siteId, OptionNames.DRUG_DB_VENDOR);
                    if (vendor == "F")
                    {
                        //FDB American
                        foreach (Medication medication in query)
                        {
                            //If we're diong an "all" search, then get all matches.
                            //If we're doing a "formulary" search, then we need to
                            //check the Exact Match setting to see if we pull all
                            //matches or only the ones where the BaseNdc is an exact
                            //match to the SiteFormulary.
                            if (bAll)
                            {
                                //All

                                //Get the FDB NDC Info for each one.
                                //Where we match on the medid
                                //and where the base ndc = the ndc.
                                medication.FdbNdcInfos =
                                (
                                    //Should've named this FdbNdcInfos, but we didn't.
                                    //Other places already use this, so we'll leave it.
                                    from row in _context.FdbNdcInfo
                                    where row.MedidString == medication.DrugId
                                    && row.BaseNdc == row.Ndc
                                    select row
                                ).ToList();
                            }
                            else
                            {
                                //Formulary

                                //If exact match is Y, then only get 3/4 and where the NDC is in the site_formulary table.
                                //Else, exact match is N.  Get 1/2/3/4 and don't check the NDC being in the site_formulary table.
                                //We're already doing the filtering for 1/2/3/4 or 3/4 elsewhere.
                                //This part merely needs to delineate exat match from not exact match.

                                var exactMatch = _optionRepository.GetOptionBool(siteId, OptionNames.MEDEXACTMATCH);

                                if (exactMatch)
                                {
                                    //Exact match (3 or 4).

                                    //Get the FDB NDC Info for each one.
                                    //Where we match on the medid
                                    //and where the base ndc = the ndc
                                    //and where the base ndc is in the site_formulary table
                                    //for this site.
                                    medication.FdbNdcInfos =
                                    (
                                        //Should've named this FdbNdcInfos, but we didn't.
                                        //Other places already use this, so we'll leave it.
                                        from row in _context.FdbNdcInfo
                                        join sf in _context.SiteFormulary on row.Ndc equals sf.Ndc
                                        where row.MedidString == medication.DrugId
                                        && row.BaseNdc == row.Ndc
                                        && sf.SiteId == siteId
                                        select row
                                    ).ToList();
                                }
                                else
                                {
                                    //Close match (1, 2, 3, or 4).

                                    //Get the FDB NDC Info for each one.
                                    //Where we match on the medid
                                    //and where the base ndc = the ndc.
                                    medication.FdbNdcInfos =
                                    (
                                        //Should've named this FdbNdcInfos, but we didn't.
                                        //Other places already use this, so we'll leave it.
                                        from row in _context.FdbNdcInfo
                                        where row.MedidString == medication.DrugId
                                        && row.BaseNdc == row.Ndc
                                        select row
                                    ).ToList();
                                }

                                //Now filter out the rows where the NDC is not in the site_formulary table.
                                //Get the list of all site formulary entries for this site.
                                //Might want to move this one line higher up so that we only do it once and not for each order.
                                //var siteFormularyList = _context.SiteFormulary.Where(sf => sf.SiteId == siteId).Select(sf => sf.Ndc).ToList();

                                //medication.FdbNdcInfos = medication.FdbNdcInfos.Where(fni => siteFormularyList.Contains(fni.BaseNdc)).ToList();
                            } //end if
                        } //end foreach medication.
                    } //end if (vendor = "F"?)
                } //end if (item 0 is null?)
            } //end if (count > 0?)

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
                //TODO: Confirm this works.
                //Winston Murdock EMAR-263/EMAR-545.
                EmarOrderType.PatientOrder => _context.PatientOrders.Find((long)itemId).MedicationId,
                _ => -1
            };

            if (medicationId == -1)
            {
                return null;
            }

            var query = new List<Medication>();

            //We don't want to do formulary calculations when this is a combo med.
            //So we need to go get the medication's type from the DB and then pass that along.
            //Use the id to get the medication object then pull the drug_id.
            //Winston Murdock, 04/13/2021.
            var med = _context.Medications.Find(medicationId);
            var drugId = med.DrugId;

            //Because this is a remembered item,        
            //do not apply formulary filtering to the list.
            //Just grab where the medication id matches, and
            //it's not a combo med.
            //Winston Murdock, 08/16/2021.  EMAR-1154
            //var whereExpression = GetSchedulerSetupDataWhereExpression(siteId,
            //    m =>
            //        m.Id == medicationId
            //        && m.SiteId.ToString().ToUpper() != "COMBO",
            //    drugId);
            Expression<Func<Medication, bool>> whereExpression;
            whereExpression = m =>
                m.Id == medicationId
                && m.DrugId.ToUpper() != "COMBO";
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
                        && m.SiteId == siteId,
                    drugId);

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

            //Grab the Ndc from the source table.
            //MedicationItem could have one to many NDC values.
            //Since we can't know which one to use without the user picking one, I don't know how we could handle this here.
            //Hsi-An says the UI is never getting to this by passing in MedicationItem, so I've left that as null for now.
            //That may need to change in the future.  In that case, we would need some way for the user to tell
            //us which NDC they want to use for this medication.
            //Winston Murdock, 05/14/2021.  EMAR-932.
            var Ndc = itemType switch
            {
                EmarOrderType.UserQuickListItem => _context.UserQuickListItems.Find(itemId).Ndc,
                EmarOrderType.DepartmentPreferredListItem => _context.DepartmentPreferredListItems.Find(itemId).Ndc,
                EmarOrderType.GroupRememberedOrder => _context.GroupListItems.Find(itemId).Ndc,
                EmarOrderType.PatientCartOrder => _context.PatientCartOrders.Find((long)itemId).Ndc,
                //We would need to know which NDC to grab for a medication.

                EmarOrderType.MedicationItem => null,
                //TODO: Confirm this works.
                //Winston Murdock EMAR-263/EMAR-545.
                EmarOrderType.PatientOrder => _context.PatientOrders.Find((long)itemId).Ndc,
                _ => null
            };

            //Only do this for American FDB right now.  We'll cover other vendors later.
            string vendor = _optionRepository.GetOption(siteId, OptionNames.DRUG_DB_VENDOR);
            if (vendor == "F")
            {
                //Now that we've got the NDC value, go get one row from fdb_ndc_info for it.
                //Where base_ndc = ndc and where base_ndc = ndc.
                //Then set the FdbNdcInfo's for each medication (there should only be one, but it's still a list),
                //to the row form the NDC table.
                if (Ndc != null)
                {
                    //We have an NDC value.
                    foreach (Medication medication in query)
                    {
                        //Get the FDB NDC Info for each one.
                        //Where we match on the NDC
                        //and where the base ndc = the ndc.
                        medication.FdbNdcInfos =
                        (
                            //Should've named this FdbNdcInfos, but we didn't.
                            //Other places already use this, so we'll leave it.
                            from row in _context.FdbNdcInfo
                            where row.BaseNdc == Ndc
                            && row.BaseNdc == row.Ndc
                            select row
                        ).ToList();
                    } //end if
                } //end if (ndc is not null)
            } //end if

            //If this is a user quick iist item, get the duration unit child.
            //The above is getting the user quick list item (though I'm not sure how).
            //But it's not getting the DurationUnit chld.  So I'll need to manually do that here.
            //We might need similar cases for Department Preferered List and Groups.
            //Winston Murdock, 08/20/2021.  EMAR-1162.
            if (itemType == EmarOrderType.UserQuickListItem)
            {
                int? durationUnitId = query[0].UserQuickListItems.ToList()[0].DurationUnitId;
                if (durationUnitId.HasValue)
                {
                    var uqliItems = query[0].UserQuickListItems.ToList();
                    uqliItems[0].DurationUnit =
                    (
                        from row in _context.DurationUnits
                        where row.Id == Convert.ToInt32(durationUnitId)
                        select row
                    ).FirstOrDefault();
                } //end if
            } //end if
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

        public List<FrequencyScheduleAdministration> GetSchedulerAdministrations(int siteId, EmarOrderType itemType, int itemId, DateTimeOffset start, DateTimeOffset? stop, int? duration = null, int? durationUnitId = null)
        {
            var frequencyId = itemType switch
            {
                EmarOrderType.UserQuickListItem => _context.UserQuickListItems.Find(itemId).FrequencyScheduleId,
                EmarOrderType.DepartmentPreferredListItem => _context.DepartmentPreferredListItems.Find(itemId).FrequencyScheduleId,
                EmarOrderType.GroupRememberedOrder => _context.GroupListItems.Find(itemId).FrequencyScheduleId,
                //Have to convert the order's ID to a long rather than leaving it as an int.
                EmarOrderType.PatientOrder => _context.PatientOrders.Find(Convert.ToInt64(itemId)).FrequencyScheduleId,
                _ => null
            };

            //We need to pass the duration number and duration name as parameters here so that we can
            //pass them down to the SP.
            //Winston Murdock, 08/20/2021.  EMAR-1162.
            var ret = frequencyId == null
                ? null
                : GetNewAdministrations(siteId, frequencyId.Value, _context.Sites.Find(siteId).TimeZoneName.NowWithTimeZoneOffset(), stop, duration, durationUnitId)
                    .ToList();

            return ret;
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

        public List<FrequencyScheduleAdministration> GetNewAdministrations(int siteId, int frequencyId, DateTimeOffset start, DateTimeOffset? stop, int? duration = null, int? durationUnitId = null)
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

            //If we don't ahve a duration and we don't have a stop time, then calculate a stop time.
            //Else, don't.
            //winston Murdock, 08/25/2021.  EMAR-1164.
            if ((!duration.HasValue) && (!stop.HasValue))
            {
                //We want to calculate the stop date as n number of days in the future from now in the site's time zone
                //rather than now in the client's time zone.
                //Winston Murdock, 07/07/2021.  No ticket.
                //stop ??= DateTimeOffset.Now.AddDays(int.TryParse(_optionRepository.GetOption(siteId, OptionNames.SCHEDULE_FUTURE_ITEMS), out int number) ? number : 7);
                var siteNow = _siteRepository.GetSiteTimeZone(siteId).NowWithTimeZoneOffset();
                stop ??= siteNow.AddDays(int.TryParse(_optionRepository.GetOption(siteId, OptionNames.SCHEDULE_FUTURE_ITEMS), out int number) ? number : 7);
            } //end if

            //Add the site id as a parameter to this SQL call.
            //The SQL call was using the frequency's ID to get the site's ID to then get the time zone and offset.
            //This resulted in the future administrations being offset by the difference between
            //the "from" site and the "to" site.
            //By passing in the site ID we are logged into, we can use that
            //to pull the time zone and offset.
            //Winston Murdock, 07/07/2021

            // I need the duration number and duration name (24 hours, 2 days, etc...) to be passed into this SP.
            //The user can specify 24 hours, but we still generate seven days (or whatever the setting for
            //max days is) worth of adminstrations
            //Winston Murdock, 08/20/2021.  EMAR-1162.
            administrations = _context.FrequencyScheduleAdministrations
                .FromSqlInterpolated(
                    $"EXEC [dbo].[get_frequency_schedule_items] {frequencyId}, {start}, {stop}, {siteId}, {duration}, {durationUnitId}")
                .ToList();

            return administrations;
        }

        #endregion
    }
}