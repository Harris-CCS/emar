using System.Data.Linq.Mapping;
using Dapper.Contrib.Extensions;
using PulseCheck.Data.Common.DataAccess;

namespace PulseCheck.QCPR.Data.AccessObject
{
    [Dapper.Contrib.Extensions.Table("QcprProduct")]
    public class Product : IData
    {
        [Dapper.Contrib.Extensions.Key]
        public long Id { get; set; }

        public long QcprProcedureId { get; set; }

        public string DDID { get; set; }
        public string GPI { get; set; }
        public string Code { get; set; }
        public string Form { get; set; }
        public string FormInterface { get; set; }
        public string Name { get; set; }
        public string Strength { get; set; }
        public string Interface { get; set; }
        public string ConcentrationName { get; set; }

        [Dapper.Contrib.Extensions.Write(false)]
        public Route[] Routes { get; set; }
    }
}
