using Interfaces.DomainModel;
using Interfaces.Utilities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;

namespace PulseCheck.Utilities
{
    /// <summary>
    /// Library to handle transactions (trx entries)
    /// </summary>
    public class Transaction : ITransactionUtility
    {
        private byte SiteId;
        private IPatient Patient;
        private int UserId;
        private Dictionary<string, string> Options = new Dictionary<string, string>();
        private Dictionary<string, object> Values = new Dictionary<string, object>();
        private readonly Time _time = new Time();

        private Dictionary<string, object> Defaults = new Dictionary<string, object>()
        {
            { Constants.Minutes, 0 },
            { Constants.Quantity, 1 },
            { Constants.Tid, "" },
            { Constants.Alienkey, "" },
            { Constants.Dispense, "" },
            { Constants.Status, "A" },
            { Constants.ChangeUser, 0 },
            { Constants.ChangeDate, "" },
            { Constants.Amount, 0 },
            { Constants.Face, "" },
            { Constants.ServiceType, 0 },
            { Constants.RiskRed, "" },
            { Constants.RiskGreen, "" },
            { Constants.APC, "" },
            { Constants.LevelPoints, 0 },
            { Constants.CR, "N" },
            { Constants.CPT, "" },
            { Constants.LMRP, "N" },
            { Constants.LosecsLink, 0 },
            { Constants.RVU, 0 },
            { Constants.Name, null },
            { Constants.Service, null },
            { Constants.Type, null },
            { Constants.Day, (new DateTime()).DayOfWeek },
            { Constants.IbexDate, "" },
            { Constants.Resident, 0 },
            { Constants.Doctor, 0 },
            { Constants.Nurse, 0 },
            { Constants.Extender, 0 },
            { Constants.DoctorExtender, 0 },
            { Constants.CareCoordinator, 0 },
            { Constants.Dept, "" },
            { Constants.Ward, "" },
            { Constants.Bed, "" }
        };

        private Dictionary<String, SqlDbType> ParameterTypes = new Dictionary<string, SqlDbType>
        {
            { Constants.Name, SqlDbType.VarChar },
            { Constants.Service, SqlDbType.Int },
            { Constants.Date, SqlDbType.Char },
            { Constants.ThruDate, SqlDbType.VarChar },
            { Constants.Minutes, SqlDbType.Int },
            { Constants.Quantity, SqlDbType.Char },
            { Constants.Tid, SqlDbType.VarChar },
            { Constants.Alienkey, SqlDbType.VarChar },
            { Constants.Dispense, SqlDbType.VarChar },
            { Constants.Status, SqlDbType.Char },
            { Constants.ChangeUser, SqlDbType.Int },
            { Constants.ChangeDate, SqlDbType.VarChar },
            { Constants.Amount, SqlDbType.Decimal },
            { Constants.Face, SqlDbType.Char },
            { Constants.ServiceType, SqlDbType.Int },
            { Constants.RiskRed, SqlDbType.VarChar },
            { Constants.RiskGreen, SqlDbType.VarChar },
            { Constants.APC, SqlDbType.Char },
            { Constants.LevelPoints, SqlDbType.Int },
            { Constants.CR, SqlDbType.Char },
            { Constants.CPT, SqlDbType.VarChar },
            { Constants.LMRP, SqlDbType.Char },
            { Constants.LosecsLink, SqlDbType.Int },
            { Constants.Losecs, SqlDbType.Int },
            { Constants.RVU, SqlDbType.Decimal },
            { Constants.Site, SqlDbType.SmallInt },
            { Constants.Ibex, SqlDbType.Char },
            { Constants.Type, SqlDbType.Char },
            { Constants.SystemDate, SqlDbType.Char },
            { Constants.User, SqlDbType.Int },
            { Constants.Day, SqlDbType.TinyInt },
            { Constants.IbexDate, SqlDbType.DateTime },
            { Constants.Resident, SqlDbType.Int },
            { Constants.Doctor, SqlDbType.Int },
            { Constants.Nurse, SqlDbType.Int },
            { Constants.Extender, SqlDbType.Int },
            { Constants.DoctorExtender, SqlDbType.Int },
            { Constants.CareCoordinator, SqlDbType.Int },
            { Constants.Ward, SqlDbType.Char },
            { Constants.Dept, SqlDbType.Char },
            { Constants.Bed, SqlDbType.VarChar }
        };

