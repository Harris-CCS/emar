using System;
using Emar.Core.Sites.Service;
using Emar.Data;
using Emar.Data.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace Emar.Api.Controllers
{
    [ApiController]
    public class UtilitiesController : ControllerBase
    {
        private readonly EmarContext _context;
        private readonly ISiteService _siteService;

        public UtilitiesController(EmarContext context, ISiteService siteService)
        {
            _context = context;
            _siteService = siteService;
        }

        [HttpGet("api/EfConfiguration/Confirm")]
        public ActionResult<string> ConfirmEfConfiguration()
        {
            try
            {
                var rpt = new EfToDbSynchHelper(_context).CompareEfToDb();
                return Ok(rpt);
            }
            catch (Exception e)
            {
                var message = e.Message;
                Exception next = e;
                while (next.InnerException != null)
                {
                    next = next.InnerException;
                    message = next.Message + " -- " + message;
                }
                return Problem(message);
            }
        }

        [HttpPost("api/EfConfiguration/AddTable")]
        public ActionResult<string> AddTablesToEfConfiguration()
        {
            throw new NotImplementedException();
            try
            {
                new EfToDbSynchHelper(_context).CompareEfToDb();
                return Ok();
            }
            catch (Exception e)
            {
                return Problem(e.Message);
            }
        }

        [HttpGet("api/sites")]
        public ActionResult<int> GetSiteByName(
            [FromQuery] string siteName
        )
        {
            int siteId = _siteService.GetSiteIdByName(siteName);
            return Ok(siteId);
        }
    }
}
