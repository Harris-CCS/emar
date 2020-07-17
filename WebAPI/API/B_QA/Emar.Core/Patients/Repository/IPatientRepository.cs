using Emar.Data.Entities;

namespace Emar.Core.Patients.Repository
{
    public interface IPatientRepository
    {
        PagedList<Patient> GetPatients(PatientsResourceParameters resourceParameters, bool includeOrders);
        Patient GetPatient(long? patientId, PatientsResourceParameters resourceParameters, bool includeOrders);
        long? GetPatientId(long? patientId, PatientsResourceParameters resourceParameters);
        long GetInternalPatientId(short extId1, string extId2);
        Patient GetPatientByAccountNumber(string accountNumber);
    }
}