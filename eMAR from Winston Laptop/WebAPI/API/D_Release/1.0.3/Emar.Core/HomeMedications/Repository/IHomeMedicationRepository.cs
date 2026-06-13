using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Emar.Data.Entities;

namespace Emar.Core.HomeMedications.Repository
{
    public interface IHomeMedicationRepository
    {
        IEnumerable<PatientHomeMedication> GetHomeMedications(Expression<Func<PatientHomeMedication, bool>> wherePredicate);
        PatientHomeMedication GetHomeMedication(long medicationId);
        IEnumerable<PatientHomeMedication> GetPatientHomeMedications(Expression<Func<PatientHomeMedication, bool>> wherePredicate);
        FdbBrandName GetPatientHomeMedicationFdbBrandName(long medicationId);
        FdbBrandName GetPatientHomeMedicationFdbBrandNameByPcRoutedGenId(string internalDrugId);
    }
}