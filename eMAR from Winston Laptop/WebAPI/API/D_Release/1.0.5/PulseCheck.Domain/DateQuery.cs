using PulseCheck.Utilities;

namespace PulseCheck.Domain
{
    /// <summary>
    /// Date query object
    /// </summary>
    public class DateQuery : Query
    {
        /// <summary>
        /// Default empty constructor
        /// </summary>
        public DateQuery(Query copy) : base(copy)
        {
            Type = Constants.TYPE_DATE;
        }

        public override string DisplayValue { get { return (new Time()).ShortDate(Value); } }
    }
}