using System;
using Emar.Data;
using Emar.Data.Entities;

namespace Emar.Core.Patients.Model.Mappings
{
    public static class PatientMapper
    {
        public static PatientDto MapPatient(Patient pt)
        {
            if (pt == null)
                return null;

            PatientDto ret = new PatientDto
            {
                Id = pt.Id,
                SiteId = pt.SiteId,
                FirstName = pt.FirstName.Trim(),
                MiddleName = (pt.MiddleName == null) ? pt.MiddleName : pt.MiddleName.Trim(),
                LastName = pt.LastName.Trim(),
                Suffix = (pt.NameSuffix == null) ? pt.NameSuffix : pt.NameSuffix.Trim(),
                Active = true, //pt.Active,
                Gender = pt.Gender,
                DateOfBirth = pt.DateOfBirth,
                Age  = pt.Age,
                AgeUnits = pt.AgeUnits
            };

            // Calculate the age if the date-of-birth is present
            if (pt.DateOfBirth == null) return ret;
            var dateOfBirth = (DateTime) pt.DateOfBirth;
            var ageTimeSpan = DateTime.Now.Subtract(dateOfBirth);
            if (ageTimeSpan.TotalDays < 180)
            {
                ret.Age = (int) Math.Truncate(ageTimeSpan.TotalDays);
                ret.AgeUnits = "days";
            }
            else if (ageTimeSpan.TotalDays < 700)
            {
                ret.Age = (DateTime.Now.Day < dateOfBirth.Day  ? -1 : 0) +
                          DateTime.Now.Month - dateOfBirth.Month +
                          (DateTime.Now.Year - dateOfBirth.Year) * 12;
                ret.AgeUnits = "months";
            }
            else
            {
                ret.Age = (DateTime.Now.Month < dateOfBirth.Month || (DateTime.Now.Month == dateOfBirth.Month) &&
                              DateTime.Now.Day < dateOfBirth.Day
                                  ? -1
                                  : 0) +
                          (DateTime.Now.Year - dateOfBirth.Year);
                ret.AgeUnits = "years";
            }
            return ret;
        }
    }
}
