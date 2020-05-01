using System.Collections.Generic;
using System.Threading.Tasks;
using DomainModel;

namespace Interfaces.Services
{
    public interface ISiteService
    {
        Task<Site> GetSiteByIdAsync(byte siteId, bool includeDetails = false);
        Task<List<Site>> GetSitesAsync(string login, bool includeDetails = false);
        Task<List<Site>> GetSitesBySubjectIdAsync(string subjectId);
        Task<List<Department>> GetDepartmentsAsync(byte siteId, bool includeDetails = false);
        Task<Department> GetDepartmentByKeyAsync(byte siteId, string dept, User user, bool includePatients = false);
        Task<Department> GetDepartmentByKeyAsync(byte siteId, string dept, User user, string filter = "");
        Task<Department> GetDepartmentByKeyForMTBAsync(byte siteId, string dept, User user, string filter = "");
        Task<List<Patient>> GetPatientsBySiteAndDeptAsync(byte siteId, string dept, User user, string expand = "");
        Task<List<Location>> GetAvailableLocationsBySiteIdAsync(byte siteId, string dept = null);
        Task<List<Location>> GetShareLocationsBySiteIdAsync(byte siteId, string dept = null);
        Task<List<Group>> GetMedPathwaysBySiteIdAsync(byte siteId);
        Task<List<Group>> GetOrderPathwaysBySiteIdAsync(byte siteId);
        Task<Dictionary<string, MetaData>> GetMedMetaDataBySiteIdAsync(byte siteId);
        Task<List<SiteElement>> GetCommentsBySiteIdAsync(byte siteId);
        Task<Dictionary<string, string>> GetSignupInfo(byte siteId, User user);
        Task<List<ClinicalPathway>> SearchClinicalPathwaysBySiteIdAsync(byte siteId, string search, int limit = 100);
        Task<List<string>> SearchMedicationBrandsBySiteIdAsync(byte siteId, string search, int limit = 100);
        Task<List<Service>> SearchOrdersBySiteIdAsync(byte siteId, string brand, int userId, int limit = 100);
        Task<ClinicalPathway> GetPathway(byte siteId, int pathwayNum);
    }
}
