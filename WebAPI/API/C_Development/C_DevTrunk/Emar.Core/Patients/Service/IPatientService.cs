using System.Collections.Generic;
using Emar.Core.Patients.Model;

namespace Emar.Core.Patients.Service
{
    public interface IPatientService
    {
        PatientDto GetPatient(long patientId, ResourceParameters resourceParameters);
        IEnumerable<PatientDto> GetPatients(ResourceParameters resourceParameters);
    }
}