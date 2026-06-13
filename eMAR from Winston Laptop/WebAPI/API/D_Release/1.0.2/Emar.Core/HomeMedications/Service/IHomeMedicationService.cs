using Emar.Core.HomeMedications.Model;

namespace Emar.Core.HomeMedications.Service
{
    public interface IHomeMedicationService
    {
        HomeMedicationDto GetHomeMedication(long homeMedicationId);
    }
}