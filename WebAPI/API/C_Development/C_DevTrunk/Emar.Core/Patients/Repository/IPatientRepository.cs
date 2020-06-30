using System.Collections.Generic;
using Emar.Data.Entities;

namespace Emar.Core.Patients.Repository
{
    public interface IPatientRepository
    {
        IEnumerable<Patient> GetPatients(ResourceParameters resourceParameters);
        Patient GetPatient(long? patientId, ResourceParameters resourceParameters);
        long? GetPatientId(long? patientId, ResourceParameters resourceParameters);
    }
}