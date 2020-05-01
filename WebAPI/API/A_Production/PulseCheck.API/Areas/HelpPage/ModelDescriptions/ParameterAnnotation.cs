using System;

namespace PulseCheck.API.Areas.HelpPage.ModelDescriptions
{
    /// <summary>
    /// Parameter annotation
    /// </summary>
    public class ParameterAnnotation
    {
        /// <summary>
        /// Attirbute of annotation
        /// </summary>
        public Attribute AnnotationAttribute { get; set; }

        /// <summary>
        /// Documentation for annotation
        /// </summary>
        public string Documentation { get; set; }
    }
}