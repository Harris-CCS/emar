namespace Emar.Core.FdbObjects.Model
{
    public class FdbBrandNameDto
    {
        public decimal Medid { get; set; }

        string _longBrandName;
        public string LongBrandName
        {
            get => _longBrandName?.Trim();
            set => _longBrandName = value?.Trim();
        }

        string _active;
        public string Active
        {
            get => _active?.Trim();
            set => _active = value?.Trim();
        }

        public decimal? MedNameId { get; set; }

        string _pcMedNameId;
        public string PcMedNameId
        {
            get => _pcMedNameId?.Trim();
            set => _pcMedNameId = value?.Trim();
        }

        public decimal? RoutedGenId { get; set; }

        string _pcRoutedGenId;
        public string PcRoutedGenId
        {
            get => _pcRoutedGenId?.Trim();
            set => _pcRoutedGenId = value?.Trim();
        }

        string _brandName;
        public string BrandName
        {
            get => _brandName?.Trim();
            set => _brandName = value?.Trim();
        }

        string _deaSchedule;
        public string DeaSchedule
        {
            get => _deaSchedule?.Trim();
            set => _deaSchedule = value?.Trim();
        }

        string _rxOtc;
        public string RxOtc
        {
            get => _rxOtc?.Trim();
            set => _rxOtc = value?.Trim();
        }

        public int ErxSearch { get; set; }
    }
}
