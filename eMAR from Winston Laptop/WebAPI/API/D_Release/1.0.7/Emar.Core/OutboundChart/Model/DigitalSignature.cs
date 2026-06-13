using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Emar.Core.OutboundChart.Model
{
    /// <summary>
    /// Represent a digital signature made on the chart
    /// </summary>
    [NotMapped]
    public class DigitalSignature
    {
        /// <summary>
        /// User who signed
        /// </summary>
        public MinimalUser User { get; set; }

        /// <summary>
        /// Signature status (pending or active)
        /// </summary>
        public Status Status { get; set; }

        /// <summary>
        /// Datetime when signature was made
        /// </summary>
        public DateTime? Date { get; set; }
    }
}
