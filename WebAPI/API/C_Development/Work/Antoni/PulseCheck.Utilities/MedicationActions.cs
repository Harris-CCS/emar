using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using PulseCheck.IDomain;

namespace PulseCheck.Utilities
{
    /// <summary>
    /// Retrieve Med Service actions/statuses
    /// </summary>
    public class MedicationActions
    {
        /// <summary>
        /// MedicationActions Site
        /// </summary>
        private ISite Site;

        /// <summary>
        /// List of route files for site
        /// </summary>
        private List<string> RouteFiles = new List<string>();

        /// <summary>
        /// Dictionary of authentication settings information for site
        /// </summary>
        private Dictionary<string, List<string>> AuthSettings = new Dictionary<string, List<string>>();

        /// <summary>
        /// Default MedicationActions constructor
        /// </summary>
        /// <param name="site">ISite instance</param>
        public MedicationActions(ISite site)
        {
            Site = site;
        }

        /// <summary>
        /// Get action data by code
        /// </summary>
        /// <param name="data">Dictionary with code, custom, and auth information</param>
        /// <returns>Dictionary of action data</returns>
        public Dictionary<string, string> GetAction(Dictionary<string, string> data)
        {
            var code = ((data.ContainsKey("code") ? data["code"] : "") ?? "");
            var action = new Dictionary<string, string>();
            if (data.ContainsKey("custom") && !string.IsNullOrWhiteSpace(data["custom"]) && data["custom"].Equals("1"))
            {
                var byCode = GetCustomBy("code");
            } else
            {
                action["code"] = code;
                action["name"] = Constants.NAME.ContainsKey(code) ? Constants.NAME[code] : "";
                action["description"] = Constants.DESCRIPTION.ContainsKey(code) ? Constants.DESCRIPTION[code] : "";
                action["verbiage"] = Constants.VERBIAGE.ContainsKey(code) ? Constants.VERBIAGE[code] : "";
                if (Constants.TEMPLATE.ContainsKey(code) && Constants.TEMPLATE[code])
                {
                    action["template"] = "1";
                }
            }

            if (data.ContainsKey("auth") && !string.IsNullOrWhiteSpace(data["auth"])) {
                action["auth"] = GetAuthSetting(action);
            }

            return action;
        }

        /// <summary>
        /// Get action data by name
        /// </summary>
        /// <param name="name">Action name</param>
        /// <param name="auth">Include auth</param>
        /// <returns>Dictionary of action data</returns>
        public Dictionary<string, string> GetActionByName(string name, string auth = "")
        {
            var data = new Dictionary<string, string>
            {
                { "code", name.ToUpperInvariant() },
                { "custom", "1" },
                { "auth", auth }
            };

            if (name.ToLowerInvariant().StartsWith("c_"))
            {
                var parts = name.Split(new char[] { '_' }, 2);
                data["code"] = parts.Length == 2 ? parts[1] : "";
            } else
            {
                foreach(var code in Constants.NAME.Keys)
                {
                    if (name.ToLowerInvariant().Equals(Constants.NAME[code]))
                    {
                        data["code"] = code;
                        data["custom"] = "0";
                        break;
                    }
                }
            }

            return GetAction(data);
        }

        /// <summary>
        /// Get auth settings for the requested action
        /// </summary>
        /// <param name="data">Dictionary of action data</param>
        /// <returns>Auth mode</returns>
        public string GetAuthSetting(Dictionary<string, string> data)
        {
            if (AuthSettings.Keys.Count == 0)
            {
                LoadAuthSettings();
            }

            var mode = AuthSettings["mode"].First();
            if (!mode.Equals("N"))
            {
                var authCode = (data.ContainsKey("code") ? data["code"] : "");
                var type = "custom";
                if (!data.ContainsKey("custom") || string.IsNullOrWhiteSpace(data["custom"]) || data["custom"].Equals("0"))
                {
                    authCode = Constants.AUTH_CODE.ContainsKey(authCode) ? Constants.AUTH_CODE[authCode] : "";
                    if (string.IsNullOrWhiteSpace(authCode))
                    {
                        return "";
                    }
                    type = "static";
                }

                if (AuthSettings.ContainsKey(type)) {
                    foreach (var t in AuthSettings[type])
                    {
                        if (authCode.Equals(t))
                        {
                            return mode;
                        }
                    }
                }
            }

            return "";
        }

