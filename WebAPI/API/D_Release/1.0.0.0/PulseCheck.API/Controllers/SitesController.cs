using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Http;
using DomainModel;
using Interfaces.Services;
using IdentityServer3.Core.Extensions;
using PulseCheck.Utilities;
using System.Linq;

namespace PulseCheck.API.Controllers
{ 
    /// <summary>
    /// Sites controller for PulseCheck API
    /// </summary>
    public class SitesController : ApiController
    {
        private readonly ISiteService _siteService;        
        private readonly IUserService _userService;
        private readonly Authentication _authUtil = new Authentication();

        /// <summary>
        /// SitesController constructor
        /// </summary>
        /// <param name="siteService"></param>
        /// <param name="userService"></param>
        public SitesController(ISiteService siteService, IUserService userService)
        {
            _siteService = siteService;
            _userService = userService;
        }

        /// <summary>
        /// Get a list of PulseCheck sites that can be accessed by the authenticated API user
        /// </summary>
        /// <remarks></remarks>
        /// <returns>
        /// List of sites
        /// </returns>
        /// <response code="200"></response>
        [VersionedRoute("api/sites", 1)]
        [Route("api/v1/sites")]
        [HttpGet]
        public async Task<IEnumerable<Site>> GetV1()
        {
            var caller = User as ClaimsPrincipal;
            var subjectId = caller.GetSubjectId();
            return await _siteService.GetSitesBySubjectIdAsync(subjectId);
        }

        /// <summary>
        /// Get detailed information about a particular PulseCheck site
        /// </summary>
        /// <remarks></remarks>
        /// <returns>
        /// Site information dictionary
        /// </returns>
        /// <response code="200"></response>
        [VersionedRoute("api/sites/{siteId}", 1)]
        [Route("api/v1/sites/{siteId}")]
        [HttpGet]
        public async Task<Site> GetSiteV1(byte siteId)
        {
            var userId = _authUtil.GetAuthenticatedUserId(User);
            var user = await _userService.GetUserByIdAsync(userId);
            var site = await _siteService.GetSiteByIdAsync(siteId, true);

            // TODO: This sort should be able to be removed. It's doing this because the
            // mobile app is just using the first department it gets back, instead of  
            // looking up the one the user last looked at.
            if (site.Departments != null)
                site.Departments = site.Departments.OrderBy(d => d.Dept != user.DeptView).ThenBy(d => d.Dept).ToList();

            return site;
        }

        /// <summary>
        /// Get detailed information about a particular department within the authenticated API user's site
        /// </summary>
        /// <remarks>Pass expand parameter to expand data that would normally be omitted</remarks>
        /// <returns>
        /// Department information dictionary
        /// </returns>
        /// <param name="dept">Department identifier</param>
        /// <param name="expand">Optional expand parameter</param>
        /// <response code="200"></response>
        [VersionedRoute("api/site/department/{dept}", 1)]
        [Route("api/v1/site/department/{dept}")]
        [HttpGet]
        public async Task<Department> GetDepartmentV1(string dept, string expand = "")
        {
            var userId = _authUtil.GetAuthenticatedUserId(User);
            var user = await _userService.GetUserByIdAsync(userId);
            var department = await _siteService.GetDepartmentByKeyAsync(user.SiteId, dept, user, false);
            department.Patients = await _siteService.GetPatientsBySiteAndDeptAsync(user.SiteId, dept, user, expand);

            return department;
        }

        /// <summary>
        /// Get information necessary to display the tracking board for a particular department within the authenticated API user's site
        /// </summary>
        /// <remarks>Pass filter parameter to set a new user filter and filter results accordingly</remarks>
        /// <returns>
        /// Department information dictionary
        /// </returns>
        /// <param name="dept">Department identifier</param>
        /// <param name="filter">Filter identifier</param>
        /// <response code="200"></response>
        [VersionedRoute("api/site/department/{dept}/trackingboard", 1)]
        [Route("api/v1/site/department/{dept}/trackingboard")]
        [HttpGet]
        public async Task<Department> GetDepartmentMTBV1(string dept, string filter = "")
        {
            var userId = _authUtil.GetAuthenticatedUserId(User);
            var user = await _userService.GetUserByIdAsync(userId);
            var result = await _siteService.GetDepartmentByKeyForMTBAsync(user.SiteId, dept, user, filter);

            return result;
        }

