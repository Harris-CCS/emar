using System;
using System.Collections.Generic;
using System.Linq;
using Emar.Core;
using Emar.Core.Patients.Model;
using Emar.Core.Patients.Service;
using Emar.Data;
using Emar.Data.Entities;

namespace Emar.Core.Patients.Repository
{
    public class PatientRepository : IPatientRepository
    {
        private readonly EmarContext _context;
#if PAGING || SORTING || EXPANDO
        private readonly IPropertyMappingService _propertyMappingService;
#endif

        public PatientRepository()
        {

        }

#if ORIGINAL
        public PatientRepository(EmarContext emarContext)
        {
            _context = emarContext ?? throw new ArgumentNullException(nameof(emarContext));
        }
#endif
#if PAGING || SORTING || EXPANDO
        public PatientRepository(EmarContext emarContext, IPropertyMappingService propertyMappingService)
        {
            _context = emarContext ?? throw new ArgumentNullException(nameof(emarContext));
            _propertyMappingService = propertyMappingService ?? throw new ArgumentNullException(nameof(propertyMappingService));
        }
#endif

#if ORIGINAL
        public IEnumerable<Patient> GetPatients(ResourceParameters resourceParameters)
        {
            var patients = _context.Patients.AsEnumerable();

            if (!resourceParameters.IncludeInactive)
            {
                patients = patients.Where(pt => pt.Active.Equals(true));
            }

            if (resourceParameters.Site != null)
            {
                patients = patients.Where(pt => pt.SiteId.Equals(resourceParameters.Site));
            }

            return patients;
        }
#endif
#if PAGING || SORTING || EXPANDO
        public PagedList<Patient> GetPatients(ResourceParameters resourceParameters)
        {
            var patients = _context.Patients.AsEnumerable();

            if (!resourceParameters.IncludeInactive)
            {
                patients = patients.Where(pt => pt.Active.Equals(true));
            }

            if (resourceParameters.Site != null)
            {
                patients = patients.Where(pt => pt.SiteId.Equals(resourceParameters.Site));
            }

            if (resourceParameters.OrderBy != null)
            {
                //get property mapping dictionary
                var propertyMappingDictionary = _propertyMappingService.GetPropertyMapping<PatientDto, Patient>();

                patients = patients.AsQueryable().ApplySort(resourceParameters.OrderBy, propertyMappingDictionary);
            }

            return PagedList<Patient>.Create(patients.AsQueryable(), resourceParameters.PageNumber, resourceParameters.PageSize);
        }
#endif

        public Patient GetPatient(long? patientId, ResourceParameters resourceParameters)
        {
            patientId = (long)GetPatientId(patientId, resourceParameters);

            Patient patient = _context.Patients.Find(patientId);

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

    }
}