        public List<Dictionary<string, string>> GetCustomBy(string type)
        {
            return new List<Dictionary<string, string>>
            {
                new Dictionary<string, string> { }
            };
        }

        /// <summary>
        /// Preload auth settings
        /// </summary>
        public void LoadAuthSettings()
        {
            AuthSettings["mode"] = new List<string> {
                Site.GetOrgOption("MED_SVC_AUTHENTICATION")
            };
            AuthSettings["static"] = new List<string>();
            AuthSettings["custom"] = new List<string>();

            var res = new DB.Select
            {
                Sql = "SELECT field_val, field_num FROM site_preferences WHERE site=@site AND field_num IN(3,4) ORDER BY field_num, field_seq",
                Parameters = new SqlParameter[]
                {
                    new SqlParameter("@site", SqlDbType.TinyInt) { Value = Site.Id }
                }
            }.RunForListOfDictionaries();

            foreach(var r in res)
            {
                if (r["field_num"].Equals("3"))
                {
                    AuthSettings["static"].Add(r["field_val"]);
                } else
                {
                    AuthSettings["custom"].Add(r["field_val"]);
                }
            }
        }

        /// <summary>
        /// Medication Action constants
        /// </summary>
        public static class Constants
        {
            #region Actions
            public const string ORDER = "O";
            public const string ACKNOWLEDGE = "A";
            public const string HOLD = "H";
            public const string UNHOLD = "U";
            public const string GIVE = "G";
            public const string REPEAT = "R";
            public const string CANCEL = "C";
            public const string DELETE = "D";
            public const string COSIGN = "S";
            public const string DISCONTINUE = "E";
            public const string DISCONTINUED = "F";
            #endregion

            #region Display types
            public const string DISPLAY_BEFORE_GIVEN = "B";
            public const string DISPLAY_AFTER_GIVEN = "A";
            public const string DISPLAY_ALWAYS = "Y";
            #endregion

            #region Color codes
            public const string NO_COLOR = "";
            public const string RED = "O";
            public const string PURPLE = "I";
            public const string BLUE = "S";
            public const string ORANGE = "A";
            public const string YELLOW = "Y";
            public const string GRAY = "C";
            public const string GREEN = "D";
            #endregion

            /// <summary>
            /// Color codes
            /// </summary>
            public static readonly List<Dictionary<string, string>> COLOR_CODES = new List<Dictionary<string, string>>
            {
                new Dictionary<string, string> { { "value", NO_COLOR }, { "text", "None" } },
                new Dictionary<string, string> { { "value", ORANGE }, { "text", "Orange"} },
                new Dictionary<string, string> { { "value", PURPLE }, { "text", "Purple" } }
            };

            /// <summary>
            /// Display codes
            /// </summary>
            public static readonly List<Dictionary<string, string>> DISPLAY_CODES = new List<Dictionary<string, string>>
            {
                new Dictionary<string, string> { { "value", DISPLAY_BEFORE_GIVEN }, { "text", "Before Given" } },
                new Dictionary<string, string> { { "value", DISPLAY_AFTER_GIVEN }, { "text", "After Given" } },
                new Dictionary<string, string> { { "value", DISPLAY_ALWAYS }, { "text", "Always" } }
            };

            /// <summary>
            /// Authentication mode text
            /// </summary>
            /// <remarks>Note that these values may be different for the same key in desktop PulseCheck, because of the use of a PIN on mobile vs a Password on desktop</remarks>
            public static readonly Dictionary<string, string> AUTH_TEXT = new Dictionary<string, string>
            {
                { "P", "Authenticated using password." },
                { "A", "Authenticated using password and biometrics." },
                { "B", "Authenticated using biometrics." },
            };

            /// <summary>
            /// Available standard actions, sorted in order
            /// </summary>
            public static readonly List<string> SORTED_ACTIONS = new List<string>
            {
                ORDER,
                ACKNOWLEDGE,
                HOLD,
                UNHOLD,
                GIVE,
                REPEAT,
                CANCEL,
                DELETE,
                COSIGN,
                DISCONTINUE,
                DISCONTINUED
            };

