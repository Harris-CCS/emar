using System.Data.Linq.Mapping;
using PulseCheck.Data.Common.DataAccess;

namespace PulseCheck.QCPR.Data.AccessObject
{
    [Dapper.Contrib.Extensions.Table("QcprRoute")]
    public class Route : IData
    {
        [Dapper.Contrib.Extensions.Key]
        public long Id { get; set; }
        public long QcprProductId { get; set; }
        public string Name { get; set; }
    }

}
