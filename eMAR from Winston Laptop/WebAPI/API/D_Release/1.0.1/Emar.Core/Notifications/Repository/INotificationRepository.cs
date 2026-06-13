using System.Collections.Generic;
using Emar.Data.Entities;

namespace Emar.Core.Notifications.Repository
{
    /// <summary>
    /// Notification Repository Interface
    /// </summary>
    public interface INotificationRepository
    {
        int GetNotificationCount(int userId, int? siteId);
        IEnumerable<Notification> GetNotifications(int userId, int? siteId);
    }
}