namespace PulseCheck.API.Areas.HelpPage.ModelDescriptions
{
    /// <summary>
    /// Key-Value pair model description
    /// </summary>
    public class KeyValuePairModelDescription : ModelDescription
    {
        /// <summary>
        /// Key model description
        /// </summary>
        public ModelDescription KeyModelDescription { get; set; }

        /// <summary>
        /// Value model description
        /// </summary>
        public ModelDescription ValueModelDescription { get; set; }
    }
}