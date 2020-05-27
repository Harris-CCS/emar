using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PulseCheck.Domain;

namespace PulseCheck.IRepository
{
    public interface IPatientRepository
    {
        Task<List<Patient>> GetPatientsBySiteAndDeptAsync(byte siteId, string dept, User user, string expand = "");
        Task<List<Patient>> GetPatientsBySiteAndDeptForMTBAsync(byte siteId, string dept, User user, string filter = "");
        Task<Patient> GetPatientByIdAsync(byte siteId, string patientId, User user, string expand = "");
        int PostCommentByIdAsync(byte siteId, string patientId, int userId, Int32? commentId, string commentName, bool removeComment);
        Task<List<Allergy>> GetPatientAllergies(byte siteId, string patientId, User user);
        Task<List<CurrentMedication>> GetPatientCurrentMedications(byte siteId, string patientId, User user);
        Task<string> SignChart(byte siteId, string patientId, User user);
        Task<List<Order>> GetPatientLateResults(byte siteId, string patientId, User user);
        Task<List<Order>> GetPatientOrders(byte siteId, string patientId);
    }
}
