namespace PulseCheck.Domain
{
    /// <summary>
    /// Permissions constants
    /// </summary>
    public static class Permission
    {
        #region Permission value constants
        /// <summary>
        /// "Write" permission flag
        /// </summary>
        public const string WRITE_PERM = "W";

        /// <summary>
        /// "Read" permission flag
        /// </summary>
        public const string READ_PERM = "R";

        /// <summary>
        /// "Exclude" permission flag
        /// </summary>
        public const string EXCLUDE_PERM = "X";
        #endregion

        #region Permission name constants
        /// <summary>
        /// Master Account Administration
        /// </summary>
        public const string ACCOUNTS = "ACCOUNTS";

        /// <summary>
        /// Administrators permission identifier
        /// </summary>
        public const string ADMINISTRATORS = "ADMINISTRATORS";
        
        /// <summary>
        /// Allergies permission identifier
        /// </summary>
        public const string ALLERGIES = "ALLERGIES";

        /// <summary>
        /// Associates permission identifier
        /// </summary>
        public const string ASSOCIATES = "ASSOCIATES";

        /// <summary>
        /// "Care Coordinator Charts" permission identifier
        /// </summary>
        public const string CARE_COORD_CHART = "CARE_COORD_CHART";

        /// <summary>
        /// "Change Patient" permission identifier
        /// </summary>
        public const string CHANGE_PATIENT = "CHANGE_PATIENT";

        /// <summary>
        /// "Charge Nurse View" permission identifier
        /// </summary>
        public const string CHARGE_NURSE_VIEW = "CHARGE_RN_VIEW";

        /// <summary>
        /// Comments permission identifier
        /// </summary>
        public const string COMMENTS = "COMMENTS";

        /// <summary>
        /// Doctors permission identifier
        /// </summary>
        public const string DOCTORS = "DOCTORS";

        /// <summary>
        /// Flowsheet permission identifier
        /// </summary>
        public const string FLOWSHEET = "FLOWSHEET";

        /// <summary>
        /// Medication Services permission identifier
        /// </summary>
        public const string MED_SVC = "MEDICATION_SVC";

        /// <summary>
        /// Nurses permission identifier
        /// </summary>
        public const string NURSES = "NURSES";

        /// <summary>
        /// Orders permission identifier
        /// </summary>
        public const string ORDERS = "ORDERS";

        /// <summary>
        /// Passwords permission identifier
        /// </summary>
        public const string PASSWORDS = "PASSWORD_ADMIN";

        /// <summary>
        /// Comments permission identifier
        /// </summary>
        public const string PATIENT_COMMENTS = "COMMENTS";

        /// <summary>
        /// Patient notes permission identifier
        /// </summary>
        public const string PATIENT_NOTES = "PATIENT_NOTES";

        /// <summary>
        /// Chart signing permission identifier
        /// </summary>
        public const string SIGNATURES = "SIGNATURES";

        /// <summary>
        /// Scribe permission identifier
        /// </summary>
        public const string SCRIBE = "SCRIBE";

        /// <summary>
        /// Tracking Board permission identifier
        /// </summary>
        public const string TRACKING_BOARD = "TRACKING_BOARD";

        /// <summary>
        /// Transfer permission identifier
        /// </summary>
        public const string TRANSFER = "TRANSFER";

        /// <summary>
        /// "View Complaint" permission identifier
        /// </summary>
        public const string VIEW_COMPLAINT = "VIEW_COMPLAINT";

        /// <summary>
        /// "Visit History" permission identifier
        /// </summary>
        public const string VISIT_HISTORY = "VISIT_HISTORY";
        #endregion
    }
}