namespace DomainModel
{
    public class Location
    {
        public string GroupType { get; set; }
        public string Department { get; set; }
        public string Ward { get; set; }
        private string _bed { get; set; }
        public string Bed
        {
            get { return this._bed != null ? this._bed.Trim() : ""; }
            set { this._bed = value?.Trim() ?? ""; }
        }
        public string Id { get; set; }
        public string Name { get; set; }

        // TODO: This idea keeps coming up. Need to refactor Patient/Person with an interface to make this easier and reusable.
        public MinimallyIdentifiedPatient Patient { get; set; }

        public class MinimallyIdentifiedPatient
        {
            public string Ibex { get; set; }
            public string LastName { get; set; }
            public string FirstName { get; set; }
            public string MiddleName { get; set; }
            public string Suffix { get; set; }
        }
    }
}