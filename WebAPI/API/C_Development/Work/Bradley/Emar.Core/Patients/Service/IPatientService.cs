using Emar.Core.Patients.Model;

namespace Emar.Core.Patients.Service
{
    public interface IPatientService
    {
        PatientDto GetPatient(int patientId);
    }
}