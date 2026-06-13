using Emar.Core.Carts.Model;
using Emar.Core.Helpers;
using Emar.Core.Options.Model;
using Emar.Core.Options.Repository;
using Emar.Core.Orders.Model;
using Emar.Core.Orders.Model.Mappings;
using Emar.Core.OutboundChart.Model;
using Emar.Core.OutboundChart.Service;
using Emar.Core.OutboundData.Model;
using Emar.Core.OutboundData.Service;
using Emar.Core.ResourceParameters;
using Emar.Core.Sites.Repository;
using Emar.Data;
using Emar.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Diagnostics;

namespace Emar.Core.Carts.Repository
{
    public class CartOrderRepository : ICartOrderRepository
    {
        private readonly EmarContext _context;
        private readonly IPropertyMappingService _propertyMappingService;
        private readonly IOdsEmarOutboundService _odsEmarOutboundService;
        private readonly IOcsEmarOutboundService _ocsEmarOutboundService;
        private readonly MemoryCache _cache;
        private readonly ISiteRepository _siteRepository;
        private readonly IOptionRepository _optionRepository;

        public CartOrderRepository(EmarContext emarContext, IPropertyMappingService propertyMappingService, EmarMemoryCache cache, IOdsEmarOutboundService odsEmarOutboundService,
                                   IOcsEmarOutboundService ocsEmarOutboundService, ISiteRepository siteRepository, IOptionRepository optionRepository)
        {
            _context = emarContext ?? throw new ArgumentNullException(nameof(emarContext));
            _propertyMappingService = propertyMappingService ?? throw new ArgumentNullException(nameof(propertyMappingService));
			_odsEmarOutboundService = odsEmarOutboundService ?? throw new ArgumentNullException(nameof(odsEmarOutboundService));
            _ocsEmarOutboundService = ocsEmarOutboundService ?? throw new ArgumentNullException(nameof(ocsEmarOutboundService));
            _cache = cache.Cache;
            _siteRepository = siteRepository;
            _optionRepository = optionRepository;
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
            var ret = _context.PatientCartOrders
                .Include(order => order.AntimicrobialIndication)
                .Include(order => order.PatientProblem)
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

            //We need to only pull in rows from the allergy reaction view where
            //the order id matches and where the order table is "patient_cart_orders"
            //The EF join is only setup on the order_id which can go to patient_cart_orders or patient_orders.
            //When I tried merely adding a filter to only pull allergy reaction view rows with "patient_cart_orders" as the table,
            //orders that do not have any reactions were not returned (when we do want them to be returned).
            //Perhaps there's a way to setup an Entity Framework join using two column instead of one.
            //But I haven't found one on Google yet.
            //So we're going to manually set the AllergyReactionsView child of each patient cart order here.
            //This merely queries the data we already grabbed from the DB and does not require any extra DB hits.
            //Winston Murdockm 07/12/2021.  EMAR-1059.

            //I tried crafting a lambda/where to do this, but I couldn't craft it perfectly.
            //I can get patient cart orders where there are "patient_cart_orders" rows in the view
            //or where there is nothing in the view.
            //But I cannot get it to pull patient cart orders where there are rows in the view
            //but they are "patient_orders" rows.
            //A patient will never have a ton of cart orders for a given user, so this
            //shouldn't be an "expensive" operation.
            //Winston Murdock, 07/13/2021.  EMAR-1059.

            //Check that the count is greater than 0.
            //Don't try this if the query to the DB didn't return anything.
            if (ret.Count() > 0)
            {
                //Filter the returned allergy reactions views rows to only
                //include rows that are "patient_cart_orders" rows.
                //Since the order_id column could either join to
                //patient_cart_orders or patient_orders, this
                //prevents us from pulling in "patient_orders" rows here.
                //Winston Murdock, 07/12/2021.  EMAR-1059.
                foreach (PatientCartOrder patientCartOrder in ret)
                {
                    patientCartOrder.AllergyReactionsView =
                    (
                        from row in patientCartOrder.AllergyReactionsView
                        where row.OrderTable == "patient_cart_orders"
                        select row
                    ).ToList();

                    //For each cart order, filter the list of interactions to
                    //only have the ones where drug_num is 1.
                    //We did the same thing for patient orders a few weeks ago.
                    //Winston Murdock, 04/14/2022.  PC-27058
                    //See if we have any OrderInteractions.
                    if (patientCartOrder.OrderInteractions.Any())
                    {
                        //The order_interactions table has two rows for each interactions.
                        //If this cart order is Tylenol, and it interacts with a Warfarin cart order
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
                        patientCartOrder.OrderInteractions = patientCartOrder.OrderInteractions.Where(oi => oi.DrugNum == 1).ToList();
                    } //end if (any OrderInteractions?)
                } //end foreach cart order.
            } //end if (count > 0?)


            return ret;
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
                //Log any error.
                using (EventLog eventLog = new EventLog("Application"))
                {
                    string sException = ex.Message + "\n";
                    sException += "inner exception = " + ex.InnerException + "\n";
                    sException += "source = " + ex.Source + "\n";
                    sException += ex.StackTrace + "\n";

                    eventLog.Source = "PulseCheck EMAR API";
                    eventLog.WriteEntry(sException, EventLogEntryType.Error, 101, 1);
                } //end using.

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
                    //Grab all of the OrderInteractions for this patient's cart orders.
                    //Where PatientCartOrderId.hasValue filters out any interactions
                    //for PatientOrders and leaves only interactions for
                    //PatientCartOrders.
                    //Then where PatientId matches gives us only the ones for this
                    //CartOrder's Patient.
                    //The last thing the service method does is recalculating the
                    //interactions for all cart orders for this patient.
                    //So it's fine to delete them all now since we will recalculate
                    //them just after this.
                    //This function is called when deleting one CartOrder.
                    //And it is called multiple times when deleting all CartOrders
                    //in the cart for a specified UserId and PatientId.
                    //Deleting the first cart order will delete all of these.
                    //And each other time through here will get zero interactions.
                    //Thus, they won't fall into that loop.
                    //Winston Murdock, 04/15/2022.  PC-27058

                    //Per Romel, we only want a cart order to show interactions to
                    //other cart orders in the same user's cart.
                    //If I have a drug in my cart for a patient that interacts
                    //with a drug in Romel's cart for the same patient, we do not
                    //want to show that interaction.
                    //Thus, we should only delete the order interactions for this
                    //orders in this patient's/user's cart.
                    //Winston Murdock, 04/18/2022.  PC-27058
                    var patientOrderInteractions =
                        (
                            from oi in _context.OrderInteractions
                            join pco in _context.PatientCartOrders on oi.PatientCartOrderId equals pco.Id
                            where oi.PatientCartOrderId.HasValue
                            && pco.PatientId == cartOrder.PatientId
                            && pco.UserId == cartOrder.UserId
                            select oi
                        ).ToList();

                    var medicationInteractions = new Collection<MedicationInteraction>();

                    //foreach (var orderInteraction in cartOrder.OrderInteractions)
                    foreach (var orderInteraction in patientOrderInteractions)
                    {
                        if (orderInteraction.MedicationInteraction != null)
                        {
                            medicationInteractions.Add(orderInteraction.MedicationInteraction);
                        }
                    }

                    //if (cartOrder.OrderInteractions != null)
                    if (patientOrderInteractions != null)
                    {
                        try
                        {
                            //foreach (var interaction in cartOrder.OrderInteractions)
                            foreach (var interaction in patientOrderInteractions)
                            {
                                //var orderInteractions = _context.OrderInteractions
                                //    .Where(x => x.MedicationInteractionId == interaction.MedicationInteractionId);

                                //_context.OrderInteractions.RemoveRange(orderInteractions);

                                //Remove this OrderInteraction/
                                _context.OrderInteractions.Remove(interaction);
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
                .Include(order => order.OrderInteractions)
                    .ThenInclude(interaction => interaction.DrugInteractionView)
                .Include(order => order.AllergyReactionsView)
                .Include(order => order.OrderReactions)
                    .ThenInclude(reaction => reaction.PatientAllergy)
                .ToList();

            if (!cartOrders.Any())
            {
                return false;
            }

            try
            {
                var secs = 0;
                List<OdsPatientOrderParameters> odsPOPList = new List<OdsPatientOrderParameters>();
                List<OcsChartParameters> ocsCPList = new List<OcsChartParameters>();
                var addDatetime = cartOrders.First().Patient.Site.TimeZoneName.NowWithTimeZoneOffset();
                var siteId = cartOrders.First().Patient.SiteId;
                var orderingPhysicianUserId = int.TryParse(cartPreCheckoutResponseData.OrderingPhysicianUserId, out int number)
                                                  ? number
                                                  : (int?)null;

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

                //Moved this out of the loop through orders to here.
                //This way we don't calculate it each time for each order.
                //Winston Murdock, 01/12/2022.  PC-26905.
                bool needPharmVerif = CalculateIfPatientNeedsPharmVerifOnOrders(cartOrders[0].Patient);

                foreach (var cartOrder in cartOrders)
                {
                    if (cartOrder.UserQuickListItemId != null)
                    {
                        _context.UserQuickListItems.Find(cartOrder.UserQuickListItemId).UsagesThisWeek += 1;
                    }

                    var losecs = addDatetime.AddSeconds(secs);
                    _context.PatientOrders.Add(OrderMapper.MapCartOrderToOrder(
                        cartOrder,
                        losecs,
                        orderingPhysicianUserId));

                    //This block determines if this order will get an Rx icon or not.
                    //If the logic in my CreateNotifications method is changed, this logic needs to change to match it.
                    //Need to check the "ERHOLD" indicator status.  That flags a patient as being an "inpatient" patient.
                    //Need to remove the "ER" patient checks on count and priority.  We no longer generate pharmacy notifications for "ER" patients.
                    //Winston Murdock.  PC-26905.
                    //bool needPharmVerif = cartOrder.Patient.DispositionTypeCode.ToUpper() == "INP" || cartOrder.Patient.DispositionTypeCode.ToUpper() == "INPT"
                    //                   || cartOrder.Patient.DispositionTypeCode.ToUpper() == "OBS" || cartOrder.CartOrderAdministrations.Count() > 1
                    //                   || cartOrder.Priority == 4;

                    //bool needPharmVerif = cartOrder.Patient.DispositionTypeCode.ToUpper() == "INP" || cartOrder.Patient.DispositionTypeCode.ToUpper() == "INPT"
                    //|| cartOrder.Patient.DispositionTypeCode.ToUpper() == "OBS" || CheckPatientIndicators(cartOrder.PatientId) == 1;

                    //Moving this outside of the order-specific section since this isn't based on anything about the order
                    //but is only based on things about the patient.
                    //Winston Murdock, 01/12/2022.  PC-26905.
                    //bool needPharmVerif = CalculateIfPatientNeedsPharmVerifOnOrders(cartOrder.Patient);

                    // ODS filing data for SendNewPatientOrder
                    var odsOrder = new OdsPatientOrderParameters
                    {
                        PatientId = cartOrder.PatientId,
                        Losecs = DateTimeOffset.Parse(losecs.ToString("yyyy-MM-dd HH:mm:ss zz")),
                        AddUserId = cartOrder.UserId,
                        OrderingPhysicianId = orderingPhysicianUserId ?? 0,
                        SiteId = siteId,
                        Dose = cartOrder.Dose.ToString(),
                        MedNotes = cartOrder.OrderNotes,
                        AmIndication = cartOrder.AntimicrobialIndicationId.ToString(),
                        OrderDate = cartOrder.AddDatetime,
                        Route = cartOrder.MedicationRouteId.ToString(),
                        Unit = cartOrder.MedicationUnitId.ToString(),
                        MedicationId = cartOrder.MedicationId,
                        Prn = cartOrder.Prn,
                        Administrations = cartOrder.CartOrderAdministrations,
                        PharmVerificationReq = needPharmVerif,
                        Ndc = cartOrder.Ndc,
                    };
                    odsPOPList.Add(odsOrder);

                    // OCS filing data for SendChartLinesAsync
                    var ocsParams = new OcsChartParameters
                    {
                        patiendId = (int)cartOrder.PatientId,
                        losecs = DateTimeOffset.Parse(losecs.ToString("yyyy-MM-dd HH:mm:ss zz")),
                        user = cartOrder.UserId,
                        orderingPhysicianId = orderingPhysicianUserId ?? 0,
                        site = (byte)siteId,
                        medicationId = cartOrder.MedicationId,
                        userQuicklistOrder = cartOrder.UserQuickListItemId != null ? true : false,
                        medNotes = cartOrder.OrderNotes,
                        orderInteractions = cartOrder.OrderInteractions,
                        orderReactions = cartOrder.OrderReactions,
                        allergyReactionView = cartOrder.AllergyReactionsView,
                        Dose = cartOrder.Dose.ToString(),
                        Route = cartOrder.MedicationRouteId.ToString(),
                        Unit = cartOrder.MedicationUnitId.ToString(),
                        FrequencyId = (int)cartOrder.FrequencyScheduleId,
                        Duration = cartOrder.Duration.ToString(),
                        DurationId = cartOrder.DurationUnitId,
                        patientCartOrderId = cartOrder.Id,
                        OrderDate = cartOrder.AddDatetime.ToString("yyyyMMddHHmmss"),
                        PharmVerifStatus = needPharmVerif ? (byte)1 : (byte)0,
                        PRNIndication = cartOrder.PrnIndication,
                        Ndc = cartOrder.Ndc,
                        //I commented these lines out since we'll populate these two fields below.
                        //I save a DB hit by doing it my way, and I avoid a clunky, nested, ternary
                        //operator that would've been rather hard to read by doing so.
                        //Winston Murdock, 04/11/2022.  PC-27140
                        //AntiMicrobialIndication = cartOrder.AntimicrobialIndication != null ? cartOrder.AntimicrobialIndication.Code : "",
                        //AntiMicrobialIndicationText = cartOrder.AntimicrobialIndication != null ? cartOrder.AntimicrobialIndication.Description : ""
                    };

                    //Set AntiMicrobialIndication and AntiMicrobialIndicationText below so that
                    //it's easier to read than using nested ternary opterators above.
                    //This also lets us only query the AntimicrobialIndicaion table once.
                    //Winston Murdock, 04/11/2022.  PC-27140

                    //See if we have an Indication Id.
                    if (cartOrder.AntimicrobialIndicationId.HasValue)
                    {
                        //Get the AntimicrobialIndication entity.
                        var amIndication = _context.AntimicrobialIndications.Find(cartOrder.AntimicrobialIndicationId);

                        //If we have a Code, use it.  Else, use empty string.
                        if (amIndication.Code.Length > 0)
                        {
                            //Use the Code.
                            ocsParams.AntiMicrobialIndication = amIndication.Code;
                        }
                        else
                        {
                            //No Code.  Use empty string.
                            ocsParams.AntiMicrobialIndication = "";
                        } //end if (indication Code length > 0?)

                        //If we have a Description, use it.  Else, use empty string.
                        if (amIndication.Description.Length > 0)
                        {
                            //Use the Description.
                            ocsParams.AntiMicrobialIndicationText = amIndication.Description;
                        }
                        else
                        {
                            //No Description.  Use empty string.
                            ocsParams.AntiMicrobialIndicationText = "";
                        } //end if (indication Descripotion length > 0?)
                    }
                    else
                    {
                        //No indication ID.
                        //Use empty string for the code.
                        ocsParams.AntiMicrobialIndication = "";

                        //If we have a value for Text, then use it.
                        //Else, use empty string.
                        if (cartOrder.AntimicrobialIndicationText != null)
                        {
                            //Text is not null, check the length.
                            if (cartOrder.AntimicrobialIndicationText.Length > 0)
                            {
                                //Use Text.
                                ocsParams.AntiMicrobialIndicationText = cartOrder.AntimicrobialIndicationText;
                            }
                            else
                            {
                                //No Text.  Use empty string.
                                ocsParams.AntiMicrobialIndicationText = "";
                            }
                        }
                        else
                        {
                            //No Text.  Use empty string.
                            ocsParams.AntiMicrobialIndicationText = "";
                        } //end if
                    } //end if (indication ID has a value?)

                    ocsCPList.Add(ocsParams);

                    //Moving the clean up cart order logic (which does need to happen)
                    //to be after the logic that writes stuff back to PulseCheck.
                    //This will necessitate an extra DB hit, but it should ensure
                    //that we have the interactions and reaction rationalia when we're
                    //writing back to PCED.
                    //See the CleanUpCartOrders method below.
                    //Winston Murdock, 03/04/2022.  PC=27035

                    secs++;
                } //foreach cart order

                var emarTransaction = _context.Database.BeginTransaction();
//                var ibexTransaction = IbexContext.Database.BeginTransaction();
//                var chartingTransaction = ChartingContext.Database.BeginTransactionAsync();
                try
                {
                    i = _context.SaveChanges(true);

                    // call GetPatientOrderIds to get the just inserted patient_order_id value(s)
                    odsPOPList = GetPatientOrderIds(odsPOPList);
                    // send the patient order to be filed with the ODS
                    _odsEmarOutboundService.SendNewPatientOrder(odsPOPList);

                    // send the chart lines to be filed with the OCS
                    var ocsReturn = "";
                    var j = 0;
                    foreach (var ocsCPItem in ocsCPList)
                    {
                        ocsCPItem.patientOrderId = odsPOPList[j].PatientOrderId;
                        ocsReturn = _ocsEmarOutboundService.SendChartLinesAsync(ocsCPItem);
                        if (!string.IsNullOrEmpty(ocsReturn)) break;
                        j++;
                    }

                    if (string.IsNullOrEmpty(ocsReturn))
                    {
                        emarTransaction.Commit();

                        //Create a notification for the orders in this cart.
                        CreateNotifications(odsPOPList);

                        //Delete the cart orders (since we've moved them to patient orders)
                        //and all child info (reactions, administrations, etc...).
                        CleanUpCartOrders(cartOrders);
                    }
                    else
                    {
                        emarTransaction.Rollback();
                    }
//                    emarTransaction.Commit();
//                    ibexTransaction.Commit();
//                    chartingTransaction.CommitAsync();
                }
                catch (Exception e)
                {
                    using (EventLog eventLog = new EventLog("Application"))
                    {
                        string sException = e.Message + "\n";
                        sException += "inner exception = " + e.InnerException + "\n";
                        sException += "source = " + e.Source + "\n";
                        sException += e.StackTrace + "\n";

                        eventLog.Source = "PulseCheck EMAR API";
                        eventLog.WriteEntry(sException, EventLogEntryType.Error, 101, 1);
                    } //end using.

                    emarTransaction.Rollback();
//                    ibexTransaction.Rollback();
//                    chartingTransaction.RollbackAsync();
                    throw ;
                }

//                i = _context.SaveChanges(true);
//                // call GetPatientOrderIds to get the just inserted patient_order_id value(s)
//                odsPOPList = GetPatientOrderIds(odsPOPList);
//                // send the patient order to be filed with the ODS
//                _odsEmarOutboundService.SendNewPatientOrder(odsPOPList);
//                // send the chart lines to be filed with the OCS
//                var result = _ocsEmarOutboundService.SendChartLinesAsync(ocsParams);
            }
            catch (Exception ex)
            {
                using (EventLog eventLog = new EventLog("Application"))
                {
                    string sException = ex.Message + "\n";
                    sException += "inner exception = " + ex.InnerException + "\n";
                    sException += "source = " + ex.Source + "\n";
                    sException += ex.StackTrace + "\n";

                    eventLog.Source = "PulseCheck EMAR API";
                    eventLog.WriteEntry(sException, EventLogEntryType.Error, 101, 1);
                } //end using.

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
            //Getting this error on Emerus Prod.
            //Unable to cast object of type 'System.Collections.Generic.List`1[Emar.Data.Entities.OrderInstruction]'
            //to type 'Emar.Data.Entities.FrequencySchedule'.
            //
            //This was using "OrderInstructions" instead of "FrequencySchedules" as the name of the cache entry.
            //For Emerus Prod, the "Q24H" frequency has an ID of 8.
            //And there is an order instruction with ID of 8.
            //so if order instruction 8 has already been cached by the API,
            //then grabbing the "8OrderInstruction" cache entry would pull that up.
            //This would result in trying to shoehorn an Order Instruction into Frequency Schedule
            //object and hit the error above.
            //The fix is to use FrequencySchedules as the name of the cache entry here.
            //Winston Murdock, 09/07/2021.  EMAR-1137.
            //return _cache.GetOrCreate(frequencyId + CacheKeys.FrequencySchedules, entry =>
            //{
            //    entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30);

            //    var ret = _context.FrequencySchedules
            //        .Include(f => f.FrequencyType)
            //        .FirstOrDefault(f => f.Id == frequencyId);

            //    entry.Size = 1;

            //    return ret;
            //});

            //Get the frequency without looking in the cache first.
            //Looking in the cache was giving an error for Daily on Emeurs prod (id = 7)
            //System.InvalidCastException: Unable to cast object of type 'System.Collections.Generic.List`1[Emar.Data.Entities.FrequencySchedule]' to type 'Emar.Data.Entities.FrequencySchedule'.
            //Winston Murdock, 12/16/2021.  No Ticket
            var ret = _context.FrequencySchedules
                    .Include(f => f.FrequencyType)
                    .FirstOrDefault(f => f.Id == frequencyId);
            return ret;
        }

        public List<OdsPatientOrderParameters> GetPatientOrderIds(List<OdsPatientOrderParameters> odsPOPList)
        {
            foreach (OdsPatientOrderParameters odsPop in odsPOPList)
            {
                var patientOrders = _context.PatientOrders
                                            .Where(x => x.PatientId == odsPop.PatientId)
                                            .Where(x => x.AddDatetime == odsPop.Losecs)
                                            .Where(x => x.MedicationId == odsPop.MedicationId)
                                            .ToList()
                                            .FirstOrDefault();
                odsPop.PatientOrderId = patientOrders.Id;
            }
            return odsPOPList;
        }

        public string GetComboName(int medicationId)
        {
            var query = from m in _context.Medications
                        where m.Id == medicationId
                        && m.DrugId.Equals("COMBO")
                        select m.DisplayName;

            return query.FirstOrDefault() ?? "";
        }

        public static byte SetPharmacyVerificationStatusByDispositionTypeCode(string? dispositionTypeCode)
        {
            //Use the disposition type code to see if this order needs pharmacy verification or not.
            //Winston Murdock, 04/29/2021.

            //Default the return to 0.
            //Only set it to 1 if we meet the criteria.
            byte ret = 0;

            //If the parameter is null, then don't check its value.
            if (!string.IsNullOrEmpty(dispositionTypeCode))
            {
                //If the disposition type code is either "INP" or "INPT" or "OBS" then we'll return 1.
                //Else, we'll return 0.
                //Per Jim, we might need a 
                if (dispositionTypeCode.ToUpper() == "INP" || dispositionTypeCode.ToUpper() == "INPT" || dispositionTypeCode.ToUpper() == "OBS")
                {
                    ret = 1;
                } //end if (value is "INP" or "INPT" or "OBS")
            } //end if (param is not null?)

            // Return.
            return ret;
        } //end SetPharmacyVerificationStatusByDispositionTypeCode

        public string? GetMatchNdcByMedIdAndSiteId(int medicationId, int siteId)
        {
            //Get the NDC for this order from the match table.
            //if it's in there, and all three values aren't 0/0/0, we'll grab it
            //Else, we'll return null.
            //Winston Murdock, 05/11/2021.  EMAR-932.
            string? sNdc = null;

            //Check SFM.
            var query = from sfm in _context.SiteFormularyMatch
                        where sfm.SiteId == siteId
                        && sfm.MedicationId == medicationId
                        &&
                        (
                            sfm.InpatientMatch != 0 &&
                            sfm.OutpatientMatch != 0 &&
                            sfm.PyxisMatch != 0
                        )
                        select sfm.Ndc;

            sNdc = query.FirstOrDefault() ?? null;
            
            return sNdc;
        } //end GetOrderNdc

        public void CreateNotifications(List<OdsPatientOrderParameters> odsPOPList)
        {
            //By the time we get here, the orders have already been moved from
            //patient_cart_orders into patient_orders.
            //If this client is creating pharmacy notifications for new orders then
            //If this is an "inpatient" patient, then create a notification and then add details for each order.
            //If this is an "ED" patient (i.e. not an "inpatient" patient), then
            //If all orders are a “once” frequency, then don’t add a notification.
            //Else do add a notification and then add details for each order that is not a “once” frequency.
            //Winston Murdock, 09/29/2021.  EMAR-1221

            //Change to not create pharmacy notifications for "ER" patients.
            //Winston Murdock, 01/07/2022.  PC-26905

            //See if this client is creating pharmacy notifications for new orders.
            if (CheckActionIsInIni("NewOrder", true))
            {
                //Only do anything if we actually have orders in the list.
                if (odsPOPList.Count > 0)
                {
                    long patientId = odsPOPList[0].PatientId;

                    //Look at the patient.
                    //using the patient ID, get the patient object.
                    Data.Entities.Patient thisPatient = _context.Patients.Find(patientId);

                    //We need the emar site id here not the ibex site id.
                    //And this was pulling the ibex site id.
                    //So this only works when the ibex site id happens to exist in the emar DB.
                    //Colin and I ran into this when testing at Emerus Test with the Northeast site.
                    //Its ibex site id of 32 does not exist in emar's sites table, which
                    //caused an error when getting the site's time zone.
                    //Thus, I changed this to pull the emar site id.
                    //Winston Murdock, 11/29/2021.  PC-26828
                    //int siteId = odsPOPList[0].SiteId;
                    int siteId = thisPatient.SiteId;

                    var siteNow = _siteRepository.GetSiteTimeZone(siteId).NowWithTimeZoneOffset();

                    //Now that I've got the patient, check the disposition type code to see if this is an "inpatient" patient or not.
                    //We've already got the logic in this method, so reuse it.
                    //This method returns a byte (since phamracy verification status is that), so work with that rather than using a boolean here.
                    //byte isInPatient = SetPharmacyVerificationStatusByDispositionTypeCode(thisPatient.DispositionTypeCode);

                    ////Also need to check the patient's custom indicators to see
                    ////if one of them is "EDHOLD" or not.
                    ////If we've already got isInPatient set to 1, then we don't
                    ////need to do this check since we know that this
                    ////is an "inpatient" patient
                    ////Winston Murdock, 01/06/2022.  PC=26905
                    //if (isInPatient == 0)
                    //{
                    //    isInPatient = CheckPatientIndicators(thisPatient.Id);
                    //} //end if

                    //Notification entity object that we'll use for both cases of the if/else.
                    PharmacyNotification notification = new PharmacyNotification();

                    //See if this is an "inpatient" or "ER" patient.
                    //We know this list will have at least one order.
                    //So we don't need to have a check to make sure the list isn't empty.
                    if (odsPOPList[0].PharmVerificationReq)
                    {
                        //Pharmacy verification is required.
                        //Create a new notification.
                        notification.PatientId = patientId;
                        notification.Type = "Order";
                        notification.EnteredDatetime = siteNow;

                        //For each order, create a PharmacyNotificationOrder object.
                        //Then add it to the child collection in the PharmacyNotification parent.
                        //Per Brad, when I call SaveChanges on the DB context, it will automatically
                        //handle getting the ID of the row in inpatient_notifications and then will
                        //set the inpatient_notification_id for each of these rows in
                        //inpatient_notification_orders to that value.
                        foreach (OdsPatientOrderParameters patientOrder in odsPOPList.ToList())
                        {
                            PharmacyNotificationOrder notificationOrder = new PharmacyNotificationOrder
                            {
                                PatientOrderId = patientOrder.PatientOrderId
                            };
                            notification.PharmacyNotificationOrders.Add(notificationOrder);

                            //Also update the pharmacy verification status of this patient order to 1.
                            //Without this, the "rx" logo won't show on the mar patient page.
                            //And the pharmacists won't know this order needs to be verified.
                            _context.PatientOrders.Find(patientOrder.PatientOrderId).PharmacyVerificationStatus = 1;
                        } //end foreach
                        
                        //Add the newly created PharmacyNotification (and its InpatientNotificationOrder children) to the context.
                        _context.PharmacyNotifications.Add(notification);

                        //Save the context to the DB.
                        var i = _context.SaveChanges(true);
                    }
                    //We don't genreate pharmacy notifications for "ER" patients any more.
                    //Winston Murdock, 01/06/2021.  PC-26905
                    //else
                    //{
                    //    //"ER" patient.

                    //    //Generate a notification for all orders that have a priority of Routine
                    //    //or that have multiple order administrations.
                    //    //I'm leaving the variable name as it was before this logic was changed.
                    //    //Winston Murdock, 11/11/2021.  PC-26780

                    //    //Since you can't join an EF LINQ query with a local list of entity objects
                    //    //(https://stackoverflow.com/a/20503315),
                    //    //loop through odsPopList and add the PatientOrderId into an array here.
                    //    var patientOrderIds = new List<long>();
                    //    foreach (OdsPatientOrderParameters item in odsPOPList)
                    //    {
                    //        //Add this one's Id to the list.
                    //        patientOrderIds.Add(item.PatientOrderId);
                    //    } //end foreach.

                    //    //Get the actual list of Patient Orders.
                    //    //Where the Id is in the list of Ids we just added
                    //    //and where the priority is Routine or the order has multiple administrations.
                    //    var notOnceAndStatOrdersForThisCart =
                    //    (
                    //        _context.PatientOrders.Where
                    //        (
                    //            x => patientOrderIds.Contains(x.Id)
                    //            &&
                    //            (
                    //                x.Priority == (byte)OrderPriorities.Routine
                    //                || x.OrderAdministrations.Count > 1
                    //            )
                    //        )
                    //    );

                    //    //If notOnceOrdersForThisCart has any entries, then there were one, or more, orders
                    //    //that have a priority of routine or that have multiple administrations.
                    //    //Then generate notifications for those orders.
                    //    if (notOnceAndStatOrdersForThisCart.ToList().Count > 0)
                    //    {
                    //        notification.PatientId = patientId;
                    //        notification.Type = "Order";
                    //        notification.EnteredDatetime = siteNow;

                    //        //For each order in the list of orders that we want to create pharmacy
                    //        //notifications for, create a PharmacyNotificationOrder object.
                    //        //Then add it to the child collection in the InpatientNotification parent.
                    //        foreach (PatientOrder patientOrder in notOnceAndStatOrdersForThisCart)
                    //        {
                    //            PharmacyNotificationOrder notificationOrder = new PharmacyNotificationOrder
                    //            {
                    //                PatientOrderId = patientOrder.Id
                    //            };
                    //            notification.PharmacyNotificationOrders.Add(notificationOrder);

                    //            //Also update the pharmacy verification status of this patient order to 1.
                    //            //Without this, the "rx" logo won't show on the mar patient page.
                    //            //And the pharmacists won't know this order needs to be verified.
                    //            _context.PatientOrders.Find(notificationOrder.PatientOrderId).PharmacyVerificationStatus = 1;
                    //        } //end foreach

                    //        //Add the newly created PharmacyNotification (and its PharmacyNotificationOrder children) to the context.
                    //        _context.PharmacyNotifications.Add(notification);

                    //        //Save the context to the DB.
                    //        var i = _context.SaveChanges(true);
                    //    } //end if (This cart has any orders that are not a "once" frequency?)
                    //} //end if ("inpatient" patient?)
                } //end if (any patient orders in the list?)
            } // end if (is this client creating pharmacy notifications for new orders?)
        } //end CreateNotification

        public bool CheckActionIsInIni(string action, bool bOrder)
        {
            //If the action is in the ini file setting for order or administration (based on bOrder),
            //return true so that the calling method generates a notification.
            //Else return false so that that the calling method does not generate a notification.
            //The .ini file is at inetpub/wwwroot/eMARAPI/pharmacy_notification/pharmacy_notifications.ini.

            bool bRet = false;

            try
            {
                IniFile iniFile = new IniFile();
                string[] sValues;

                //See if this action was fired on an order or an administration.
                if (bOrder)
                {
                    //Order
                    //Get the order actions that fire off a notification into an array.
                    sValues = iniFile.Read("Order", "Actions").Split(",");
                }
                else
                {
                    //Administration
                    //Get the administration actions that fire off a notification into an array.
                    sValues = iniFile.Read("Administration", "Actions").Split(",");
                } //end if

                //See if the passed in action is in sValues.
                if (sValues.Any(x => x == action))
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

        public bool CalculateIfPatientNeedsPharmVerifOnOrders(Emar.Data.Entities.Patient patient)
        {
            //We were calculating this twice.
            //Jim was doing it for his ODS stuff (to determine whether or not to show the rx icon in PCED).
            //And Winston was doing it when determining whether or not to create pharmacy notifications in eMAR.
            //Let's do the calculation in only one place.
            //Jim's stuff will call this.
            //If we ever need to change anything about this for checking out a cart, we only do it in one place.
            //Winston's stuff will check the boolean that Jim's stuff has already set.
            //Winston Murdock, 01/12/2022.  PC-26905

            bool bRet = false;
            byte byteTemp;

            //This guy checks the patient's disposition type code.
            //Eventually, we will want the list of disposition type
            //codes to reside in the site_options table in the DB.
            //for now, we hardcode the list.
            byteTemp = SetPharmacyVerificationStatusByDispositionTypeCode(patient.DispositionTypeCode);

            //If the temp byte if already 1, then we have an "inpatient"
            //patient and don't need to check this patient's indicators.
            //If it's zero, then we need to check the patient's
            //indicators to see if there is an "EDHOLD" indicator.
            if (byteTemp == 0)
            {
                byteTemp = CheckPatientIndicators(patient.Id);
            } //end if

            //If the byte is not 0, then we set the return boolean to true.
            //Else, we leave it at false.
            if (byteTemp != 0)
            {
                bRet = true;
            } //end if

            //Return.
            return bRet;
        }  //end CalculateIfPatientNeedsPharmVerifOnOrders

        public FdbBrandName? GetFdbBrandNameForCartOrder(int itemId, int siteId)
        {
            //Get the FdbBrandname for this medication detail.

            if (_optionRepository.GetOption(siteId, OptionNames.DRUG_DB_VENDOR).ToUpper() == "F")
            {
                //FDB.
                var md = _context.MedicationDetails.Find(itemId);
                var ret = _context.FdbBrandName.Where(x => x.MedidString == md.DrugId).FirstOrDefault();
                return ret;
            }
            else
            {
                return null;
            } //end if

        }

        public void CleanUpCartOrders(List<PatientCartOrder> cartOrders)
        {
            foreach (PatientCartOrder cartOrder in cartOrders)
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
                        foreach (var interaction in cartOrder.OrderInteractions)
                        {
                            var orderInteractions = _context.OrderInteractions
                                .Where(x => x.MedicationInteractionId == interaction.MedicationInteractionId);

                            _context.OrderInteractions.RemoveRange(orderInteractions);
                        }

                    }

                    if (medicationInteractions.Count > 0)
                    {
                        foreach (var interaction in medicationInteractions)
                        {
                            if (interaction != null)
                            {
                                _context.MedicationInteractions.Remove(interaction);
                            }
                        }
                    }
                }

                //Delete the cart order administrations.
                if (cartOrder.CartOrderAdministrations != null)
                {
                    _context.CartOrderAdministrations.RemoveRange(cartOrder.CartOrderAdministrations);
                } //end if


                //Delete the reactions for this cart order.
                if (cartOrder.OrderReactions != null)
                {
                    _context.OrderReactions.RemoveRange(cartOrder.OrderReactions);
                } //end if

                //Lastly, delete the patient cart order itself.
                _context.PatientCartOrders.Remove(cartOrder);
            } //end foreach.

            //Save all these deletes back to the DB.
            _context.SaveChanges(true);
        } //end CleanUpCartOrders
    }
}