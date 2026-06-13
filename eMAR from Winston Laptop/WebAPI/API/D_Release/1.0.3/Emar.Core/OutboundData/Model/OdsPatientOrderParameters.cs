using System;
using System.Collections.Generic;
using System.Text;

namespace Emar.Core.OutboundData.Model
{
    public class OdsPatientOrderParameters
    {
        public long PatientId { get; set; }
        public string Ibex { get; set; }
        public DateTimeOffset Losecs { get; set; }
        public int AddUserId { get; set; }
        public int OrderingPhysicianId { get; set; }
        public int SiteId { get; set; }
        public string BrandName { get; set; }
        public string Dose { get; set; }
        public string MedNotes { get; set; }
        public string AmIndication { get; set; }
        public DateTimeOffset OrderDate { get; set; }
        public string Route { get; set; }
        public string Unit { get; set; }
        public int MedicationId { get; set; }
        public long PatientOrderId { get; set; }
        public string DisplayName { get; set; }
        public string Type { get; set; }
        public bool PharmVerificationReq { get; set; }
    }
}
