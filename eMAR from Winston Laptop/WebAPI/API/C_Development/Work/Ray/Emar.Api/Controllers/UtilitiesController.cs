using Emar.Api.Helpers;
using Emar.Core.Helpers;
using Emar.Core.Templates.Model;
using Emar.Core.Templates.Service;
using Emar.Data;
using Emar.Data.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Net;

namespace Emar.Api.Controllers
{
    [ApiController]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class UtilitiesController : ControllerBase
    {
        private readonly EmarContext _emarContext;
        private readonly IbexContext _ibexContext;
        private readonly ITemplateService _templateService;
        private readonly MemoryCache _cache;

        public UtilitiesController(EmarContext emarContext, IbexContext ibexContext, ITemplateService templateService, EmarMemoryCache cache)
        {
            _emarContext = emarContext;
            _ibexContext = ibexContext;
            _templateService = templateService;
            _cache = cache.Cache;
        }

        [HttpGet("api/EfConfiguration/Confirm")]
        public ActionResult<string> ConfirmEfConfiguration(
            [FromBody] EfToDbSynchHelperParams parm,
            [FromHeader(Name = "DebuggingOutput")] bool debugOutput,
            [FromHeader(Name = "Context")] string context = "EmarContext"
        )
        {
            try
            {
                object ret = context.ToLowerInvariant() switch
                {
                    "emarcontext" => new EfToDbSynchHelper(_emarContext, context).CompareEfToDb(parm),
                    "ibexcontext" => new EfToDbSynchHelper(_ibexContext, context).CompareEfToDb(parm),
                    _ => throw new ArgumentException("From ConfirmEfConfiguration()", nameof(context))
                };

                if (ret.GetType() == typeof(EfToDbSynchHelper.EfDiscrepancyReport))
                    return Conflict(((EfToDbSynchHelper.EfDiscrepancyReport)ret).CreateOutputText());
                if (!debugOutput)
                    return Ok();
                return Ok((IEnumerable<EfToDbSynchHelper.EfTableAttributes>)ret);
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

        [HttpPost("api/EfConfiguration/Tables")]
        public ActionResult<string> AddTablesToEfConfiguration(
            [FromBody] EfToDbSynchHelperParams parm,
            [FromHeader(Name = "Context")] string context = "EmarContext")
        {
            try
            {
                EfToDbSynchHelper.EfDiscrepancyReport rpt = context.ToLowerInvariant() switch
                {
                    "emarcontext" => new EfToDbSynchHelper(_emarContext, context).AddTables(parm),
                    "ibexcontext" => new EfToDbSynchHelper(_ibexContext, context).AddTables(parm),
                    _ => throw new ArgumentException("From AddTablesToEfConfiguration()", nameof(context))
                };

                var rptText = rpt.CreateOutputText();
                return Ok(rptText);
            }
            catch (Exception e)
            {
                return Problem(e.Message);
            }
        }

        /// <summary>
        /// This call is only for testing out the templates directly (without having to fire an action)
        /// </summary>
        /// <param name="mediaType"></param>
        /// <param name="siteId"></param>
        /// <param name="templateName">Name of the template to get the JSON for</param>
        /// <returns></returns>
        [HttpGet("api/templates/{templateId}", Name = nameof(GetTemplateDefinition))]
        [ProducesResponseType(typeof(TemplateDto), 200)] // (OK) - the resource is sent in the response
        [ProducesResponseType(400)] // (bad request) - indicates a bad request (e.g. wrong parameter)
        [ProducesResponseType(404)] // (not found) - the resource does not exits
                                    //[ProducesResponseType(406)] // (not acceptable) - the server does not support the required representation
        public ActionResult<TemplateDto> GetTemplateDefinition(
            [FromHeader(Name = "Accept")] string mediaType,
            [FromHeader(Name = "EMAR-Site")] int? siteId,
            [FromRoute(Name = "templateId")] string templateName
            )
        {
            if (!MediaTypes.IsValidMediaType(mediaType))
            {
                return BadRequest("Unsupported media type header provided.");
            }

            if (!siteId.HasValue)
            {
                return BadRequest("The Site ID must be provided.");
            }

            TemplateDto ret;

            try
            {
                var hateoasHelper = new HateoasLinkHelper();
                hateoasHelper.SetAdministrationActionTemplateResultLink(
                    Url.Link(nameof(OrdersController.FileAdministrationActionTemplateResult),
                        new { administrationId = -99, actionId = -98, templateId = -97 }));
                int templateId = _templateService.GetTemplateId(templateName);

                ret = _templateService.GetTemplateDefinition(templateId, siteId.Value, hateoasHelper);
            }
            catch (Exception ex)
            {
                return Problem(Emar.Core.Helpers.Utilities.ExtractExceptionMessages(ex), statusCode: (int)HttpStatusCode.InternalServerError);
            }

            if (ret == null)
            {
                return NotFound($"No template found with name = '{templateName}'.");
            }

            return Ok(ret);
        }



        [HttpGet("api/testingvariables")]
        public ActionResult<Dictionary<string, string>> GetTestingVariables(
            [FromBody] Dictionary<string, string> queryStrings)
        {
            var ret = new ReturnReport();

            bool errorEncountered = false;
            using (var emarConn = new SqlConnection(_emarContext.Database.GetDbConnection().ConnectionString))
            using (var ibexConn = new SqlConnection(_ibexContext.Database.GetDbConnection().ConnectionString))
            {
                using (var emarComm = new SqlCommand("", emarConn))
                using (var ibexComm = new SqlCommand("", ibexConn))
                {
                    foreach (var query in queryStrings)
                    {
                        string queryResult;
                        string tokenError = "";
                        string key = "";
                        try
                        {
                            string sqlTarget;
                            if (query.Key.StartsWith("Ibex:", true, null))
                            {
                                sqlTarget = "ibex";
                                key = query.Key.Substring("ibex".Length + 1);
                            }
                            else
                            {
                                sqlTarget = "emar";
                                key = query.Key;
                            }

                            var commText = query.Value;
                            int tokenStart;
                            while ((tokenStart = commText.IndexOf("<<", StringComparison.Ordinal)) > -1)
                            {
                                var tokenEnd = commText.IndexOf(">>", tokenStart, StringComparison.Ordinal);
                                var token = commText.Substring(tokenStart + 2, tokenEnd - tokenStart - 2);

                                if (!ret.ReturnValues.TryGetValue(token, out var tokenValue))
                                {
                                    // Make it return the bad value
                                    tokenError = token;
                                    break;
                                }

                                commText = commText.Replace("<<" + token + ">>", tokenValue);
                            }



                            if (tokenError == "")
                            {
                                if (key.Equals("CacheFlush", StringComparison.InvariantCultureIgnoreCase))
                                {
                                    foreach (var s in commText.Split(',', StringSplitOptions.RemoveEmptyEntries))
                                        _cache.Remove(s);
                                    continue;
                                }

                                SqlCommand comm;
                                switch (sqlTarget)
                                {
                                    case "ibex":
                                        comm = ibexComm;
                                        if (ibexConn.State != ConnectionState.Open)
                                            ibexConn.Open();
                                        break;
                                    case "emar":
                                        comm = emarComm;
                                        if (emarConn.State != ConnectionState.Open)
                                            emarConn.Open();
                                        break;
                                    default:
                                        throw new ArgumentException($"Switch statement didn't account for value: {sqlTarget}");
                                }

                                comm.CommandText = commText;
                                var stopwatch = new Stopwatch();
                                stopwatch.Start();
                                queryResult = comm.ExecuteScalar()?.ToString();
                                stopwatch.Stop();
                                if (stopwatch.ElapsedMilliseconds > 200)
                                {
                                    ret.AddPerformanceReport(key, stopwatch.ElapsedMilliseconds);
                                }
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

                        if (queryResult != null
                            && key.StartsWith("Combo", StringComparison.InvariantCultureIgnoreCase)
                            && !queryResult.StartsWith("ERROR:"))
                        {
                            // Code in the Request that ends up in this "Comboxxx" loop
                            // "Combo": "DECLARE @quickListItemId int, @quickListUserId int; SELECT @quickListItemId = id,
                            // @quickListUserId = user_id FROM user_quick_list_items WHERE site_id = 16 and drug_id = 282127
                            // and brand_name = 'Percocet';
                            // SELECT CONCAT('QuickListItemId|', @quickListItemId, '~QuickListUserId|', @quickListUserId)",
                            foreach (var ss in queryResult.Split('~').Select(s => s.Split('|')))
                            {
                                ret.AddResponse(ss[0], ss[1]);
                            }
                        }
                        else if (!key.StartsWith("Ignore", StringComparison.InvariantCultureIgnoreCase) ||
                                 tokenError != "")
                            ret.AddResponse(key, queryResult);
                    }
                }
            }

            if (errorEncountered)
                return BadRequest(ret);

            return Ok(ret);
        }
    }

    /// <summary>
    /// Output for the ad hoc SQL Queries
    /// </summary>
    internal class ReturnReport
    {
        /// <summary>
        /// Response Values
        /// </summary>
        public Dictionary<string, string> ReturnValues { get; private set; }

        /// <summary>
        /// If any SQL query takes more than the threshold amount of time, it will be reported here
        /// </summary>
        public Dictionary<string, long> PerformanceReport { get; private set; }

        internal void AddPerformanceReport(string queryKey, in long elapsedMilliseconds)
        {
            PerformanceReport ??= new Dictionary<string, long>();

            PerformanceReport.Add(queryKey, elapsedMilliseconds);
        }

        internal void AddResponse(string queryKey, string queryResult)
        {
            ReturnValues ??= new Dictionary<string, string>();

            ReturnValues.Add(queryKey, queryResult);
        }
    }
}
