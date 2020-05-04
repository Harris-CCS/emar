using System.Collections.Generic;

namespace DomainModel
{
    /// <summary>
    /// Constants used throughout the domain model
    /// </summary>
    public static class Constants
    {
        /// <summary>
        /// General Active Status
        /// </summary>
        public static Status ACTIVE_STATUS = new Status()
        {
            Code = "A",
            Description = "Active",
            Style = new Style()
        };

        /// <summary>
        /// Allergy/Medication Confirmed status
        /// </summary>
        public static Status CONFIRMED_STATUS = new Status()
        {
            Code = "C",
            Description = "Confirmed",
            Style = new Style()
        };

        /// <summary>
        /// General Inactive status
        /// </summary>
        public static Status INACTIVE_STATUS = new Status()
        {
            Code = "I",
            Description = "Inactive",
            Style = new Style()
        };

        /// <summary>
        /// Pending status for digital signatures, and maybe other things.
        /// </summary>
        public static Status PENDING_STATUS = new Status()
        {
            Code = "P",
            Description = "Pending",
            Style = new Style()
        };

        /// <summary>
        /// Allergy/Medication Rejected status
        /// </summary>
        public static Status REJECTED_STATUS = new Status()
        {
            Code = "R",
            Description = "Rejected",
            Style = new Style()
        };

        /// <summary>
        /// Allergy/Medication Unconfirmed status
        /// </summary>
        public static Status UNCONFIRMED_STATUS = new Status()
        {
            Code = "U",
            Description = "Unconfirmed",
            Style = new Style()
        };

        /// <summary>
        /// Allergy/Medication Viewed status. Only used for CCD operations
        /// </summary>
        public static Status VIEWED_STATUS = new Status()
        {
            Code = "V",
            Description = "Viewed",
            Style = new Style()
        };

        // TODO: This should be placed in the database somewhere, and maybe split apart as general and others.
        /// <summary>
        /// List of the Statuses
        /// </summary>
        public static List<Status> Statuses => new List<Status>()
        {
            ACTIVE_STATUS,
            CONFIRMED_STATUS,
            INACTIVE_STATUS,
            PENDING_STATUS,
            REJECTED_STATUS,
            UNCONFIRMED_STATUS,
            VIEWED_STATUS
        };

        // TODO: Move these into User* constants
        #region Role descriptions
        /// <summary>
        /// Name used for Attending role
        /// </summary>
        public const string Role_Attending = "Attending";

        /// <summary>
        /// Name used for Resident role
        /// </summary>
        public const string Role_Resident = "Resident";

        /// <summary>
        /// Name used for Extender role
        /// </summary>
        public const string Role_Extender = "Extender";

        /// <summary>
        /// Name used for Primary Nurse role
        /// </summary>
        public const string Role_PrimaryNurse = "Primary Nurse";

        /// <summary>
        /// Name used for Nurse Extender role
        /// </summary>
        public const string Role_NurseExtender = "Nurse Extender";

        /// <summary>
        /// Name used for Ordering Doctor role
        /// </summary>
        public const string Role_Ordering_Doc = "Ordering Doctor";

        /// <summary>
        /// Name used for Care Coordinator role
        /// </summary>
        public const string Role_CareCoordinator = "Care Coordinator";

        /// <summary>
        /// Name used for Scribe role
        /// </summary>
        public const string Role_Scribe = "Scribe";

        /// <summary>
        /// Name used for First Doctor role
        /// </summary>
        public const string Role_First_Doctor = "First Doctor";

        /// <summary>
        /// Name used for First Resident role
        /// </summary>
        public const string Role_First_Resident = "First Resident";

        /// <summary>
        /// Name used for First Doctor Extender role
        /// </summary>
        public const string Role_First_Doctor_Extender = "First Doctor Extender";
        #endregion

        #region Role column identifiers
        /// <summary>
        /// Column identifier for Doctor role
        /// </summary>
        public const string Id_Doctor = "doctor";

        /// <summary>
        /// Column identifier for First Doctor role
        /// </summary>
        public const string ID_FirstDoctor = "firstdoctor";

        /// <summary>
        /// Column identifier for First Resident role
        /// </summary>
        public const string ID_FirstResident = "firstresident";

        /// <summary>
        /// Column identifier for First Doctor Extender role
        /// </summary>
        public const string ID_FirstDoctorExtender = "firstdrextender";

