using Emar.Core.Helpers;
using Emar.Core.Options.Model;
using Emar.Core.Options.Repository;
using Emar.Core.Orders.Repository;
using Emar.Core.Patients.Model;
using Emar.Core.Patients.Model.Mappings;
using Emar.Core.Patients.Repository;
using Emar.Core.ResourceParameters;
using Emar.Core.Templates.Model.Mappings;
using Emar.Core.Templates.Repository;
using System.Collections.Generic;
using System.Linq;
using static Emar.Core.Patients.Model.Constants;

namespace Emar.Core.Patients.Service
{
    public class PatientService : IPatientService
    {
        private readonly IPatientRepository _patientRepository;
        private readonly IOptionRepository _optionRepository;
        private readonly ITemplateRepository _templateRepository;
        private readonly IOrderRepository _orderRepository;

        public PatientService(IPatientRepository patientRepository, IOptionRepository optionRepository,
            ITemplateRepository templateRepository, IOrderRepository orderRepository)
        {
            _patientRepository = patientRepository;
            _optionRepository = optionRepository;
            _templateRepository = templateRepository;
            _orderRepository = orderRepository;
        }

        public PagedList<PatientDto> GetPatients(PatientsResourceParameters resourceParameters, bool includeOrders)
        {
            var patients = _patientRepository.GetPatients(resourceParameters, includeOrders);

            if ((patients == null) ||
                (!patients.Any()))
            {
                return null;
            }

            var patientList = (
                from patient in patients
                let siteId = patient.SiteId
                let drugDbVendor = _optionRepository.GetOption(siteId, OptionNames.DRUG_DB_VENDOR)
                let codeShareSites = _orderRepository.GetCodeShareSites(siteId).ToList()
                select PatientMapper.MapPatient(patient, drugDbVendor, null, codeShareSites, resourceParameters.UserId))
                .ToList();

            return new PagedList<PatientDto>(patientList, patients.TotalCount, patients.CurrentPage, patients.PageSize);
        }

        public PatientDto GetPatient(long patientId, PatientsResourceParameters resourceParameters, bool includeOrders,
            string orderLinkBase, string adminLinkBase)
        {
            var patient = _patientRepository.GetPatient(patientId, resourceParameters, includeOrders);

            if (patient == null)
            {
                return null;
            }

            var siteId = patient.SiteId;
            var drugDbVendor = _optionRepository.GetOption(siteId, OptionNames.DRUG_DB_VENDOR);
            var codeShareSites = _orderRepository.GetCodeShareSites(siteId).ToList();

            OrderActionMapperHelper actionHelper = null;
            if (includeOrders)
                actionHelper = new OrderActionMapperHelper(_templateRepository, siteId, orderLinkBase, adminLinkBase);

            var patientDto = PatientMapper.MapPatient(patient, drugDbVendor, actionHelper, codeShareSites, resourceParameters.UserId);

            return patientDto;
        }

        /// <summary>
        /// Implemented to retrieve a patient by Site/Ibex instead of the Internal Id
        /// </summary>
        /// <param name="extId1">The external Site ID</param>
        /// <param name="extId2">the external Ibex number of the patient</param>
        /// <param name="includeOrders">Should orders be included with the patient output</param>
        /// <param name="orderLinkBase"></param>
        /// <param name="adminLinkBase"></param>
        /// <param name="userId">The user passed to the mapper for the sake of the "MyPatients" property</param>
        /// <returns></returns>
        public PatientDto GetPatient(short extId1, string extId2, bool includeOrders,
            string orderLinkBase, string adminLinkBase, int userId)
        {
            var patientId = _patientRepository.GetInternalPatientId(extId1, extId2);
            if (patientId == 0)
                return null;

            var patient = _patientRepository.GetPatient(patientId, null, includeOrders);

            var siteId = patient.SiteId;
            var drugDbVendor = _optionRepository.GetOption(siteId, OptionNames.DRUG_DB_VENDOR);
            var codeShareSites = _orderRepository.GetCodeShareSites(siteId).ToList();

            OrderActionMapperHelper actionHelper = null;
            if (includeOrders)
                actionHelper = new OrderActionMapperHelper(_templateRepository, patient.SiteId, orderLinkBase, adminLinkBase);

            var patientDto = PatientMapper.MapPatient(patient, drugDbVendor, actionHelper, codeShareSites, userId);

            return patientDto;
        }

        /// <summary>
        /// Implemented to retrieve a patient by Account Number, Custom Number or Person Number instead of the Internal Id
        /// </summary>
        /// <param name="number">Number to search for the patient with - [getPatientBy] determines what kind of number this is</param>
        /// <param name="getPatientBy">Type of number to search. Values include: Id, MedicalRecordNumber,
        /// AccountNumber, CustomNumber, PersonNumber</param>
        /// <param name="includeOrders">Should orders be included with the patient output</param>
        /// <param name="orderLinkBase">Order Link Base for creating URLs for Order Actions</param>
        /// <param name="adminLinkBase">Admin Link Base for creating URLs for Administration Actions</param>
        /// <param name="userId">The user passed to the mapper for the sake of the "MyPatients" property</param>
        /// <returns></returns>
        public PatientDto GetPatientByNumber(string number, GetPatientBy getPatientBy, bool includeOrders,
            string orderLinkBase, string adminLinkBase, int userId)
        {
            var patient = _patientRepository.GetPatientByNumber(number, getPatientBy, includeOrders);
            var siteId = patient.SiteId;
            var drugDbVendor = _optionRepository.GetOption(patient.SiteId, OptionNames.DRUG_DB_VENDOR);
            var codeShareSites = _orderRepository.GetCodeShareSites(siteId).ToList();

            OrderActionMapperHelper actionHelper = null;
            if (includeOrders)
                actionHelper = new OrderActionMapperHelper(_templateRepository, patient.SiteId, orderLinkBase, adminLinkBase);

            var patientDto = PatientMapper.MapPatient(patient, drugDbVendor, actionHelper, codeShareSites, userId);

            return patientDto;
        }

        public Dictionary<string, string> GetExternalRootSitePatientId(string number, GetPatientBy getPatientBy, string rootType)
        {
            return _patientRepository.GetExternalRootSitePatientId(number, getPatientBy, rootType);
        }
    }
}
