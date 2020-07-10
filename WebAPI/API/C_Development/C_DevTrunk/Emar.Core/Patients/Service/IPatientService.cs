using Emar.Core.Patients.Model;

namespace Emar.Core.Patients.Service
{
    public interface IPatientService
    {
        PagedList<PatientDto> GetPatients(PatientsResourceParameters resourceParameters, bool includeOrders);
        PatientDto GetPatient(long patientId, PatientsResourceParameters resourceParameters, bool includeOrders);
        PatientDto GetPatient(short extId1, string extId2);
        PatientDto GetPatient(string accountNumber);
    }
}