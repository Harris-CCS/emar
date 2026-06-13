using Emar.Core.Templates.Model;
using Emar.Data.Entities;
using System.Collections.Generic;

namespace Emar.Core.Templates.Repository
{
    public interface ITemplateRepository
    {
        IEnumerable<OrderAvailableAction> GetSiteOrderActions(int siteId);
        IEnumerable<OrderAdministrationAvailableAction> GetSiteOrderAdministrationActions(int siteId);

        // Updates based on Actions
        long FileOrderEvent(in int userId, long orderId, ActionEnum actionId, int siteId, int? templateId = null,
            Dictionary<string, string> templateResponses = null);
        long FileAdminEvent(in int userId, long adminId, ActionEnum actionId, int siteId, int? templateId = null,
            Dictionary<string, string> templateResponses = null);

        // Template-specific methods
        Template GetTemplate(int templateId);
        Template GetTemplateForAdministrationAction(long adminId, ActionEnum actionId, int siteId);
        Template GetTemplateForOrderAction(long orderId, ActionEnum actionId, int siteId);
        
        // Utility methods
        int GetTemplateId(string templateName);
    }
}