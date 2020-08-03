using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using Emar.Core.Helpers;
using Emar.Core.Patients.Model;
using Emar.Core.ResourceParameters;
using Emar.Data;
using Emar.Data.Entities;
using Microsoft.EntityFrameworkCore;
using static Emar.Core.Patients.Model.Constants;

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

        public PagedList<Patient> GetPatients(PatientsResourceParameters resourceParameters, bool includeOrders)
        {
            IEnumerable<Patient> patients = GetPatients(((resourceParameters != null) && resourceParameters.IncludeOrders) || includeOrders);

            if (!resourceParameters.IncludeInactive)
            {
                patients = patients
                    .Where(pt => pt.Active == true);
            }

            if (resourceParameters.SiteId != null)
            {
                patients = patients
                    .Where(pt => pt.SiteId == resourceParameters.SiteId);
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

        IEnumerable<Patient> GetPatients(bool includeOrders = true)
        {
            if (includeOrders)
            {
                return _context.Patients
                        .Include(patient => patient.PatientOrders)
                            .ThenInclude(order => order.OrderAdministrations)
                        .Include(patient => patient.PatientOrders)
                            .ThenInclude(order => order.MedicationRoute)
                        .Include(patient => patient.PatientOrders)
                            .ThenInclude(order => order.MedicationUnit)
                        .Include(patient => patient.PatientOrders)
                            .ThenInclude(order => order.AddUser)
                        .Include(patient => patient.PatientOrders)
                            .ThenInclude(order => order.OrderPhysicianUser)
                        .Include(patient => patient.Site)
                                .ThenInclude(site => site.SiteOptions)
                                    .ThenInclude(siteOptions => siteOptions.Option)
                        .ToList();
            }

            return _context.Patients
                    .Include(patient => patient.Site)
                        .ThenInclude(site => site.SiteOptions)
                            .ThenInclude(siteOptions => siteOptions.Option)
                    .ToList();
        }

        public Patient GetPatient(long? patientId, PatientsResourceParameters resourceParameters, bool includeOrders)
        {
            return GetPatients(((resourceParameters != null) && resourceParameters.IncludeOrders) || includeOrders)
                    .FirstOrDefault(patient => patient.Id == patientId);
        }

        public long? GetPatientId(long? patientId, PatientsResourceParameters resourceParameters)
        {
            if ((resourceParameters != null) &&
                (resourceParameters.ExtId1 != null) &&
                (resourceParameters.ExtId2 != null))
            {
                patientId = _context.ExternalIds
                                .Where(@x_id =>
                                        @x_id.ExternalId.Equals(resourceParameters.ExtId1 + "|" + resourceParameters.ExtId2) &&
                                        @x_id.Entity.ToLower().Equals(@"patients") &&
                                        @x_id.Vendor.ToLower().Equals(@"pulsecheck"))
                                .FirstOrDefault()
                                .InternalId;
            }

            return patientId;
        }

        public long GetInternalPatientId(short extId1, string extId2)
        {
            var ptId = from e in _context.ExternalIds
                       where e.ExternalId == extId1 + "|" + extId2
                             && e.Entity == "patients"
                             && e.Vendor == "pulsecheck"
                       select e.InternalId;

            return ptId.FirstOrDefault();
        }

        public Patient GetPatientByNumber(string number, GetPatientBy getPatientBy)
        {
            switch (getPatientBy)
            {
                case GetPatientBy.AccountNumber:
                    return GetPatients()
                            .FirstOrDefault(p => p.AccountNumber == number);
                case GetPatientBy.CustomNumber:
                    return GetPatients()
                            .FirstOrDefault(p => p.CustomNumber == number);
                case GetPatientBy.PersonNumber:
                    return GetPatients()
                            .FirstOrDefault(p => p.PersonNumber == number);
            }

            return null;
        }
    }
}
