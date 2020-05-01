using System.ComponentModel.DataAnnotations.Schema;

namespace PulseCheck.Domain
{
    [NotMapped]
    public class MinimalSite
    {
        public MinimalSite()
        {
            _name = "";
        }

        public byte Id { get; set; }

        private string _name;
        public string Name
        {
            get { return this._name.Trim(); }
            set { this._name = value.Trim(); }
        }
    }
}