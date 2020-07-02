using System.Collections.Generic;
using Emar.Core.Patients.Model;

namespace Emar.Core.Patients.Service
{
    public interface IPatientService
    {
        PatientDto GetPatient(long patientId, ResourceParameters resourceParameters);
#if ORIGINAL
        IEnumerable<PatientDto> GetPatients(ResourceParameters resourceParameters);
#endif
#if PAGING || SORTING || EXPANDO
        PagedList<PatientDto> GetPatients(ResourceParameters resourceParameters);
#endif
    }
}