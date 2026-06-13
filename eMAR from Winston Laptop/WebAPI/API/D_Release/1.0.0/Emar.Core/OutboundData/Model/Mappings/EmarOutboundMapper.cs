using Emar.Data.IbexEntities;
using Emar.Core.OutboundData.Model;
using Emar.Core.Templates.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Emar.Core.OutboundData.Model.Mappings
{
    public static class EmarOutboundMapper
    {

        /// <param name="logger"></param>
        /// <returns></returns>
        public static Medication MapMedication(OdsPatientOrderParameters dbObj)
        {
            if (dbObj == null)
                return null;

            var ret = new Medication()
            {
                Ibex = dbObj.Ibex,
                Site = dbObj.SiteId,
                Losecs = Int32.Parse(dbObj.Losecs.ToString("ddHHmmss")),
                Status = "A",
                Type = dbObj.Type,
                OrderForUser = dbObj.OrderingPhysicianId,
                OrderUser = dbObj.AddUserId,
                Name = dbObj.BrandName,
                DataSource = "E",
                Dose = dbObj.Dose.Replace(".00", ""),
                MedNotes = dbObj.MedNotes,
                Indication = dbObj.AmIndication,
                OrderDate = dbObj.OrderDate.ToString("yyyyMMddHHmmss"),
                Route = dbObj.Route,
                Unit = dbObj.Unit,
                EmarPatientOrderId = dbObj.PatientOrderId,
            };

            return ret;
        }

        public static MedicationDetails MapMedicationDetails(OdsPatientOrderParameters Pop, OdsMedicationDetails Md)
        {
            if (Pop == null || Md == null)
                return null;

            var ret = new MedicationDetails()
            {
                Ibex = Pop.Ibex,
                Site = Pop.SiteId,
                Losecs = Int32.Parse(Pop.Losecs.ToString("ddHHmmss")),
                BrandName = Md.BrandName,
                ActiveName = Md.ActiveName,
                DrugRoute = Md.DrugRoute,
                DrugForm = Md.DrugForm,
                DrugStrength = Md.DrugStrength,
                DrugDbType = "F",
                ActiveId = Md.ActiveId,
                DrugId = Md.DrugId,
                PackagingId = Md.PackagingId,
                DrugCategoryId = Md.DrugCategoryId,
                Type = "D",
                EmarMedicationId = Pop.MedicationId,
            };

            return ret;
        }

        public static EmarMedicationAdministrations MapEmarMedicationAdministrations(OdsAdministrationParameters dbObj)
        {
            if (dbObj == null)
                return null;

            var ret = new EmarMedicationAdministrations()
            {
                Ibex = dbObj.Ibex,
                Site = dbObj.SiteId,
                Losecs = dbObj.Losecs,
                MedAdminType = dbObj.Action,
                MedAdminUser = dbObj.AddUserId,
                MedAdminDate = dbObj.EventDateTime.ToString("yyyyMMddHHmmss"),
                MedAdminSysdate = dbObj.AddDatetime.ToString("yyyyMMddHHmmss"),
                StopUser = dbObj.AddUserId,
                StopDate = dbObj.StopDate,
                StopSysdate = dbObj.AddDatetime.ToString("yyyyMMddHHmmss"),
                IvSite = dbObj.IVSite,
                IvLocation = dbObj.IVLocation,
                PatientOrderId = dbObj.OrderId,
                IvType = dbObj.IVType,
                IvEdit = dbObj.IVEdit,
                OrderAdministrationsId = dbObj.AdministrationId,
            };

            return ret;
        }
    }
}

