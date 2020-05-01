using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PulseCheck.QCPR.Domain.Data
{
    public class GetProductsResponse
    {
        public string DDID { get; set; }

        public string GPI { get; set; }

        public string Code { get; set; }

        public string Name { get; set; }

        public string Strength { get; set; }

        public long Id { get; set; }
    }
}
