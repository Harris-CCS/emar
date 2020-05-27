using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using PulseCheck.Domain;
using PulseCheck.ILogic;
using PulseCheck.IRepository;
using PulseCheck.Utilities;

namespace PulseCheck.Logic
{
    public class SiteManager : ISiteManager
    {
        private readonly ISiteRepository _siteRepository;
        private readonly IDepartmentRepository _departmentRepository;
        private readonly IAreaRepository _areaRepository;
        private readonly UserAccountManager _userAccountService;
        private readonly IUserManager _userService;
        private readonly IPatientRepository _patientRepository;
        private readonly IUserMappingRepository _userMappingRepository;

        /// <summary>
        /// Site service constructor
        /// </summary>
        /// <param name="siteRepository">ISiteRepository instance</param>
        /// <param name="departmentRepository">IDepartmentRepository instance</param>
        /// <param name="areaRepository">IAreaRepository instance</param>
        /// <param name="userAccount">UserAccount instance</param>
        /// <param name="userService">IUserService Instance</param>
        /// <param name="patientRepository">IPatientRepository instance</param>
        /// <param name="userMappingRepository">IUserMappingRepository instance</param>
        public SiteManager(ISiteRepository siteRepository, IDepartmentRepository departmentRepository, IAreaRepository areaRepository, UserAccountManager userAccount, IUserManager userService, IPatientRepository patientRepository, IUserMappingRepository userMappingRepository)
        {
            _siteRepository = siteRepository;
            _departmentRepository = departmentRepository;
            _areaRepository = areaRepository;               // TODO: Would it make more sense to put this under the departmentRepository?
            _userAccountService = userAccount;
            _userService = userService;
            _patientRepository = patientRepository;
            _userMappingRepository = userMappingRepository;
        }

        /// <summary>
        /// Get a site by ID, optionally including extra details about the site (department information)
        /// </summary>
        /// <param name="siteId">Site idenitifer</param>
        /// <param name="includeDetails">Boolean flag for whether details should be included</param>
        /// <returns>Site object</returns>
        public async Task<Site> GetSiteByIdAsync(byte siteId, bool includeDetails = false)
        {
            var result = await _siteRepository.GetSiteByIdAsync(siteId);

            if (result != null)
                result.Rules = GetSiteRules(result.Id);

            if (!includeDetails)
            {
                return result;
            }

            result.Departments = await GetDepartmentsAsync(result.Id);

            return result;
        }

        /// <summary>
        /// Get a list of sites that a user can access, based on their login
        /// </summary>
        /// <param name="login">The user's login</param>
        /// <param name="includeDetails">Boolean flag for whether extra site details should be included</param>
        /// <returns>List of Site objects</returns>
        public async Task<List<Site>> GetSitesAsync(string login, bool includeDetails = false)
        {
            var result = (await _userMappingRepository.GetMappedSites(login)).ToList();

            if (!includeDetails)
            {
                return result;
            }

            // Get the Departments
            foreach (var site in result)
            {
                site.Departments = await GetDepartmentsAsync(site.Id);
                site.Rules = GetSiteRules(site.Id);
            }

            return result;
        }

        /// <summary>
        /// Get a list of sites that a user can access, based on their subject ID
        /// </summary>
        /// <param name="subjectId">User's subject ID</param>
        /// <returns>List of Site objects</returns>
        public async Task<List<Site>> GetSitesBySubjectIdAsync(string subjectId)
        {
            Guid id;
            if (!Guid.TryParse(subjectId, out id))
            {
                return null;
            }

            var userAccount = _userAccountService.GetByID(id);
            var sites = await _userMappingRepository.GetMappedSites(userAccount.Username);

            foreach (Site site in sites)
            {
                site.Rules = GetSiteRules(site.Id);
            }

            return sites;
        }

        private IEnumerable<SiteRule> GetSiteRules(long siteId)
        {
            return new List<SiteRule>()
            {
                new SiteRule()
                {
                    Id = 1,
                    SiteId = siteId,
                    Category = "Mobile:Authentication",
                    Name = "OrderAuth:DisplayPinPadFirst",
                    BoolValue = true
                },
                new SiteRule()
                {
                    Id = 2,
                    SiteId = siteId,
                    Category = "Mobile:Authentication",
                    Name = "LoginAuth:DisplayPinPadFirst",
                    BoolValue = false
                }
            };
        }

        /// <summary>
        /// Get a list of comments defined for a site
        /// </summary>
        /// <param name="siteId">Site identifier</param>
        /// <returns>List of SiteElement objects</returns>
        public async Task<List<SiteElement>> GetCommentsBySiteIdAsync(byte siteId)
        {
            var result = await _siteRepository.GetCommentsBySiteIdAsync(siteId);
            return result;
        }

        /// <summary>
        /// Get the meta data associated with medication ordering
        /// </summary>
        /// <param name="siteId">Site identifier</param>
        /// <returns>Dictionary of MetaData objects</returns>
        public async Task<Dictionary<string, MetaData>> GetMedMetaDataBySiteIdAsync(byte siteId)
        {
            var result = await _siteRepository.GetMedMetaDataBySiteIdAsync(siteId);
            return result;
        }

