using System;
using log4net;

namespace PulseCheck.Common.Logging
{
    public class Logger : ILogger
    {
        private readonly ILog _log;


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="type"></param>
        public Logger(Type type)
        {
            _log = LogManager.GetLogger(type);

            if(_log == null)
                throw new NullReferenceException(nameof(_log));
        }

        public void Debug(string message)
        {
            _log?.Debug(message);
        }

        public void Debug(string message, Exception ex)
        {
            _log?.Debug(message, ex);
        }

        public void Info(string message)
        {
            _log?.Info(message);
        }

        public void Info(string message, Exception ex)
        {
            _log?.Info(message, ex);
        }

        public void Warn(string message)
        {
            _log?.Warn(message);
        }

        public void Warn(string message, Exception ex)
        {
            _log?.Warn(message, ex);
        }

        public void Error(string message)
        {
            _log?.Error(message);
        }

        public void Error(string message, Exception ex)
        {
            _log?.Error(message, ex);
        }

        public void Fatal(string message)
        {
            _log?.Fatal(message);
        }

        public void Fatal(string message, Exception ex)
        {
            _log?.Fatal(message, ex);
        }
    }
}
