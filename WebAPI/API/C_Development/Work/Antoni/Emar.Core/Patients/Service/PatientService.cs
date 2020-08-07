using System.Collections.Generic;
using System.Linq;
using Emar.Core.Helpers;
using Emar.Core.Patients.Model;
using Emar.Core.Patients.Model.Mappings;
using Emar.Core.Patients.Repository;
using Emar.Core.ResourceParameters;
using Emar.Data.Entities;
using static Emar.Core.Patients.Model.Constants;

namespace Emar.Core.Patients.Service
{
    public class PatientService : IPatientService
    {
        private readonly IPatientRepository _patientRepository;

        public PatientService(IPatientRepository patientRepository)
        {
            _patientRepository = patientRepository;
        }

        public PagedList<PatientDto> GetPatients(PatientsResourceParameters resourceParameters, bool includeOrders)
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

        public PatientDto GetPatient(long patientId, PatientsResourceParameters resourceParameters, bool includeOrders)
        {
            Patient patient = _patientRepository.GetPatient(patientId, resourceParameters, includeOrders);

            if (patient == null)
            {
                return null;
            }

            PatientDto patientDto = PatientMapper.MapPatient(patient);

            return patientDto;
        }

        /// <summary>
        /// Implemented to retrieve a patient by Site/Ibex instead of the Internal Id
        /// </summary>
        /// <param name="extId1">The external Site ID</param>
        /// <param name="extId2">the external Ibex number of the patient</param>
        /// <returns></returns>
        public PatientDto GetPatient(short extId1, string extId2)
        {
            var patientId = _patientRepository.GetInternalPatientId(extId1, extId2);
            if (patientId == 0)
                return null;

            Patient patient = _patientRepository.GetPatient(patientId, null, false);
            PatientDto patientDto = PatientMapper.MapPatient(patient);

            return patientDto;
        }

        /// <summary>
        /// Implemented to retrieve a patient by Account Number, Custom Number or Person Number instead of the Internal Id
        /// </summary>
        /// <param name="accountNumber"></param>
        /// <returns></returns>
        public PatientDto GetPatientByNumber(string number, GetPatientBy getPatientBy)
        {
            Patient patient = _patientRepository.GetPatientByNumber(number, getPatientBy);
            PatientDto patientDto = PatientMapper.MapPatient(patient);

            return patientDto;
        }

        public Dictionary<string, string> GetExternalRootSitePatientId(string number, GetPatientBy getPatientBy)
        {
            return _patientRepository.GetExternalRootSitePatientId(number, getPatientBy);
        }
    }
}