        /// <summary>
        /// Create a new, Transaction object
        /// </summary>
        /// <param name="siteId">Site Identifier</param>
        /// <param name="patient">Patient Object</param>
        /// <param name="userId">User Identifier</param>
        /// <param name="values">Dictionary of transaction values</param>
        /// <param name="options">Dictionary of transaction options</param>
        /// <returns></returns> 
        public Transaction(byte siteId, IPatient patient, int userId, Dictionary<string, object> values, Dictionary<string, string> options = null)
        {
            SiteId = siteId;
            Patient = patient;
            UserId = userId;
            Values = values;

            if (options != null)
            {
                Options = options;
            }

            Defaults[Constants.Site] = SiteId;
            Defaults[Constants.Ibex] = Patient.Ibex;
            Defaults[Constants.User] = UserId;

            // Assign doctor values from patient to Values dictionary if not already present.
            foreach (IMinimalProvider p in patient.Providers)
            {
                if (p.User != null)
                {
                    if (!Values.ContainsKey(p.Role.Id))
                    {
                        Values[p.Role.Id] = p.User.Id;
                    }
                    if (p.Role.Id.Equals(Constants.PrimaryNurse) && !Values.ContainsKey(Constants.Nurse))
                    {
                        Values[Constants.Nurse] = Values[p.Role.Id];
                    }
                }
            }

            // Assign location values from patient to Values dictionary if not already present.
            if (patient.Department != null && !Values.ContainsKey(Constants.Dept))
            {
                Values[Constants.Dept] = patient.Department;
            }
            if (patient.Ward != null && !Values.ContainsKey(Constants.Ward))
            {
                Values[Constants.Ward] = patient.Ward;
            }
            if (patient.Bed != null && !Values.ContainsKey(Constants.Bed))
            {
                Values[Constants.Bed] = patient.Bed;
            }
        }