        /// <summary>
        /// Get available beds/areas for the current site of the authenticated API user
        /// </summary>
        /// <remarks></remarks>
        /// <returns>
        /// List of Location information dictionaries
        /// </returns>
        /// <response code="200"></response>
        [VersionedRoute("api/site/locations", 1)]
        [Route("api/v1/site/locations")]
        [HttpGet]
        public async Task<List<Location>> GetLocationsV1()
        {
            var userId = _authUtil.GetAuthenticatedUserId(User);
            var user = await _userService.GetUserByIdAsync(userId);
            var locations = await _siteService.GetAvailableLocationsBySiteIdAsync(user.SiteId);

            return locations;
        }

        /// <summary>
        /// Get medication ordering metadata (route options, override options, etc.)
        /// </summary>
        /// <returns>Metadata information</returns>
        /// <response code="200"></response>
        [VersionedRoute("api/site/pathway/{pathwayNum}", 1)]
        [Route("api/v1/site/pathway/{pathwayNum}")]
        [HttpGet]
        public async Task<ClinicalPathway> GetOrderPathwaysV1(int pathwayNum)
        {
            var userId = _authUtil.GetAuthenticatedUserId(User);
            var user = await _userService.GetUserByIdAsync(userId);
            if (user.HasAtLeastReadPermission(Permission.ORDERS))
            {
                var groups = await _siteService.GetPathway(user.SiteId, pathwayNum);
                return groups;
            }

            return null;
        }

        /// <summary>
        /// Get medication groups/pathways for the current site of the authenticated API user
        /// </summary>
        /// <returns>List of Group objects</returns>
        /// <response code="200"></response>
        [VersionedRoute("api/site/pathways/meds", 1)]
        [Route("api/v1/site/pathways/meds")]
        [HttpGet]
        public async Task<List<Group>> GetMedPathwaysV1()
        {
            var userId = _authUtil.GetAuthenticatedUserId(User);
            var user = await _userService.GetUserByIdAsync(userId);
            if (user.HasAtLeastReadPermission(Permission.MED_SVC))
            {
                var pathways = await _siteService.GetMedPathwaysBySiteIdAsync(user.SiteId);
                return pathways;
            }

            return null;
        }        


        /// <summary>
        /// Get medication ordering metadata (route options, override options, etc.)
        /// </summary>
        /// <returns>Metadata information</returns>
        /// <response code="200"></response>
        [VersionedRoute("api/site/meds/meta", 1)]
        [Route("api/v1/site/meds/meta")]
        [HttpGet]
        public async Task<Dictionary<string, MetaData>> GetMedMetaDataV1()
        {
            var userId = _authUtil.GetAuthenticatedUserId(User);
            var user = await _userService.GetUserByIdAsync(userId);
            if (user.HasAtLeastReadPermission(Permission.MED_SVC))
            {
                var meta = await _siteService.GetMedMetaDataBySiteIdAsync(user.SiteId);
                return meta;
            }

            return null;
        }

        /// <summary>
        /// Search for a medication
        /// </summary>
        /// <returns>A list of medications</returns>
        /// <response code="200"></response>
        [VersionedRoute("api/site/meds/search", 1)]
        [Route("api/v1/site/meds/search")]
        [HttpGet]
        public async Task<List<string>> SearchMedBrandsV1([FromUri]string brand)
        {
            var userId = _authUtil.GetAuthenticatedUserId(User);
            var user = await _userService.GetUserByIdAsync(userId);
            if (user.HasAtLeastReadPermission(Permission.MED_SVC))
            {
                var meds = await _siteService.SearchMedicationBrandsBySiteIdAsync(user.SiteId, brand);
                return meds;
            }

            return null;
        }

        /// <summary>
        /// Get order pathways for the current site of the authenticated API user
        /// </summary>
        /// <returns>List of Group objects</returns>
        /// <response code="200"></response>
        [VersionedRoute("api/site/pathways/orders", 1)]
        [Route("api/v1/site/pathways/orders")]
        [HttpGet]
        public async Task<List<Group>> GetOrderPathwaysV1()
        {
            var userId = _authUtil.GetAuthenticatedUserId(User);
            var user = await _userService.GetUserByIdAsync(userId);
            if (user.CanNavigateTo(Navigation.Constants.ORDERS))
            {
                var pathways = await _siteService.GetOrderPathwaysBySiteIdAsync(user.SiteId);
                return pathways;
            }

            return null;
        }

