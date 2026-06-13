namespace Emar.Core.ResourceParameters
{
    public class PageResource
    {
        #region Paging
        //EMAR-616.  Change the page size to allow enough patients
        //so that we don't show multiple pages.
        //Since this affects anything that uses pagination, I'm leaving
        //the page size at 10. and only changing the max page size.
        //Winston Murdock, 01/19/2021.
        //const int MaxPageSize = 20;
        const int MaxPageSize = 200;
        //Changing this from 10 to 100 in hopes of showing all routes, units, etc...
        //Per Brad, Antoni added this on his own without it being part of a requirment.
        //Rather than trying to extricate it from wherever it's being called, just
        //bump the page size up to a higher number.
        //Winston Murdock, 03/22/2021.
        //private int _pageSize = 10;
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
