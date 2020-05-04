using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace DomainModel
{
    /// <summary>
    /// Information about a patient's chart/EMR. Note this contains high-level information retrieved using the EMR utility.
    /// </summary>
    [NotMapped]
    public class Chart
    {
        public List<DigitalSignature> DigitalSignatures { get; set; }
        public bool SignableEvents { get; set; }

        /// <summary>
        /// Default empty constructor
        /// </summary>
        public Chart()
        {
            DigitalSignatures = new List<DigitalSignature>();
            SignableEvents = false;
        }
    }
}