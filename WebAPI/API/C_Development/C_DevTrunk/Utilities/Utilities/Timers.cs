using Interfaces.DomainModel;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace PulseCheck.Utilities
{
    /// <summary>
    /// Library to work with timers
    /// </summary>
    public static class Timers
    {
        public static Dictionary<string, Dictionary<string, string>> TimerType = new Dictionary<string, Dictionary<string, string>>
        {
            { Constants.AMBULANCE, new Dictionary<string, string> {
                { Constants.DESCRIPTION, "Time of Ambulance" },
                { Constants.SERVICE, "2" },
                { Constants.BEFORE_TRIAGE, "1" },
            } },
            { Constants.ADT_MATCH, new Dictionary<string, string> {
                { Constants.DESCRIPTION, "Time to ADT Match" },
                { Constants.SERVICE, "3" },
            } },
            { Constants.BED, new Dictionary<string, string> {
                { Constants.DESCRIPTION, "Time to Bed" },
                { Constants.SERVICE, "10" },
            } },
            { Constants.CALL_IN, new Dictionary<string, string> {
                { Constants.DESCRIPTION, "Time of Call In" },
                { Constants.SERVICE, "6" },
                { Constants.BEFORE_TRIAGE, "1" },
            } },
            { Constants.CARE_COORD_DONE, new Dictionary<string, string> {
                { Constants.DESCRIPTION, "Time to Care Coordinator Done" },
                { Constants.SERVICE, "27" },
            } },
            { Constants.CARE_COORD_REQ, new Dictionary<string, string> {
                { Constants.DESCRIPTION, "Time to Care Coordinator Requested" },
                { Constants.SERVICE, "26" },
            } },
            { Constants.CARE_COORDINATOR, new Dictionary<string, string> {
                { Constants.DESCRIPTION, "Time to Care Coordinator Assigned" },
                { Constants.SERVICE, "25" },
            } },
            { Constants.DISPO, new Dictionary<string, string> {
                { Constants.DESCRIPTION, "Time to Disposition" },
                { Constants.SERVICE, "30" },
            } },
            { Constants.DOCTOR, new Dictionary<string, string> {
                { Constants.DESCRIPTION, "Time to Doctor" },
                { Constants.SERVICE, "20" },
            } },
            { Constants.DR_EXTENDER, new Dictionary<string, string> {
                { Constants.DESCRIPTION, "Time to Doctor Extender" },
                { Constants.SERVICE, "21" },
            } },
            { Constants.EXTENDER, new Dictionary<string, string> {
                { Constants.DESCRIPTION, "Time to Extender" },
                { Constants.SERVICE, "23" },
            } },
            { Constants.FULL_TRIAGE, new Dictionary<string, string> {
                { Constants.DESCRIPTION, "Time to Full Triage" },
                { Constants.SERVICE, "4" },
            } },
            { Constants.GREET, new Dictionary<string, string> {
                { Constants.DESCRIPTION, "Time of Greet" },
                { Constants.SERVICE, "1" },
                { Constants.BEFORE_TRIAGE, "1" },
            } },
            { Constants.NAME_INDICATOR, new Dictionary<string, string> {
                { Constants.DESCRIPTION, "Name Indicator" },
                { Constants.SERVICE, "41" },
                { Constants.CAN_HAVE_MANY, "1" },
            } },
            { Constants.NURSE, new Dictionary<string, string> {
                { Constants.DESCRIPTION, "Time to Nurse" },
                { Constants.SERVICE, "24" },
            } },
            { Constants.PATIENT_INDICATOR, new Dictionary<string, string> {
                { Constants.DESCRIPTION, "Patient Indicator" },
                { Constants.SERVICE, "40" },
                { Constants.CAN_HAVE_MANY, "1" },
            } },
            { Constants.PATIENT_REG, new Dictionary<string, string> {
                { Constants.DESCRIPTION, "Patient being registered" },
                { Constants.SERVICE, "7" },
            } },
            { Constants.REGISTRATION, new Dictionary<string, string> {
                { Constants.DESCRIPTION, "Time to Registration" },
                { Constants.SERVICE, "0" },
            } },
            { Constants.RESIDENT, new Dictionary<string, string> {
                { Constants.DESCRIPTION, "Time to Resident" },
                { Constants.SERVICE, "22" }
            } },
            { Constants.SECOND_DOCTOR, new Dictionary<string, string>
            {
                { Constants.DESCRIPTION, "Time to Second Doctor" },
                { Constants.SERVICE, "16614" }
            } },

            { Constants.STEP_2_TRIAGE, new Dictionary<string, string> {
                { Constants.DESCRIPTION, "Time  to Step 2 Triage" },
                { Constants.SERVICE, "5" }
            } }
        };

        /// <summary>
        /// Create a new timer for a patient
        /// </summary>
        /// <param name="timerName">Timer name (from constants)</param>
        /// <param name="patient"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        public static int CreateTimer(string timerName, IPatient patient, IUser user)
        {
            if (!TimerType.ContainsKey(timerName))
            {
                return 0;
            }

            var timerInfo = TimerType[timerName];
            var canHaveMany = timerInfo.ContainsKey(Constants.CAN_HAVE_MANY) && timerInfo[Constants.CAN_HAVE_MANY].Equals("1");
            var timerService = timerInfo[Constants.SERVICE];

            // Most of the auto timers should only have one entry, so check trx for them and don't try to insert if we find one.
            if (!canHaveMany && TimerAlreadyExists(user.SiteId, patient.Ibex, Constants.AUTO_TIMER_TYPE, timerService))
            {
                return 0;
            }

            // Create dictionary with info for the transaction
            Dictionary<string, object> Values = new Dictionary<string, object> {
                { Transaction.Constants.Name, timerName },
                { Transaction.Constants.Service, timerService },
                { Transaction.Constants.Type, Constants.AUTO_TIMER_TYPE }
            };

            // TODO: There was some stuff done here in the Perl code, that didn't appear necessary at the time of .NET implementation.
            var t = new Transaction(user.SiteId, patient, user.Id, Values, null);
            var losecs = t.AddTransaction();

            return losecs;
        }

        private static bool TimerAlreadyExists(byte site, string ibex, string timerType, string timerService)
        {
            // TODO: This used caching in the Perl code. Maybe want to do that here too.
            var losecs = new DB.Select
            {
                Sql = "SELECT losecs FROM trx WHERE ibex=@ibex AND site=@site AND type=@type AND service=@service",
                Parameters = new SqlParameter[]
                {
                    new SqlParameter("@ibex", SqlDbType.Char) { Value = ibex },
                    new SqlParameter("@site", SqlDbType.TinyInt) { Value = site },
                    new SqlParameter("@type", SqlDbType.Char) { Value = timerType },
                    new SqlParameter("@service", SqlDbType.Int) { Value = timerService }
                }
            }.RunForInt();

            return losecs > 0;
        }

        /// <summary>
        /// Constants used in Timers
        /// </summary>
        public class Constants
        {
            #region Dictionary key constants
            /// <summary>
            /// Timer description key
            /// </summary>
            internal const string DESCRIPTION = "desc";

            /// <summary>
            /// Timer service key
            /// </summary>
            internal const string SERVICE = "service";

            /// <summary>
            /// Timer key for flagging whether a timer is a before triage timer
            /// </summary>
            internal const string BEFORE_TRIAGE = "before_triage";

            /// <summary>
            /// Timer key for flagging whether a timer can have multiple instances
            /// </summary>
            internal const string CAN_HAVE_MANY = "can_have_many";
            #endregion

            #region Timer names
            /// <summary>
            /// "Time of Ambulance" identifier
            /// </summary>
            public const string AMBULANCE = "AMBULANCE";

            /// <summary>
            /// "Time to ADT Match" identifier
            /// </summary>
            public const string ADT_MATCH = "ADT_MATCH";

            /// <summary>
            /// "Time to Bed" identifier
            /// </summary>
            public const string BED = "BED";

            /// <summary>
            /// "Time of Call In" identifier
            /// </summary>
            public const string CALL_IN = "CALL_IN";

            /// <summary>
            /// "Time to Care Coordinator Done" identifier
            /// </summary>
            public const string CARE_COORD_DONE = "CARE_COORD_DONE";

            /// <summary>
            /// "Time to Care Coordinator Requested" identifier
            /// </summary>
            public const string CARE_COORD_REQ = "CARE_COORD_REQ";

            /// <summary>
            /// "Time to Care Coordinator Assigned" identifier
            /// </summary>
            public const string CARE_COORDINATOR = "CARE_COORDINATOR";

            /// <summary>
            /// "Time to Dispo" identifier
            /// </summary>
            public const string DISPO = "DISPO";

            /// <summary>
            /// "Time to Doctor" identifier
            /// </summary>
            public const string DOCTOR = "DOCTOR";

            /// <summary>
            /// "Time to Doctor Extender" identifier
            /// </summary>
            public const string DR_EXTENDER = "DR_EXTENDER";

            /// <summary>
            /// "Time to Extender" identifier
            /// </summary>
            public const string EXTENDER = "EXTENDER";

            /// <summary>
            /// "Time to Full Triage" identifier
            /// </summary>
            public const string FULL_TRIAGE = "FULL_TRIAGE";

            /// <summary>
            /// "Time of Greet" identifier
            /// </summary>
            public const string GREET = "GREET";

            /// <summary>
            /// "Name Indiciator" identifier
            /// </summary>
            public const string NAME_INDICATOR = "NAME_INDICATOR";

            /// <summary>
            /// "Time to Nurse" identifier
            /// </summary>
            public const string NURSE = "NURSE";

            /// <summary>
            /// "Patient Indicator" identifier
            /// </summary>
            public const string PATIENT_INDICATOR = "PATIENT_INDICATOR";

            /// <summary>
            /// "Patient being registered" identifier
            /// </summary>
            public const string PATIENT_REG = "PATIENT_REG";

            /// <summary>
            /// "Time to Registration" identifier
            /// </summary>
            public const string REGISTRATION = "REGISTRATION";

            /// <summary>
            /// "Time to Resident" identifier
            /// </summary>
            public const string RESIDENT = "RESIDENT";

            /// <summary>
            /// "Time to Second Doctor" identifier
            /// </summary>
            public const string SECOND_DOCTOR = "SECOND_DOCTOR";

            /// <summary>
            /// "Time to Step 2 Triage" identifier
            /// </summary>
            public const string STEP_2_TRIAGE = "STEP_2_TRIAGE";
            #endregion

            #region Timer types
            /// <summary>
            /// Auto timer identifier
            /// </summary>
            internal const string AUTO_TIMER_TYPE = "S";

            /// <summary>
            /// Manual timer identifier
            /// </summary>
            internal const string MANUAL_TIMER_TYPE = "T";
            #endregion
        }
    }
}
 