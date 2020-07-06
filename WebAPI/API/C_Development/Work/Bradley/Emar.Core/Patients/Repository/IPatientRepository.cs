using Emar.Data.Entities;

namespace Emar.Core.Patients.Repository
{
    public interface IPatientRepository
    {
        PagedList<Patient> GetPatients(ResourceParameters resourceParameters, bool includeOrders);
        Patient GetPatient(long? patientId, ResourceParameters resourceParameters, bool includeOrders);
        long? GetPatientId(long? patientId, ResourceParameters resourceParameters);
    }
}