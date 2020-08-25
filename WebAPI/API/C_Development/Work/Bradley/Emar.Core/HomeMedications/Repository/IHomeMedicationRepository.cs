using System;
using System.Collections.Generic;
using Emar.Data.Entities;

namespace Emar.Core.HomeMedications.Repository
{
    public interface IHomeMedicationRepository
    {
        IEnumerable<PatientHomeMedication> GetMedications(Func<PatientHomeMedication, bool> wherePredicate);
    }
}
