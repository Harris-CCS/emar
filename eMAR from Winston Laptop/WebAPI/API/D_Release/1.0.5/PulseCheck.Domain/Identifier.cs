namespace PulseCheck.Domain
{
    public class Identifier
    {
        private string _name { get; set; }
        public string Name
        {
            get { return this._name != null ? this._name.Trim() : ""; }
            set { this._name = value?.Trim() ?? ""; }
        }


        private string _value { get; set; }
        public string Value
        {
            get { return this._value != null ? this._value.Trim() : "";  }
            set { this._value = value?.Trim() ?? ""; }
        }
    }
}