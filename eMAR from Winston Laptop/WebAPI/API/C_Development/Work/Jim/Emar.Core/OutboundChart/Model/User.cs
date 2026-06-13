using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;

namespace Emar.Core.OutboundChart.Model
{
    public class User : Person, IUser
    {
        /// <summary>
        /// New User constructor
        /// </summary>
        public User()
        {
            Status = new Status();
        }

        public int Id { get; set; }

        private string _initials { get; set; }
        /// <summary>
        /// User's initials
        /// </summary>
        public string Initials
        {
            get { return this._initials != null ? this._initials.Trim() : ""; }
            set { this._initials = value?.Trim() ?? ""; }
        }

        /// <summary>
        /// User's type
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Subordinate flag for the user
        /// </summary>
        public string Subordinate { get; set; }

        /// <summary>
        /// Ordering only flag for the user
        /// </summary>
        public string OrderingOnly { get; set; }

        /// <summary>
        /// User status
        /// </summary>
        public Status Status { get; set; }

        /// <summary>
        /// User's current site assignment
        /// </summary>
        public byte SiteId { get; set; }

        // Deptview always defaults to "ED"
        private string _deptView { get; set; }
        /// <summary>
        /// User's current department view
        /// </summary>
        public string DeptView
        {
            get { return !string.IsNullOrWhiteSpace(this._deptView) ? this._deptView.Trim() : "ED"; }
            set { this._deptView = value?.Trim() ?? "ED"; }
        }

        /// <summary>
        /// User's hospital (external) ID
        /// </summary>
        public string HospitalId { get; set; }

        /// <summary>
        /// User's hospital's name
        /// </summary>
        public string SiteName { get; set; }

        public string FullName
        {
            get { return LastName + ", " + FirstName; }
        }

        //Favorites

        /// <summary>
        /// Flag for whether this user is active
        /// </summary>
        /// <returns></returns>
        public bool IsActive()
        {
            return Status.Code.Equals(Constants.ACTIVE_STATUS.Code);
        }

        /// <summary>
        /// Flag for whether this user is ordering only
        /// </summary>
        /// <returns></returns>
        public bool IsOrderingOnly()
        {
            return (OrderingOnly != null && OrderingOnly.ToUpper().Trim().Equals("Y"));
        }

        /// <summary>
        /// Flag for whether this user is a subordinate
        /// </summary>
        /// <returns></returns>
        public bool IsSubordiante()
        {
            return (Subordinate != null && Subordinate.ToUpper().Trim().Equals("Y"));
        }

        public MinimalUser ToMinimalUser()
        {
            return new MinimalUser
            {
                Id = this.Id,
                Initials = this.Initials,
                SiteId = this.SiteId,
                FirstName = this.FirstName,
                LastName = this.LastName,
                MiddleName = this.MiddleName,
                Suffix = this.Suffix,
                Prefix = this.Prefix
            };
        }

        #region User type checks
        /// <summary>
        /// Determine whether this user is an administrator
        /// </summary>
        /// <returns>Boolean for whether the user is an administrator</returns>
        public bool IsAdministrator()
        {
            return IsType(Constants.UserTypes.ADMINISTRATOR);
        }

        /// <summary>
        /// Determine whether this user is an associate
        /// </summary>
        /// <returns>Boolean for whether the user is an associate</returns>
        public bool IsAssociate()
        {
            return IsType(Constants.UserTypes.ASSOCIATE);
        }

        /// <summary>
        /// Determine whether this user is a nurse
        /// </summary>
        /// <returns>Boolean for whether the user is a nurse</returns>
        public bool IsNurse()
        {
            return IsType(Constants.UserTypes.NURSE);
        }

        /// <summary>
        /// Determine whether this user is a physician/doctor
        /// </summary>
        /// <returns>Boolean for whether the user is a physician/doctor</returns>
        public bool IsPhysician()
        {
            return IsType(Constants.UserTypes.DOCTOR);
        }

        /// <summary>
        /// Determine whether this user is a certain type
        /// </summary>
        /// <param name="type">Boolean for whether the user's type matches the provided type</param>
        /// <returns></returns>
        public bool IsType(string type)
        {
            if (string.IsNullOrWhiteSpace(type))
            {
                return false;
            }
            type = type.Substring(0, 1).ToUpperInvariant();
            return Type.Equals(type);
        }
        #endregion

        /// <summary>
        /// Get the user's formatted name
        /// </summary>
        /// <param name="init">Boolean flag for whether we want initials</param>
        /// <returns>Formatted name</returns>
        public string GetName(bool init = false)
        {
            if (!init)
            {
                return FullName;
            }
            else if (!string.IsNullOrWhiteSpace(Initials))
            {
                return Initials;
            }
            else
            {
                return FirstName.Substring(0, 1) + "." + LastName.Substring(0, 1) + ".";
            }
        }

        /// <summary>
        /// Constants used throughout the User model
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
        }
    }
}