using System.ComponentModel.DataAnnotations.Schema;

namespace DomainModel
{
    public class Person
    {
        public Person()
        {
            _suffix = "";
            _middlename = "";
            //ContactInfo = new ContactInfo();    
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

        //[NotMapped]
        //public ContactInfo ContactInfo { get; set; }
    }
}