using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using PulseCheck.Domain;
using PulseCheck.IRepository;

namespace PulseCheck.Data.Repositories
{
    public class UserRepository : BaseRepository, IUserRepository
    {
        public UserRepository(IbexContext context) : base(context)
        {

        }

        /// <summary>
        /// Get users with ids matching in the provided list of ids
        /// </summary>
        /// <param name="userIds">List of user ids to match</param>
        /// <returns>List of matching User objects</returns>
        public async Task<List<User>> GetUsersByIdAsync(List<int> userIds)
        {
            return await _context.Users.Where(u => userIds.Contains(u.Id)).ToListAsync();
        }

        /// <summary>
        /// Get a single user by id
        /// </summary>
        /// <param name="id">User identifier</param>
        /// <returns>User object</returns>
        public async Task<User> GetUserByIdAsync(int id)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
        }

        /// <summary>
        /// Get a user's favorite orders
        /// </summary>
        /// <param name="user">User object</param>
        /// <returns>List of Group objects</returns>
        public async Task<List<Service>> GetUserFavoriteOrders(User user)
        {
            var result = await _context.GetUserFavoriteOrders(user.Id).ToListAsync();
            return result.Select(x => new Service {
                Code = x.Code,
                Name = x.Name,
                MaxQuantity = x.MaxQty,
                IsUserFavorite = true,
                Number = x.Number,
                InterfaceType = x.Face
            }).OrderBy(o => o.Name).ToList();
        }

        /// <summary>
        /// Add an order to a user's list of favorites
        /// </summary>
        /// <param name="user">User object</param>
        /// <param name="num">Order identifier</param>
        /// <returns>int for result</returns>
        public int AddUserFavoriteOrder(User user, int num)
        {
            var added = _context.AddUserFavoriteOrder(user.Id, num);
            return added.FirstOrDefault();
        }

        /// <summary>
        /// Remove an order from a user's list of favorites
        /// </summary>
        /// <param name="user">User object</param>
        /// <param name="num">Order identifier</param>
        /// <returns>int for result</returns>
        public int RemoveUserFavoriteOrder(User user, int num)
        {
            return _context.RemoveUserFavoriteOrder(user.Id, num).FirstOrDefault();
        }

        /// <summary>
        /// Search PulseCheck users based on name
        /// </summary>
        /// <param name="name">Search value that will match on the first or last name</param>
        /// <returns>list of users whose first or last name matches the search</returns>
        public async Task<List<User>> SearchUsersForAccount(string login, string name)
        {
            var mappedUsers = await _context.UserMapping.ToListAsync();

            var existingMaps = mappedUsers.Select(u => u.UserNum).ToList();

            // Don't search sites where the account already has a mapping or mobile is turned off
            var ineligibleSites = mappedUsers.Where(a => a.Login == login).Select(um => um.SiteId).ToList();
            var allSites = await _context.Sites.Where(u => !ineligibleSites.Contains(u.Id)).ToListAsync();
            foreach (var site in allSites)
            {
                if (site.GetOrgOption("HANDHELD") != "Y")
                    ineligibleSites.Add(site.Id);
            }

            return await _context.Users
                .Where(u => u.FirstName.StartsWith(name) || u.LastName.StartsWith(name))
                .Where(u => u.Status.Code.Equals(Constants.ACTIVE_STATUS.Code))
                .Where(u => !ineligibleSites.Contains(u.SiteId))
                .Where(u => !existingMaps.Contains(u.Id))
                .OrderBy(u => u.LastName)
                    .ThenBy(u => u.FirstName)
                .Take(50)
                .ToListAsync();
        }
    }
}
