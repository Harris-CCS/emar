using System.Collections.Generic;

namespace DomainModel
{
    /// <summary>
    /// Object to represent MetaData for an element
    /// </summary>
    public class MetaData
    {
        /// <summary>
        /// MetaData name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// MetaData description
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// List of MetaData Options
        /// </summary>
        public List<Option> Options { get; set; }

        /// <summary>
        /// Default MetaData constructor
        /// </summary>
        public MetaData()
        {
            Options = new List<Option>();
        }

        /// <summary>
        /// MetaData constructor with name
        /// </summary>
        /// <param name="name">Name for MetaData object</param>
        public MetaData(string name)
        {
            Name = name;
            Options = new List<Option>();
        }

        /// <summary>
        /// Add an Option to the MetaData Options list
        /// </summary>
        /// <param name="text"></param>
        /// <param name="value"></param>
        public void AddOption(string text, string value)
        {
            Options.Add(new Option(text, value));
        }
    }
}