using System;
using System.Collections.Generic;
using System.Text;
using Emar.Data.Entities;

namespace Emar.Core.Users.Model
{
    public class UserHeaderDto
    {
        public int Id { get; set; }
        public string Type { get; set; }
        public bool Active { get; set; }
        public string InitialsDisplay { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName
        {
            get
            {
                var firstName = (FirstName ?? "").Trim();

                if (firstName.Length == 1)
                {
                    firstName += ".";
                }

                var ret = firstName;

                ret += (ret != "" && !string.IsNullOrWhiteSpace(LastName)) ? " " : "";
                ret += (LastName ?? "").Trim();

                return ret;
            }
        }
        public bool OrderingOnlyPhysician { get; set; }
        public bool NameDisplayPreference { get; set; }

        public Site Site { get; set; }
    }
}
