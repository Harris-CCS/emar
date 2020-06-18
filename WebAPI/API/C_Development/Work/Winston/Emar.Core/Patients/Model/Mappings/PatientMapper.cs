using System;
using System.Collections.Generic;
using System.Text;
using Emar.Data;

namespace Emar.Core.Patients.Model.Mappings
{
    public static class PatientMapper
    {
        public static PatientDto MapPatient(Patient pt)
        {
            PatientDto ret = new PatientDto
            {
                Id = pt.Id,
                FirstName = pt.FirstName,
                MiddleName = pt.MiddleName,
                LastName = pt.LastName,
                Suffix = pt.Suffix
            };
            return ret;
        }
    }
}
