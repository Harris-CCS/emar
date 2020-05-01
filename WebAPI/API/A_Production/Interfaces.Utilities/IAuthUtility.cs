using System.Security.Principal;
namespace Interfaces.Utilities
{
    public interface IAuthUtility
    {
        int GetAuthenticatedUserId(IPrincipal User);
        byte GetAuthenticatedUserSite(IPrincipal User);
        bool ValidateDomainCredentials(string username, string password, string domain);
    }
}
