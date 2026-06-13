using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using Emar.Core.OutboundData.Model;
using Emar.Core.OutboundData.Model.Mappings;
using Emar.Core.OutboundData.Repository;
using Emar.Data;
using Emar.Data.Entities;
using HelperDB = Emar.Core.Helpers.DB;
using System.Data.SqlClient;
using System.Data;

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
                odsOrderItem.Type = isComboMed ? OutboundChart.Model.Medication.Constants.TYPE_COMBO : OutboundChart.Model.Medication.Constants.TYPE_MEDICATION;
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

                // Try to find patient on MTB first, then if they cannot be found, try to find them in archive.
                // We only need to update the ord* values if the patient is on the MTB. But maybe we want to throw an error if we can't
                // find the patient anywhere?
                bool foundPatient = false;
                var patient = _ibexContext.Patients.FirstOrDefault(g => g.Ibex == odsOrder.Ibex && g.Site == odsOrder.SiteId);
                if (patient == null)
                {
                    var archivePatient = _ibexContext.EmarArchivedPatientsRetrieveViews.FirstOrDefault(p => p.ExternalId == odsOrder.Ibex && p.ExternalSiteId == odsOrder.SiteId);
                    if (archivePatient != null)
                    {
                        foundPatient = true;
                    }
                }
                else
                {
                    // If this is a PRN order, don't touch ord30.
                    if (!odsOrder.Prn)
                    {
                        DateTimeOffset currentDateTime = DateTimeOffset.Now;

                        // Check administrations for this order, find the earliest one.
                        DateTimeOffset? earliestAdmin = null;
                        if (odsOrder.Administrations != null)
                        {
                            foreach (CartOrderAdministration administration in odsOrder.Administrations)
                            {
                                DateTimeOffset adminTime = administration.AdministrationScheduledDatetime;
                                if ((adminTime != null) && (earliestAdmin == null || adminTime < earliestAdmin))
                                {
                                    earliestAdmin = adminTime;
                                }
                            }
                        }

                        DateTimeOffset useAdminTime = currentDateTime;
                        if (earliestAdmin != null)
                        {
                            useAdminTime = (DateTimeOffset)earliestAdmin;
                        }

                        int medDueTime = 0;
                        try
                        {
                            medDueTime = Convert.ToInt32(
                                new HelperDB.Select
                                {
                                    Sql = "SELECT med_due_time FROM org WHERE site=@site",
                                    Parameters = new SqlParameter[]
                                    {
                                        new SqlParameter("@site", SqlDbType.TinyInt) { Value = odsOrder.SiteId }
                                    }
                                }.RunForScalar()
                            );
                        }
                        catch (Exception ex)
                        {
                            throw new Exception(ex.Message);
                        }

                        useAdminTime = useAdminTime.AddMinutes(medDueTime * -1);

                        // If resulting time is <= the current time, set ord30 to red now, clear out ord30_alt and ord_dt. They are not needed.
                        if (useAdminTime <= currentDateTime)
                        {
                            patient.Ord30 = "O";
                            patient.Ord30Alternate = null;
                            patient.Ord30DateTime = null;

                        // Else when the resulting time is in the future, move the current ord30 to ord30_alt and set ord30_dt, so we continue
                        // to display whatever the current status is, until the calculated datetime is reached.
                        } else
                        {
                            patient.Ord30Alternate = patient.Ord30;
                            patient.Ord30 = "O";
                            patient.Ord30DateTime = useAdminTime;
                        }

                        _ibexContext.Entry(patient).Property(p => p.Ord30).IsModified = true;
                        _ibexContext.Entry(patient).Property(p => p.Ord30Alternate).IsModified = true;
                        _ibexContext.Entry(patient).Property(p => p.Ord30DateTime).IsModified = true;
                    } else
                    {
                        patient.Ord58++;
                        _ibexContext.Entry(patient).Property(p => p.Ord58).IsModified = true;
                    }

                    if (odsOrderItem.PharmVerificationReq)
                    {
                        // set the pharmacy verification required acknowledge to true
                        patient.Ord57 = "Y";
                        _ibexContext.Entry(patient).Property(p => p.Ord57).IsModified = true;
                    }
                }

                if (!foundPatient)
                {
                    // throw exception here?
                }
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
                // first check the emar_med_administrations table by getting the last losecs
                var lastEmarMedAdminLosecs = GetLastLosecsFromEmarMedAdmin(parameters.OrderId, parameters.AdministrationId);
                // check is there is an existing give or if the action is not give or if the action is give plus there is an
                //   existing entry in emar_med_administrations
                if (order.GiveDate != null || !parameters.Action.Equals("Give")
                 || (parameters.Action.Equals("Give") && lastEmarMedAdminLosecs > 0))
                {
                    // add new entry to emar_med_administrations since either second give or any other action
                    parameters.Ibex = order.Ibex;
                    // determine the current losecs in use
                    // if no losecs for the patient in the emar_med_administrations table, use the one from the med table
                    var currLosecs = lastEmarMedAdminLosecs > 0 ? lastEmarMedAdminLosecs : order.Losecs;
                    // if this is a new order administration, then create new losecs (max+1)
                    parameters.Losecs = parameters.NewOrderAdmin ? GetMaxLosecs(order.Ibex, order.Site) + 1 : currLosecs;
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

        public int GetLastLosecsFromEmarMedAdmin(long patientOrderId, long administrationId)
        {
            IQueryable<int> query;
            if (administrationId > 0)
            {
                // if there's an adminId then include it in the query
                query = from e in _ibexContext.EmarMedicationAdministrations
                        where e.PatientOrderId == patientOrderId && e.OrderAdministrationsId == administrationId
                        orderby e.Losecs descending
                        select e.Losecs;
            }
            else
            {
                // no adminId included
                query = from e in _ibexContext.EmarMedicationAdministrations
                        where e.PatientOrderId == patientOrderId
                        orderby e.Losecs descending
                        select e.Losecs;
            }

            return query.FirstOrDefault();
        }

        public int GetMaxLosecs(string ibex, int site)
        {
            // TODO: rewrite into a more elegant single query
            // For example. equivalent to:
            // select top 1 case when m.losecs > e.losecs then m.losecs else e.losecs end as maxlosecs from med m
            // left join emar_med_administrations e on m.emar_patient_order_id = e.patient_order_id
            // where m.ibex = <ibex> and m.site = <site> order by maxlosecs desc

            var query = from e in _ibexContext.EmarMedicationAdministrations
                        where e.Ibex == ibex && e.Site == site
                        orderby e.Losecs descending
                        select e.Losecs;
            var eMax = query.FirstOrDefault();

            query = from m in _ibexContext.Medications
                    where m.Ibex == ibex && m.Site == site
                    orderby m.Losecs descending
                    select m.Losecs;
            var mMax = query.FirstOrDefault();

            return eMax > mMax ? eMax : mMax;
        }
    }
}