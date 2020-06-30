using System.Collections.Generic;
using System.Linq;
using Emar.Core.Patients.Model;
using Emar.Core.Patients.Model.Mappings;
using Emar.Core.Patients.Repository;
using Emar.Data.Entities;

namespace Emar.Core.Patients.Service
{
    public class PatientService : IPatientService
    {
        private readonly IPatientRepository _patientRepository;

        public PatientService(IPatientRepository patientRepository)
        {
            _patientRepository = patientRepository;
        }

        public IEnumerable<PatientDto> GetPatients(ResourceParameters resourceParameters)
        {
            var entityPatients = _patientRepository.GetPatients(resourceParameters);
            var patientList = new List<PatientDto>();

            foreach (Patient patient in entityPatients)
            {
                patientList.Add(PatientMapper.MapPatient(patient));
            }

            return patientList;
        }

        public PatientDto GetPatient(long patientId, ResourceParameters resourceParameters)
        {
            Patient entityPatient = _patientRepository.GetPatient(patientId, resourceParameters);
            PatientDto patientDto = PatientMapper.MapPatient(entityPatient);

            return patientDto;
        }
    }
}
