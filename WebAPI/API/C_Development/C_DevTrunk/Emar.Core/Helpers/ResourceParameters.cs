namespace Emar.Core
{
    public class ResourceParameters
    {
        public short? Site { get; set; }        // PulseCheck exclusive
        public string Ibex { get; set; }        // PulseCheck exclusive
        public string DepartmentCode { get; set; }
        public string WardCodes { get; set; }
        public string RoomBedCode { get; set; }
        public long? PatientId { get; set; }
        public bool IncludeInactive { get; set; } = false;
        public bool IncludeOrders { get; set; } = true;
        public bool IncludePatient { get; set; } = true;
        public bool IncludeAdministrations { get; set; } = true;
        public bool IncludeAdministrationsEvents { get; set; } = true;

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

        public bool AskingForLegacyPulseCheckPatient()
        {
            return Site != null && !string.IsNullOrWhiteSpace(Ibex);
        }
    }
}
