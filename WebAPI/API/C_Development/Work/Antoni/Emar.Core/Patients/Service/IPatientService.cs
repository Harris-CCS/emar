using System.Collections.Generic;
using Emar.Core.Helpers;
using Emar.Core.Patients.Model;
using Emar.Core.ResourceParameters;
using static Emar.Core.Patients.Model.Constants;

namespace Emar.Core.Patients.Service
{
    public interface IPatientService
    {
        PagedList<PatientDto> GetPatients(PatientsResourceParameters resourceParameters, bool includeOrders);
        PatientDto GetPatient(long patientId, PatientsResourceParameters resourceParameters, bool includeOrders);
        PatientDto GetPatient(short extId1, string extId2);
        PatientDto GetPatientByNumber(string number, GetPatientBy getPatientBy);
        Dictionary<string, string> GetExternalRootSitePatientId(string number, GetPatientBy getPatientBy, string rootType);
    }
}