using System.Collections.Generic;
using Emar.Core.Patients.Model;

namespace Emar.Core.Patients.Service
{
    public interface IPatientService
    {
        PatientDto GetPatient(long patientId);
        IEnumerable<PatientDto> GetPatients(bool activeOnly, int siteId);
        long GetPatientIdFromPulseCheck(in int siteId, string ibex);
    }
}