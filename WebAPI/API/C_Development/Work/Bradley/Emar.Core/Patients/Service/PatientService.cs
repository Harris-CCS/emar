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

        public PagedList<PatientDto> GetPatients(ResourceParameters resourceParameters, bool includeOrders)
        {
            var patients = _patientRepository.GetPatients(resourceParameters, includeOrders);

            if ((patients == null) ||
                (!patients.Any()))
            {
                return null;
            }

            var patientList = new List<PatientDto>();

            foreach (Patient patient in patients)
            {
                patientList.Add(PatientMapper.MapPatient(patient));
            }

            return new PagedList<PatientDto>(patientList, patients.TotalCount, patients.CurrentPage, patients.PageSize);
        }

        public PatientDto GetPatient(long patientId, ResourceParameters resourceParameters, bool includeOrders)
        {
            Patient patient = _patientRepository.GetPatient(patientId, resourceParameters, includeOrders);

            if (patient == null)
            {
                return null;
            }

            PatientDto patientDto = PatientMapper.MapPatient(patient);

            return patientDto;
        }

        public PatientDto GetPatient(short site, string ibex)
        {
            var patientId = _patientRepository.GetInternalPatientId(site, ibex);
            if (patientId == 0)
                return null;

            Patient patient = _patientRepository.GetPatient(patientId, null, false);
            PatientDto patientDto = PatientMapper.MapPatient(patient);

            return patientDto;
        }
    }
}
