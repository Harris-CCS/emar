using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PulseCheck.Domain;
using PulseCheck.IDomain;

namespace PulseCheck.ILogic
{
    public interface IPatientManager
    {
        Task<Patient> GetPatientByIdAsync(byte siteId, string patientId, User user, string expand = "");
        Task<List<Allergy>> GetPatientAllergies(byte siteId, string patientId, User user);
        Task<List<CurrentMedication>> GetPatientCurrentMedications(byte siteId, string patientId, User user);
        Task<ClinicalPathway> GetPatientPathway(byte siteId, string patientId, int pathwayNum, User user);
        Task<List<Order>> GetPatientLateResults(byte siteId, string patientId, User user);
        Task<List<OrderResult>> GetPatientResults(byte siteId, string patientId, User user, string mostCurrentOnly = "");
        Task<List<Order>> GetPatientOrders(byte siteId, string patientId, bool includeQueries);
        Task<string> SignChart(byte siteId, string patientId, User user);
        Task<string> AcknowledgeMedOrder(byte siteId, string patientId, User user, int orderId);
        Task<MedicationDTO> GetMedOrder(ISite site, string patientId, User user, int orderId);
        Task<List<MedicationDTO>> GetMedOrders(ISite site, string patientId, User user);
        Task<string> PostMedOrders(ISite site, string patientId, User user, string type, int orderingPhysicianId, string notes, List<string> serviceOptions, string authType, List<OrderMedication> orders);
        Task<bool> PostPatientResults(byte siteId, string patientId, User user, List<int> lineNums);
        Task<string> PlaceOrder(byte siteId, string patientId, User user, List<Order> orders);
        int PostComment(byte siteId, string patientId, int userId, Int32? commentId, string commentName = null, bool removeComment = false);
        Task<List<Group>> CreateOrderServices(User user, string patientId, List<Service> services);

        Task<List<Query>> GetServiceQueries(User user, string patientId, string serviceCode);
    }
}
