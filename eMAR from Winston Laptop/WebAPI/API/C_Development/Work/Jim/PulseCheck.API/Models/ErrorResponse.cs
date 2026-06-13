using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;

namespace PulseCheck.API.Models
{
    /// <summary>
    /// Extends IHttpActionResult to allow us to build error responses consistent error responses.
    /// </summary>
    public class ErrorResponse : IHttpActionResult
    {
        private string message { get; set; }
        private HttpStatusCode statusCode { get; set; }
        private HttpRequestMessage request { get; set; }

        /// <summary>
        /// Build a response using a provided message and status code
        /// </summary>
        /// <param name="msg">Message</param>
        /// <param name="code">Status code</param>
        /// <param name="req">HttpRequestMessage associated with this response</param>
        public ErrorResponse(string msg, int code, HttpRequestMessage req)
        {
            var r = new Error(msg, code);
            message = r.message;
            statusCode = r.statusCode;
            request = req;
        }

        /// <summary>
        /// Generate the HttpResponseMessage Task. There shouldn't be a need to call this directly. It's here because the interface needs it.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns></returns>
        public Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(request.CreateResponse(statusCode, new _response() { Message = message }));
        }
       
        private class _response
        {
            public string Message { get; set; }
        }
    }
}