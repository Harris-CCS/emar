using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Emar.Core.OutboundChart.Model;
using Emar.Core.OutboundData.Model;
using Emar.Core.OutboundData.Model.Mappings;
using Emar.Core.OutboundData.Repository;
using Emar.Data;

namespace Emar.Core.OutboundData.Service.IbexSpecific
{
    public class OdsEmarOutboundService : IOdsEmarOutboundService
    {
        private readonly IbexContext _ibexContext;
        private readonly IEmarOutboundDataRepository _emarOutboundDataRepository;

        public OdsEmarOutboundService(IbexContext ibexContext, IEmarOutboundDataRepository emarOutboundDataRepository)
		{
			_ibexContext = ibexContext;
            _emarOutboundDataRepository = emarOutboundDataRepository;
		}

        public void SendNewPatientOrder(List<OdsPatientOrderParameters> odsOrderList)
        {
            //var emr = new EMR(siteId, patientId, true);
            var ibex = _emarOutboundDataRepository.GetExternalPatientId(odsOrderList[0].PatientId);
            //var emr = new EMR(1, ibex, false);

            // set the tracking behavior to all
            _ibexContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.TrackAll;
            // repeat for each item in the list
            foreach (OdsPatientOrderParameters odsOrderItem in odsOrderList)
            {
                var comboName = _emarOutboundDataRepository.GetComboName(odsOrderItem.MedicationId);
                var isComboMed = comboName.Length > 0;
                odsOrderItem.DisplayName = comboName;
                odsOrderItem.Type = isComboMed ? Medication.Constants.TYPE_COMBO : Medication.Constants.TYPE_MEDICATION;
                var odsOrder = ConvertOdsOrderData(odsOrderItem);
                _ibexContext.Medications.Add(EmarOutboundMapper.MapMedication(odsOrder));
                // determine if group order
                if (isComboMed)
                {
                    List<int> detailIds = _emarOutboundDataRepository.GetMedicationDetailsIds(odsOrder.MedicationId);
                    foreach (int detailId in detailIds)
                    {
                        OdsMedicationDetails odsMedDetails = ConvertOdsMedicationDetailsDataFromMedDetailsId(detailId);
                        _ibexContext.MedicationDetails.Add(EmarOutboundMapper.MapMedicationDetails(odsOrder, odsMedDetails));
                    }
                }
                else
                {
                    OdsMedicationDetails odsMedDetails = ConvertOdsMedicationDetailsData(odsOrder);
                    _ibexContext.MedicationDetails.Add(EmarOutboundMapper.MapMedicationDetails(odsOrder, odsMedDetails));
                }
                var patient = _ibexContext.Patients.First(g => g.Ibex == odsOrder.Ibex && g.Site == odsOrder.SiteId);
                // if patient is null, throw exception here?
                patient.Ord30 = "O"; // set the med svc acknowledge to order/red
                _ibexContext.Entry(patient).Property(p => p.Ord30).IsModified = true; // necessary?

            }
            _ibexContext.SaveChanges();
            // set the tracking behavior to none
            _ibexContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        }

