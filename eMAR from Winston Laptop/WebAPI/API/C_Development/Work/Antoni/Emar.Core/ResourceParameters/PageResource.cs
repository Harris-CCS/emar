namespace Emar.Core.ResourceParameters
{
    public class PageResource
    {
        #region Paging
        const int MaxPageSize = 20;
        private int _pageSize = 10;

        public int PageSize
        {
            get => _pageSize;
            set => _pageSize = (value > MaxPageSize) ? MaxPageSize : value;
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
