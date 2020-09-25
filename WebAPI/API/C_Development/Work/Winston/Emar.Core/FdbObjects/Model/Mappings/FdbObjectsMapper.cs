using Emar.Data.Entities;

namespace Emar.Core.FdbObjects.Model.Mappings
{
    public static class FdbObjectsMapper
    {
        public static FdbAllergyNameDto MapFdbAllergyName(FdbAllergyName fdbAllergyName)
        {
            if (fdbAllergyName == null)
            {
                return null;
            }

            FdbAllergyNameDto fdbAllergyNameDto = new FdbAllergyNameDto
            {
                Medid = fdbAllergyName.Medid,
                MedName = fdbAllergyName.MedName,
                MedNameId = fdbAllergyName.MedNameId,
                PcMedNameId = fdbAllergyName.PcMedNameId,
                HiclSeqno = fdbAllergyName.HiclSeqno,
                PcHiclSeqno = fdbAllergyName.PcHiclSeqno,
                AllergyName = fdbAllergyName.AllergyName
            };

            return fdbAllergyNameDto;
        }

        public static FdbBrandNameDto MapFdbBrandName(FdbBrandName fdbBrandName)
        {
            if (fdbBrandName == null)
            {
                return null;
            }

            FdbBrandNameDto fdbBrandNameDto = new FdbBrandNameDto
            {
                Medid = fdbBrandName.Medid,
                LongBrandName = fdbBrandName.LongBrandName,
                Active = fdbBrandName.Active,
                MedNameId = fdbBrandName.MedNameId,
                PcMedNameId = fdbBrandName.PcMedNameId,
                RoutedGenId = fdbBrandName.RoutedGenId,
                PcRoutedGenId = fdbBrandName.PcRoutedGenId,
                BrandName = fdbBrandName.BrandName,
                DeaSchedule = fdbBrandName.DeaSchedule,
                RxOtc = fdbBrandName.RxOtc,
                ErxSearch = fdbBrandName.ErxSearch
            };

            return fdbBrandNameDto;
        }

        public static FdbNdcInfoDto MapFdbNdcInfo(FdbNdcInfo fdbNdcInfo)
        {
            if (fdbNdcInfo == null)
            {
                return null;
            }

            FdbNdcInfoDto fdbNdcInfoDto = new FdbNdcInfoDto
            {
                Ndc = fdbNdcInfo.Ndc,
                BaseNdc = fdbNdcInfo.BaseNdc,
                Repackaged = fdbNdcInfo.Repackaged,
                Medid = fdbNdcInfo.Medid,
                Packaging = fdbNdcInfo.Packaging,
                Strength = fdbNdcInfo.Strength,
                DaysObsolete = fdbNdcInfo.DaysObsolete,
                GcnSeqno = fdbNdcInfo.GcnSeqno,
                HiclSeqno = fdbNdcInfo.HiclSeqno,
                RoutedGenId = fdbNdcInfo.RoutedGenId
            };

            return fdbNdcInfoDto;
        }
    }
}
