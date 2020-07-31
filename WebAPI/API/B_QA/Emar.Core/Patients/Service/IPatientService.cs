using Emar.Core.Helpers;
using Emar.Core.Patients.Model;
using Emar.Core.ResourceParameters;

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