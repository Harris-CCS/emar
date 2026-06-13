using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace PulseCheck.QCPR.Domain.Data
{
    public class Route
    {
        [JsonProperty("name")]
        public string Name { get; set; }
    }

}
