using System;
using Microsoft.AspNetCore.Mvc;
using Emar.Data;
using Emar.Data.Helpers;

namespace Emar.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UtilitiesController : ControllerBase
    {
        private readonly EmarContext _context;

        public UtilitiesController(EmarContext context)
        {
            _context = context;
        }

        // GET: api/Utilities
        [HttpGet("ConfirmEfConfiguration")]
        public ActionResult<string> ConfirmEfConfiguration()
        {
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
    }
}
