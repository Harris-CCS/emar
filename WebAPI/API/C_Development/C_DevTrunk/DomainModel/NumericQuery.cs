using System;

namespace DomainModel
{
    /// <summary>
    /// Numeric query
    /// </summary>
    public class NumericQuery : Query
    {
        /// <summary>
        /// Query maximum value
        /// </summary>
        public int? MaxValue { get; set; }

        /// <summary>
        /// Default empty constructor
        /// </summary>
        public NumericQuery(Query copy) : base(copy)
        {
            Type = Constants.TYPE_NUMERIC;
        }

        public override bool Validate()
        {
            var isGood = MaxValue == null || Convert.ToInt32(Value) <= MaxValue;
            if (!isGood)
            {
                Error = Constants.ERROR_MAX_VALUE;
            }

            return isGood;
        }
    }
}