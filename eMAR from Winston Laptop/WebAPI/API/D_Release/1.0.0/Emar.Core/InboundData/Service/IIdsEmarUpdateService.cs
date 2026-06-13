using Emar.Data.Entities;

namespace Emar.Core.InboundData.Service
{
    public interface IIdsEmarUpdateService
    {
        void FileUser(User user);
        void FilePatient(Patient dto);
        void FilePatientIndicators(Patient dto);
        void DeactivateUser(string externalId);
        void DeactivatePatient(string externalId);
    }
}