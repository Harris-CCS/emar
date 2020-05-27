using System;

namespace PulseCheck.Common.Logging
{
    public static class LoggerFactory
    {
        public static Logger LoggerFor(Type type)
        {
            log4net.Config.XmlConfigurator.Configure();
            return new Logger(type);
        }
    }
}
