using System.Collections.Generic;
using DomainModel.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace DomainModel
{
    public class Demographics
    {
        public Demographics()
        {
            Gender = Gender.Unknown;
            Races = new List<Race>();
            Age = new Age();
            Ethnicity = new Ethnicity();
            PreferredLanguage = new Language();            
        }

        [JsonConverter(typeof(StringEnumConverter))]
        public Gender Gender { get; set; }
        public List<Race> Races { get; set; }
        public Age Age { get; set; }
        public Ethnicity Ethnicity { get; set; }
        public Language PreferredLanguage { get; set; }
    }
}