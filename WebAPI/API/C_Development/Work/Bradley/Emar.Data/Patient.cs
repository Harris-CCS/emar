using System;
using System.Collections.Generic;
using System.Text;

namespace Emar.Data
{
    public class Patient
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public string Suffix { get; set; }
    }
}
