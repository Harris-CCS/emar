using System.Collections.Generic;
using System.Threading.Tasks;
using PulseCheck.Domain;

namespace PulseCheck.IRepository
{
    public interface IMedicationRepository
    {
        Task<List<Medication>> GetMedicationsByPatientIdAsync(byte id, string patientId);
        Task<Medication> GetMedicationByIdAsync(byte id, string patientId, int orderId);
        Task<Medication> GetMedicationByLosecsAsync(byte id, string patientId, int losecs);
        Task<int> Save(Medication med);
    }
}
