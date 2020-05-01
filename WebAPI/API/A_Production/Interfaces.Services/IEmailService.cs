using DomainModel.Membership;
using System.Threading.Tasks;

namespace Interfaces.Services
{
    public interface IEmailService
    {
        Task<bool> SendNewAccountEmail(UserAccount account, string accessToken, string devicePasscode);

        Task<bool> SendAccountPasswordResetEmail(UserAccount account, string accessToken);

        Task<bool> SendDeviceAuthorizationEmail(UserAccount account, string devicePasscode);
    }
}
