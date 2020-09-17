using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Emar.Data.Entities;

namespace Emar.Core.HomeMedications.Repository
{
    public interface IHomeMedicationRepository
    {
        IEnumerable<PatientHomeMedication> GetMedications(Func<PatientHomeMedication, bool> wherePredicate);
        IEnumerable<PatientHomeMedication> GetPatientHomeMedications(Expression<Func<PatientHomeMedication, bool>> wherePredicate);
        FdbBrandName GetPatientHomeMedicationFdbBrandName(long medicationId);
    }
}