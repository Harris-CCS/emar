using Emar.Data.Entities;

namespace Emar.Core.Medications.Model.Mappings
{
    public static class DoseRangeCheckingInfoMapper
    {
        public static DoseRangeCheckingInfoDto MapDoseRangeCheckingInfo(DoseRangeCheckingInfo dbObj)
        {
            if (dbObj == null)
            {
                return null;
            }

            var ret = new DoseRangeCheckingInfoDto
            {
                GcnSeqno = dbObj.GcnSeqno,
                TypeDescription = dbObj.TypeDescription,
                AgeDdescription = dbObj.AgeDdescription,
                WeightDescription = dbObj.WeightDescription,
                AmountLow = dbObj.AmountLow,
                AmountHigh = dbObj.AmountHigh,
                UnitDoseAbbreviation = dbObj.UnitDoseAbbreviation,
                MaxFrequency = dbObj.MaxFrequency,
                Condition1Description = dbObj.Condition1Description,
                RenalDescription = dbObj.RenalDescription,
                RouteDescription = dbObj.RouteDescription
            };

            return ret;
        }
    }
}
