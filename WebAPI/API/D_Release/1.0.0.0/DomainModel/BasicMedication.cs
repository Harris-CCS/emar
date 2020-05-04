using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DomainModel
{
    public class BasicMedication
    {
        /// <summary>
        /// Medication identifier
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Medication name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Boolean indicating this medication needs Antimicrobial Indication
        /// </summary>
        public bool HasIndication { get; set; }

        /// <summary>
        /// Interactions associated with this Medication
        /// </summary>
        public List<Dictionary<string, string>> Interactions { get; set; }

        /// <summary>
        /// Reactions associated with this Medication
        /// </summary>
        public List<Dictionary<string, string>> Reactions { get; set; }

        /// <summary>
        /// Default empty object
        /// </summary>
        public BasicMedication()
        {
            Interactions = new List<Dictionary<string, string>>();
            Reactions = new List<Dictionary<string, string>>();
        }
    }
}