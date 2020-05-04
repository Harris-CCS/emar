using DomainModel.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;

namespace DomainModel
{
    public class Age
    {
        public byte Value { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public AgeUnit Unit { get; set; }

        public DateTime? DateOfBirth { get; set; }
    }
}