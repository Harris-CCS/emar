using System;
using PulseCheck.Common.Logging;

namespace PulseCheck.Database.Management.Logic.Manager
{
    public class ManagerBase
    {
        protected ILogger Logger { get; }

        public ManagerBase(ILogger logger = null)
        {
            Logger = logger;
        }

        protected void LogInfo(string message, Exception ex = null)
        {
            Logger?.Info(message, ex);
        }

        protected void LogError(string message, Exception ex = null)
        {
            Logger?.Error(message, ex);
        }
    }
}
