using System;
using System.Diagnostics;
using System.Reflection;

namespace PulseCheck.Utilities
{
    public class Version
    {
        public int MinorVersion { get; set; }
        public int MajorVersion { get; set; }
        public override string ToString()
        {
            return MajorVersion + "." + MinorVersion;
        }

        public Version(string productVersion)
        {
            var versionParts = productVersion.Split('.');
            MajorVersion = Convert.ToInt32(versionParts[0]);
            MinorVersion = Convert.ToInt32(versionParts[1]);
        }

        /// <summary>
        /// Returns version like 2.1.15
        /// </summary>
        public static Version APIVersion
        {
            get
            {
                return new Version(FileVersionInfo.GetVersionInfo(Assembly.GetCallingAssembly().Location).ProductVersion);
            }
        }
    }
}
