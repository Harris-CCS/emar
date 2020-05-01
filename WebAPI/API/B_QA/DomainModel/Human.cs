using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DomainModel.Options;

namespace DomainModel
{
    public class Human
    {
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public string Suffix { get; set; }
        public string Prefix { get; set; }
        public Gender GenderIdentity { get; set; }
        public Gender BirthGender { get; set; }
        public DateTime DateOfBirth { get; set; }
    }
}