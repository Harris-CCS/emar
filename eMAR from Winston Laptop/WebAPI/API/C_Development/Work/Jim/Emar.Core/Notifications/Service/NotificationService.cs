using System;
using System.Collections.Generic;
using System.Linq;
using Emar.Core.Notifications.Model;
using Emar.Core.Notifications.Model.Mappings;
using Emar.Core.Notifications.Repository;

namespace Emar.Core.Notifications.Service
{
    /// <summary>
    /// Notification Service
    /// </summary>
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _notificationRepository;
        
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="notificationRepository"></param>
        public NotificationService(INotificationRepository notificationRepository)
        {
            _notificationRepository = notificationRepository ?? throw new ArgumentNullException(nameof(notificationRepository));
        }

        /// <summary>
        /// Get notification count
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="siteId"></param>
        /// <returns></returns>
        public int GetNotificationCount(int userId, int? siteId)
        {
            return _notificationRepository.GetNotificationCount(userId, siteId);
        }

        /// <summary>
        /// Get the list of notifications
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="siteId"></param>
        /// <returns></returns>
        public IEnumerable<NotificationDto> GetNotifications(int userId, int? siteId)
        {
            var notifications = _notificationRepository.GetNotifications(userId, siteId);

            var retNotifications = notifications
                .Select(notification =>
                    NotificationMapper.MapNotification(notification, userId))
                .ToList();

            return retNotifications;
        }
    }
}