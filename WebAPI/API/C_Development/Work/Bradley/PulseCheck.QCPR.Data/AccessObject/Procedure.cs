using System.Data.Linq.Mapping;
using PulseCheck.Data.Common.DataAccess;

namespace PulseCheck.QCPR.Data.AccessObject
{
    [Dapper.Contrib.Extensions.Table("QcprProcedure")]
    public class Procedure : IData
    {
        [Dapper.Contrib.Extensions.Key]
        public long QcprProcedureId { get; set; }

        public long QcprImportId { get; set; }

        public int? SiteId { get; set; }

        public string Code { get; set; }
        public string Facility { get; set; }
        public string Interface { get; set; }
        public string Name { get; set; }

        [Dapper.Contrib.Extensions.Write(false)]
        public Product[] Products { get; set; }
    }
}
