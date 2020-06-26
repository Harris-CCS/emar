using System.Collections.Generic;
using Emar.Core.Patients.Model;
using Emar.Data;
using Emar.Data.Entities;

namespace Emar.Core.Patients.Repository
{
    public interface IPatientRepository
    {
        Patient GetPatient(long patientId);
        IEnumerable<Patient> GetPatients(bool activeOnly, int siteId);
    }
}