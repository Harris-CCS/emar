using Emar.Core.Patients.Model;
using Emar.Data;

namespace Emar.Core.Patients.Repository
{
    public class PatientRepository:IPatientRepository
    {
        public Patient GetPatient(int patientId)
        {
            var pt = new Patient
            {
                Id = patientId,
                FirstName = "Winston",
                MiddleName = "Bradley",
                LastName = "Biliardis",
                Suffix = "Jr."
            };
            return pt;
        }
    }
}
