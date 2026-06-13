namespace PulseCheck.Domain
{
    public class Urgency
    {
        private string _name { get; set; }
        public string Name
        {
            get { return this._name.Trim(); }
            set { this._name = value?.Trim() ?? ""; }
        }
        private string _eun { get; set; }
        public string Eun
        {
            get { return this._eun != null ? this._eun.Trim() : null; }
            set { this._eun = value?.Trim() ?? ""; }
        }
        public Style Style { get; set; }
    }
}