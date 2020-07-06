using System.Collections.Generic;
using Emar.Data.Entities;

namespace Emar.Core.Patients.Repository
{
    public interface IPatientRepository
    {
#if ORIGINAL
        IEnumerable<Patient> GetPatients(ResourceParameters resourceParameters);
#endif
#if PAGING || SORTING || EXPANDO
        PagedList<Patient> GetPatients(ResourceParameters resourceParameters);
#endif
        Patient GetPatient(long? patientId, ResourceParameters resourceParameters);
        long? GetPatientId(long? patientId, ResourceParameters resourceParameters);
    }
}