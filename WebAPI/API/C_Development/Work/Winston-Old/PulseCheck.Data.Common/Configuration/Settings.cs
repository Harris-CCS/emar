using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PulseCheck.Data.Common.Configuration
{
    public static class Settings
    {

        public static bool GetBool(string key)
        {
            bool retVal = false;

            if (ConfigurationManager.AppSettings.AllKeys.Contains(key))
            {
                bool.TryParse(ConfigurationManager.AppSettings[key], out retVal);
            }

            return retVal;
        }

        public static int GetInt(string key)
        {
            int retVal = 0;

            if (ConfigurationManager.AppSettings.AllKeys.Contains(key))
            {
                int.TryParse(ConfigurationManager.AppSettings[key], out retVal);
            }

            return retVal;
        }

        public static string GetString(string key)
        {
            string retVal = null;

            if (ConfigurationManager.AppSettings.AllKeys.Contains(key))
            {
                 retVal = ConfigurationManager.AppSettings[key];
            }

            return retVal;
        }
    }
}
