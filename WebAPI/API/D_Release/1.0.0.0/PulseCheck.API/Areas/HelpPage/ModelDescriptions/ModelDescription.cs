using System;

namespace PulseCheck.API.Areas.HelpPage.ModelDescriptions
{
    /// <summary>
    /// Describes a type model.
    /// </summary>
    public abstract class ModelDescription
    {
        /// <summary>
        /// Model documentation
        /// </summary>
        public string Documentation { get; set; }

        /// <summary>
        /// Model type
        /// </summary>
        public Type ModelType { get; set; }

        /// <summary>
        /// Model name
        /// </summary>
        public string Name { get; set; }
    }
}