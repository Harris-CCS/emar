using System.Collections.Generic;
using System.Linq;
using Emar.Core.Patients.Repository;
using Emar.Core.ResourceParameters;
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

        public OrderingPhysicianDataDto GetOrderingPhysicians(int siteId, long patientId, int userId)
        {
            //This is a copy of GetOrderingPhysicians(int siteId, long patientId) that adds the logged in user id as a parameter.
            //We'll use that to return the logged in user if they happen to be a doctor.
            //Then we'll only look for the pateint's attending if this user isn't a doctor.
            //Winston Murdock, 01/18/2022.  PC-26918

            var physicians = _userRepository.GetOrderingPhysicians(siteId).ToList();

            if (!physicians.Any())
                return null;

            int? attendingDoctorId = null;

            //See if the current user is a doctor or not.
            //Make sure we have a userId.
            if (userId > 0)
            {
                //A type of "D" signifies a doctor.
                //All of the other types are for non doctors.
                var sType = _userRepository.GetUser(userId).Type;
                if (sType == "D")
                {
                    //This user is a doctor.
                    //Set attendingDoctorId to userId so that we default the selection to that user.
                    attendingDoctorId = userId;
                } //end if
            } //end if

            //If we've already set attendingDoctorId to userId above, then don't attempt to get the user's attending doctor.
            if (attendingDoctorId == null)
            {
                if (patientId > 0)
                {
                    attendingDoctorId = _patientRepository.GetPatientsAttendingDoctorIdByRole(patientId, "DOCTOR2");
                } //end if
            } //end if

            var ret = new OrderingPhysicianDataDto
            {
                //Sort the list of ordering physicians by last name and then by first name.
                //Winston Murdock, 04/12/2021.  EMAR-876
                AvailableOrderingPhysicians = physicians.Select(u => UserMapper.MapUser(u, _userRepository)).OrderBy(a => a.LastName).ThenBy(b => b.FirstName),

                //If we have a user we want to default in the list (either the current user if they are a doctor)
                //or the patient's assigned attending doctor, then return that.
                PatientsErAttendingDoc = attendingDoctorId
            };

            return ret;
        }

        public string GetUserDefaultMarFilters(int userId)
        {
            //Get the user's default MAR filters from the user_settings table.
            string sRet = _userRepository.GetUserDefaultMarFilters(userId);

            return sRet;
        } //end GetUserDefaultMarFilters

        public string SetUserDefaultFilters(int userId, PatientsResourceParameters resourceParameters)
        {
            //Set (and return) the user's default MAR filters from the user settings table.
            string sRet = _userRepository.SetUserDefaultFilters(userId, resourceParameters);

            return sRet;
        } //end SetUserDefaultFilters
    }
}