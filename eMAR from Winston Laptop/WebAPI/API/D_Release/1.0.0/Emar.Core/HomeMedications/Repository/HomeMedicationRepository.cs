using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Emar.Data;
using Emar.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Emar.Core.HomeMedications.Repository
{
    public class HomeMedicationRepository : IHomeMedicationRepository
    {
        private readonly EmarContext _context;

        public HomeMedicationRepository()
        {

        }

        public HomeMedicationRepository(EmarContext emarContext)
        {
            _context = emarContext ?? throw new ArgumentNullException(nameof(emarContext));
        }

        public IEnumerable<PatientHomeMedication> GetHomeMedications(Expression<Func<PatientHomeMedication, bool>> wherePredicate)
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

        public PatientHomeMedication GetHomeMedication(long medicationId)
        {
            return GetHomeMedications(homeMed => homeMed.Id == medicationId)
                .FirstOrDefault();
        }

        public IEnumerable<PatientHomeMedication> GetPatientHomeMedications(
            Expression<Func<PatientHomeMedication, bool>> wherePredicate)
        {
            if (wherePredicate == null)
                throw new ArgumentException("Shouldn't be calling GetPatientHomeMedications without a wherePredicate",
                    nameof(wherePredicate));

            return _context.PatientHomeMedications
                .Include(h => h.Medication)
                    .ThenInclude(m => m.MedicationDetails)
                        .ThenInclude(d => d.FdbBrandName)
                .Include(m => m.Medication)
                    .ThenInclude(m => m.MedicationDetails)
                        .ThenInclude(d => d.MedicationUnit)
                .Where(wherePredicate)
                .ToList();
        }

        public FdbBrandName GetPatientHomeMedicationFdbBrandName(long medicationId)
        {
            var query =
                from p in (from p in _context.PatientHomeMedications select p).Where(u => u.Id == medicationId)
                join n in _context.FdbNdcInfo on p.Medication.DrugId equals n.GcnSeqno.ToString()
                join s in _context.FdbBrandName on n.RoutedGenId equals s.RoutedGenId
                select s;

            return query.FirstOrDefault();
        }
        public FdbBrandName GetPatientHomeMedicationFdbBrandNameByPcRoutedGenId(string internalDrugId)
        {
            var query =
                from p in (from p in _context.PatientHomeMedications select p).Where(u => u.InternalDrugId == internalDrugId)
                join n in _context.FdbBrandName on p.InternalDrugId equals n.PcRoutedGenId
                select n;

            return query.FirstOrDefault();
        }
    }
}