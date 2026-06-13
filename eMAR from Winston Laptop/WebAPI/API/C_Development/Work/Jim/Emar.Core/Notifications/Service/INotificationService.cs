using System.Collections.Generic;
using Emar.Core.Notifications.Model;

namespace Emar.Core.Notifications.Service
{
    /// <summary>
    /// Notification Service Interface
    /// </summary>
    public interface INotificationService
    {
        int GetNotificationCount(int userId, int? siteId);
        IEnumerable<NotificationDto> GetNotifications(int userId, int? siteId);
    }
}