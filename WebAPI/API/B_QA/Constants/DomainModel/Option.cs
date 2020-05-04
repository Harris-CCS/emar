namespace DomainModel
{
    /// <summary>
    /// Object to represent a simple option
    /// </summary>
    public class Option
    {
        /// <summary>
        /// Option Text
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// Option Value
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// Default Object constructor
        /// </summary>
        public Option()
        {

        }

        /// <summary>
        /// Create a new Option with the provided text and value
        /// </summary>
        /// <param name="text">Option text</param>
        /// <param name="value">Option value</param>
        public Option(string text, string value = null)
        {
            Text = text;
            Value = value;
        }
    }
}