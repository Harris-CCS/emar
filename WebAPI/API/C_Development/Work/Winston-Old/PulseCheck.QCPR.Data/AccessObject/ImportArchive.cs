using System;
using System.Data.Linq.Mapping;
using PulseCheck.Data.Common.DataAccess;

namespace PulseCheck.QCPR.Data.AccessObject
{
    [Dapper.Contrib.Extensions.Table("QcprImport")]
    public class ImportArchive : IData
    {
        [Dapper.Contrib.Extensions.Key]
        public long Id { get; set; }
        public string Json { get; set; }

        [Dapper.Contrib.Extensions.Write(false)]
        public DateTime Timestamp { get; set; }
    }

}
