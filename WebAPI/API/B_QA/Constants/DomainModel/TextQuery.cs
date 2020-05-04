namespace DomainModel
{
    /// <summary>
    /// Text query object
    /// </summary>
    public class TextQuery : Query
    {
        /// <summary>
        /// Text input max length
        /// </summary>
        public int MaxLength { get; set; } = 75;

        /// <summary>
        /// Default empty constructor
        /// </summary>
        public TextQuery(Query copy) : base(copy)
        {
            Type = Constants.TYPE_TEXT;
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