using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Routing;

namespace PulseCheck.API.Controllers
{
    public class ValuesController : ApiController
    {
        // GET api/values
        [VersionedRoute("api/values", 1)]
        [Route("api/v1/values")]
        public IEnumerable<string> GetV1()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/values/5
        [VersionedRoute("api/values/{id}", 1)]
        [Route("api/v1/values/{id}")]
        public string GetV1(int id)
        {
            return "value";
        }

        // GET api/values/5
        [VersionedRoute("api/values/{id}", 2)]
        [Route("api/v2/values/{id}")]
        public string GetV2(int id)
        {
            return "value+2";
        }

        // POST api/values
        public void Post([FromBody]string value)
        {
        }

        // PUT api/values/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/values/5
        public void Delete(int id)
        {
        }
    }
}
