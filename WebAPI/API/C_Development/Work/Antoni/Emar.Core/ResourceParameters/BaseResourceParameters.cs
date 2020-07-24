namespace Emar.Core
{
    public class BaseResourceParameters
    {

        /// <summary>
        /// eMAR unique user identifier.
        /// </summary>
        public int? UserId { get; set; }

        #region Paging
        const int maxPageSize = 20;
        private int _pageSize = 10;

        public int PageSize
        {
            get => _pageSize;
            set => _pageSize = (value > maxPageSize) ? maxPageSize : value;
        }

        public int PageNumber { get; set; } = 1;
        #endregion

        #region Sorting
        public string OrderBy { get; set; } = "Id";
        #endregion

        #region ExpandO (data shaping)
        public string Fields { get; set; }
        #endregion
    }
}
