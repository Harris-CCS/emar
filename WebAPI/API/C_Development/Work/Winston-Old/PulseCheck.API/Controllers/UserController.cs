using System.Threading.Tasks;
using System.Web.Http;
using PulseCheck.API.Models;
using System.Collections.Generic;
using PulseCheck.Domain;
using PulseCheck.ILogic;
using PulseCheck.IRepository;
using PulseCheck.Logic;
using PulseCheck.Utilities;

namespace PulseCheck.API.Controllers
{
    /// <summary>
    /// User controller for PulseCheck API
    /// </summary>
    public class UserController : ControllerBase
    {
        private readonly ISiteManager _siteManager;
        private readonly IUserManager _userManager;
        private readonly IUserMappingRepository _userMapping;
        private readonly UserAccountManager _userAccountManager;
        private readonly Authentication _authUtil = new Authentication();

        /// <summary>
        /// UserController constructor
        /// </summary>
        /// <param name="siteManager"></param>
        /// <param name="userManager"></param>
        /// <param name="userMapping"></param>
        /// <param name="userAccountManager"></param>
        public UserController(ISiteManager siteManager, IUserManager userManager, IUserMappingRepository userMapping, UserAccountManager userAccountManager)
        {
            _siteManager = siteManager;
            _userManager = userManager;
            _userMapping = userMapping;
            _userAccountManager = userAccountManager;
        }

        // GET: api/user
        /// <summary>
        /// Get all information available for the currently authenticated user
        /// </summary>
        /// <remarks></remarks>
        /// <returns>
        /// Full user object
        /// </returns>
        /// <response code="200"></response>
        /// <response code="401"></response>
        [VersionedRoute("api/user", 1)]
        [Route("api/v1/user")]
        [HttpGet]
        public async Task<IHttpActionResult> GetUser()
        {
            var userId = _authUtil.GetAuthenticatedUserId(User);
            var user = await _userManager.GetUserByIdAsync(userId);
            return Ok(user);
        }

        // GET: api/user/{userId}
        /// <summary>
        /// Get all information available for the the specified user
        /// </summary>
        /// <remarks>This is only available to administrators with the necessary permissions</remarks>
        /// <returns>
        /// Full user object
        /// </returns>
        /// <response code="200"></response>
        /// <response code="401"></response>
        [VersionedRoute("api/user/{userId}", 1)]
        [Route("api/v1/user/{userId}")]
        [HttpGet]
        public async Task<IHttpActionResult> GetUserById(int userId)
        {
            var currentUserId = _authUtil.GetAuthenticatedUserId(User);
            var user = await _userManager.GetUserByIdAsync(currentUserId);
            if (user.IsAdministrator())
            {
                // TODO: Additional checks needed here?
                var requestedUser = await _userManager.GetUserByIdAsync(userId);
                if (requestedUser.SiteId == user.SiteId)
                {
                    return Ok(requestedUser);
                }
            }
            return new WLRResponse(ErrorCodes.NOT_AUTHORIZED, Request);
        }

        /// <summary>
        /// Get a user's favorite orders
        /// </summary>
        /// <response code="200"></response>
        [VersionedRoute("api/user/favorites/orders", 1)]
        [Route("api/v1/user/favorites/orders")]
        [HttpGet]
        public async Task<List<Service>> GetFavoriteOrdersV1()
        {
            var userId = _authUtil.GetAuthenticatedUserId(User);
            var user = await _userManager.GetUserByIdAsync(userId);
            var pathways = await _userManager.GetUserFavoriteOrders(user);
            return pathways;
        }

        /// <summary>
        /// Add an order to a user's list of favorites
        /// </summary>
        /// <param name="service">Service to be favorited.  Number is required.</param>
        /// <response code="200"></response>
        [VersionedRoute("api/user/favorites/orders", 1)]
        [Route("api/v1/user/favorites/orders")]
        [HttpPost]
        public async Task<IHttpActionResult> AddFavoriteOrderV1([FromBody]Service service)
        {
            var userId = _authUtil.GetAuthenticatedUserId(User);
            var user = await _userManager.GetUserByIdAsync(userId);
            var result = _userManager.AddUserFavoriteOrder(user, service.Number);
            if (result == -1)
            {
                return BadRequest("Favorite limit exceeded. Remove a favorite before adding a new favorite");
            } else if (result == 0)
            {
                return BadRequest("Favorite already exists");
            }
            return Ok();
        }

        /// <summary>
        /// Remove an order from a user's list of favorites
        /// </summary>
        /// <param name="num">Favorite number</param>
        /// <response code="200"></response>
        [VersionedRoute("api/user/favorites/orders/{num}", 1)]
        [Route("api/v1/user/favorites/orders/{num}")]
        [HttpDelete]
        public async Task<IHttpActionResult> RemoveFavoriteOrderV1(int num)
        {
            var userId = _authUtil.GetAuthenticatedUserId(User);
            var user = await _userManager.GetUserByIdAsync(userId);
            var result = _userManager.RemoveUserFavoriteOrder(user, num);
            if (result == -1)
            {
                return BadRequest("Favorite not found");
            }
            return Ok();
        }
    }
}
