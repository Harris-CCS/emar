using System.Collections.Generic;
using Emar.Core.Helpers;
using Emar.Core.ResourceParameters;
using Emar.Core.Templates.Model;

namespace Emar.Core.Templates.Service
{
    public interface ITemplateService
    {
        // Utility Method for testing
        TemplateDto GetTemplateDefinition(int templateId, int siteId, HateoasLinkHelper hateoasHelper);

        // Action Methods
        ActionResultDto FireActionAgainstOrder(int userId, in long orderId, ActionEnum actionId, int siteId,
            HateoasLinkHelper hateoasHelper, BaseLinkResource resource, int? templateId = null,
            Dictionary<string, string> templateResponses = null);

        ActionResultDto FireActionAgainstAdministration(int userId, in long administrationId, ActionEnum actionId,
            int siteId, HateoasLinkHelper hateoasHelper, BaseLinkResource resource, int? templateId = null,
            Dictionary<string, string> templateResponses = null);

        int GetTemplateId(string templateName);
    }
}