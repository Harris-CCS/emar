namespace DomainModel
{
    /// <summary>
    /// "Hidden" query object
    /// </summary>
    public class HiddenQuery : Query
    {
        /// <summary>
        /// Default empty constructor
        /// </summary>
        public HiddenQuery(Query copy) : base(copy)
        {
            Type = Constants.TYPE_HIDDEN;
        }
    }
}