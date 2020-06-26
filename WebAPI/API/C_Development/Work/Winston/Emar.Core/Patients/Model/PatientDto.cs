using System;
using System.Collections.Generic;

namespace Emar.Core.Patients.Model
{
    public class PatientDto
    {
        public long Id { get; set; }
        public short SiteId { get; set; }

        public bool Active { get; set; }

        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public string Suffix { get; set; }
        public string FullName
        {
            get
            {
                var firstName = (FirstName ?? "").Trim();
                if (firstName.Length == 1)
                    firstName += ".";

                var mddleName = (MiddleName ?? "").Trim();
                if (mddleName.Length == 1)
                    mddleName += ".";

                var ret = firstName;
                ret += (ret != "" && !string.IsNullOrWhiteSpace(mddleName)) ? " " : "";
                ret += mddleName;
                ret += (ret != "" && !string.IsNullOrWhiteSpace(LastName)) ? " " : "";
                 ret += (LastName ?? "").Trim();
                ret += ((!string.IsNullOrWhiteSpace(ret) && !string.IsNullOrWhiteSpace(Suffix)) ? ", " : "") +
                    (Suffix ?? "").Trim();
                return ret;
            }
        }

        public string Gender { get; set; }

        public DateTime? DateOfBirth { get; set; }
        public int? Age { get; set; }
        public string AgeUnits { get; set; }

        public string ChiefComplaint { get; set; }

        // geographical stuff - room, ward, department
        public string SiteName { get; set; }
        public string DepartmentCode { get; set; }
        public string WardCode { get; set; }
        public string RoomBedCode { get; set; }

        // vital signs
        public int HeightInCm { get; set; }
        public int WeightInKg { get; set; }

        public string UrgencyColor { get; set; }

        //private List<Allergy> Allergies { get; set; }
        //private List<CurrentMedication> HomeMedications { get; set; }
    }
}
