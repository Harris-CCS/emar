using System;
using System.Collections.Generic;
using System.Linq;
using Emar.Core.Helpers;
using Emar.Data;
using Emar.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace Emar.Core.Notifications.Repository
{
    /// <summary>
    /// Notification Repository
    /// </summary>
    public class NotificationRepository : INotificationRepository
    {
        private readonly EmarContext _context;
        private readonly MemoryCache _cache;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="emarContext"></param>
        /// <param name="cache"></param>
        public NotificationRepository(EmarContext emarContext, EmarMemoryCache cache)
        {
            _context = emarContext ?? throw new ArgumentNullException(nameof(emarContext));
            _cache = cache.Cache;
        }

        /// <summary>
        /// Get a count of the user's notifications
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="siteId"></param>
        /// <returns></returns>
        public int GetNotificationCount(int userId, int? siteId)
        {
            var notifications = _GetNotifications(userId, siteId);
            return notifications.Count();
        }

        /// <summary>
        /// Get the user's notifications
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="siteId"></param>
        /// <returns></returns>
        public IEnumerable<Notification> GetNotifications(int userId, int? siteId)
        {
            var notificationCategories = _cache.GetOrCreate(CacheKeys.NotificationCategories, entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(8);
                var ret = _context.NotificationCategories.OrderBy(i => i.Priority).ToList();
                entry.Size = ret.Count;
                return ret;
            });

            var notifications = _GetNotifications(userId, siteId);

            // Apply category information to notifications
            foreach(Notification n in notifications)
            {
                if (n.CategoryCode != null)
                {
                    n.Category = notificationCategories.Where(c => c.Code == n.CategoryCode).FirstOrDefault();
                }
            }

            return notifications
                // Sort notifications by category priority if available, or to the top.
                .OrderBy(o => o.Category != null ? o.Category.Priority : 0)
                // Then by event date/time (most recent first), or to the top.
                .ThenByDescending(o => o.EventDateTime != null ? o.EventDateTime : DateTime.Now)
                .ToList();
        }

        /// <summary>
        /// Initial notification pull/filter
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="siteId"></param>
        /// <returns></returns>
        private IEnumerable<Notification> _GetNotifications(int userId, int? siteId)
        {
            var notifications = _context.Notifications
                .Where(
                    n => 
                        n.RecipientUserId == userId && 
                        (siteId == null || n.PatientOrder.Patient.SiteId == siteId) &&
                        n.AcknowledgedDateTime == null
                )
                .Include(n => n.PatientOrder.Patient)
                .ToList();

            return notifications;
        }
    }
}