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
        
        [VersionedRoute("api/qcpr/procedures/{name}", 1)]
        [Route("api/v1/qcpr/procedures/{name}")]
        [HttpGet]
        public IHttpActionResult GetProcedures(string name)
        {
                return Ok(_qcprManager.GetProceduresByName(name));
        }

        [VersionedRoute("api/qcpr/products/{name}", 1)]
        [Route("api/v1/qcpr/products/{name}")]
        [HttpGet]
        public IHttpActionResult GetProductsByName(string name)
        {
            return Ok(_qcprManager.GetProductsByName(name));
        }

        [VersionedRoute("api/qcpr/products/{id}", 1)]
        [Route("api/v1/qcpr/products/{id}")]
        [HttpGet]
        public IHttpActionResult GetProductsById(long id)
        {
            return Ok(_qcprManager.GetProductsById(id));
        }


    }
}
