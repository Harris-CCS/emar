using System.Collections.Generic;
using System.Linq;
using Emar.Data;
using Emar.Data.Entities;

namespace Emar.Core.Patients.Repository
{
    public class PatientRepository : IPatientRepository
    {
        private readonly EmarContext _context;

        public PatientRepository()
        {

        }

        public PatientRepository(EmarContext emarContext)
        {
            _context = emarContext;
        }

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
