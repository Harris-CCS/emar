using System.Collections.Generic;
using System.Linq;

namespace DomainModel
{
    /// <summary>
    /// Drop down query object
    /// </summary>
    public class DropdownQuery : Query
    {
        /// <summary>
        /// Codes present in drop down
        /// </summary>
        public List<Code> Codes { get; set; } = new List<Code>();

        /// <summary>
        /// Default empty constructor
        /// </summary>
        public DropdownQuery(Query copy) : base(copy)
        {
            Type = Constants.TYPE_DROPDOWN;
        }

        public override bool Validate()
        {
            // TODO: Validate that we're given a code that exists
            //var isGood = Codes.Any(x => x.Value == Value);
            //if (!isGood)
            //{
            //    Error = Constants.ERROR_BAD_CODE;
            //}
            //return isGood;
            return true;
        }
    }
}