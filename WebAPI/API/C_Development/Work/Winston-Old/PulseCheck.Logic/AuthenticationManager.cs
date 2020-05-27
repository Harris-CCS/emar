using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Web;
using PulseCheck.Domain;
using PulseCheck.ILogic;
using PulseCheck.IRepository;
using PulseCheck.Utilities;

namespace PulseCheck.Logic
{
    public class AuthenticationManager : IAuthenticationManager
    {
        private readonly IUserRepository _userRepository;

        /// <summary>
        /// Site service constructor
        /// </summary>
        /// <param name="userRepository">IUserRepository instance</param>
        public AuthenticationManager(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<User> GetValidatedWebUser()
        {
            var request = HttpContext.Current.Request;
            var cookie = request.Cookies.Get(PulseCheck.Constants.Web.PulseCheckCookieName);
            if (cookie == null)
                return null;

            var userId = new DB.Select
            {
                Sql = "select usr from ath where athkey = @key",
                Parameters = new[]
                {
                    new SqlParameter("@key", SqlDbType.VarChar) { Value = cookie.Value.ToString().Substring(1) }
                }
            }.RunForInt();

            var user = await _userRepository.GetUserByIdAsync(userId);
            return user;
        }
    }
}
