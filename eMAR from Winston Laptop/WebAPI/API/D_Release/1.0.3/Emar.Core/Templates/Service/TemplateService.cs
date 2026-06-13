using Emar.Core.Helpers;
using Emar.Core.Orders.Service;
using Emar.Core.ResourceParameters;
using Emar.Core.Sites.Repository;
using Emar.Core.Templates.Model;
using Emar.Core.Templates.Model.Mappings;
using Emar.Core.Templates.Repository;
using Emar.Data.Entities;
using System.Collections.Generic;

namespace Emar.Core.Templates.Service
{
    public class TemplateService : ITemplateService
    {
        private readonly ITemplateRepository _templateRepository;
        private readonly ISiteRepository _siteRepository;
        private readonly IOrderService _orderService;


        public TemplateService(ITemplateRepository repository, ISiteRepository siteRepository, 
            IOrderService orderService)
        {
            _templateRepository = repository;
            _siteRepository = siteRepository;
            _orderService = orderService;
        }

        #region Order/Administration Actions

        public ActionResultDto FireActionAgainstOrder(int userId, in long orderId, ActionEnum actionId, int siteId,
            HateoasLinkHelper hateoasHelper, BaseLinkResource resource, int? templateId = null,
            Dictionary<string, string> templateResponses = null)
        {
            if (templateResponses == null)
            {
                var template = _templateRepository.GetTemplateForOrderAction(orderId, actionId, siteId);

                if (template != null)
                {
                    var link = hateoasHelper.GetOrderActionTemplateResultLink(orderId, (int)actionId, template.Id);

                    return new ActionResultDto
                    {
                        Template = TemplateMapper.MapTemplate(template, _siteRepository.GetSiteTimeZone(siteId), link)
                    };
                }
            }

            var eventId =
                _templateRepository.FileOrderEvent(userId, orderId, actionId, siteId, templateId, templateResponses);

            var ret = new ActionResultDto
            {
                NewEvent = _orderService.GetEvent(eventId),
                UpdatedOrder = _orderService.GetOrder(orderId, resource)
            };

            return ret;
        }

        public ActionResultDto FireActionAgainstAdministration(int userId, in long adminId, ActionEnum actionId,
            int siteId, HateoasLinkHelper hateoasHelper, BaseLinkResource resource, int? templateId = null,
            Dictionary<string, string> templateResponses = null)
        {
            if (templateResponses == null)
            {
                var template = _templateRepository.GetTemplateForAdministrationAction(adminId, actionId, siteId);

                if (template != null)
                {
                    var link = hateoasHelper.GetAdministrationActionTemplateResultLink(adminId, (int) actionId,
                        template.Id);

                    return new ActionResultDto
                    {
                        Template = TemplateMapper.MapTemplate(template, _siteRepository.GetSiteTimeZone(siteId), link)
                    };
                }
            }

            var eventId =
                _templateRepository.FileAdminEvent(userId, adminId, actionId, siteId, templateId, templateResponses);

            var newEvent = _orderService.GetEvent(eventId);

            var ret = new ActionResultDto
            {
                NewEvent = newEvent,
                UpdatedOrder = _orderService.GetOrder(newEvent.OrderId, resource)
            };

            return ret;
        }

        #endregion

        #region Utility methods

        public int GetTemplateId(string templateName)
        {
            return _templateRepository.GetTemplateId(templateName);
        }

        public TemplateDto GetTemplateDefinition(int templateId, int siteId,
            HateoasLinkHelper hateoasHelper)
        {
            Template template = _templateRepository.GetTemplate(templateId);
            var link = hateoasHelper.GetAdministrationActionTemplateResultLink(100, 2, 2);

            return TemplateMapper.MapTemplate(template, _siteRepository.GetSiteTimeZone(siteId), link);
        }

        #endregion
    }
}