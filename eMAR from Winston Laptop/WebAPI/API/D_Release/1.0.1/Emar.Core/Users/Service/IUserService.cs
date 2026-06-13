using System.Collections.Generic;
using Emar.Core.Users.Model;
using Emar.Data.Entities;

namespace Emar.Core.Users.Service
{
    public interface IUserService
    {
        UserDto GetUser(int userId);
        //UserMinimalDto GetUserMinimal(int userId);
        UserDto GetUser(string loginName);
        UserDto GetUserByExternalId(string extId);
        OrderingPhysicianDataDto GetOrderingPhysicians(int siteId, long patientId);
    }
}