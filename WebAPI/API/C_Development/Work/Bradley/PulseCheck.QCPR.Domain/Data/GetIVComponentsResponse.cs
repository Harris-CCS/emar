using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PulseCheck.QCPR.Domain.Data
{
    public class GetIVComponentsResponse : GetProductsResponse
    {
        public bool NeedsIndication { get; set; }
    }
}
