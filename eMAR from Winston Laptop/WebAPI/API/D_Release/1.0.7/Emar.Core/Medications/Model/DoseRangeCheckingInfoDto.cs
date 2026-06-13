namespace Emar.Core.Medications.Model
{
    public class DoseRangeCheckingInfoDto
    {
        public int GcnSeqno { get; internal set; }
        public string TypeDescription { get; internal set; }
        public string AgeDdescription { get; internal set; }
        public string WeightDescription { get; internal set; }
        public string AmountLow { get; internal set; }
        public string AmountHigh { get; internal set; }
        public string UnitDoseAbbreviation { get; internal set; }
        public string MaxFrequency { get; internal set; }
        public string Condition1Description { get; internal set; }
        public string RenalDescription { get; internal set; }
        public string RouteDescription { get; internal set; }
    }
}