        /// <summary>
        /// Get a list of med groups/pathways defined for a site
        /// </summary>
        /// <param name="siteId">Site identifier</param>
        /// <returns>List of Group objects</returns>
        public async Task<List<Group>> GetMedPathwaysBySiteIdAsync(byte siteId)
        {
            var result = await _siteRepository.GetMedPathwaysBySiteIdAsync(siteId);
            return result;
        }

        /// <summary>
        /// Get a list of order pathways defined for a site
        /// </summary>
        /// <param name="siteId">Site identifier</param>
        /// <returns>List of Group objects</returns>
        public async Task<List<Group>> GetOrderPathwaysBySiteIdAsync(byte siteId)
        {
            var result = await _siteRepository.GetOrderPathwaysBySiteIdAsync(siteId);
            return result;
        }

        /// <summary>
        /// Put the services in their correct groups
        /// </summary>
        /// <param name="siteId">Site identifier</param>
        /// <param name="services">List of services to be grouped</param>
        /// <returns></returns>
        public List<Group> GroupOrderServices(byte siteId, List<Service> services)
        {
            var groupNames = new DB.Select
            {
                Sql = "SELECT cde.num,cde.name FROM cde inner join org on org.svccs=cde.site WHERE type = @type AND org.site = @site AND cde.status = 'A'",
                Parameters = new SqlParameter[]
                {
                    new SqlParameter("@type", SqlDbType.Char) { Value = "S"},
                    new SqlParameter("@site", SqlDbType.TinyInt) { Value = siteId },
                }
            }.RunForDictionary("num");

            var groups = new Dictionary<int, Group>();
            foreach (var service in services)
            {
                if (!groups.ContainsKey(service.Type))
                    groups.Add(service.Type, new Group { Name = service.Type == 0 ? "Common" : groupNames[service.Type.ToString()]["name"] });

                groups[service.Type].Services.Add(service);
            }

            return groups.Select(g => g.Value).OrderBy(g => g.Name).ToList();
        }

        public async Task<ClinicalPathway> GetPathway(byte siteId, int pathwayNum)
        {
            var clinicalPathway = await _siteRepository.GetOrderPathwayByIdAsync(siteId, pathwayNum);

            var sql = @"
                SELECT
                    svc.code, svc.svctype, svc.name, svc.face, svc.maxqty, svc.svc
                FROM
                    grp 
                    INNER JOIN svc ON grp.code = svc.code and svc.site = grp.site
                    INNER JOIN cde ON cde.num = svc.svctype
                    inner join org on org.svccs = grp.site
                WHERE
                        grp.num = @num 
                    AND grp.code = svc.code
                    AND grp.type = 'S'
                    AND org.site = @site
                    AND svc.status = 'A'
                ORDER BY
                    checkde desc, altcode, svc.name";

            var serviceInfo = new DB.Select
            {
                Sql = sql,
                Parameters = new SqlParameter[]
                {
                    new SqlParameter("@num", SqlDbType.Int) { Value = pathwayNum },
                    new SqlParameter("@site", SqlDbType.TinyInt) { Value = siteId }
                }
            }.RunForListOfDictionaries();

            var services = serviceInfo.Select(s => new Service
            {
                Number = Convert.ToInt32(s["svc"]),
                Code = s["code"],
                Name = s["name"],
                Type = Convert.ToInt32(s["svctype"]),
                InterfaceType = s["face"],
                MaxQuantity = Convert.ToInt32(s["maxqty"]),
            }).ToList();

            var groups = GroupOrderServices(siteId, services);
            clinicalPathway.Groups.AddRange(groups);

            return clinicalPathway;
        }

        /// <summary>
        /// Get a list of Departments in a site
        /// </summary>
        /// <param name="siteId">Site identifier</param>
        /// <param name="includeDetails">Boolean flag for whether extra details should be included</param>
        /// <returns>List of Department objects</returns>
        public async Task<List<Department>> GetDepartmentsAsync(byte siteId, bool includeDetails = false)
        {
            var result = await _departmentRepository.GetDepartmentsBySiteIdAsync(siteId);

            if (includeDetails)
            {
                //Get the areas    
            }

            return result;
        }

        /// <summary>
        /// Get a single department from a site
        /// </summary>
        /// <param name="siteId">Site identifier</param>
        /// <param name="dept">Department identifier</param>
        /// <param name="user">User object</param>
        /// <param name="includePatients">Boolean flag for whether patients should be included in the return</param>
        /// <returns>Department object</returns>
        public async Task<Department> GetDepartmentByKeyAsync(byte siteId, string dept, User user, bool includePatients = false)
        {
            var result = await _departmentRepository.GetDepartmentByKeyAsync(dept, siteId, true);

            if (result != null && includePatients)
            {
                result.Patients = await GetPatientsBySiteAndDeptAsync(siteId, dept, user, null);
            }

            return result;
        }

        /// <summary>
        /// Get department by key
        /// </summary>
        /// <param name="siteId">Site identifier</param>
        /// <param name="dept">Department name/key</param>
        /// <param name="user">User object</param>
        /// <returns></returns>
        public async Task<Department> GetDepartmentByKeyAsync(byte siteId, string dept, User user)
        {
            return await GetDepartmentByKeyAsync(siteId, dept, user, "all");
        }

