using Interfaces.DomainModel;
using PulseCheck.Utilities;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;

namespace DomainModel
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

        [Key]
        public int Id { get; set; }

        private string _initials { get; set; }
        /// <summary>
        /// User's initials
        /// </summary>
        [Column("init")]
        public string Initials
        {
            get { return this._initials != null ? this._initials.Trim() : ""; }
            set { this._initials = value?.Trim() ?? ""; }
        }

        /// <summary>
        /// User's type
        /// </summary>
        [Column("type")]
        public string Type { get; set; }

        /// <summary>
        /// Subordinate flag for the user
        /// </summary>
        [Column("subord")]
        public string Subordinate { get; set; }

        /// <summary>
        /// Ordering only flag for the user
        /// </summary>
        [Column("ordonly")]
        public string OrderingOnly { get; set; }

        private string _trackFilters { get; set; }
        /// <summary>
        /// Filters available to the user on the tracking board
        /// </summary>
        [Column("trackfilters")]
        public string TrackFilters
        {
            get { return this._trackFilters != null ? this._trackFilters.Trim() : ""; }
            set { this._trackFilters = value?.Trim() ?? ""; }
        }

        /// <summary>
        /// User status
        /// </summary>
        public Status Status { get; set; }

        /// <summary>
        /// User's current site assignment
        /// </summary>
        public byte SiteId { get; set; }

        /// <summary>
        /// User permissions
        /// </summary>
        private Dictionary<string, string> Permissions = new Dictionary<string, string>();

        // Deptview always defaults to "ED"
        private string _deptView { get; set; }
        /// <summary>
        /// User's current department view
        /// </summary>
        [Column("deptview")]
        public string DeptView
        {
            get { return !string.IsNullOrWhiteSpace(this._deptView) ? this._deptView.Trim() : "ED";  }
            set { this._deptView = value?.Trim() ?? "ED"; }
        }

        /// <summary>
        /// User's hospital (external) ID
        /// </summary>
        [Column("hospid")]
        public string HospitalId { get; set; }

        /// <summary>
        /// User's hospital's name
        /// </summary>
        [Column("site_name")]
        public string SiteName { get; set; }

        /// <summary>
        /// List of Filters that the user can apply to the MTB
        /// </summary>
        [NotMapped]
        public List<Filters> MTBFilters {
            get { return GetMTBFilterOptions(); }
        }

        /// <summary>
        /// List of Navigation Options available to the user
        /// </summary>
        [NotMapped]
        public List<NavigationOption> NavigationOptions
        {
            get { return GetNavigationOptions(); }
        }

        [NotMapped]
        public string FullName
        {
            get { return LastName + ", " + FirstName; }
        }

        //Favorites

        /// <summary>
        /// Check whether a user has access to a particular navigation feature
        /// </summary>
        /// <param name="navCode">Navigation code</param>
        /// <returns>Boolean flag for whether a user has access to the feature</returns>
        public bool CanNavigateTo(string navCode)
        {
            var opts = GetNavigationOptions();
            foreach(NavigationOption o in opts)
            {
                if (o.Code.Equals(navCode))
                {
                    return true;
                }
            }
            return false;
        }

        #region Permission checking
        /// <summary>
        /// Look up a user's permission value
        /// </summary>
        /// <param name="permName">Permission name</param>
        /// <returns>Permission value</returns>
        public string GetPermission(string permName)
        {
            if (!Permissions.ContainsKey(permName))
            {
                Permissions[permName] = new DB.Select
                {
                    Sql = "SELECT [dbo].[fnGetUserPermission](@userId, @permName)",
                    Parameters = new SqlParameter[]
                    {
                        new SqlParameter("@userId", SqlDbType.Int) { Value = Id },
                        new SqlParameter("@permName", SqlDbType.VarChar) { Value = permName }
                    }
                }.Run().ToString();
            }

            return Permissions[permName];
        }

        /// <summary>
        /// Does a user have read or write level for a particular permission?
        /// </summary>
        /// <param name="permName">Permission name</param>
        /// <returns>Boolean flag for whether permission is read or write level</returns>
        public bool HasAtLeastReadPermission(string permName)
        {
            var perm = GetPermission(permName);
            return (perm.Equals(Permission.READ_PERM) || perm.Equals(Permission.WRITE_PERM));
        }

        /// <summary>
        /// Does a user have exclude level for a particular permission?
        /// </summary>
        /// <param name="permName">Permission name</param>
        /// <returns>Boolean flag for whether permission is exclude level</returns>
        public bool HasExcludePermission(string permName)
        {
            return GetPermission(permName).Equals(Permission.EXCLUDE_PERM);
        }

        /// <summary>
        /// Does a user have read level for a particular permission?
        /// </summary>
        /// <param name="permName">Permission name</param>
        /// <returns>Boolean flag for whether permission is read level</returns>
        public bool HasReadPermission(string permName)
        {
            return GetPermission(permName).Equals(Permission.READ_PERM);
        }

        /// <summary>
        /// Does a user have write level for a particular permission?
        /// </summary>
        /// <param name="permName">Permission name</param>
        /// <returns>Boolean flag for whether permission is write level</returns>
        public bool HasWritePermission(string permName)
        {
            return GetPermission(permName).Equals(Permission.WRITE_PERM);
        }
        #endregion

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
        /// Get the options available to this user for filtering the MTB
        /// </summary>
        /// <returns></returns>
        public List<Filters> GetMTBFilterOptions()
        {
            var filters = new List<Filters>();
            if (HasWritePermission(Permission.SCRIBE) || !IsAdministrator())
            {
                filters.Add(new Filters
                {
                    Type = Filters.Constants.FILTER_TYPE_PATIENT,
                    Name = "Patient Filters",
                    Options = new List<Filter> {
                        new Filter { Name = "All", Code = Filters.Constants.FILTER_PATIENT_ALL },
                        new Filter { Name = "My Patients", Code = Filters.Constants.FILTER_PATIENT_MY_PATIENTS },
                        new Filter { Name = "Mine and None", Code = Filters.Constants.FILTER_PATIENT_MINE_AND_NONE }
                    }
                });
            };

            filters.Add(new Filters
            {
                Type = Filters.Constants.FILTER_TYPE_DISPO,
                Name = "Disposition Filters",
                Options = new List<Filter>
                {
                    new Filter { Name = "All", Code = Filters.Constants.FILTER_DISPO_ALL },
                    new Filter { Name = "Has Dispo", Code = Filters.Constants.FILTER_DISPO_HAS_DISPO },
                    new Filter { Name = "No Dispo", Code = Filters.Constants.FILTER_DISPO_NONE },
                    new Filter { Name = "Admissions", Code = Filters.Constants.FILTER_DISPO_ADM },
                    new Filter { Name = "Inpatient", Code = Filters.Constants.FILTER_DISPO_INP },
                    new Filter { Name = "Obs", Code = Filters.Constants.FILTER_DISPO_OBS },
                }
            });

            // Always set the selected filter
            filters.ForEach(x => x.Options.ForEach(y => y.Selected = (y.Code.Equals(TrackFilters))));

            return filters;
        }

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
            } else if (!string.IsNullOrWhiteSpace(Initials))
            {
                return Initials;
            } else
            {
                return FirstName.Substring(0, 1) + "." + LastName.Substring(0, 1) + ".";
            }
        }

        /// <summary>
        /// Get the options available to this user for navigation
        /// </summary>
        /// <returns></returns>
        public List<NavigationOption> GetNavigationOptions()
        {
            var siteInfo = new DB.Select
            {
                Sql = "SELECT root,gotrx,gotmeds FROM org WHERE site=@site",
                Parameters = new SqlParameter[]
                {
                    new SqlParameter("@site", SqlDbType.TinyInt) { Value = SiteId }
                }
            }.RunForDataRow();

            var shw = new DB.Select
            {
                Sql = "SELECT shw FROM drs WHERE num=@num",
                Parameters = new SqlParameter[]
                {
                    new SqlParameter("@num", SqlDbType.Int) { Value = Id }
                }
            }.RunForScalar().ToString();

            var navOptions = new List<NavigationOption>();

            if (HasAtLeastReadPermission(Permission.TRACKING_BOARD))
            {
                navOptions.Add(new NavigationOption
                {
                    Code = Navigation.Constants.TRACKING_BOARD,
                    Name = "Tracking Board"
                });
            }

            var ordPerm = GetPermission(Permission.ORDERS);
            if (ordPerm.Equals(Permission.READ_PERM) || ordPerm.Equals(Permission.WRITE_PERM))
            {
                if (ordPerm.Equals(Permission.WRITE_PERM))
                {
                    navOptions.Add(
                        new NavigationOption
                        {
                            Code = Navigation.Constants.ORDERS,
                            Name = "Orders"
                        }
                    );
                }

                navOptions.Add(
                    new NavigationOption
                    {
                        Code = Navigation.Constants.LABS,
                        Name = "Labs",
                    }
                );
                navOptions.Add(
                    new NavigationOption
                    {
                        Code = Navigation.Constants.XRAYS,
                        Name = "X-Rays",
                    }
                );
            }

            if (HasWritePermission(Permission.TRANSFER))
            {
                navOptions.Add(
                    new NavigationOption
                    {
                        Code = Navigation.Constants.TRANSFER,
                        Name = "Transfer"
                    }
                );
            }

            var commentsPerm = GetPermission(Permission.PATIENT_COMMENTS);
            if (commentsPerm.Equals(Permission.READ_PERM))
            {
                navOptions.Add(new NavigationOption
                {
                    Code = Navigation.Constants.PATIENT_COMMENTS_READ,
                    Name = "Read Patient Comments"
                });
            } else if (commentsPerm.Equals(Permission.WRITE_PERM))
            {
                navOptions.Add(new NavigationOption
                {
                    Code = Navigation.Constants.PATIENT_COMMENTS_WRITE,
                    Name = "Write Patient Comments"
                });
            }

            var site = new Site(SiteId);
            if (HasWritePermission(Permission.SIGNATURES))
            {
                var root = siteInfo["root"].ToString().Trim();
                var keyFile = root + "sign\\" + Id + ".prv.decrypt";
                if (File.Exists(keyFile))
                {
                    navOptions.Add(
                        new NavigationOption
                        {
                            Code = Navigation.Constants.SIGN_CHART,
                            Name = "Sign Chart"
                        }
                    );

                    // TODO: Think more about how this is handled. Probably a nicer way to implement it.
                    if (site.GetOrgOption("SIGNATURE_PIN").Equals("Y"))
                    {
                        navOptions.Last().Details.Add("AUTH_REQUIRED");
                    }
                }
            }

            if (siteInfo["gotmeds"].Equals("Y"))
            {
                if (HasAtLeastReadPermission(Permission.ALLERGIES) && siteInfo["gotrx"].Equals("Y"))
                {
                    navOptions.Add(
                        new NavigationOption
                        {
                            Code = Navigation.Constants.ALLERGIES,
                            Name = "Allergies"
                        }
                    );
                }

                if (HasAtLeastReadPermission(Permission.CHANGE_PATIENT))
                {
                    navOptions.Add(
                        new NavigationOption
                        {
                            Code = Navigation.Constants.CURRENT_MEDS,
                            Name = "Current Medications"
                        }
                    );
                }

                var medSvcPerm = GetPermission(Permission.MED_SVC);
                if (medSvcPerm.Equals(Permission.READ_PERM))
                {
                    navOptions.Add(
                        new NavigationOption
                        {
                            Code = Navigation.Constants.MED_SVC_READ,
                            Name = "Medications - Read"
                        }
                    );
                } else if (medSvcPerm.Equals(Permission.WRITE_PERM))
                {
                    navOptions.Add(
                        new NavigationOption
                        {
                            Code = Navigation.Constants.MED_SVC_WRITE,
                            Name = "Medications - Write"
                        }
                    );

                    var medActions = new MedicationActions(site);
                    var orderActions = medActions.GetActionByName("order", "1");
                    if (orderActions != null && orderActions.ContainsKey("auth"))
                    {
                        var auth = orderActions["auth"].ToUpperInvariant();
                        if (!string.IsNullOrWhiteSpace(auth) && !auth.Equals("N"))
                        {
                            if (auth.Equals("A"))
                            {
                                navOptions.Last().Details.Add("DUAL_AUTH_REQUIRED");
                            }
                            else
                            {
                                navOptions.Last().Details.Add("AUTH_REQUIRED");
                            }
                        }
                    }
                }
            }

            if (IsAdministrator() && HasWritePermission(Permission.PASSWORDS))
            {
                if (HasWritePermission(Permission.DOCTORS) || HasWritePermission(Permission.NURSES) || HasWritePermission(Permission.ASSOCIATES) || HasWritePermission(Permission.ADMINISTRATORS))
                {
                    navOptions.Add(
                        new NavigationOption
                        {
                            Code = Navigation.Constants.ADMIN_UNLOCK,
                            Name = "Administration - Unlock Account"
                        }
                    );
                }
            }

            var chargeNurse = (HasWritePermission(Permission.CHARGE_NURSE_VIEW) && shw.Substring(16, 1).Equals("1"));
            if (HasExcludePermission(Permission.SCRIBE) && HasExcludePermission(Permission.CARE_COORD_CHART))
            {
                if (chargeNurse || (IsPhysician() && !IsSubordiante()))
                {
                    navOptions.Add(
                        new NavigationOption
                        {
                            Code = Navigation.Constants.SIGNUP_ATTENDING,
                            Name = "Sign-up - Attending"
                        }
                    );
                }

                if (chargeNurse || IsPhysician())
                {
                    navOptions.Add(
                        new NavigationOption
                        {
                            Code = Navigation.Constants.SIGNUP_RESIDENT,
                            Name = "Sign-up - Resident"
                        }
                    );
                }

                if (chargeNurse || IsPhysician())
                {
                    navOptions.Add(
                        new NavigationOption
                        {
                            Code = Navigation.Constants.SIGNUP_EXTENDER,
                            Name = "Sign-up - Extender"
                        }
                    );
                }

                if (chargeNurse || (IsNurse() && !IsSubordiante()))
                {
                    navOptions.Add(
                        new NavigationOption
                        {
                            Code = Navigation.Constants.SIGNUP_PRIMARYNURSE,
                            Name = "Sign-up - Primary Nurse"
                        }
                    );
                }

                if (chargeNurse || IsNurse())
                {
                    navOptions.Add(
                        new NavigationOption
                        {
                            Code = Navigation.Constants.SIGNUP_NURSEEXTENDER,
                            Name = "Sign-up - Nurse Extender"
                        }
                    );
                }
            }

            if (chargeNurse || false)
            {
                navOptions.Add(
                    new NavigationOption
                    {
                        Code = Navigation.Constants.SIGNUP_CARECOORDINATOR,
                        Name = "Sign-up - Care Coordinator"
                    }
                );
            }

            if (chargeNurse || false)
            {
                navOptions.Add(
                    new NavigationOption
                    {
                        Code = Navigation.Constants.SIGNUP_SCRIBE,
                        Name = "Sign-up - Scribe"
                    }
                );
            }

            if (HasWritePermission(Permission.ACCOUNTS))
            {
                navOptions.Add(
                    new NavigationOption
                    {
                        Code = Navigation.Constants.ACCOUNT_ADMIN,
                        Name = "Account Administration"
                    }
                );
            }

            return navOptions;
        }

        /// <summary>
        /// Set the MTB filter for the user
        /// </summary>
        /// <param name="filter">New filter</param>
        public void SetMTBFilter(string filter)
        {
            if (!string.IsNullOrEmpty(filter))
            {
                TrackFilters = filter;
                new DB.Update
                {
                    Sql = "UPDATE drs SET trackfilters = @trackfilters WHERE num = @userId",
                    Parameters = new SqlParameter[]
                    {
                        new SqlParameter("@trackfilters", SqlDbType.VarChar) { Value = filter },
                        new SqlParameter("@userId", SqlDbType.Int) { Value = Id }
                    }
                }.Run();
            }
        }

        /// <summary>
        /// Remove the MTB filter for the user
        /// </summary>
        public void RemoveMTBFilter()
        {
            TrackFilters = null;
            new DB.Update
            {
                Sql = "UPDATE drs SET trackfilters = NULL WHERE num = @userId",
                Parameters = new SqlParameter[]
                {
                    new SqlParameter("@userId", SqlDbType.Int) { Value = Id }
                }
            }.Run();
        }
    }
}