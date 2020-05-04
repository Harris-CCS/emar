using System.Threading.Tasks;
using System.Web.Http;
using System;
using System.Linq;
using PulseCheck.API.Models;
using System.Collections.Generic;
using DomainModel;
using Interfaces.Repository;
using Interfaces.Services;
using Services;
using System.Configuration;
using System.Net;
using PulseCheck.Constants;
using IdentityModel.Client;

namespace PulseCheck.API.Controllers
{
    /// <summary>
    /// Device controller for PulseCheck API
    /// </summary>
    public class DeviceController : ApiController
    {
        private readonly IDeviceRepository _deviceRepo;
        private readonly IDeviceService _deviceService;
        private readonly IAuthenticationService _authService;
        private readonly UserAccountService _userAccountService;

        /// <summary>
        /// DeviceController constructor
        /// </summary>
        public DeviceController(IDeviceRepository deviceRepo, IDeviceService deviceService, IAuthenticationService authService, UserAccountService userAccountService)
        {
            _deviceRepo = deviceRepo;
            _deviceService = deviceService;
            _authService = authService;
            _userAccountService = userAccountService;
        }

        // POST: api/devices/active
        /// <summary>
        /// Post device information for activating a device
        /// </summary>
        [VersionedRoute("api/devices/active", 1)]
        [Route("api/v1/devices/active")]
        [HttpPost]
        [AllowAnonymous]
        public async Task<IHttpActionResult> PostActivationV1([FromBody]AuthorizedDevice dInfo)
        {
            var mobileDevice = new MobileDevice(dInfo);
            var existingDevice = await _deviceService.GetDeviceByIdAsync(mobileDevice.DeviceId);

            // If the device exists, check if it's authorized.  If it's not, give an error to 
            // the bad person trying to get into the system.
            if (existingDevice != null)
            {
                if (existingDevice.IsAuthorized)
                    return Ok();
                else
                    return BadRequest("Device is not authorized");
            }

            var hasValidAuthorizationCode = false;
            if (!string.IsNullOrWhiteSpace(dInfo.AuthorizationCode))
            {
                if (dInfo.AuthorizationCode == ConfigurationManager.AppSettings["DeviceMasterPasscode"])
                    hasValidAuthorizationCode = true;
                else 
                    hasValidAuthorizationCode = await _deviceService.CheckDeviceAuthorization(dInfo.AuthorizationCode);
            }

            if (!hasValidAuthorizationCode)
                return BadRequest("Invalid activation code");

            _deviceService.AddDevice(mobileDevice);

            return Ok();
        }

        // GET: api/devices
        /// <summary>
        /// Get the active devices
        /// </summary>
        [VersionedRoute("api/devices", 1)]
        [Route("api/v1/devices")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<List<MobileDevice>> GetDevicesV1()
        {
            var validUser = await GetValidatedWebUser();
            var devices = await _deviceRepo.GetDevices();
            return devices;
        }

        // PUT: api/devices/<deviceId>
        /// <summary>
        /// Put device information to update info in DB
        /// </summary>
        [VersionedRoute("api/devices/{deviceId}", 1)]
        [Route("api/v1/devices/{deviceId}")]
        [HttpPut]
        [AllowAnonymous]
        public async Task<IHttpActionResult> PutInfoV1(string deviceId, [FromBody]MobileDevice device)
        {
            if (string.IsNullOrWhiteSpace(deviceId))
                return new ErrorResponse("Device ID missing", 400, Request);

            var deviceToUpdate = await _deviceService.GetDeviceByIdAsync(deviceId);
            if (deviceToUpdate == null)
                return new ErrorResponse("Device not found", 404, Request);

            // For now, the friendly name and authorization are the only things we'll let be changed
            deviceToUpdate.FriendlyName = device.FriendlyName;
            deviceToUpdate.IsAuthorized = device.IsAuthorized;

            if (!device.IsAuthorized)
            {
                var users = _userAccountService.GetUserAccounts();
                DomainModel.Membership.UserAccount accountUsingDevice = null;
                foreach (var user in users)
                {
                    if (user.Claims.Any(claim =>
                        string.Equals(claim.Type, PulseCheckClaims.DeviceId, StringComparison.CurrentCultureIgnoreCase) &&
                        claim.Value == deviceId
                    ))
                    {
                        accountUsingDevice = user;
                        break;                 
                    }
                }
                if (accountUsingDevice != null && accountUsingDevice.Claims.Any())
                {
                    var token = accountUsingDevice.Claims.FirstOrDefault(c => c.Type == PulseCheckClaims.AccessToken).Value;
                    var client = new TokenRevocationClient(
                        string.Format(Endpoints.TokenRevocationEndpoint, Host.Configuration.Addresses.GetIDServerBaseAddress()),
                        Identifiers.APIClientId,
                        Identifiers.APIClientSecret
                    );
                    await client.RevokeAccessTokenAsync(token);

                    _userAccountService.RemovePulseCheckClaims(accountUsingDevice.ID);
                }
            }

            var result = await _deviceService.Save(deviceToUpdate);
            if (result == 0)
                return new ErrorResponse("Device update failed", 500, Request);

            return Ok();
        }

        // GET: api/devices/<deviceId>
        /// <summary>
        /// Get device existence/authorization status
        /// </summary>
        /// <returns>
        /// Response code for device status
        /// </returns>
        /// <response code="200">Device is present and authorized</response>
        /// <response code="400">Device ID is missing</response>
        /// <response code="404">No such active device exists</response>
        [VersionedRoute("api/devices/{deviceId}", 1)]
        [Route("api/v1/devices/{deviceId}")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<IHttpActionResult> GetDeviceV1(string deviceId)
        {
            if (string.IsNullOrWhiteSpace(deviceId))
                return new ErrorResponse("Device ID missing", 400, Request);

            var device = await _deviceService.GetDeviceByIdAsync(deviceId);
            if (device == null)
            {
                return new ErrorResponse("Device not found", 404, Request);
            } else if (!device.IsAuthorized)
            {
                return new ErrorResponse("Device not authorized", 404, Request);
            }

            return Ok(device);
        }

        /// <summary>
        /// Get device existence/authorization status
        /// </summary>
        /// <returns>
        /// Response code for device status
        /// </returns>
        /// <response code="200">Device is present and authorized</response>
        /// <response code="400">Device ID is missing</response>
        /// <response code="404">No such active device exists</response>
        [VersionedRoute("api/devices/{deviceId}", 1)]
        [Route("api/v1/devices/{deviceId}")]
        [HttpDelete]
        [AllowAnonymous]
        public async Task<IHttpActionResult> DeleteDeviceV1(string deviceId)
        {
            var validUser = await GetValidatedWebUser();
            if (string.IsNullOrWhiteSpace(deviceId))
                return new ErrorResponse("Device ID missing", 400, Request);

            var device = await _deviceService.GetDeviceByIdAsync(deviceId);
            if (device == null)
                return new ErrorResponse("Device not found", 404, Request);

            await _deviceService.DeleteDevice(device.DeviceId);

            return Ok();
        }

        private async Task<User> GetValidatedWebUser()
        {
            var user = await _authService.GetValidatedWebUser();
            if (user == null || !user.CanNavigateTo(Navigation.Constants.ACCOUNT_ADMIN))
                throw new HttpResponseException(HttpStatusCode.Unauthorized);

            return user;
        }
    }
}
