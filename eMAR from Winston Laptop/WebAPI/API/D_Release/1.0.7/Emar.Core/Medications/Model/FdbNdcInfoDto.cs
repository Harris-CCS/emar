using System;
using System.Collections.Generic;
using System.Text;

namespace Emar.Core.Medications.Model
{
    public class FdbNdcInfoDto
    {
        public string Ndc { get; set; }

        public string BaseNdc { get; set; }

        public int Repackaged { get; set; }

        public decimal Medid { get; set; }

        public string MedidString { get; set; }

        public string Packaging { get; set; }

        public string Strength { get; set; }

        public int? DaysObsolete { get; set; }

        public decimal? GcnSeqno { get; set; }

        public decimal? HiclSeqno { get; set; }

        public decimal? RoutedGenId { get; set; }

        public string DoseForm { get; set; }

        public string Route { get; set; }

        public decimal? DrugCat { get; set; }

    }
}
