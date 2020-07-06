using Emar.Core.Patients.Model;

namespace Emar.Core.Patients.Service
{
    public interface IPatientService
    {
        PagedList<PatientDto> GetPatients(ResourceParameters resourceParameters, bool includeOrders);
        PatientDto GetPatient(long patientId, ResourceParameters resourceParameters, bool includeOrders);
    }
}