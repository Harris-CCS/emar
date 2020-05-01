using System;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;

namespace PulseCheck.API.Models
{
    /// <summary>
    /// Extends IHttpActionResult to allow us to build error responses using nothing more than the single-character PulseCheck code.
    /// </summary>
    public class WLRResponse : System.Exception, IHttpActionResult
    {
        private string message { get; set; }
        private string animalURL { get; set; }
        private string additionalInfo { get; set; }
        private HttpStatusCode statusCode { get; set; }
        private HttpRequestMessage request { get; set; }

        /// <summary>
        /// Build a response using the negated ASCII code for the character code. This can be called immediately
        /// after receiving a negative int as a result code from a stored procedure.
        /// </summary>
        /// <param name="code">Negated ASCII code for character code</param>
        /// <param name="req">HttpRequestMessage associated with this response</param>
        public WLRResponse(int code, HttpRequestMessage req)
        {
            char letter = Convert.ToChar(code * -1);
            var r = new Error(letter);
            request = req;
            init(r);
        }

        /// <summary>
        /// Build a response using the single-character error code. This can be called anywhere in the code and should use
        /// the constants defined in PulseCheck.API.ErrorCodes.
        /// </summary>
        /// <param name="code">Character code</param>
        /// <param name="req">HttpRequestMessage associated with this response</param>
        public WLRResponse(char code, HttpRequestMessage req)
        {
            var r = new Error(code);
            request = req;
            init(r);
        }

        /// <summary>
        /// Build a response using the single-character error code, and include a string of additional information.
        /// This can be called anywhere in the code and should use the constants defined in PulseCheck.API.ErrorCodes.
        /// </summary>
        /// <param name="code">Character code</param>
        /// <param name="AdditionalInfo">String of additional information to include in response</param>
        /// <param name="req">HttpRequestMessage associated with this response</param>
        public WLRResponse(char code, string AdditionalInfo, HttpRequestMessage req)
        {
            var r = new Error(code);
            request = req;
            init(r);
            additionalInfo = AdditionalInfo;
        }

        /// <summary>
        /// Initialize object attributes using a provided Error object
        /// </summary>
        /// <param name="r">Error instance</param>
        private void init(Error r)
        {
            message = r.message;
            statusCode = r.statusCode;
            animalURL = r.animalURL;
            additionalInfo = r.additionalInfo;
        }

        /// <summary>
        /// Create the _response object
        /// </summary>
        /// <returns>New _response object</returns>
        public _response AssembleResponse()
        {
            var r = new _response()
            {
                Message = message,
                AdditionalInfo = additionalInfo
            };

            if (!string.IsNullOrWhiteSpace(animalURL))
            {
                animalURL = "/rmi/images/" + animalURL + ".jpg";
                if (ConfigurationManager.AppSettings.AllKeys.Contains("PulseCheckURL"))
                {
                    var pcURL = ConfigurationManager.AppSettings["PulseCheckURL"].Trim();
                    if (pcURL.EndsWith("/"))
                    {
                        pcURL = pcURL.Substring(0, pcURL.Length - 1);
                    }
                    animalURL = pcURL + animalURL;
                }
                r.AnimalURL = animalURL;
            }

            return r;
        }

        /// <summary>
        /// Generate the HttpResponseMessage Task. There shouldn't be a need to call this directly. It's here because the interface needs it.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns></returns>
        public Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(request.CreateResponse(statusCode, AssembleResponse()));
        }
       
        /// <summary>
        /// Generic error response
        /// </summary>
        public class _response
        {
            /// <summary>
            /// Response message
            /// </summary>
            public string Message { get; set; }

            /// <summary>
            /// URL for response's animal error image
            /// </summary>
            public string AnimalURL { get; set; }

            /// <summary>
            /// Optional string of additional information to include with response message
            /// </summary>
            public string AdditionalInfo { get; set; }
        }
    }
}