using System;
using System.Collections.Generic;
using System.Text;

namespace Emar.Core.OutboundChart.Model
{
    public class OcsVitals
    {
        public string enteredDatetime { get; set; }
        public string BPSystolic { get; set; }
        public string BPDiastolic { get; set; }
        public string PULSE { get; set; }
        public string TEMPERATURE { get; set; }
        public string O2SAT { get; set; }
        public string MAP { get; set; }
        public string RESPIRATORY { get; set; }
        public string PAIN { get; set; }
        public string ENDTIDALCO2 { get; set; }
        public string bpCondition { get; set; }
        public string bpSite { get; set; }
        public string pulseSelect1 { get; set; }
        public string pulseSelect2 { get; set; }
        public string temperatureSelect1 { get; set; }
        public string temperatureSelect2 { get; set; }
        public string on { get; set; }
        public string mapSelect1 { get; set; }
        public string mapSelect2 { get; set; }
        public string respiratorySelect1 { get; set; }
        public string respiratorySelect2 { get; set; }
        public string painSelect1 { get; set; }
        public string painSelect2 { get; set; }
        public string endtidalCo2Select1 { get; set; }
        public string endtidalCo2Select2 { get; set; }

        /// <summary>
        /// Defines constants used to reference various pieces of information in the Vitals.
        /// </summary>
        public class Constants
        {
            public const string BPS = "BPS";
            public const string BPD = "BPD";
            public const string ABP = "ABP";
            public const string ABP_1 = "ABP_1";
            public const string MAP = "MAP";
            public const string AMAP = "AMAP";
            public const string AMAP_1 = "AMAP_1";
            public const string PULSE = "Pulse";
            public const string APULSE = "APulse";
            public const string APULSE_1 = "APulse_1";
            public const string RESP = "Resp";
            public const string ARESP = "AResp";
            public const string ARESP_1 = "AResp_1";
            public const string TEMP = "Temp";
            public const string ATEMP = "ATemp";
            public const string ATEMP_1 = "ATemp_1";
            public const string PAIN = "Pain";
            public const string APAIN = "APain";
            public const string APAIN_1 = "APain_1";
            public const string O21 = "O21";
            public const string AO21 = "AO21";
            public const string AO21_1 = "AO21_1"; // currently not used
            public const string ENDTIDAL = "EndTidal";
            public const string AENDTIDAL = "AEndTidal";
            public const string AENDTIDAL_1 = "AEndTidal_1";

            /// <summary>
            /// Chart Object Map
            /// </summary>
            public static readonly Dictionary<string, string> OBJECT_MAP = new Dictionary<string, string>
            {
            { BPS,         "BPSystolic" },
            { BPD,         "BPDiastolic" },
            { ABP,         "bpCondition" },
            { ABP_1,       "bpSite" },
            { MAP,         "MAP" },
            { AMAP,        "mapSelect1" },
            { AMAP_1,      "mapSelect2" },
            { PULSE,       "PULSE" },
            { APULSE,      "pulseSelect1" },
            { APULSE_1,    "pulseSelect2" },
            { RESP,        "RESPIRATORY" },
            { ARESP,       "respiratorySelect1" },
            { ARESP_1,     "respiratorySelect2" },
            { TEMP,        "TEMPERATURE" },
            { ATEMP,       "temperatureSelect1" },
            { ATEMP_1,     "temperatureSelect2" },
            { PAIN,        "PAIN" },
            { APAIN,       "painSelect1" },
            { APAIN_1,     "painSelect2" },
            { O21,         "O2SAT" },
            { AO21,        "on" },
//            { AO21_1,      "^S^VVEAO21_1" }, // Could add in the future
            { ENDTIDAL,    "ENDTIDALCO2" },
            { AENDTIDAL,   "endtidalCo2Select1" },
            { AENDTIDAL_1, "endtidalCo2Select2" }
            };

            /// <summary>
            /// Chart Markup Map
            /// </summary>
            // Currently this mimics markup in PCED since markup in DB (prompts table) doesn't match PCED markup
            public static readonly Dictionary<string, string> MARKUP_MAP = new Dictionary<string, string>
            {
            { BPS,         "^CBP:^VVEBPS" },
            { BPD,         "^C/^VVEBPD" },
            { ABP,         "^S^VVEABP" },
            { ABP_1,       "^S^VVEABP_1" },
            { MAP,         "^CMAP:^VVEMAP" },
            { AMAP,        "^S^VVEAMAP" },
            { AMAP_1,      "^S^VVEAMAP_1" },
            { PULSE,       "^CPulse:^VVEPulse" },
            { APULSE,      "^S^VVEAPulse" },
            { APULSE_1,    "^S^VVEAPulse_1" },
            { RESP,        "^CResp:^VVEResp" },
            { ARESP,       "^S^VVEAResp" },
            { ARESP_1,     "^S^VVEAResp_1" },
            { TEMP,        "^CTemp:^VVETemp" },
            { ATEMP,       "^S^VVEATemp" },
            { ATEMP_1,     "^S^VVEATemp_1" },
            { PAIN,        "^CPain:^VVEPain" },
            { APAIN,       "^S^VVEAPain" },
            { APAIN_1,     "^S^VVEAPain_1" },
            { O21,         "^CO2 sat:^VVEO21" },
            { AO21,        "^S^VVEAO21" },
//            { AO21_1,      "^S^VVEAO21_1" }, // Could add in the future
            { ENDTIDAL,    "^CEndTidal:^VVEEndTidal" },
            { AENDTIDAL,   "^S^VVEAEndTidal" },
            { AENDTIDAL_1, "^S^VVEAEndTidal_1" }
            };
        }
    }
}
