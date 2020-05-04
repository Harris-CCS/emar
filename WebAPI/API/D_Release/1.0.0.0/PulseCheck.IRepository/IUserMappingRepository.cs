using System.Collections.Generic;
using System.Threading.Tasks;
using PulseCheck.Domain;

namespace PulseCheck.IRepository
{
    public interface IUserMappingRepository
    {
        Task<List<Site>> GetMappedSites(string login);
        Task<int> GetSiteLoginUserNum(string login, int siteId);
        Task<UserMapping> GetUserMappingInfo(string login, int siteId);
        Task<List<UserMapping>> GetUserMappingInfo(string login, string domain);
        bool RemoveAccountUser(string login, int userId);
        int AddAccountUser(string login, int userId);
        bool RemoveAllAccountUsers(string login);
    }
}
