using System;

namespace PulseCheck.API.Areas.HelpPage.ModelDescriptions
{
    /// <summary>
    /// Use this attribute to change the name of the <see cref="ModelDescription"/> generated for a type.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum, AllowMultiple = false, Inherited = false)]
    public sealed class ModelNameAttribute : Attribute
    {
        /// <summary>
        /// Set the name of the Model
        /// </summary>
        /// <param name="name"></param>
        public ModelNameAttribute(string name)
        {
            Name = name;
        }

        /// <summary>
        /// Model name
        /// </summary>
        public string Name { get; private set; }
    }
}