        /// <summary>
        /// Add a new transaction entry to the database
        /// </summary>
        public int AddTransaction()
        {
            var parameters = new List<SqlParameter>();

            var sysDate = _time.Timestamp();
            Values[Constants.SystemDate] = sysDate;
            if (Values.ContainsKey(Constants.Date) && Values[Constants.Date] != null)
            {
                Values[Constants.Date] = Values[Constants.Date].ToString().Substring(0, 12);
            } else
            {
                Values[Constants.Date] = sysDate.Substring(0, 12);
            }
            if (Values.ContainsKey(Constants.ThruDate) && Values[Constants.ThruDate] != null)
            {
                Values[Constants.ThruDate] = Values[Constants.ThruDate].ToString().Substring(0, 12);
            } else
            {
                Values[Constants.ThruDate] = "";
            }

            var i = Defaults[Constants.Ibex].ToString();
            Values[Constants.IbexDate] =
                i.Substring(0, 4) + "-" + i.Substring(4, 2) + "-" +
                i.Substring(6, 2) + " " + i.Substring(8, 2) + ":" +
                i.Substring(10, 2) + ":" + i.Substring(12, 2);

            StringBuilder trxSQL = new StringBuilder();
            StringBuilder paramNames = new StringBuilder();
            trxSQL.Append("INSERT INTO trx(");
            foreach (String paramName in ParameterTypes.Keys)
            {
                // Skip losecs, it will be added later.
                if (paramName.Equals(Constants.Losecs))
                {
                    continue;
                }
                var param = GetParameter(paramName);
                if (param != null)
                {
                    trxSQL.Append(paramName);
                    trxSQL.Append(",");
                    paramNames.Append(param.ParameterName);
                    paramNames.Append(",");
                    parameters.Add(param);
                }
            }

            trxSQL.Append("losecs)");

            var SQL = "";
            List<SqlParameter> cmdParams = null;
            SqlException exception = null;

            // If a specific losecs is passed in through Options, only try the insert once.
            if (Options.ContainsKey(Constants.Losecs))
            {
                trxSQL.Append("VALUES(");
                paramNames.Append("@losecs");
                trxSQL.Append(paramNames.ToString());
                trxSQL.Append(")");

                SQL = trxSQL.ToString();

                Defaults[Constants.Losecs] = Options[Constants.Losecs];
                parameters.Add(GetParameter(Constants.Losecs));

                try
                {
                    var result = new DB.Insert()
                    {
                        Sql = SQL,
                        Parameters = parameters.ToArray()
                    }.Run();

                    return Convert.ToInt32(Defaults[Constants.Losecs]);

                } catch (SqlException ex)
                {
                    DTFL.Write(SiteId, UserId, ex, SQL, parameters.ToArray());
                    return 0;
                }
            }

            // Otherwise try the insert up to 100 times with random losecs values.
            else
            {
                var losecs = _time.DiffSeconds(Defaults[Constants.Ibex].ToString(), Values[Constants.Date].ToString());
                losecs = (losecs < 1) ? 1 : losecs;
                var r = new Random();

                var connection = DB.GetConnectionString();
                using (var con = new SqlConnection(connection))
                {
                    trxSQL.Append(" SELECT ");
                    trxSQL.Append(paramNames.ToString());
                    trxSQL.Append("@losecs WHERE NOT EXISTS(SELECT losecs FROM ibex..med WHERE site=@site AND ibex=@ibex AND losecs=@losecs)");
                    SQL = trxSQL.ToString();

                    con.Open();
                    for (int t = 0; t < 100; t++)
                    {
                        try
                        {
                            Defaults[Constants.Losecs] = (r.Next(1, 1000) + losecs);
                            var losecsParam = GetParameter(Constants.Losecs);
                            cmdParams = parameters;
                            cmdParams.Add(losecsParam);
                            var result = new DB.Insert()
                            {
                                Connection = con,
                                Sql = SQL,
                                Parameters = cmdParams.ToArray()
                            }.Run();

                            if (result > 0)
                            {
                                return Convert.ToInt32(Defaults[Constants.Losecs]);
                            }
                        } catch (SqlException ex)
                        {
                            exception = ex;
                        }
                    }
                }

                // If we get all the way down here, something went wrong.
                DTFL.Write(SiteId, UserId, exception, SQL, cmdParams.ToArray());
                return 0;
            }
        }

        private SqlParameter GetParameter(String name)
        {
            var param = new SqlParameter
            {
                ParameterName = "@" + name,
                SqlDbType = ParameterTypes[name]
            };

            if (Values.ContainsKey(name) && Values[name] != null)
            {
                param.Value = Values[name];
            } else if (Defaults.ContainsKey(name))
            {
                param.Value = Defaults[name];
            }
            else
            {
                return null;
            }

            return param;
        }

        /// <summary>
        /// Constants for use with Transaction objects
        /// </summary>
        public class Constants
        {
            #region trx table column name constants
            // --- trx table column name constants --- //
            /// <summary>
            /// Name of transaction
            /// </summary>
            public const string Name = "name";

            /// <summary>
            /// Service for transaction
            /// </summary>
            public const string Service = "service";

            /// <summary>
            /// Start datetime of transaction
            /// </summary>
            public const string Date = "trxdate";

            /// <summary>
            /// End datetime of transaction
            /// </summary>
            public const string ThruDate = "thrudate";

            /// <summary>
            /// Minutes of transaction
            /// </summary>
            public const string Minutes = "mins";

            /// <summary>
            /// Quantity of transaction
            /// </summary>
            public const string Quantity = "qty";

            /// <summary>
            /// Tid of transaction
            /// </summary>
            public const string Tid = "tid";

