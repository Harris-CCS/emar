using System.Collections.Generic;
using Emar.Core.Helpers;
using Emar.Core.ResourceParameters;
using Emar.Data.Entities;
using static Emar.Core.Patients.Model.Constants;

namespace Emar.Core.Patients.Repository
{
    public interface IPatientRepository
    {
        PagedList<Patient> GetPatients(PatientsResourceParameters resourceParameters, bool includeOrders);
        Patient GetPatient(long? patientId, PatientsResourceParameters resourceParameters, bool includeOrders);
        long? GetPatientId(long? patientId, PatientsResourceParameters resourceParameters);
        long GetInternalPatientId(short extId1, string extId2);
        Dictionary<string, string> GetExternalRootSitePatientId(string number, GetPatientBy getPatientBy);
        Patient GetPatientByNumber(string number, GetPatientBy getPatientBy);
    }
}