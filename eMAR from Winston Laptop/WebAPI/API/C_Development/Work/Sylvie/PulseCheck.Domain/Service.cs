using System.Collections.Generic;

namespace PulseCheck.Domain
{
    public class Service
    {
        private string _code;
        public string Code {
            get { return _code; }
            set { _code = value.TrimEnd(); }
        }
        public string Name { get; set; }
        public int Number { get; set; }
        public List<Query> Queries { get; set; } = new List<Query>();
        public int MaxQuantity { get; set; }
        public string InterfaceType { get; set; }
        public int Type { get; set; }
        public bool IsUserFavorite { get; set; }

        public Service ()
        {

        }
    }
}