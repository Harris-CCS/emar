using Emar.Core.HomeMedications.Repository;

namespace Emar.Core.HomeMedications.Service
{
    public class HomeMedicationService: IHomeMedicationService
    {
        private readonly IHomeMedicationRepository _homeMedicationRepository;

        public HomeMedicationService(IHomeMedicationRepository homeMedicationRepository)
        {
            _homeMedicationRepository = homeMedicationRepository;
        }

    }
}
