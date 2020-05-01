using System.Collections.Generic;

namespace DomainModel
{
    /// <summary>
    /// "Drop down or other" query object
    /// </summary>
    public class DropdownOrOtherQuery : Query
    {
        /// <summary>
        /// Codes present in drop down
        /// </summary>
        public List<Code> Codes { get; set; } = new List<Code>();

        /// <summary>
        /// Text input max length
        /// </summary>
        public int MaxLength { get; set; } = 75;

        /// <summary>
        /// Default empty constructor
        /// </summary>
        public DropdownOrOtherQuery(Query copy) : base(copy)
        {
            Type = Constants.TYPE_DROPDOWNOO;
        }

        public override bool Validate()
        {
            var isGood = Value.Length <= MaxLength;
            if (!isGood)
            {
                Error = Constants.ERROR_MAX_LENGTH;
            }
            return isGood;
        }
    }
}