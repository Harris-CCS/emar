using System.Security.Principal;

namespace PulseCheck.IUtilities
{
    public interface IAuthUtility
    {
        int GetAuthenticatedUserId(IPrincipal User);
        byte GetAuthenticatedUserSite(IPrincipal User);
        bool ValidateDomainCredentials(string username, string password, string domain);
    }
}
