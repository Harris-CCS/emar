namespace DomainModel
{
    /// <summary>
    /// Object to represent a patient indicators in the system
    /// </summary>
    public class Indicator
    {
        /// <summary>
        /// Indicator name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Indicator text
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// Indicator style
        /// </summary>
        public Style Style { get; set; }
    }
}