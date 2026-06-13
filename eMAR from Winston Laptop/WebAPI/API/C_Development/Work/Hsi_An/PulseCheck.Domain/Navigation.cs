namespace PulseCheck.Domain
{
    /// <summary>
    /// Class to help with navigation
    /// </summary>
    public static class Navigation
    {
        /// <summary>
        /// Constant identifiers involved in navigation
        /// </summary>
        public static class Constants
        {
            #region Admin features
            /// <summary>
            /// "Unlock user account" admin feature identifier
            /// </summary>
            public static string ADMIN_UNLOCK = "ADMUNL";

            /// <summary>
            /// Mobile account administration (including devices)
            /// </summary>
            public static string ACCOUNT_ADMIN = "ACCADM";
            #endregion

            #region General features
            /// <summary>
            /// Allergies identifier
            /// </summary>
            public static string ALLERGIES = "ALG";

            /// <summary>
            /// Current medications identifier
            /// </summary>
            public static string CURRENT_MEDS = "CMEDS";

            /// <summary>
            /// Labs identifier
            /// </summary>
            public static string LABS = "LAB";

            /// <summary>
            /// Medication services identifier for read
            /// </summary>
            public static string MED_SVC_READ = "MEDR";

            /// <summary>
            /// Medication services identifier for write
            /// </summary>
            public static string MED_SVC_WRITE = "MEDW";

            /// <summary>
            /// Tracking board identifier
            /// </summary>
            public static string TRACKING_BOARD = "MTB";

            /// <summary>
            /// Orders identifier
            /// </summary>
            public static string ORDERS = "ORD";

            /// <summary>
            /// Identifier for when a user is able to read patient comments
            /// </summary>
            public static string PATIENT_COMMENTS_READ = "PCR";

            /// <summary>
            /// Identifier for when a user is able to write patient comments
            /// </summary>
            public static string PATIENT_COMMENTS_WRITE = "PCW";

            /// <summary>
            /// Sign Chart identifier
            /// </summary>
            public static string SIGN_CHART = "SGN";

            /// <summary>
            /// Transfer identifier
            /// </summary>
            public static string TRANSFER = "TRN";

            /// <summary>
            /// X-rays identifier
            /// </summary>
            public static string XRAYS = "XRAY";
            #endregion

            #region Patient signups
            /// <summary>
            /// Attending signup identifier
            /// </summary>
            public static string SIGNUP_ATTENDING = "SUATT";

            /// <summary>
            /// Care Coordinator signup identifier
            /// </summary>
            public static string SIGNUP_CARECOORDINATOR = "SUCC";

            /// <summary>
            /// Extender signup identifier
            /// </summary>
            public static string SIGNUP_EXTENDER = "SUEXT";

            /// <summary>
            /// Nurse Extender signup identifier
            /// </summary>
            public static string SIGNUP_NURSEEXTENDER = "SURNE";

            /// <summary>
            /// Primary Nurse signup identifier
            /// </summary>
            public static string SIGNUP_PRIMARYNURSE = "SUPRN";

            /// <summary>
            /// Resident signup identifier
            /// </summary>
            public static string SIGNUP_RESIDENT = "SURES";

            /// <summary>
            /// Scribe signup identifier
            /// </summary>
            public static string SIGNUP_SCRIBE = "SUS";
            #endregion
        }
    }
}