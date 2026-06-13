using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PulseCheck.QCPR.Domain.Data
{
    public class Product
    {
        public string DDID { get; set; }

        public string GPI { get; set; }

        [JsonProperty("code")]
        public string Code { get; set; }

        [JsonProperty("form")]
        public string Form { get; set; }

        [JsonProperty("form_interface")]
        public string FormInterface { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("route")]
        public Route[] Routes { get; set; }

        [JsonProperty("strength")]
        public string Strength { get; set; }

        [JsonProperty("interface")]
        public string Interface { get; set; }

        [JsonProperty("concentration_name")]
        public string ConcentrationName { get; set; }
    }
}
