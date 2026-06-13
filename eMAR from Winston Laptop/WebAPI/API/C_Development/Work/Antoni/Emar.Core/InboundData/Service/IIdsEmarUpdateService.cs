using Emar.Core.InboundData.Model;

namespace Emar.Core.InboundData.Service
{
    public interface IIdsEmarUpdateService
    {
        void FileUser(InboundUserDataDto user);
        void FilePatient(InboundPatientDataDto dto);
        void DeactivateUser(string externalId);
    }
}