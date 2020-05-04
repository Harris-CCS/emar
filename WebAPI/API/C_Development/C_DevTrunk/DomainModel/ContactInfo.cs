using System.Collections.Generic;

namespace DomainModel
{
    public class ContactInfo
    {
        public ContactInfo()
        {
            Address = new Address();
            Phones = new List<Phone>();
            Emails = new List<string>();
        }

        public Address Address { get; set; }
        public List<Phone> Phones { get; set; }
        public List<string> Emails { get; set; }
    }
}