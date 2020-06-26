using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using Emar.Data;
using Emar.Data.Entities;

namespace Emar.Core.Patients.Repository
{
    public class PatientRepository:IPatientRepository
    {
        private readonly EmarContext _context;
        //private Dictionary<int, Patient> _testingPatients;

        public PatientRepository(EmarContext emarContext)
        {
            _context = emarContext;
            //    if (_testingPatients == null)
            //    {
            //        var filename = @".\bin\Debug\netcoreapp3.1\Patients\Repository\MockData\PatientData.txt";
            //        if (!File.Exists(filename))
            //            throw new FileNotFoundException("Couldn't locate the Mock Patient Data file", filename);

            //        _testingPatients = new Dictionary<int, Patient>();

            //        //foreach (var ln in File.ReadLines(filename))
            //        //{
            //        //    var cvsLines = ln.Split('|');
            //        //    var pt = new Patient
            //        //    {
            //        //        Id = Convert.ToInt32(cvsLines[0]),
            //        //        SiteId = Convert.ToInt16(cvsLines[1]),
            //        //        Active = Convert.ToBoolean(cvsLines[2].Trim() != "0"),

            //        //        FirstName = cvsLines[3],
            //        //        MiddleName = cvsLines[4],
            //        //        LastName = cvsLines[5],
            //        //        NameSuffix = cvsLines[6],
            //        //        Gender = cvsLines[7],
            //        //    };
            //        //    _testingPatients.Add(pt.Id, pt);
            //        //}

            //        var pts = File.ReadLines(filename)
            //            .Select(line => line.Split('|')).Select(cvsLines => new Patient
            //            {
            //                Id = Convert.ToInt32(cvsLines[0]),
            //                SiteId = Convert.ToInt16(cvsLines[1]),
            //                Active = Convert.ToBoolean(cvsLines[2].Trim() != "0"),

            //                FirstName = cvsLines[3],
            //                MiddleName = cvsLines[4],
            //                LastName = cvsLines[5],
            //                NameSuffix = cvsLines[6],
            //                Gender = cvsLines[7],
            //                //DateOfBirth = cvsLines.Length > 8 ? Convert.ToDateTime(cvsLines[8]) : (DateTime?)null,
            //                //Age = cvsLines.Length > 9 ? Convert.ToInt32(cvsLines[9]) : (int?)null,
            //                //AgeUnits = cvsLines.Length > 10 ? cvsLines[10] : null
            //            });

            //        foreach (var pt in pts)
            //            _testingPatients.Add(pt.Id, pt);
            //    }
        }
        public Patient GetPatient(long patientId)
        {
            var patient = _context.Patients.Find(patientId);
            
            return patient;
        }

        public IEnumerable<Patient> GetPatients(bool activeOnly, int siteId)
        {
            var patients = _context.Patients.ToList();

            return patients;
            //if (activeOnly)
            //    return from pt in _testingPatients
            //        where pt.Value.Active
            //        select pt.Value;

            //return _testingPatients.Values;
        }

    }
}
