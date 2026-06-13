using Emar.Core.InboundData.Model.Mappings;
using Emar.Core.Patients.Repository;
using Emar.Core.Users.Repository;
using Emar.Data.Entities;

namespace Emar.Core.InboundData.Service
{
    public class IdsEmarUpdateService : IIdsEmarUpdateService
    {
        private readonly IUserRepository _userRepository;
        private readonly IPatientRepository _patientRepository;

        public IdsEmarUpdateService(IUserRepository userRepository, IPatientRepository patientRepository)
        {
            _userRepository = userRepository;
            _patientRepository = patientRepository;
        }

        public void FileUser(User inboundUser)
        {
            // Figure out if this is a new user
            inboundUser.Id = _userRepository.GetInternalUserId(inboundUser.ExternalId);

            // Map the InboundUserDataDto to the Emar.Data.Entities.User and save to the DB
            //User user = IdsGenericMapper.MapInboundUserDataDto(userId, inboundUser, _userRepository);
            
            _userRepository.FileUser(inboundUser, inboundUser.ExternalId);
        }

        public void DeactivateUser(string externalId)
        {
            // Figure out if this is an existing user
            var userId = _userRepository.GetInternalUserId(externalId);
            if (userId == 0)
                // it isn't an existing user, we're done
                return;

            _userRepository.DeactivateUser(userId);
        }

        public void DeactivatePatient(string externalId)
        {
            // Figure out if this is an existing patient
            var patientId = _patientRepository.GetInternalPatientId(externalId);
            if (patientId == 0)
                // it isn't an existing user, we're done
                return;

            _patientRepository.DeactivatePatient(patientId);
        }

        public void FilePatient(Patient patient)
        {
            SetInternalPatientId(patient);
            _patientRepository.FilePatient(patient);
        }

        public void FilePatientIndicators(Patient patient)
        {
            SetInternalPatientId(patient);
            _patientRepository.FilePatientIndicators(patient);
        }

        private void SetInternalPatientId(Patient patient)
        {
            // Figure out if this is a new user
            var patientId = _patientRepository.GetInternalPatientId(
                patient.ExternalSiteId,
                patient.ExternalPatientId);

            patient.Id = patientId;
        }
    }
}