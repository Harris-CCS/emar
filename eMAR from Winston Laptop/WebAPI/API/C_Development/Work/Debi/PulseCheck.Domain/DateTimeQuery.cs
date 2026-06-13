using PulseCheck.Utilities;

namespace PulseCheck.Domain
{
    /// <summary>
    /// Date and time query object
    /// </summary>
    public class DateTimeQuery : Query
    {
        /// <summary>
        /// Default empty constructor
        /// </summary>
        public DateTimeQuery(Query copy) : base(copy)
        {
            Type = Constants.TYPE_DATETIME;
        }

        public override string DisplayValue { get { return (new Time()).ShortDateTime(Value); } }
    }
}