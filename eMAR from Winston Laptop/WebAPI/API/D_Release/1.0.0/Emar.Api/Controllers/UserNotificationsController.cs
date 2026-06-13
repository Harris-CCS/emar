using System;
using System.Collections.Generic;
using Emar.Api.Helpers;
using Emar.Core.Notifications.Model;
using Emar.Core.Notifications.Service;
using Microsoft.AspNetCore.Mvc;

namespace Emar.Api.Controllers
{
    /// <summary>
    /// User Notifications Controller
    /// </summary>
    [Route("api/usernotifications")]
    [ApiController]
    [Consumes(MediaTypes.PcEmar, MediaTypes.Json)]
    public class UserNotificationsController : ControllerBase
    {
        private readonly INotificationService _notificationService;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="notificationService"></param>
        public UserNotificationsController(INotificationService notificationService)
        {
            _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
        }

        /// <summary>
        /// Return a list of notifications for a user
        /// </summary>
        /// <param name="mediaType">Media type from the "Accept" header</param> 
        /// <param name="siteId">(Optional) The site to retrieve the user's notifications for. If omitted, return the user's notifications for all sites</param>
        /// <param name="userId">The user to retrieve the notifications for</param>
        /// <returns></returns>
        [HttpGet("/api/usernotifications", Name = nameof(GetNotifications))]
        [ProducesResponseType(typeof(IEnumerable<NotificationDto>), 200)] // (OK) - the resource is sent in the response
        [ProducesResponseType(400)] // (bad request) - indicates a bad request (e.g. wrong parameter)
        public ActionResult<IEnumerable<NotificationDto>> GetNotifications(
            [FromHeader(Name = "Accept")] string mediaType,
            [FromHeader(Name = "EMAR-Site")] int? siteId,
            [FromHeader(Name = "EMAR-User")] int userId
            )
        {
            if (!MediaTypes.IsValidMediaType(mediaType))
            {
                return BadRequest("Unsupported media type header provided.");
            }

            var ret = _notificationService.GetNotifications(userId, siteId.HasValue ? siteId.Value : (int?)null);

            return Ok(ret);
        }

        /// <summary>
        /// Return a count of notifications for a user
        /// </summary>
        /// <param name="mediaType">Media type from the "Accept" header</param>
        /// <param name="siteId">(Optional) The site to retrieve the user's notifications for. If omitted, return the count of user's notifications for all sites</param>
        /// <param name="userId">The user to retrieve the notifications for</param>
        /// <returns></returns>
        [HttpGet("/api/usernotifications/count", Name = nameof(GetNotificationsCount))]
        [ProducesResponseType(typeof(Dictionary<string, object>), 200)] // (OK) - the resource is sent in the response
        [ProducesResponseType(400)] // (bad request) - indicates a bad request (e.g. wrong parameter)
        public ActionResult<Dictionary<string, object>> GetNotificationsCount(
            [FromHeader(Name = "Accept")] string mediaType,
            [FromHeader(Name = "EMAR-Site")] int? siteId,
            [FromHeader(Name = "EMAR-User")] int userId
            )
        {
            if (!MediaTypes.IsValidMediaType(mediaType))
            {
                return BadRequest("Unsupported media type header provided.");
            }

            var ret = _notificationService.GetNotificationCount(userId, siteId.HasValue ? siteId.Value : (int?)null);

            return Ok(new Dictionary<string, object>
            {
                { "total", ret }
            });
        }
    }
}