            /// <summary>
            /// Alienkey of transaction
            /// </summary>
            public const string Alienkey = "alienkey";

            /// <summary>
            /// Dispense value of transaction
            /// </summary>
            public const string Dispense = "dispense";

            /// <summary>
            /// Status of transaction
            /// </summary>
            public const string Status = "status";

            /// <summary>
            /// Change user of transaction
            /// </summary>
            public const string ChangeUser = "usrchg";

            /// <summary>
            /// Change date of transaction
            /// </summary>
            public const string ChangeDate = "datechg";

            /// <summary>
            /// Amount of transaction
            /// </summary>
            public const string Amount = "amt";

            /// <summary>
            /// Face of transaction
            /// </summary>
            public const string Face = "face";

            /// <summary>
            /// Service type of transaction
            /// </summary>
            public const string ServiceType = "svctype";

            /// <summary>
            /// Riskred value of transaction
            /// </summary>
            public const string RiskRed = "riskred";

            /// <summary>
            /// Riskgreen value of transaction
            /// </summary>
            public const string RiskGreen = "riskgreen";

            /// <summary>
            /// APC value of transaction
            /// </summary>
            public const string APC = "apc";

            /// <summary>
            /// Level points of transaction
            /// </summary>
            public const string LevelPoints = "lvlpts";

            /// <summary>
            /// CR value of transaction
            /// </summary>
            public const string CR = "cr";

            /// <summary>
            /// CPT value of transaction
            /// </summary>
            public const string CPT = "cpt";

            /// <summary>
            /// LMRP value of transaction
            /// </summary>
            public const string LMRP = "lmrp";

            /// <summary>
            /// Losecslink value of transaction
            /// </summary>
            public const string LosecsLink = "losecslink";

            /// <summary>
            /// Losecs value of transaction
            /// </summary>
            public const string Losecs = "losecs";

            /// <summary>
            /// RVUs associated with transaction
            /// </summary>
            public const string RVU = "rvu";

            /// <summary>
            /// Site identifier for transaction
            /// </summary>
            public const string Site = "site";

            /// <summary>
            /// Patient identifier for transaction
            /// </summary>
            public const string Ibex = "ibex";

            /// <summary>
            /// Type identifier for transaction
            /// </summary>
            public const string Type = "type";

            /// <summary>
            /// System datetime identifier for transaction
            /// </summary>
            public const string SystemDate = "sysdate";

            /// <summary>
            /// User identifier for transaction
            /// </summary>
            public const string User = "usr";

            /// <summary>
            /// Transaction day number
            /// </summary>
            public const string Day = "trxday";

            /// <summary>
            /// Ibex_dt value for transaction
            /// </summary>
            public const string IbexDate = "ibex_dt";

            /// <summary>
            /// Patient's resident caregiver
            /// </summary>
            public const string Resident = "resident";

            /// <summary>
            /// Patient's doctor caregiver
            /// </summary>
            public const string Doctor = "doctor";

            /// <summary>
            /// Patient's nurse caregiver
            /// </summary>
            public const string Nurse = "nurse";

            /// <summary>
            /// Patient's extender caregiver
            /// </summary>
            public const string Extender = "extender";

            /// <summary>
            /// Patient's doctor extender caregiver
            /// </summary>
            public const string DoctorExtender = "drextender";

            /// <summary>
            /// Patient's care coordinator caregiver
            /// </summary>
            public const string CareCoordinator = "care_coordinator";

            /// <summary>
            /// Patient's primary nurse caregiver
            /// </summary>
            public const string PrimaryNurse = "primarynurse";

            /// <summary>
            /// Patient's current location - ward
            /// </summary>
            public const string Ward = "ward";

            /// <summary>
            /// Patient's current location - department
            /// </summary>
            public const string Dept = "dept";

            /// <summary>
            /// Patient's current location - bed
            /// </summary>
            public const string Bed = "bed";
            #endregion
        }
    }
}