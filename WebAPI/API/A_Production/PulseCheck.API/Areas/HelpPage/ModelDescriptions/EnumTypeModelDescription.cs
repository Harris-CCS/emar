using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace PulseCheck.API.Areas.HelpPage.ModelDescriptions
{
    /// <summary>
    /// Enum model description
    /// </summary>
    public class EnumTypeModelDescription : ModelDescription
    {
        /// <summary>
        /// Enum type model description
        /// </summary>
        public EnumTypeModelDescription()
        {
            Values = new Collection<EnumValueDescription>();
        }

        /// <summary>
        /// Collection of descriptions of Enum type values
        /// </summary>
        public Collection<EnumValueDescription> Values { get; private set; }
    }
}