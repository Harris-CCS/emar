using PulseCheck.Utilities;

namespace PulseCheck.Domain
{
    /// <summary>
    /// Time query object
    /// </summary>
    public class TimeQuery : Query
    {
        /// <summary>
        /// Default empty constructor
        /// </summary>
        public TimeQuery(Query copy) : base(copy)
        {
            Type = Constants.TYPE_TIME;
        }

        public override string DisplayValue { get { return (new Time()).ShortTime(Value); } }
    }
}