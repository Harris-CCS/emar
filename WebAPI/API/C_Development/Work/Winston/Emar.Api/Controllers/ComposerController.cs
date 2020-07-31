using System.Collections.Generic;
using Emar.Api.Helpers;
using Emar.Core.Orders.Model;
using Emar.Core.Orders.Service;
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

        public ComposerController(IOrderService orderService)
        {
            _orderService = orderService;
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
        public ActionResult<IEnumerable<FrequencyDto>> GetComposerFrequencies(int siteId)
        {
            var ret = _orderService.GetFrequencies(siteId);
            if (ret == null)
                return NotFound($"Found no Frequencies for Site ID: {siteId}");
            return Ok(ret);
        }

        [HttpGet("units/{siteId}", Name = nameof(GetComposerUnits))]
        public ActionResult<IEnumerable<UnitDto>> GetComposerUnits(int siteId)
        {
            IEnumerable<UnitDto> ret = _orderService.GetUnits(siteId);
            if (ret == null)
                return NotFound($"Found no Units for Site ID: {siteId}");
            return Ok(ret);
        }
    }
}
