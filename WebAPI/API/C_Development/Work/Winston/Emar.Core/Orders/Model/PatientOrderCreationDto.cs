using System;
using System.Collections.Generic;
using System.Text;
using Emar.Data.Entities;

namespace Emar.Core.Orders.Model
{
    public class PatientOrderCreationDto
    {
        public long PatientId { get; set; }

        public int AddUserId { get; set; }

        public DateTimeOffset AddDatetime { get; set; }

        public string Ndc { get; set; }

        public string DrugId { get; set; }

        public string BrandName { get; set; }

        public decimal? Dose { get; set; }

        public string DoseUnit { get; set; }

        public int? MedicationRouteId { get; set; }

        public short Priority { get; set; }

        public int? FrequencyId { get; set; }

        public bool Prn { get; set; }

        public bool PointInTime { get; set; }

        public string OrderStatus { get; set; }

        public DateTimeOffset BeginDatetime { get; set; }

        public DateTimeOffset? EndDatetime { get; set; }

        public string OrderNotes { get; set; }

        public IEnumerable<OrderAdministration>? OrderAdministrations { get; set; }

        public IEnumerable<OrderEvent>? OrderEvents { get; set; }
    }
}
