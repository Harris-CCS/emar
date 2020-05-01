using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using PulseCheck.Domain;
using PulseCheck.QCPR.Domain.Contract;
using PulseCheck.QCPR.Domain.Data;
using PulseCheck.Utilities;

namespace PulseCheck.API.Controllers
{
    // [RoutePrefix("api/qcpr")]
    public class QcprController : ControllerBase
    {

        private IQcprManager _qcprManager;
        private readonly Authentication _authUtil = new Authentication();

        /// <summary>
        /// Ctor
        /// </summary>
        public QcprController()
        {
        }

        /// <summary>
        /// Ctor
        /// </summary>
        public QcprController(IQcprManager qcprManager)
        {
            _qcprManager = qcprManager;
        }
        
        [VersionedRoute("api/qcpr/{siteId}/procedures/{name}", 1)]
        [Route("api/v1/qcpr/{siteId}/procedures/{name}")]
        [HttpGet]
        [AllowAnonymous]
        public IHttpActionResult GetProcedures(byte siteId, string name)
        {
                return Ok(_qcprManager.GetProceduresByName(siteId, name));
        }

        [VersionedRoute("api/qcpr/procedure/{id}/products", 1)]
        [Route("api/v1/qcpr/procedure/{id}/products")]
        [HttpGet]
        [AllowAnonymous]
        public IHttpActionResult GetProductsByProcedureId(long id)
        {
            return Ok(_qcprManager.GetProductsByProcedureId(id));
        }

        [VersionedRoute("api/qcpr/{siteId}/products/{name}", 1)]
        [Route("api/v1/qcpr/{siteId}/products/{name}")]
        [HttpGet]
        [AllowAnonymous]
        public IHttpActionResult GetProductsByName(byte siteId, string name)
        {
            return Ok(_qcprManager.GetProductsByName(siteId, name));
        }

        [VersionedRoute("api/qcpr/product/{id}", 1)]
        [Route("api/v1/qcpr/product/{id}")]
        [HttpGet]
        [AllowAnonymous]
        public IHttpActionResult GetProductById(long id)
        {
            return Ok(_qcprManager.GetProductById(id));
        }
    }
}
