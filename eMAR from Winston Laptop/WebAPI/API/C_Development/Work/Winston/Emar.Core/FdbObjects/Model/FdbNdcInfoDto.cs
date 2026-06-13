namespace Emar.Core.FdbObjects.Model
{
    public class FdbNdcInfoDto
    {
        string _ndc;
        public string Ndc
        {
            get => _ndc?.Trim();
            set => _ndc = value?.Trim();
        }

        string _baseNdc;
        public string BaseNdc
        {
            get => _baseNdc?.Trim();
            set => _baseNdc = value?.Trim();
        }

        public int Repackaged { get; set; }

        public decimal Medid { get; set; }

        public string Packaging { get; set; }

        string _strength;
        public string Strength
        {
            get => _strength?.Trim();
            set => _strength = value?.Trim();
        }

        public int? DaysObsolete { get; set; }

        public decimal? GcnSeqno { get; set; }

        public decimal? HiclSeqno { get; set; }

        public decimal? RoutedGenId { get; set; }
    }
}
