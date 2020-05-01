using System.Net;

namespace PulseCheck.API.Models
{
    /// <summary>
    /// Generic error response object
    /// </summary>
    public class Error
    {
        /// <summary>
        /// Letter code for error response
        /// </summary>
        public char letter { get; set; }

        /// <summary>
        /// Longer, meaningful message associated with error code
        /// </summary>
        public string message { get; set; }

        /// <summary>
        /// URL to an animal image for this error
        /// </summary>
        public string animalURL { get; set; }

        /// <summary>
        /// String of additional text that can be displayed with the error
        /// </summary>
        public string additionalInfo { get; set; }

        /// <summary>
        /// HTTP status code associated with error code
        /// </summary>
        public HttpStatusCode statusCode { get; set; }

        /// <summary>
        /// Create a new Error for a provided error code
        /// </summary>
        /// <param name="code"></param>
        public Error(char code)
        {
            letter = code;
            message = ErrorCodes.errors[code].message;
            animalURL = ErrorCodes.errors[code].animal;
            additionalInfo = ErrorCodes.errors[code].additionalInfo;
            statusCode = (HttpStatusCode)ErrorCodes.errors[code].statusCode;
        }

        /// <summary>
        /// Create a new Error from a provided message and status code
        /// </summary>
        /// <param name="msg">Error message</param>
        /// <param name="code">Status code</param>
        public Error(string msg, int code)
        {
            letter = 'X';
            message = msg;
            animalURL = null;
            additionalInfo = null;
            statusCode = (HttpStatusCode)code;
        }
    }
}
