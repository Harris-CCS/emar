using System.Collections.Generic;

namespace Emar.Core.Templates.Model
{
    public enum ActionEnum
    {
        Acknowledge = 1,
        Cancel = 2,
        Complete = 3,
        CompleteDiscontinue = 4,
        CoSign = 5,
        Delete = 6,
        FollowUp = 7,
        Give = 8,
        Hold = 9,
        MissedDose = 10,
        OrderDiscontinue = 11,
        Repeat = 12,
        Reschedule = 13,
        UnHold = 14,
        Modify = 15,
        PharmVerification = 16
    }

    public enum PromptType
    {
        CheckBox,
        CheckBoxCheckChildren,
        CheckBoxShowChildren,
        Date,
        DateTime,
        DropDownListBox,
        FreeText,
        Information,
        MultiLineFreeText,
        Notify,
        threeStateButton
    }

    public static class OdsConstants
    {
        public const string True = @"true";
        public const string False = @"false";
        public const string Unknown = @"Unknown";
        // prompt text values for give, stop, iv site, and iv location
        public const string At = @"At";
        public const string GivenAt = @"Given At";
        public const string DocumentedAt = @"Documented At";
        public const string SiteNumber = @"Site";
        public const string IVNumber = @"IV Number";
        public const string OtherIVSite = @"Other IV Site";
        public const string IVLocation = @"Location";
        public const string IVDiscontinued = @"Date/Time ~~(infusionDiscontinued)";
        public const string IVContinuedUponTransfer = @"Date/Time ~~(infusionContinuedUponTransfer)";
        public const string IVStopTimeUnknown = @"Stop time unknown";
        // Injection & Infusion medication route types
        public const string Injection = @"Injection";
        public const string Infusion = @"Infusion";
        public const string Hydration = @"Hydration";
        // template names
        public const string IntravenousInI = @"IntravenousInI";
        public const string Intravenous = @"Intravenous";
        public const string Intramuscular = @"Intramuscular";
        public const string Subcutaneous = @"Subcutaneous";
        public const string Intraosseous = @"Intraosseous";
        public const string FollowUp = @"FollowUp";
        // Vitals
        public const string BPSystolic = @"BP (Systolic)";
        public const string BPDiastolic = @"BP (Diastolic)";
        public const string Pulse = @"PULSE";
        public const string Temperature= @"TEMPERATURE";
        public const string O2Sat = @"O2 SAT";
        public const string On = @"on";
        public const string Map = @"MAP";
        public const string Respitory = @"RESPIRATORY";
        public const string Pain = @"PAIN";
        public const string EndTidal = @"END-TIDAL CO2";
        // Vital Attributes
        public const string BPConditionAttr = @" ~~(bpCondition)";
        public const string BPSiteAttr = @" ~~(bpSite)";
        public const string PulseAttr1 = @" ~~(pulseSelect1)";
        public const string PulseAttr2 = @" ~~(pulseSelect2)";
        public const string TemperatureAttr1 = @" ~~(temperatureSelect1)";
        public const string TemperatureAttr2 = @" ~~(temperatureSelect2)";
        public const string MapAttr1 = @" ~~(mapSelect1)";
        public const string MapAttr2 = @" ~~(mapSelect2)";
        public const string RespitoryAttr1 = @" ~~(respiratorySelect1)";
        public const string RespitoryAttr2 = @" ~~(respiratorySelect2)";
        public const string PainAttr1 = @" ~~(painSelect1)";
        public const string PainAttr2 = @" ~~(painSelect2)";
        public const string EndTidalAttr1 = @" ~~(end-tidalCo2Select1)";
        public const string EndTidalAttr2 = @" ~~(end-tidalCo2Select2)";

        public const string BPSys = @"BP Systolic";
        public const string BPDia = @"BP Diastolic";
        public static readonly Dictionary<string, string> VITALS_TO_OCS_MAP = new Dictionary<string, string>
        {
                { BPSys, "BPSystolic" },
                { BPDia, "BPDiastolic" },
                { "MAP", "MAP" },
                { "Pulse", "PULSE" },
                { "Respiration", "RESPIRATORY" },
                { "Temperature", "TEMPERATURE" },
                { "Pain", "PAIN" },
                { "O2 Saturation", "O2SAT" },
                { "End-Tidal CO2", "ENDTIDALCO2" }
        };

        public const string BPPatIndicator = @"Ord11";
        public static readonly Dictionary<string, string> VITALS_TO_PAT_MAP = new Dictionary<string, string>
        {
                { BPSys, BPPatIndicator },
                { BPDia, BPPatIndicator },
                { "MAP", "VSMapLevel" },
                { "Pulse", "Ord12" },
                { "Respiration", "Ord13" },
                { "Temperature", "Ord14" },
                { "Pain", "Ord15" },
                { "O2 Saturation", "Ord23" },
                { "End-Tidal CO2", "VSEndTidalLevel" }
        };

        #region range type identifiers
        /// <summary>
        /// Panic low range identifier
        /// </summary>
        public const string RANGE_PANIC_LOW = "Panic low";

        /// <summary>
        /// Normal low range identifier
        /// </summary>
        public const string RANGE_NORMAL_LOW = "Normal low";

        /// <summary>
        /// Normal high range identifier
        /// </summary>
        public const string RANGE_NORMAL_HIGH = "Normal high";

        /// <summary>
        /// Panic high range identifier
        /// </summary>
        public const string RANGE_PANIC_HIGH = "Panic high";
        #endregion
    }
}