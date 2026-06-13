using Emar.Core.Carts.Model;
using Emar.Core.Helpers;
using Emar.Core.Orders.Model.Mappings;
using Emar.Core.OutboundChart.Model;
using Emar.Core.OutboundChart.Service;
using Emar.Core.OutboundData.Model;
using Emar.Core.OutboundData.Service;
using Emar.Core.ResourceParameters;
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

        public CartOrderRepository(EmarContext emarContext, IPropertyMappingService propertyMappingService, EmarMemoryCache cache, IOdsEmarOutboundService odsEmarOutboundService,
                                   IOcsEmarOutboundService ocsEmarOutboundService)
        {
            _context = emarContext ?? throw new ArgumentNullException(nameof(emarContext));
            _propertyMappingService = propertyMappingService ?? throw new ArgumentNullException(nameof(propertyMappingService));
			_odsEmarOutboundService = odsEmarOutboundService ?? throw new ArgumentNullException(nameof(odsEmarOutboundService));
            _ocsEmarOutboundService = ocsEmarOutboundService ?? throw new ArgumentNullException(nameof(ocsEmarOutboundService));
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
                } //end foreach allergy reaction view.
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
                        PharmVerificationReq = cartOrder.Patient.DispositionTypeCode.ToUpper() == "INP" || cartOrder.Patient.DispositionTypeCode.ToUpper() == "INPT"
                                            || cartOrder.Patient.DispositionTypeCode.ToUpper() == "OBS",
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
                        PharmVerifStatus = SetPharmacyVerificationStatusByDispositionTypeCode(cartOrder.Patient.DispositionTypeCode),
                        PRNIndication = cartOrder.PrnIndication,
                        AntiMicrobialIndication = cartOrder.AntimicrobialIndication != null ? cartOrder.AntimicrobialIndication.Code : "",
                        AntiMicrobialIndicationText = cartOrder.AntimicrobialIndication != null ? cartOrder.AntimicrobialIndication.Description : ""
                    };
                    ocsCPList.Add(ocsParams);

                    _context.CartOrderAdministrations.RemoveRange(cartOrder.CartOrderAdministrations);
                    _context.PatientCartOrders.Remove(cartOrder);

                    secs++;
                }

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
            return _cache.GetOrCreate(frequencyId + CacheKeys.FrequencySchedules, entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30);

                var ret = _context.FrequencySchedules
                    .Include(f => f.FrequencyType)
                    .FirstOrDefault(f => f.Id == frequencyId);

                entry.Size = 1;

                return ret;
            });
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
    }
}