using System;
using System.Collections.Generic;
using System.Text;

namespace Emar.Core.OutboundData.Model
{
    public class OdsAdministrationParameters
    {
        public long OrderId { get; set; }
        public long AdministrationId { get; set; }
        public string Action { get; set; }
        public int AddUserId { get; set; }
        public int SiteId { get; set; }
        public DateTimeOffset AddDatetime { get; set; }
        public DateTimeOffset EventDateTime { get; set; }
        public string Ibex { get; set; }
        public int Losecs { get; set; }
        public string IVType { get; set; }
        public int? IVSite { get; set; }
        public string IVLocation { get; set; }
        public string IVEdit { get; set; }
        public string StopDate { get; set; }
        public bool NewOrderAdmin { get; set; }
    }
}