        public void SendAdministrationAction(OdsAdministrationParameters parameters)
        {
            //If this order doesn't exist in ibex, then we'll need to pass an error up the chain.
            //Rather than trying to throw an error, actually catch it.
            //Winston Murdock and Colin Clarkson, 02/16/2021.  EMAR-700
            try
            {
                var order = _ibexContext.Medications.First(g => g.EmarPatientOrderId == parameters.OrderId);
               
                //Commenting this out since we've added a try/catch block around all of this.
                //Winston Murdock, 02/18/2021.
                //if (order == null)
                //{
                //    // shouldn't happen but will throw exception if it does
                //    throw new ArgumentException($"Requested patient order with Id {parameters.OrderId}, which does not exist.", nameof(parameters.OrderId));
                //}

                // set the tracking behavior to all
                _ibexContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.TrackAll;
                // check is there is an existing give or if the action is not give
                if (order.GiveDate != null || !parameters.Action.Equals("Give"))
                {
                    // add new entry to emar_med_administrations since either second give or any other action
                    parameters.Ibex = order.Ibex;
                    // determine the last losecs in use
                    var lastLosecs = GetLastLosecsFromEmarMedAdmin(parameters.OrderId);
                    lastLosecs = lastLosecs > 0 ? lastLosecs : order.Losecs;
                    // if this is a new order administration, then create new losecs (+1)
                    parameters.Losecs = parameters.NewOrderAdmin ? lastLosecs + 1 : lastLosecs;
                    parameters.AddUserId = _emarOutboundDataRepository.GetExternalUserId(parameters.AddUserId);
                    parameters.SiteId = _emarOutboundDataRepository.GetExternalSiteId(parameters.SiteId);
                    _ibexContext.EmarMedicationAdministrations.Add(EmarOutboundMapper.MapEmarMedicationAdministrations(parameters));
                    // If an IV then set the iv_type in the med record as well
                    if (!string.IsNullOrWhiteSpace(parameters.IVType))
                    {
                        order.IVType = parameters.IVType;
                        _ibexContext.Entry(order).Property(p => p.IVType).IsModified = true;
                    }
                }
                else
                {
                    // update med table if first give
                    order.GiveDate = parameters.EventDateTime.ToString("yyyyMMddHHmmss");
                    order.GiveSysDate = parameters.AddDatetime.ToString("yyyyMMddHHmmss");
                    order.GiveUser = _emarOutboundDataRepository.GetExternalUserId(parameters.AddUserId);
                    order.IVEdit = "G";
                    order.IVLocation = parameters.IVLocation;

                    //This is a fix/change that Jim has already made but hasn't pushed up to Dev Trunk yet.
                    //I need to get EMAR-649 up to 57c today, so he directed me to apply this one-line fix.
                    //Winston Murdock, 02/10/2021.
                    //order.IVSite = (int)parameters.IVSite;
                    order.IVSite = parameters.IVSite;

                    order.IVType = parameters.IVType;
                    // having issue with using SaveChanges here but if Property is set to modified for each field
                    // individually then SaveChanges works afterwards
                    _ibexContext.Entry(order).Property(p => p.GiveDate).IsModified = true;
                    _ibexContext.Entry(order).Property(p => p.GiveSysDate).IsModified = true;
                    _ibexContext.Entry(order).Property(p => p.GiveUser).IsModified = true;
                    _ibexContext.Entry(order).Property(p => p.IVEdit).IsModified = true;
                    _ibexContext.Entry(order).Property(p => p.IVLocation).IsModified = true;
                    _ibexContext.Entry(order).Property(p => p.IVSite).IsModified = true;
                    _ibexContext.Entry(order).Property(p => p.IVType).IsModified = true;
                }

                _ibexContext.SaveChanges();
                // set the tracking behavior to none
                _ibexContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
            }
            catch (System.InvalidOperationException Ex)
            {
                //If the order doesn't exist in ibex, then we'll need to pass this error up.
                throw new ArgumentException($"Requested patient order with Id {parameters.OrderId}, which does not exist.", nameof(parameters.OrderId));
            }
            catch (Exception Ex)
            {
                //Some other type of error.
                //Just pass the error message up the stack.
                throw new Exception(Ex.Message);
            }//end try/catch

        }

        public OdsPatientOrderParameters ConvertOdsOrderData(OdsPatientOrderParameters odsOrder)
        {
            odsOrder.Ibex = _emarOutboundDataRepository.GetExternalPatientId(odsOrder.PatientId);
            odsOrder.AddUserId = _emarOutboundDataRepository.GetExternalUserId(odsOrder.AddUserId);
            odsOrder.OrderingPhysicianId = _emarOutboundDataRepository.GetExternalUserId(odsOrder.OrderingPhysicianId);
            odsOrder.SiteId = _emarOutboundDataRepository.GetExternalSiteId(odsOrder.SiteId);
            odsOrder.BrandName = odsOrder.DisplayName.Length > 0 ? odsOrder.DisplayName : _emarOutboundDataRepository.GetFdbBrandName(odsOrder.MedicationId);
            odsOrder.AmIndication = _emarOutboundDataRepository.GetAmIndication(odsOrder.AmIndication);
            odsOrder.Route = _emarOutboundDataRepository.GetRoute(odsOrder.Route);
            odsOrder.Unit = _emarOutboundDataRepository.GetUnit(odsOrder.Unit);

            return odsOrder;
        }

        public OdsMedicationDetails ConvertOdsMedicationDetailsData(OdsPatientOrderParameters odsOrder)
        {
            return _emarOutboundDataRepository.GetMedicationDetails(odsOrder.MedicationId);
        }

        public OdsMedicationDetails ConvertOdsMedicationDetailsDataFromMedDetailsId(int detailsId)
        {
            return _emarOutboundDataRepository.GetMedicationDetailsFromMedDetailsId(detailsId);
        }

        public bool GetEmarMedAdministrationStatus(long adminId)
        {
            var adminOrder = _ibexContext.EmarMedicationAdministrations.FirstOrDefault(e => e.OrderAdministrationsId == adminId)?.Id;
            return adminOrder != null ? true : false;
        }

        public int GetLastLosecsFromEmarMedAdmin(long patientOrderId)
        {
            var query = from e in _ibexContext.EmarMedicationAdministrations
                        where e.PatientOrderId == patientOrderId
                        orderby e.Losecs descending
                        select e.Losecs;

            return query.FirstOrDefault();
        }
    }
}