        /// <summary>
        /// Column identifier for Resident role
        /// </summary>
        public const string Id_Resident = "resident";

        /// <summary>
        /// Column identifier for Extender role
        /// </summary>
        public const string Id_Extender = "extender";

        /// <summary>
        /// Column identifier for Doctor Extender role
        /// </summary>
        public const string Id_DoctorExtender = "drextender";

        /// <summary>
        /// Column identifier for Care Coordinator role
        /// </summary>
        public const string Id_CareCoordinator = "care_coordinator";

        /// <summary>
        /// Column identifier for Primary Nurse role
        /// </summary>
        public const string Id_PrimaryNurse = "primarynurse";

        /// <summary>
        /// Column identifier for Scribe role
        /// </summary>
        public const string Id_Scribe = "scribe";
        #endregion

        #region Expand parameter values
        public static class Expando
        {
            /// <summary>
            /// Expand all expandable attributes
            /// </summary>
            public const string EXPAND_ALL = "*all";

            /// <summary>
            /// Expand patient comments
            /// </summary>
            public const string EXPAND_COMMENTS = "patient.comments";

            /// <summary>
            /// Expand digital signatures in the chart
            /// </summary>
            public const string EXPAND_DIGITALSIGNATURES = "patient.chart.digitalsignatures";

            /// <summary>
            /// Expand patient encounters
            /// </summary>
            public const string EXPAND_ENCOUNTERS = "patient.encounters";

            /// <summary>
            /// Expand comments in patient encounters
            /// </summary>
            public const string EXPAND_ENCOUNTER_COMMENTS = "patient.encounters.comments";

            /// <summary>
            /// Expand digital signatures in each encounter chart
            /// </summary>
            public const string EXPAND_ENCOUNTER_DIGITALSIGNATURES = "patient.encounters.chart.digitalsignatures";

            /// <summary>
            /// Expand races
            /// </summary>
            public const string EXPAND_RACES = "patient.demographics.races";

            /// <summary>
            /// Expand vitalsigns
            /// </summary>
            public const string EXPAND_VITALSIGNS = "patient.vitalsigns";

            /// <summary>
            /// All patient expansion options
            /// </summary>
            public static readonly List<string> OPTIONS_FOR_EXPAND = new List<string>
            {
                EXPAND_ALL,
                EXPAND_COMMENTS,
                EXPAND_DIGITALSIGNATURES,
                EXPAND_ENCOUNTERS,
                EXPAND_ENCOUNTER_COMMENTS,
                EXPAND_ENCOUNTER_DIGITALSIGNATURES,
                EXPAND_RACES,
                EXPAND_VITALSIGNS
            };

            /// <summary>
            /// Get options for patient information expansion
            /// </summary>
            /// <returns></returns>
            public static string GetPatientOptions()
            {
                var list = OPTIONS_FOR_EXPAND;
                list.Sort();
                return string.Join(",", list);
            }
        }
        #endregion

        #region order entry
        /// <summary>
        /// Source for an order is the mobile app
        /// </summary>
        public const string Data_Source_Mobile = "M";

        /// <summary>
        /// Source for an order is the web
        /// </summary>
        public const string Source_Web = "W";
        #endregion

        /// <summary>
        /// User type constants
        /// </summary>
        public static class UserTypes
        {
            /// <summary>
            /// Administrator type identifier
            /// </summary>
            public const string ADMINISTRATOR = "A";

            /// <summary>
            /// Doctor/Physician type identifier
            /// </summary>
            public const string DOCTOR = "D";

            /// <summary>
            /// Nurse type identifier
            /// </summary>
            public const string NURSE = "N";

            /// <summary>
            /// Associate type identifier
            /// </summary>
            public const string ASSOCIATE = "S";
        }

        /// <summary>
        /// Razor e-mail Templates
        /// </summary>
        public static class EmailTemplates
        {
            /// <summary>
            /// Razor template key for a new account
            /// </summary>
            public const string NEW_ACCOUNT = "NewAccount";

            /// <summary>
            /// Razor template key for resetting a password
            /// </summary>
            public const string PASSWORD_RESET = "PasswordReset";

            /// <summary>
            /// Razor template key for authorizing a new device
            /// </summary>
            public const string DEVICE_AUTHORIZATION = "DeviceAuthorization";
        }
    }
}