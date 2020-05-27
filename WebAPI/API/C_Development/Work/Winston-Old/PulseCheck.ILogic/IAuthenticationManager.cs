using System.Threading.Tasks;
using PulseCheck.Domain;

namespace PulseCheck.ILogic
{
    public interface IAuthenticationManager
    {
        Task<User> GetValidatedWebUser();
    }
}
