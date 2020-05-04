namespace DomainModel
{
    public class Race
    {
        private string _name { get; set; }
        public string Name
        {
            get { return this._name.Trim(); }
            set { this._name = value?.Trim() ?? ""; }
        }

        private string _code { get; set; }
        public string Code
        {
            get { return this._code.Trim(); }
            set { this._code = value?.Trim() ?? ""; }
        }
    }
}