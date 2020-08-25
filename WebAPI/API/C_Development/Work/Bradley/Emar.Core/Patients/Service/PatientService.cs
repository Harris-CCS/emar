using System.Collections.Generic;
using System.Linq;
using Emar.Core.Helpers;
using Emar.Core.Options.Repository;
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
        private IOptionRepository _optionRepository;

        public PatientService(IPatientRepository patientRepository, IOptionRepository optionRepository)
        {
            _patientRepository = patientRepository;
            _optionRepository = optionRepository;
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
                patientList.Add(PatientMapper.MapPatient(patient, _optionRepository.GetOption(patient.SiteId, AppConstants.LongDateFormat)));
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

            PatientDto patientDto = PatientMapper.MapPatient(patient, _optionRepository.GetOption(patient.SiteId, AppConstants.LongDateFormat));

            return patientDto;
        }

        /// <summary>
        /// Implemented to retrieve a patient by Site/Ibex instead of the Internal Id
        /// </summary>
        /// <param name="extId1">The external Site ID</param>
        /// <param name="extId2">the external Ibex number of the patient</param>
        /// <param name="includeOrders">Should orders be included with the patient output</param>
        /// <returns></returns>
        public PatientDto GetPatient(short extId1, string extId2, bool includeOrders)
        {
            var patientId = _patientRepository.GetInternalPatientId(extId1, extId2);
            if (patientId == 0)
                return null;

            Patient patient = _patientRepository.GetPatient(patientId, null, includeOrders);
            PatientDto patientDto = PatientMapper.MapPatient(patient, _optionRepository.GetOption(patient.SiteId, AppConstants.LongDateFormat));

            return patientDto;
        }

        /// <summary>
        /// Implemented to retrieve a patient by Account Number, Custom Number or Person Number instead of the Internal Id
        /// </summary>
        /// <param name="number">Number to search for the patient with - [getPatientBy] determines what kind of number this is</param>
        /// <param name="getPatientBy">Type of number to search. Values include: Id, MedicalRecordNumber,
        /// AccountNumber, CustomNumber, PersonNumber</param>
        /// <param name="includeOrders">Should orders be included with the patient output</param>
        /// <returns></returns>
        public PatientDto GetPatientByNumber(string number, GetPatientBy getPatientBy, bool includeOrders)
        {
            Patient patient = _patientRepository.GetPatientByNumber(number, getPatientBy, includeOrders);
            PatientDto patientDto = PatientMapper.MapPatient(patient, _optionRepository.GetOption(patient.SiteId, AppConstants.LongDateFormat));

            return patientDto;
        }

        public Dictionary<string, string> GetExternalRootSitePatientId(string number, GetPatientBy getPatientBy, string rootType)
        {
            return _patientRepository.GetExternalRootSitePatientId(number, getPatientBy, rootType);
        }
    }
}
