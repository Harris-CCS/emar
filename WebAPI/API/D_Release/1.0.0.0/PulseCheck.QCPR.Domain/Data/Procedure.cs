using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace PulseCheck.QCPR.Domain.Data
{
    public class Procedure
    {
        public long ImportArchiveId { get; set; }

        [JsonProperty("code")]
        public string Code { get; set; }

        [JsonProperty("facility")]
        public string Facility { get; set; }

        [JsonProperty("interface")]
        public string  Interface { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("product")]
        public Product[] Products { get; set; }
    }
}
