namespace DomainModel
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

        public override string DisplayValue { get { return (new PulseCheck.Utilities.Time()).ShortTime(Value); } }
    }
}