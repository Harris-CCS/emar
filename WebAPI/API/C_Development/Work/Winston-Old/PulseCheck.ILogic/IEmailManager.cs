using System.Threading.Tasks;
using PulseCheck.Domain.Membership;

namespace PulseCheck.ILogic
{
    public interface IEmailManager
    {
        Task<bool> SendNewAccountEmail(UserAccount account, string accessToken, string devicePasscode);

        Task<bool> SendAccountPasswordResetEmail(UserAccount account, string accessToken);

        Task<bool> SendDeviceAuthorizationEmail(UserAccount account, string devicePasscode);
    }
}
