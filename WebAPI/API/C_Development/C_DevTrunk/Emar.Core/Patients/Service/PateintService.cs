using Emar.Core.Patients.Model;
using Emar.Core.Patients.Model.Mappings;
using Emar.Core.Patients.Repository;
using Emar.Data;

namespace Emar.Core.Patients.Service
{
    public class PatientService : IPatientService
    {
        private IPatientRepository _patientRepository;

        public PatientService(IPatientRepository patientRepository)
        {
            _patientRepository = patientRepository;
        }
        public PatientDto GetPatient(int patientId)
        {
            Patient entityPatient = _patientRepository.GetPatient(patientId);

            PatientDto ret = PatientMapper.MapPatient(entityPatient);
            return ret;

        }
    }
}
