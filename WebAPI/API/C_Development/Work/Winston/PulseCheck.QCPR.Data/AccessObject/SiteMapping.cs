using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PulseCheck.Data.Common.DataAccess;

namespace PulseCheck.QCPR.Data.AccessObject
{
    [Dapper.Contrib.Extensions.Table("QcprSiteMapping")]
    public class SiteMapping : IData
    {

        public int SiteId { get; set; }
        public string FacilityName { get; set; }
    }
}
