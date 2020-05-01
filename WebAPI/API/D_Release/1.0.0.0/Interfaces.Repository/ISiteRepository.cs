using System.Collections.Generic;
using System.Threading.Tasks;
using DomainModel;

namespace Interfaces.Repository
{
    public interface ISiteRepository
    {
        Task<IEnumerable<Site>> GetSitesAsync();
        Task<Site> GetSiteByIdAsync(byte id);
        Task<List<SiteElement>> GetCommentsBySiteIdAsync(byte id);
        Task<List<Location>> GetAvailableLocationsBySiteIdAsync(byte id, string dept = null);
        Task<List<Location>> GetShareLocationsBySiteIdAsync(byte id, string dept = null);
        Task<Dictionary<string, string>> GetSignupInfo(byte id, User user);
        Task<List<Group>> GetMedPathwaysBySiteIdAsync(byte id);
        Task<List<Group>> GetOrderPathwaysBySiteIdAsync(byte id);
        Task<Dictionary<string, MetaData>> GetMedMetaDataBySiteIdAsync(byte id);
        Task<List<ClinicalPathway>> SearchClinicalPathwaysBySiteIdAsync(byte id, string name, int limit);
        Task<List<string>> SearchMedicationBrandsBySiteIdAsync(byte id, string brand, int limit);
        Task<ClinicalPathway> GetOrderPathwayByIdAsync(byte id, int pathwayId);
        Task<List<Service>> SearchOrdersBySiteIdAsync(byte siteId, string name, int limit, int userId);
    }
}