        /// <summary>
        /// Get order pathways for the current site of the authenticated API user
        /// </summary>
        /// <param name="name">Clinical pathway name search value</param>
        /// <param name="limit">Optional search result limit (defaults to 100)</param>
        /// <returns>List of ClinicalPathway objects</returns>
        /// <response code="200"></response>
        [VersionedRoute("api/site/pathways/orders/search", 1)]
        [Route("api/v1/site/pathways/orders/search")]
        [HttpGet]
        public async Task<List<ClinicalPathway>> SearchClinicalPathwaysV1(string name, int limit = 100)
        {
            var userId = _authUtil.GetAuthenticatedUserId(User);
            var user = await _userService.GetUserByIdAsync(userId);
            if (user.CanNavigateTo(Navigation.Constants.ORDERS))
            {
                var pathways = await _siteService.SearchClinicalPathwaysBySiteIdAsync(user.SiteId, name, limit);
                return pathways;
            }

            return null;
        }

        /// <summary>
        /// Get available beds/areas for the current site and specified department of the authenticated API user
        /// </summary>
        /// <remarks></remarks>
        /// <returns>
        /// List of Location information dictionaries
        /// </returns>
        /// <param name="dept">Department ID</param>
        /// <response code="200"></response>
        [VersionedRoute("api/site/department/{dept}/locations", 1)]
        [Route("api/v1/site/department/{dept}/locations")]
        [HttpGet]
        public async Task<List<Location>> GetDeptLocationsV1(string dept)
        {
            var userId = _authUtil.GetAuthenticatedUserId(User);
            var user = await _userService.GetUserByIdAsync(userId);
            var locations = await _siteService.GetAvailableLocationsBySiteIdAsync(user.SiteId, dept);

            return locations;
        }

        /// <summary>
        /// Get beds in the specified department that can be shared.
        /// </summary>
        /// <param name="dept">Department ID</param>
        /// <returns>List of location information dictionaries</returns>
        /// <response code="200"></response>
        [VersionedRoute("api/site/department/{dept}/sharelocations", 1)]
        [Route("api/v1/site/department/{dept}/sharelocations")]
        [HttpGet]
        public async Task<List<Location>> GetSharingOptionsV1(string dept)
        {
            var userId = _authUtil.GetAuthenticatedUserId(User);
            var user = await _userService.GetUserByIdAsync(userId);
            var locations = await _siteService.GetShareLocationsBySiteIdAsync(user.SiteId, dept);

            return locations;
        }

        /// <summary>
        /// Get available structued comments for the current site of the authenticated API user
        /// </summary>
        /// <remarks></remarks>
        /// <returns>
        /// List of SiteELement information dictionaries for comments
        /// </returns>
        /// <response code="200"></response>
        [VersionedRoute("api/site/comments", 1)]
        [Route("api/v1/site/comments")]
        [HttpGet]
        public async Task<List<SiteElement>> GetCommentsV1()
        {
            var userId = _authUtil.GetAuthenticatedUserId(User);
            var user = await _userService.GetUserByIdAsync(userId);
            var comments = await _siteService.GetCommentsBySiteIdAsync(user.SiteId);

            return comments;
        }

        /// <summary>
        /// Get information used for displaying the signup dialog for a site.
        /// </summary>
        /// <remarks></remarks>
        /// <returns>
        /// Dictionary of signup information
        /// </returns>
        /// <response code="200"></response>
        [VersionedRoute("api/site/signup", 1)]
        [Route("api/v1/site/signup")]
        [HttpGet]
        public async Task<Dictionary<string, string>> GetSignupV1()
        {
            var userId = _authUtil.GetAuthenticatedUserId(User);
            var user = await _userService.GetUserByIdAsync(userId);
            var info = await _siteService.GetSignupInfo(user.SiteId, user);

            return info;
        }

        /// <summary>
        /// Get a specific option for a given site
        /// </summary>
        /// <param name="name">Name of the option</param>
        /// <returns>Value of the option</returns>
        [VersionedRoute("api/site/option", 1)]
        [Route("api/v1/site/option")]
        [HttpGet]
        public async Task<string> GetOptionV1([FromUri]string name)
        {
            var userId = _authUtil.GetAuthenticatedUserId(User);
            var user = await _userService.GetUserByIdAsync(userId);
            var site = await _siteService.GetSiteByIdAsync(user.SiteId);

            var setting = site.GetOrgOption(name);
            return setting;
        }
    }
}
