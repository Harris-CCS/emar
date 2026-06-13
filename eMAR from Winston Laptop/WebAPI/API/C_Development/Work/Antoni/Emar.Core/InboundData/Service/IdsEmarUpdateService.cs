using Emar.Core.InboundData.Model;
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

        public void FileUser(InboundUserDataDto inboundUserDataDto)
        {
            // Figure out if this is a new user
            var userId = _userRepository.GetInternalUserId(inboundUserDataDto.ExternalId);

            // Map the InboundUserDataDto to the Emar.Data.Entities.User and save to the DB
            User user = IdsGenericMapper.MapInboundUserDataDto(userId, inboundUserDataDto, _userRepository);
            
            _userRepository.FileUser(user, inboundUserDataDto.ExternalId);
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

        public void FilePatient(InboundPatientDataDto inboundPatientDataDto)
        {
            // Figure out if this is a new user
            var patientId = _patientRepository.GetInternalPatientId(inboundPatientDataDto.ExternalId);

            // Map the InboundPatientDataDto to the Emar.Data.Entities.Patient and save to the DB
            Patient patient = IdsGenericMapper.MapInboundPatientDataDto(patientId, inboundPatientDataDto);

            _patientRepository.FilePatient(patient, inboundPatientDataDto.ExternalId);
        }
    }
}