using System;
using System.Collections.Generic;
using System.Linq;
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

    }
}
