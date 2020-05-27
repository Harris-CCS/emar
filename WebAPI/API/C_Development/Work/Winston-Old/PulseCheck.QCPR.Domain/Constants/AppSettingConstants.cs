using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PulseCheck.QCPR.Domain.Constants
{
    public static class AppSettingConstants
    {
        public static string QcprBaseUrl => "QcprBaseUri";
        public static string QcprProceduresUri => "QcprProceduresUri";
        public static string UseRedisCache => "UseRedisCache";
    }
}
