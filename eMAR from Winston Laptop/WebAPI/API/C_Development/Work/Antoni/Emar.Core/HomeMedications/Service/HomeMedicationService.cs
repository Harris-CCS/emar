using Emar.Core.HomeMedications.Model;
using Emar.Core.HomeMedications.Model.Mappings;
using Emar.Core.HomeMedications.Repository;

namespace Emar.Core.HomeMedications.Service
{
    public class HomeMedicationService : IHomeMedicationService
    {
        private readonly IHomeMedicationRepository _homeMedicationRepository;

        public HomeMedicationService(IHomeMedicationRepository homeMedicationRepository)
        {
            _homeMedicationRepository = homeMedicationRepository;
        }

        public HomeMedicationDto GetHomeMedication(long homeMedicationId)
        {
            var homeMedication = _homeMedicationRepository.GetHomeMedication(homeMedicationId);

            if (homeMedication == null)
            {
                return null;
            }

            return HomeMedicationMapper.MapHomeMedication(homeMedication);
        }
    }
}