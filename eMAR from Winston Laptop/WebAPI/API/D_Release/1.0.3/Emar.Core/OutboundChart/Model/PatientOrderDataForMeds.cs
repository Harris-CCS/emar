using System;
using System.Collections.Generic;
using System.Text;

namespace Emar.Core.OutboundChart.Model
{
    public class PatientOrderDataForMeds
    {
        public long PatentOrderId;
        public long PatientOrderAdminId;
        public int orderingPhysicianId;
        public int medicationId;
        public string medNotes;
        public string Dose;
        public string Route;
        public string Unit;
        public int FrequencyId;
        public int? Duration;
        public int? DurationId;
        public string OrderDate;
        public string AntiMicrobialIndication;
        public string AntiMicrobialIndicationText;
    }
}
