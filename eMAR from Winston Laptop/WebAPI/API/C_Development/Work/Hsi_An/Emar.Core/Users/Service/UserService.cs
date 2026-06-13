using System.Collections.Generic;
using System.Linq;
using Emar.Core.Patients.Repository;
using Emar.Core.Users.Model;
using Emar.Core.Users.Model.Mappings;
using Emar.Core.Users.Repository;
using Emar.Data.Entities;

namespace Emar.Core.Users.Service
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IPatientRepository _patientRepository;

        public UserService(IUserRepository userRepository, IPatientRepository patientRepository)
        {
            _userRepository = userRepository;
            _patientRepository = patientRepository;
        }

        public IEnumerable<UserDto> GetUsers()
        {
            var users = _userRepository.GetUsers().ToList();

            var usersDto = users.Select(u => UserMapper.MapUser(u, _userRepository));

            return usersDto;
        }

        public UserDto GetUser(int userId)
        {
            var user = _userRepository.GetUser(userId);

            if (user == null)
                return null;

            var userDto = UserMapper.MapUser(user, _userRepository);

            return userDto;
        }

        public UserDto GetUser(string loginName)
        {
            User user = _userRepository.GetUser(loginName);

            if (user == null)
                return null;

            var userDto = UserMapper.MapUser(user, _userRepository);

            return userDto;
        }

        public UserDto GetUserByExternalId(string extId)
        {
            User user = _userRepository.GetUserByExternalId(extId);

            if (user == null)
                return null;

            var userDto = UserMapper.MapUser(user, _userRepository);

            return userDto;
        }

        public OrderingPhysicianDataDto GetOrderingPhysicians(int siteId, long patientId)
        {
            var physicians = _userRepository.GetOrderingPhysicians(siteId).ToList();

            if (!physicians.Any())
                return null;

            int? attendingDoctorId = null;
            if (patientId > 0)
            {
                attendingDoctorId = _patientRepository.GetPatientsAttendingDoctorIdByRole(patientId, "DOCTOR2");
            }

            var ret = new OrderingPhysicianDataDto
            {
                //Sort the list of ordering physicians by last name and then by first name.
                //Winston Murdock, 04/12/2021.  EMAR-876
                AvailableOrderingPhysicians = physicians.Select(u => UserMapper.MapUser(u, _userRepository)).OrderBy(a => a.LastName).ThenBy(b => b.FirstName),
                PatientsErAttendingDoc = attendingDoctorId
            };
            
            return ret;
        }
    }
}