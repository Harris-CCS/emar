using System;
using System.Collections.Generic;
using System.Text;
using Emar.Core.Users.Model;
using Emar.Data.Entities;

namespace Emar.Core.Patients.Model
{
    public class PatientAllergyDto
    {
        public long Id { get; set; }

        public long PatientId { get; set; }
        internal string Class { get; set; }
        internal string Category { get; set; }
        public string InternalDrugId { get; set; }
        public string Ndc { get; set; }
        public string DrugId { get; set; }
        public string Name { get; set; }
        public string AlternateName { get; set; }
        public string AllergyDrugId { get; set; }
        public bool IsActive { get; set; }
        public string Comment { get; set; }
        public string Schedule { get; set; }
        public string Reaction { get; set; }
        public string Severity { get; set; }

        public string ParentDrugId { get; set; }
        public string ParentDrugName { get; set; }

        // BRM: 8/24/2020 - user data not needed by UI (Marco)
        //public int AddUserId { get; set; }
        //public UserDto AddUser { get; set; }
        //public DateTimeOffset? AddDatetime { get; set; }

        //public int ChangeUserId { get; set; }
        //public UserDto ChangeUser { get; set; }
        //public DateTimeOffset? ChangeDatetime { get; set; }

        public string ActionStatus { get; set; }
        public string InformationSourceCode { get; set; }
        public string InformationSource
        {
            get
            {
                switch (InformationSourceCode)
                {
                    case "PC": return "PulseCheck";
                    case "HIE": return "HIE/CCD";
                    case "ADT": return "Interface";
                    default:
                        return InformationSourceCode;
                }
            }
        }
    }
}