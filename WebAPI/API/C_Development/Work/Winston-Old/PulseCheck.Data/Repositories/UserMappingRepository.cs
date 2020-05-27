using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using PulseCheck.Domain;
using PulseCheck.IRepository;

namespace PulseCheck.Data.Repositories
{
    public class UserMappingRepository : BaseRepository, IUserMappingRepository
    {
        public UserMappingRepository(IbexContext context) : base(context)
        {

        }

        /// <summary>
        /// Retrieve a list of sites that the provided login can access
        /// </summary>
        /// <param name="login">Login ID</param>
        /// <returns>List of Site objects</returns>
        public async Task<List<Site>> GetMappedSites(string login)
        {
            // Do not include sites where the user has exceeded the site's retry limit.
            var mappedSites = await _context.UserMapping.Where(u => u.Login == login).Where(u => u.HasMobile).ToListAsync();
            List <byte> siteNums = mappedSites.Where(x => x.Ctr < x.Retry).Select(x => x.SiteId).ToList();
            return await _context.Sites.Where(u => siteNums.Contains(u.Id)).ToListAsync();
        }

        /// <summary>
        /// Retrieve the user identifier associated with a login and site identifier in the PulseCheck database
        /// </summary>
        /// <param name="login">Login ID</param>
        /// <param name="siteId">Site identifier</param>
        /// <returns>User ID int</returns>
        public async Task<int> GetSiteLoginUserNum(string login, int siteId)
        {
            var details = await _context.UserMapping.Where(u => u.Login == login).Where(s => s.SiteId == siteId).ToListAsync();
            if (details.Count == 1)
            {
                return details[0].UserNum;
            }
            return 0;
        }

        /// <summary>
        /// Get all UserMapping information associated with a particular login (could return multiple sites)
        /// </summary>
        /// <param name="login">Login ID</param>
        /// <returns>List of UserMapping objects</returns>
        public async Task<List<UserMapping>> GetUserMappingInfo(string login, string domain)
        {
            var details = new List<UserMapping>();
            if (string.IsNullOrWhiteSpace(domain))
            {
                details = await _context.UserMapping.Where(u => u.Login == login && u.HasMobile).ToListAsync();
            } else
            {
                // In case the site doesn't have the users set up with domain\login, make it.
                // If they do, and it's sent is as domain\domain\login, that should be okay
                var fullLogin = domain + @"\" + login;
                details = await _context.UserMapping.Where(u => u.HasMobile && (u.DomainLogin == login || u.DomainLogin == fullLogin) && u.WindowsDomains.ToUpper().IndexOf(domain.ToUpper()) >= 0).ToListAsync();
            }

            return details;
        }

        /// <summary>
        /// Get the UserMapping information associated with a particular login and site identifier
        /// </summary>
        /// <param name="login">Login ID</param>
        /// <param name="siteId">Site identifier</param>
        /// <returns>UserMapping object</returns>
        public async Task<UserMapping> GetUserMappingInfo(string login, int siteId)
        {
            var details = await _context.UserMapping.Where(u => u.Login == login).Where(s => s.SiteId == siteId).ToListAsync();
            if (details.Count == 1)
            {
                return details[0];
            }
            return null;
        }

        /// <summary>
        /// Get the UserMapping information associated with a particular login and site identifier
        /// </summary>
        /// <param name="login">Login ID</param>
        /// <param name="siteId">Site identifier</param>
        /// <returns>UserMapping object</returns>
        public async Task<UserMapping> GetAccountUserMappingInfo(string login, int userId)
        {
            var details = await _context.UserMapping.Where(a => a.Login == login).Where(u => u.UserNum == userId).ToListAsync();
            if (details.Count == 1)
            {
                return details[0];
            }
            return null;
        }

        /// <summary>
        /// Remove a mapped user from an account
        /// </summary>
        /// <param name="login">The login of the account</param>
        /// <param name="userId">The ID of the PulseCheck user to remove</param>
        /// <returns>A bool indicating a single mapped user was removed</returns>
        public bool RemoveAccountUser(string login, int userId)
        {
            var entriesDeleted = _context.RemoveAccountUser(login, userId);
            return entriesDeleted.FirstOrDefault() == 1;
        }

        /// <summary>
        /// Remove all mapped users from an account
        /// </summary>
        /// <param name="login">The login of the account</param>
        /// <returns>A bool indicating there wasn't an issue removing any.  There could be zero mapped users, so we check for more than -1 entries removed.</returns>
        public bool RemoveAllAccountUsers(string login)
        {
            var entriesDeleted = _context.RemoveAllAccountUsers(login);
            return entriesDeleted.FirstOrDefault() > -1;
        }

        /// <summary>
        /// Add a mapped user to an account
        /// </summary>
        /// <param name="login">The login of the account</param>
        /// <param name="userId">The ID of the PulseCheck user to map to the account</param>
        /// <returns></returns>
        public int AddAccountUser(string login, int userId)
        {
            return _context.AddAccountUser(login, userId).FirstOrDefault();
        }
    }
}
