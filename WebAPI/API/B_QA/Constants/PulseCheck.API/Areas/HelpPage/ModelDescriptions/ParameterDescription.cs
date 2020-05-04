using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace PulseCheck.API.Areas.HelpPage.ModelDescriptions
{
    /// <summary>
    /// Describes parameters
    /// </summary>
    public class ParameterDescription
    {
        /// <summary>
        /// Empty ParameterDescription constructor
        /// </summary>
        public ParameterDescription()
        {
            Annotations = new Collection<ParameterAnnotation>();
        }

        /// <summary>
        /// Annotations for ParameterDescription
        /// </summary>
        public Collection<ParameterAnnotation> Annotations { get; private set; }

        /// <summary>
        /// Documentation for ParameterDescription
        /// </summary>
        public string Documentation { get; set; }

        /// <summary>
        /// Name of ParameterDescription
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Description of type of ParameterDescription
        /// </summary>
        public ModelDescription TypeDescription { get; set; }
    }
}