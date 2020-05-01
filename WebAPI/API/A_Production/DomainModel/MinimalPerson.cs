namespace DomainModel
{
    public class MinimalPerson
    {
        public MinimalPerson()
        {
            _suffix = "";
            _middlename = "";
        }

        public string FirstName { get; set; }
        public string LastName { get; set; }

        private string _middlename { get; set; }
        public string MiddleName
        {
            get { return this._middlename.Trim(); }
            set { this._middlename = value.Trim(); }
        }

        private string _suffix { get; set; }
        public string Suffix
        {
            get { return this._suffix.Trim(); }
            set { this._suffix = value?.Trim() ?? ""; }
        }

        public string Prefix { get; set; }
    }
}