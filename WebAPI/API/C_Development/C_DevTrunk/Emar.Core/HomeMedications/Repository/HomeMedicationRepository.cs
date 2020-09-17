using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Emar.Data;
using Emar.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Emar.Core.HomeMedications.Repository
{
    public class HomeMedicationRepository: IHomeMedicationRepository
    {
        private readonly EmarContext _context;

        public HomeMedicationRepository()
        {

        }

        public HomeMedicationRepository(EmarContext emarContext)
        {
            _context = emarContext ?? throw new ArgumentNullException(nameof(emarContext));
        }

        public IEnumerable<PatientHomeMedication> GetMedications(Func<PatientHomeMedication, bool> wherePredicate)
        {
            return _context.PatientHomeMedications
                    .Include(med => med.MedicationRoute)
                    .Include(med => med.MedicationUnit)
                    .Include(med => med.AddUser)
                    .Include(med => med.Patient)
                        .ThenInclude(patient => patient.Site)
                            .ThenInclude(site => site.SiteOptions)
                                .ThenInclude(siteOptions => siteOptions.Option)
                    .Where(wherePredicate)
                    .AsEnumerable();
        }

        public IEnumerable<PatientHomeMedication> GetPatientHomeMedications(Expression<Func<PatientHomeMedication, bool>> wherePredicate)
        {
            return _context.PatientHomeMedications
                    .Where(wherePredicate)
                    .AsEnumerable();
        }

        public FdbBrandName GetPatientHomeMedicationFdbBrandName(long medicationId)
        {
            var query =
                from p in (from p in _context.PatientHomeMedications select p).Where(u => u.Id == medicationId)
                join n in _context.FdbNdcInfo on p.DrugId equals n.GcnSeqno.ToString()
                join s in _context.FdbBrandName on n.RoutedGenId equals s.RoutedGenId
                select s;

            return query.FirstOrDefault();
        }
    }
}