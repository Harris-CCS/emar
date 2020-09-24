using System.Collections.Generic;
using Emar.Api.Helpers;
using Emar.Core.Orders.Model;
using Emar.Core.Orders.Service;
using Emar.Core.Templates.Model;
using Emar.Core.Templates.Service;
using Microsoft.AspNetCore.Mvc;

namespace Emar.Api.Controllers
{
    [Route("api/orders/composerOptions")]
    [ApiController]
    [Produces(MediaTypes.Json)]
    [Consumes(MediaTypes.Json)]
    public class ComposerController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly ITemplateService _templateService;

        public ComposerController(IOrderService orderService, ITemplateService templateService)
        {
            _orderService = orderService;
            _templateService = templateService;
        }

        [HttpGet("{brandName}", Name = nameof(GetComposerOptions))]
        public ActionResult<ComposerOptionsDto> GetComposerOptions(string brandName)
        {
            ComposerOptionsDto ret = _orderService.GetComposerSetupData(brandName);
            if (ret == null)
                return NotFound($"Found no setup options for Brand Name {brandName}");
            return Ok(ret);
        }

        [HttpGet("frequencies/{siteId}", Name = nameof(GetComposerFrequencies))]
        public ActionResult<IEnumerable<MockFrequencyDto>> GetComposerFrequencies(int siteId)
        {
            var ret = _orderService.GetFrequencies(siteId);
            if (ret == null)
                return NotFound($"Found no Frequencies for Site ID: {siteId}");
            return Ok(ret);
        }

        [HttpGet("units/{siteId}", Name = nameof(GetComposerUnits))]
        public ActionResult<IEnumerable<MockUnitDto>> GetComposerUnits(int siteId)
        {
            IEnumerable<MockUnitDto> ret = _orderService.GetUnits(siteId);
            if (ret == null)
                return NotFound($"Found no Units for Site ID: {siteId}");
            return Ok(ret);
        }

        [HttpGet("templates/{templateId}", Name = nameof(GetTemplateDefinition))]
        public ActionResult<TemplateDto> GetTemplateDefinition(int templateId)
        {
            TemplateDto ret = _templateService.GetTemplateDefinition(templateId);

            if (ret == null)
                return NotFound($"Didn't find a template for tempate ID: {templateId}");

            return Ok(ret);
        }
    }
}

    
