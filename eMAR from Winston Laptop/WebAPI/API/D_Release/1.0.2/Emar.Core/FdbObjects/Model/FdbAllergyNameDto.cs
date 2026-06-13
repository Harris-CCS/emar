namespace Emar.Core.FdbObjects.Model
{
    public class FdbAllergyNameDto
    {
        public decimal Medid { get; set; }

        string _medName;
        public string MedName
        {
            get => _medName?.Trim();
            set => _medName = value?.Trim();
        }

        public decimal? MedNameId { get; set; }

        string _pcMedNameId;
        public string PcMedNameId
        {
            get => _pcMedNameId?.Trim();
            set => _pcMedNameId = value?.Trim();
        }

        public decimal? HiclSeqno { get; set; }

        string _pcHiclSeqno;
        public string PcHiclSeqno
        {
            get => _pcHiclSeqno?.Trim();
            set => _pcHiclSeqno = value?.Trim();
        }

        string _allergyName;
        public string AllergyName
        {
            get => _allergyName?.Trim();
            set => _allergyName = value?.Trim();
        }
    }
}
