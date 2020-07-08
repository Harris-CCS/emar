using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Emar.Core.Patients.Model;
using Emar.Data;
using Emar.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Emar.Core.Patients.Repository
{
    public class PatientRepository : IPatientRepository
    {
        private readonly EmarContext _context;
        private readonly IPropertyMappingService _propertyMappingService;

        public PatientRepository()
        {

        }

        public PatientRepository(EmarContext emarContext, IPropertyMappingService propertyMappingService)
        {
            _context = emarContext ?? throw new ArgumentNullException(nameof(emarContext));
            _propertyMappingService = propertyMappingService ?? throw new ArgumentNullException(nameof(propertyMappingService));
        }

        public PagedList<Patient> GetPatients(ResourceParameters resourceParameters, bool includeOrders)
        {
            IEnumerable<Patient> patients;

            if ((includeOrders) ||
                (resourceParameters.IncludeOrders))
            {
                patients = GetPatientsWithOrders();
            }
            else
            {
                patients = GetPatientsWithoutOrders();
            }

            if (!resourceParameters.IncludeInactive)
            {
                patients = patients
                    .Where(pt => pt.Active == true);
            }

            if (resourceParameters.Site != null)
            {
                patients = patients
                    .Where(pt => pt.SiteId == resourceParameters.Site);
            }

            if (resourceParameters.DepartmentCode != null)
            {
                patients = patients
                    .Where(pt => pt.DepartmentCode == resourceParameters.DepartmentCode);
            }

            if (resourceParameters.WardCodes != null)
            {
                var wardCodes = resourceParameters.WardCodes.Split(",");

                patients = patients
                    .Where(pt => wardCodes.Contains(pt.WardCode));
            }

            if (resourceParameters.RoomBedCode != null)
            {
                patients = patients
                    .Where(pt => pt.RoomBedCode == resourceParameters.RoomBedCode);
            }

            if (resourceParameters.OrderBy != null)
            {
                //get property mapping dictionary
                var propertyMappingDictionary = _propertyMappingService.GetPropertyMapping<PatientDto, Patient>();

                patients = patients
                    .AsQueryable().ApplySort(resourceParameters.OrderBy, propertyMappingDictionary);
            }


            return PagedList<Patient>.Create(patients.AsQueryable(), resourceParameters.PageNumber, resourceParameters.PageSize);
        }

        IEnumerable<Patient> GetPatientsWithOrders()
        {
            return _context.Patients
                    .Include(patient => patient.Orders)
                        .ThenInclude(order => order.Events)
                    .Include(patient => patient.Orders)
                        .ThenInclude(order => order.Administrations)
                            .ThenInclude(administration => administration.Events)
                    .AsEnumerable();
        }

        IEnumerable<Patient> GetPatientsWithoutOrders()
        {
            return _context.Patients
                    .AsEnumerable();
        }

        public Patient GetPatient(long? patientId, ResourceParameters resourceParameters, bool includeOrders)
        {
            var patient = _context.Patients.Find(patientId);

            //if (resourceParameters.IncludeOrders || includeOrders)
            //{
            //    patient = _context.Patients
            //        .Include(patient => patient.Orders)
            //            .ThenInclude(order => order.Events)
            //        .Include(patient => patient.Orders)
            //            .ThenInclude(order => order.Administrations)
            //                .ThenInclude(administration => administration.Events)
            //        .FirstOrDefault(patient => patient.Id == patientId);
            //}

            return patient;
        }

        public long? GetPatientId(long? patientId, ResourceParameters resourceParameters)
        {
            if ((resourceParameters != null) &&
                (resourceParameters.Site != null) &&
                (resourceParameters.Ibex != null))
            {
                patientId = _context.ExternalIds
                                .Where(@x_id =>
                                        @x_id.External_Id.Equals(resourceParameters.Site + "|" + resourceParameters.Ibex) &&
                                        @x_id.Entity.ToLower().Equals(@"patients") &&
                                        @x_id.Vendor.ToLower().Equals(@"pulsecheck"))
                                .FirstOrDefault()
                                .InternalId;
            }

            return patientId;
        }

        public long GetInternalPatientId(short site, string ibex)
        {
            var ptId = from e in _context.ExternalIds
                where e.External_Id == site + "|" + ibex
                      && e.Entity == "patients"
                      && e.Vendor == "pulsecheck"
                select e.InternalId;

            return ptId.FirstOrDefault();
        }
    }
}
