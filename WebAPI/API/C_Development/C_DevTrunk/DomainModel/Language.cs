namespace DomainModel
{
    public class Language
    {
        private string _name { get; set; }
        public string Name
        {
            get { return this._name != null ? this._name.Trim() : ""; }
            set { this._name = value?.Trim() ?? ""; }
        }

        private string _code { get; set; }
        public string Code
        {
            get { return this._code != null ? this._code.Trim() : ""; }
            set { this._code = value?.Trim() ?? ""; }
        }
    }
}