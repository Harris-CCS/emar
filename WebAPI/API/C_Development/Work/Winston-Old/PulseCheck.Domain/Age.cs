using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using PulseCheck.Domain.Options;

namespace PulseCheck.Domain
{
    public class Age
    {
        public byte Value { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public AgeUnit Unit { get; set; }

        public DateTime? DateOfBirth { get; set; }
    }
}