        /// <summary>
        /// Get department by key with data expansion
        /// </summary>
        /// <param name="siteId">Site identifier</param>
        /// <param name="dept">Department name/key</param>
        /// <param name="user">User object</param>
        /// <param name="expand">Expansion parameter</param>
        /// <returns>Department object</returns>
        public async Task<Department> GetDepartmentByKeyAsync(byte siteId, string dept, User user, string expand)
        {
            var result = await _departmentRepository.GetDepartmentByKeyAsync(dept, siteId, true);
            if (result != null)
            {
                result.Patients = await GetPatientsBySiteAndDeptAsync(siteId, dept, user, expand);
            }

            return result;
        }

        /// <summary>
        /// Get department listing for the MTB
        /// </summary>
        /// <param name="siteId">Site identifier</param>
        /// <param name="dept">Department identifier</param>
        /// <param name="user">User object</param>
        /// <param name="filter">Optional filter</param>
        /// <returns></returns>
        public async Task<Department> GetDepartmentByKeyForMTBAsync(byte siteId, string dept, User user, string filter)
        {
            var result = await _departmentRepository.GetDepartmentByKeyAsync(dept, siteId, false);
            if (result != null)
            {
                result.Patients = await _patientRepository.GetPatientsBySiteAndDeptForMTBAsync(siteId, dept, user, filter);
            }

            return result;
        }

        /// <summary>
        /// Get a list of patients in a certain site and department
        /// </summary>
        /// <param name="siteId">Site identifier</param>
        /// <param name="dept">Department name</param>
        /// <param name="user">User object</param>
        /// <param name="expand">Data expansion parameter</param>
        /// <returns>List of Patient objects</returns>
        public async Task<List<Patient>> GetPatientsBySiteAndDeptAsync(byte siteId, string dept, User user, string expand)
        {
            return await _patientRepository.GetPatientsBySiteAndDeptAsync(siteId, dept, user, expand);
        }

        /// <summary>
        /// Get a list of available locations in a certain site and (optionally) department
        /// </summary>
        /// <param name="siteId">Site identifier</param>
        /// <param name="dept">Optional department name</param>
        /// <returns>List of Location objects</returns>
        public async Task<List<Location>> GetAvailableLocationsBySiteIdAsync(byte siteId, string dept = null)
        {
            var result = await _siteRepository.GetAvailableLocationsBySiteIdAsync(siteId, dept);
            return result;
        }

        /// <summary>
        /// Get a list of all locations in a certain site and (optionally) department
        /// </summary>
        /// <param name="siteId">Site identifier</param>
        /// <param name="dept">Optional department name</param>
        /// <returns>List of Location objects</returns>
        public async Task<List<Location>> GetShareLocationsBySiteIdAsync(byte siteId, string dept = null)
        {
            var result = await _siteRepository.GetShareLocationsBySiteIdAsync(siteId, dept);
            return result;
        }

        /// <summary>
        /// Get signup information dictionary for a site and user
        /// </summary>
        /// <param name="siteId">Site identifier</param>
        /// <param name="user">User object</param>
        /// <returns>Information dictionary</returns>
        public async Task<Dictionary<string, string>> GetSignupInfo(byte siteId, User user)
        {
            var result = await _siteRepository.GetSignupInfo(siteId, user);
            return result;
        }

        /// <summary>
        /// Search for clinical pathways by substring match on name
        /// </summary>
        /// <param name="siteId">Site identifier</param>
        /// <param name="name">Search name</param>
        /// <param name="limit">Optional result limit</param>
        /// <returns>List of matching ClinicalPathways</returns>
        public async Task<List<ClinicalPathway>> SearchClinicalPathwaysBySiteIdAsync(byte siteId, string name, int limit = 100)
        {
            var result = await _siteRepository.SearchClinicalPathwaysBySiteIdAsync(siteId, name, limit);
            return result;
        }

        /// <summary>
        /// Search for medications by substring match on name
        /// </summary>
        /// <param name="siteId">Site identifier</param>
        /// <param name="name">Search name</param>
        /// <param name="limit">Optional result limit</param>
        /// <returns>List of matching ClinicalPathways</returns>
        public async Task<List<string>> SearchMedicationBrandsBySiteIdAsync(byte siteId, string brand, int limit = 100)
        {
            var result = await _siteRepository.SearchMedicationBrandsBySiteIdAsync(siteId, brand, limit);
            return result;
        }

        /// <summary>
        /// Search for medications by substring match on name
        /// </summary>
        /// <param name="siteId">Site identifier</param>
        /// <param name="name">Search name</param>
        /// <param name="limit">Optional result limit</param>
        /// <returns>List of matching Orders</returns>
        public async Task<List<Service>> SearchOrdersBySiteIdAsync(byte siteId, string name, int userId, int limit = 100)
        {
            var result = await _siteRepository.SearchOrdersBySiteIdAsync(siteId, name, limit, userId);
            return result;
        }
    }
}