            /// <summary>
            /// Available statuses, sorted in order
            /// </summary>
            public static readonly List<Dictionary<string, string>> SORTED_STATUSES = new List<Dictionary<string, string>>
            {
                new Dictionary<string, string> { { "code", ORDER },        { "color", RED },      { "for", "" } },
                new Dictionary<string, string> { { "code", GIVE },         { "color", GREEN },    { "for", "text" } },
                new Dictionary<string, string> { { "code", "" },           { "color", PURPLE },   { "for", "" } },
                new Dictionary<string, string> { { "code", ACKNOWLEDGE },  { "color", BLUE },     { "for", "" } },
                new Dictionary<string, string> { { "code", "" },           { "color", ORANGE },   { "for", "" } },
                new Dictionary<string, string> { { "code", HOLD },         { "color", YELLOW},    { "for", "" } },
                new Dictionary<string, string> { { "code", DELETE },       { "color", NO_COLOR }, { "for", "status" } },
                new Dictionary<string, string> { { "code", CANCEL },       { "color", GRAY },     { "for", "status" } },
                new Dictionary<string, string> { { "code", GIVE },         { "color", GREEN },    { "for", "status" } },
                new Dictionary<string, string> { { "code", DISCONTINUE },  { "color", GRAY  },    { "for", "" } },
                new Dictionary<string, string> { { "code", GIVE },         { "color", GREEN },    { "for", "indicator" } },
                new Dictionary<string, string> { { "code", DISCONTINUED }, { "color", GREEN },    { "for", "" } }
            };

            /// <summary>
            /// Action names
            /// </summary>
            public static readonly Dictionary<string, string> NAME = new Dictionary<string, string>
            {
                { ORDER, "order" },
                { ACKNOWLEDGE, "ack" },
                { HOLD, "hold" },
                { UNHOLD, "unhold" },
                { GIVE, "give" },
                { REPEAT, "rep" },
                { CANCEL, "cancel" },
                { DELETE, "del" },
                { COSIGN, "cosign" },
                { DISCONTINUE, "discontinue" },
                { DISCONTINUED, "discontinued" }
            };

            /// <summary>
            /// Action descriptions
            /// </summary>
            public static readonly Dictionary<string, string> DESCRIPTION = new Dictionary<string, string>
            {
                { ORDER, "Order" },
                { ACKNOWLEDGE, "Acknowledge" },
                { HOLD, "Hold" },
                { UNHOLD, "Unhold" },
                { GIVE, "Give" },
                { REPEAT, "Repeat" },
                { CANCEL, "Cancel" },
                { DELETE, "Delete" },
                { COSIGN, "Co-sign" },
                { DISCONTINUE, "Discontinue" },
                { DISCONTINUED, "Discontinued" }
            };

            /// <summary>
            /// Action verbiage
            /// </summary>
            public static readonly Dictionary<string, string> VERBIAGE = new Dictionary<string, string>
            {
                { ORDER, "Ordered" },
                { ACKNOWLEDGE, "Acknowledged" },
                { HOLD, "Held" },
                { UNHOLD, "Hold Canceled" },
                { GIVE, "Given" },
                { CANCEL, "Canceled" },
                { DELETE, "Deleted" },
                { COSIGN, "Co-signed" },
                { DISCONTINUE, "Discontinue Ordered" },
                { DISCONTINUED, "Discontinued" }
            };

            /// <summary>
            /// Action auth codes
            /// </summary>
            public static readonly Dictionary<string, string> AUTH_CODE = new Dictionary<string, string>
            {
                { ORDER, ORDER },
                { HOLD, HOLD },
                { UNHOLD, HOLD },
                { GIVE, GIVE },
                { CANCEL, CANCEL },
                { DELETE, DELETE },
                { COSIGN, COSIGN },
                { DISCONTINUE, DISCONTINUE },
                { DISCONTINUED, DISCONTINUE }
            };

            /// <summary>
            /// Dictionary defining whether a particular action has a template
            /// </summary>
            public static readonly Dictionary<string, bool> TEMPLATE = new Dictionary<string, bool>
            {
                { HOLD, true },
                { UNHOLD, true },
                { GIVE, true },
                { CANCEL, true },
                { DISCONTINUE, true },
                { DISCONTINUED, true }
            };
        }
    }
}