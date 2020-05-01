using NLog;
using System.Net.Http;
using System.Text;
using System.Web.Http.ExceptionHandling;

namespace PulseCheck.API.Helpers
{
    /// <summary>
    /// Log exceptions in the API
    /// </summary>
    public class NLogExceptionLogger : ExceptionLogger
    {
        private static readonly Logger Nlog = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Log an exception
        /// </summary>
        /// <param name="context">ExceptionLoggerContext instance</param>
        public override void Log(ExceptionLoggerContext context)
        {
            Nlog.Log(LogLevel.Error, context.Exception, RequestToString(context.Request));
        }

        /// <summary>
        /// Stringify a request
        /// </summary>
        /// <param name="request">HttpRequestMessage instance</param>
        /// <returns></returns>
        private static string RequestToString(HttpRequestMessage request)
        {
            var message = new StringBuilder();
            if (request.Method != null)
            {
                message.Append(request.Method);
            }

            if (request.RequestUri != null)
            {
                message.Append(" ").Append(request.RequestUri);
            }

            return message.ToString();
        }
    }
}