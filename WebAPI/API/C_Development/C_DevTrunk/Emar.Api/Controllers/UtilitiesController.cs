using System;
using System.Collections.Generic;
using System.Linq;
using Emar.Core.Sites.Service;
using Emar.Data;
using Emar.Data.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Emar.Api.Controllers
{
    [ApiController]
    [ApiExplorerSettings(IgnoreApi = true)]
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
        public ActionResult<string> ConfirmEfConfiguration([FromBody] EfToDbSynchHelperParams parm)
        {
            try
            {
                var rpt = new EfToDbSynchHelper(_context).CompareEfToDb(parm);
                if (rpt == null)
                    return Ok("No problems found");

                var rptText = rpt.CreateOutputText();
                return Ok(rptText);
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
        public ActionResult<string> AddTablesToEfConfiguration([FromBody] EfToDbSynchHelperParams parm)
        {
            throw new NotImplementedException();
            try
            {
                new EfToDbSynchHelper(_context).CompareEfToDb(parm);
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

        [HttpGet("api/testingvariables")]
        public ActionResult<Dictionary<string,string>> GetTestingVariables(
            [FromBody] Dictionary<string,string> queryStrings)
        {
            var ret = new Dictionary<string,string>();

            bool errorEncountered = false;
            using (var conn = new SqlConnection(_context.Database.GetDbConnection().ConnectionString))
            {
                using (var comm = new SqlCommand("", conn))
                {
                    conn.Open();

                    foreach (var query in queryStrings)
                    {
                        string queryResult;
                        string tokenError = "";
                        try
                        {
                            var commText = query.Value;
                            int tokenStart = 0;
                            while ((tokenStart = commText.IndexOf("<<", StringComparison.Ordinal)) > -1)
                            {
                                var tokenEnd = commText.IndexOf(">>", tokenStart, StringComparison.Ordinal);
                                var token = commText.Substring(tokenStart + 2, tokenEnd - tokenStart - 2);

                                if (!ret.TryGetValue(token, out var tokenValue))
                                {
                                    // Make it return the bad value
                                    tokenError = token;
                                    break;
                                }

                                commText = commText.Replace("<<" + token + ">>", tokenValue);
                            }

                            if (tokenError == "")
                            {
                                comm.CommandText = commText;
                                queryResult = comm.ExecuteScalar()?.ToString();
                            }
                            else
                            {
                                queryResult = $"ERROR: Token '{tokenError}' doesn't have a value from the previously-evaluated strings.";
                                errorEncountered = true;
                            }
                        }
                        catch (Exception e)
                        {
                            queryResult = $"ERROR: {e.Message}";
                            errorEncountered = true;
                        }

                        if (queryResult != null &&
                            query.Key.StartsWith("Combo", StringComparison.InvariantCultureIgnoreCase) &&
                            !queryResult.StartsWith("ERROR:"))
                        {
                            // Code in the Request that ends up in this "Comboxxx" loop
                            // "Combo": "DECLARE @quickListItemId int, @quickListUserId int; SELECT @quickListItemId = id,
                            // @quickListUserId = user_id FROM user_quick_list_items WHERE site_id = 16 and drug_id = 282127
                            // and brand_name = 'Percocet';
                            // SELECT CONCAT('QuickListItemId|', @quickListItemId, '~QuickListUserId|', @quickListUserId)",
                            foreach (var ss in queryResult.Split('~').Select(s => s.Split('|')))
                            {
                                ret.Add(ss[0], ss[1]);
                            }
                        }
                        else if (!query.Key.StartsWith("Ignore", StringComparison.InvariantCultureIgnoreCase) || 
                                 tokenError != "")
                            ret.Add(query.Key, queryResult);
                    }
                }
            }

            if (errorEncountered)
                return BadRequest(ret);

            return Ok(ret);
        }
    }
}
