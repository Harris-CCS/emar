using System.Collections.ObjectModel;

namespace PulseCheck.API.Areas.HelpPage.ModelDescriptions
{
    /// <summary>
    /// Complex type model description
    /// </summary>
    public class ComplexTypeModelDescription : ModelDescription
    {
        /// <summary>
        /// Complex type model description
        /// </summary>
        public ComplexTypeModelDescription()
        {
            Properties = new Collection<ParameterDescription>();
        }

        /// <summary>
        /// Collection of ParameterDescriptions for this ComplexTypeModelDescription
        /// </summary>
        public Collection<ParameterDescription> Properties { get; private set; }
    }
}