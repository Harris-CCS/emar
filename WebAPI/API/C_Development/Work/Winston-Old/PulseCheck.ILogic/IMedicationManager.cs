using System.Collections.Generic;
using System.Threading.Tasks;
using PulseCheck.Domain;
using PulseCheck.IDomain;
using PulseCheck.Utilities;

namespace PulseCheck.ILogic
{
    public interface IMedicationManager
    {
        Task<List<Medication>> GetMedicationsByPatientIdAsync(byte siteId, string patientId);
        Task<Medication> GetMedicationByIdAsync(byte siteId, string patientId, int orderId);
        Task<Medication> GetMedicationByLosecsAsync(byte siteId, string patientId, int losecs);
        Task<Group> GetMedicationGroup(User user, Site site, Group group, string patientId = null);
        Task<List<QLItem>> GetMedMostUsedList(User user, Site site, string patientId = null);
        Task<List<QLItem>> GetMedQuickList(User user, Site site, string patientId = null);
        Task<List<BasicMedication>> GetBrandMeds(User user, Site site, string patientId, string brand);
        Task<EMR.Line> ChartEntry(IPatient patient, Medication med, string sysDate, Dictionary<string, string> InterOverList, List<OverrideRationale> overrides);
    }
}
