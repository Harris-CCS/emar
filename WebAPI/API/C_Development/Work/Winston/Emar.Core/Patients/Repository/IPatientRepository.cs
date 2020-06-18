using Emar.Core.Patients.Model;
using Emar.Data;

namespace Emar.Core.Patients.Repository
{
    public interface IPatientRepository
    {
        Patient GetPatient(int patientId);
    }
}