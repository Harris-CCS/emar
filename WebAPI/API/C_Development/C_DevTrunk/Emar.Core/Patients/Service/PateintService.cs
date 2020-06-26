using System.Collections.Generic;
using System.Linq;
using Emar.Core.Patients.Model;
using Emar.Core.Patients.Model.Mappings;
using Emar.Core.Patients.Repository;
using Emar.Data;
using Emar.Data.Entities;

namespace Emar.Core.Patients.Service
{
    public class PatientService : IPatientService
    {
        private IPatientRepository _patientRepository;

        public PatientService(IPatientRepository patientRepository)
        {
            _patientRepository = patientRepository;
        }

        public PatientDto GetPatient(long patientId)
        {
            Patient entityPatient = _patientRepository.GetPatient(patientId);

            PatientDto ret = PatientMapper.MapPatient(entityPatient);
            return ret;

        }

        public long GetPatientIdFromPulseCheck(in int siteId, string ibex)
        {
            throw new System.NotImplementedException();
        }

        public IEnumerable<PatientDto> GetPatients(bool activeOnly, int siteId)
        {
            IEnumerable<Patient> patients = _patientRepository.GetPatients(activeOnly, siteId);

            //if(activeOnly)
            //    return from pt in patients
            //           where pt.Active
            //        select PatientMapper.MapPatient(pt)
            //        ;

            return from pt in patients
                select PatientMapper.MapPatient(pt);

            //foreach (var patient in patients)
            //{
            //    ret.Add(PatientMapper.MapPatient(patient));
            //}

            //return ret.ToArray();
        }
    }
}
