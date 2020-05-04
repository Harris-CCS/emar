using PulseCheck.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Data.SqlClient;
using Interfaces.DomainModel;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace DomainModel
{
    /// <summary>
    /// Medication object
    /// </summary>
    public class Medication : IMedication, ICloneable
    {
        [Key]
        public int Id { get; set; }

        public string Type { get; set; } = Constants.TYPE_MEDICATION;

        private string _name;
        public string Name {
            get { return _name; }
            set { _name = System.Text.RegularExpressions.Regex.Replace(value, "( : ){2,}", " : "); }
        }
        public string Status { get; set; }

        private string _ibex;
        public string Ibex {
            get { return _ibex; }
            set {
                _ibex = value;
                foreach (var component in Components)
                {
                    component.Ibex = _ibex;
                }
            }
        }

        private Int16 _site;
        public Int16 Site {
            get { return _site; }
            set {
                _site = value;
                foreach (var component in Components)
                {
                    component.Site = _site;
                }
            }
        }
        public int Losecs { get; set; }

        private string _route { get; set; }
        public string Route {
            get { return this._route != null ? this._route.Trim() : ""; }
            set { this._route = value?.Trim() ?? ""; }
        }

        private string _unit { get; set; }
        public string Unit {
            get { return this._unit != null ? this._unit.Trim() : ""; }
            set { this._unit = value?.Trim() ?? ""; }
        }

        private string _dose { get; set; }
        public string Dose {
            get { return this._dose != null ? this._dose.Trim() : ""; }
            set { this._dose = value?.Trim() ?? ""; }
        }

        private string _defaultRoute { get; set; }
        [NotMapped]
        public string DefaultRoute {
            get { return this._defaultRoute != null ? this._defaultRoute.Trim() : ""; }
            set { this._defaultRoute = value?.Trim() ?? ""; }
        }

        private string _defaultUnit { get; set; }
        [NotMapped]
        public string DefaultUnit {
            get { return this._defaultUnit != null ? this._defaultUnit.Trim() : ""; }
            set { this._defaultUnit = value?.Trim() ?? ""; }
        }

        private string _printedUnit { get; set; }
        [NotMapped]
        public string PrintedUnit
        {
            get { return this._printedUnit != null ? this._printedUnit.Trim() : ""; }
            set { this._printedUnit = value?.Trim() ?? ""; }
        }

        private string _defaultDose { get; set; }
        [NotMapped]
        public string DefaultDose {
            get { return this._defaultDose != null ? this._defaultDose.Trim() : ""; }
            set { this._defaultDose = value?.Trim() ?? ""; }
        }

        [NotMapped]
        public bool HasIndication
        {
            get; set;
        }

        [NotMapped]
        public bool IsObsolete { get; set; }

        private string _schedule { get; set; }
        public string Schedule {
            get { return this._schedule != null ? this._schedule.Trim() : ""; }
            set { this._schedule = value?.Trim() ?? ""; }
        }
        public string Time { get; set; }
        public string Repeat { get; set; }
        public string Notes { get; set; }
        public string Barcode { get; set; }
        public string OrderDate { get; set; }

        public string AckDate { get; set; }
        public string HoldDate { get; set; }
        public string HoldSysdate { get; set; }
        public string UnholdDate { get; set; }
        public string UnholdSysdate { get; set; }
        public string CancelDate { get; set; }
        public string CancelSysdate { get; set; }
        public string DeleteDate { get; set; }
        public string GiveDate { get; set; }
        public string GiveSysdate { get; set; }
        public string StopDate { get; set; }
        public string StopSysdate { get; set; }
        public string DiscontinueDate { get; set; }
        public string DiscontinuedDate { get; set; }
        public string DiscontinueSysdate { get; set; }
        public string DiscontinuedSysdate { get; set; }

        [Column("order_for_usr")]
        public int? OrderForUserId { get; set; }

        [Column("order_usr")]
        public int? OrderUserId { get; set; }

        [Column("ack_usr")]
        public int? AckUserId { get; set; }

        [Column("hold_usr")]
        public int? HoldUserId { get; set; }

        [Column("unhold_usr")]
        public int? UnholdUserId { get; set; }

        [Column("cancel_usr")]
        public int? CancelUserId { get; set; }

        [Column("delete_usr")]
        public int? DeleteUserId { get; set; }

        [Column("give_usr")]
        public int? GiveUserId { get; set; }

        [Column("stop_usr")]
        public int? StopUserId { get; set; }

        [Column("exclude_usr")]
        public int? ExcludeUserId { get; set; }

        [Column("discontinue_usr")]
        public int? DiscontinueUserId { get; set; }

        [Column("discontinued_usr")]
        public int? DiscontinuedUserId { get; set; }

        public string IVType { get; set; }
        public int? IVSite { get; set; }
        public string IVLocation { get; set; }
        public int? CPTLosecsLink { get; set; }
        public string Authentication { get; set; }

        private string _rate { get; set; }
        public string Rate
        {
            get { return this._rate != null ? this._rate.Trim() : "";  }
            set { this._rate = value?.Trim() ?? ""; }
        }

        private string _rateUnit { get; set; }
        public string RateUnit {
            get { return this._rateUnit != null ? this._rateUnit.Trim() : "";  }
            set { this._rateUnit = value?.Trim() ?? "";  }
        }

        public string Indication { get; set; }

        [Column("data_source")]
        public string DataSource { get; set; } = DomainModel.Constants.Data_Source_Mobile;

        private string _code { get; set; }
        [NotMapped]
        public string Code
        {
            get { return this._code != null ? this._code.Trim() : ""; }
            set { this._code = value?.Trim() ?? ""; }
        }

        [NotMapped]
        public int ProcedureCode { get; set; }

        [NotMapped]
        public int ProductCode { get; set; }

        public virtual ICollection<Component> Components { get; set; }

        protected internal static Dictionary<string, Dictionary<string, string>> CachedNDCs = new Dictionary<string, Dictionary<string, string>>();
        //protected internal int LosecsOffset = 0;
        [NotMapped]
        public DrugDB DrugDB { get; private set; }

        [NotMapped]
        private User User { get; set; }

        [NotMapped]
        private byte SiteId { get; set; }

        private Dictionary<string, Dictionary<string, string>> CustomActions = new Dictionary<string, Dictionary<string, string>>();

        private String OrderDateString { get; set; }

        private static Dictionary<string, Dictionary<string, string>> IdxInfo = new Dictionary<string, Dictionary<string, string>>();
        private static Dictionary<string, string> IdxInfoIVType = new Dictionary<string, string>();
        private static Dictionary<string, Dictionary<string, string>> IdxInfoPrinted = new Dictionary<string, Dictionary<string, string>>();
        private static Dictionary<string, Dictionary<string, Dictionary<string, string>>> MultumDenorm = new Dictionary<string, Dictionary<string, Dictionary<string, string>>>();
        private static Dictionary<byte, Dictionary<string, string>> DataItems = new Dictionary<byte, Dictionary<string, string>>();
        private static Dictionary<int?, Dictionary<string, string>> StaffData = new Dictionary<int?, Dictionary<string, string>>
        {
            { 0, new Dictionary<string, string>
            {
                { "last", "" },
                { "first", "" },
                { "num", "0" },
                { "hospid", "" },
                { "npi", "" },
                { "init", "" },
                { "medical_license", "" },
                { "dea", "" },
                { "Second_ID", "" },
                { "type", "" },
                { "ordonly", "" }
            } }
        };

        private bool ShowDrugForm = false;
        private bool ShowDrugStrength = false;

        /// <summary>
        /// Default Medication constructor
        /// </summary>
        public Medication()
        {
            Components = new List<Component>();
            Status = Constants.ACTIVE;
        }

        /// <summary>
        /// Medication constructor with a user
        /// </summary>
        /// <param name="user">User object</param>
        public Medication(User user)
        {
            User = user;
            init(user.SiteId);
        }

        /// <summary>
        /// Medication constructor with a site identifier
        /// </summary>
        /// <param name="siteId">Site identifier</param>
        public Medication(byte siteId)
        {
            init(siteId);
        }

        /// <summary>
        /// Perform init functions for this object
        /// </summary>
        /// <param name="siteId">Site identifier</param>
        private void init(byte siteId)
        {
            SiteId = siteId;
            Components = new List<Component>();
            DrugDB = new DrugDB(new Site(siteId));
            Status = Constants.ACTIVE;
            OrderDateString = (new Time()).Timestamp();

            Type = "";

            using (var con = new SqlConnection(DB.GetConnectionString()))
            {
                con.Open();
                var site = new Site(Convert.ToByte(Site));
                ShowDrugForm = site.GetOrgOption("MED_SVC_DOSE").Equals("Y");
                ShowDrugStrength = site.GetOrgOption("MED_SVC_STRENGTH").Equals("Y");
 
                var idxResults = new DB.Select
                {
                    Connection = con,
                    Sql = "SELECT * FROM [api].[GetSiteMedIdxInfo](@siteId)",
                    Parameters = new SqlParameter[]
                    {
                        new SqlParameter("@siteId", SqlDbType.TinyInt) { Value = siteId }
                    }

                }.RunForDataReader();

                while(idxResults.Read())
                {
                    var type = idxResults["type"].ToString().Trim();
                    var id = idxResults["id"].ToString().Trim();
                    var name = idxResults["name"].ToString().Trim();
                    var misc = idxResults["misc"].ToString().Trim();
                    var misc3 = idxResults["misc3"].ToString().Trim();

                    if (!IdxInfo.ContainsKey(type))
                    {
                        IdxInfo.Add(type, new Dictionary<string, string>());
                    }
                    IdxInfo[type][id] = name;

                    if (type.Equals("AC"))
                    {
                        IdxInfoIVType[id] = misc3;
                    } else if (type.Equals("BE"))
                    {
                        if (!IdxInfoPrinted.ContainsKey(type))
                        {
                            IdxInfoPrinted.Add(type, new Dictionary<string, string>());
                        }
                        IdxInfoPrinted[type][id] = misc;
                    }
                }
                idxResults.Close();
                con.Close();
            }
        }

        /// <summary>
        /// Set the fields for acknowledging a med
        /// </summary>
        /// <param name="userId">Identifier for user who is acknowledging the med</param>
        /// <returns>Boolean flag for success</returns>
        public bool Acknowledge(int userId)
        {
            if (!IsAcknowledged())
            {
                AckDate = (new Time()).Timestamp();
                AckUserId = userId;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Cache NDC information
        /// </summary>
        /// <param name="packagingId">Packaging Id</param>
        /// <param name="ndcInfo">NDC information dictionary</param>
        protected static void AddNDCToCache(string packagingId, Dictionary<string, string> ndcInfo)
        {
            if (!CachedNDCs.ContainsKey(packagingId))
                CachedNDCs.Add(packagingId, ndcInfo);
        }

        /// <summary>
        /// Load Medication Services data items settings for the site
        /// </summary>
        public Dictionary<string, string> GetDataItems()
        {
            if (!DataItems.ContainsKey(SiteId))
            {
                // Make sure all possible data items are defined initially
                DataItems[SiteId] = new Dictionary<string, string>
                {
                    { Constants.DataItem_DDS_override, "P" },
                    { Constants.DataItem_Dosage,       "P" },
                    { Constants.DataItem_Indication,   "P" },
                    { Constants.DataItem_Notes,        "P" },
                    { Constants.DataItem_Rate,         "P" },
                    { Constants.DataItem_Rationale,    "P" },
                    { Constants.DataItem_Repeat,       "P" },
                    { Constants.DataItem_Route,        "P" },
                    { Constants.DataItem_Schedule,     "P" }
                };

                var res = new DB.Select
                {
                    Sql = "SELECT name, value FROM data_items WHERE site=@site AND type=@type",
                    Parameters = new SqlParameter[]
                    {
                        new SqlParameter("@site", SqlDbType.TinyInt) { Value = SiteId },
                        new SqlParameter("@type", SqlDbType.TinyInt) { Value = 10 }
                    }
                }.RunForDataSet();

                if (res != null)
                {
                    foreach (DataRow dr in res.Tables[0].Rows)
                    {
                        DataItems[SiteId][dr["name"].ToString()] = dr["value"].ToString();
                    }
                }
            }

            return DataItems[SiteId];
        }        

        public bool WriteTrx(IPatient patient, string SeverityText, string ReactionsAndInteractions, string MedName, Component comp, int userId)
        {
            ReactionsAndInteractions.Trim(new char[] { '\'' });
            string[] drugs = ReactionsAndInteractions.Split(new string[] { "', '" }, StringSplitOptions.RemoveEmptyEntries);
            var success = true;
            foreach (var drug in drugs)
            {
                var trxName = drug + " interacts with " + MedName;
                if (trxName.Length > 80)
                    trxName = trxName.Substring(0, 80);

                var Values = new Dictionary<string, object>
                {
                    { Transaction.Constants.Name, trxName },
                    { Transaction.Constants.Service, Constants.SVCTYPE[SeverityText] },
                    { Transaction.Constants.Type, "V" },
                    { Transaction.Constants.Quantity, "1" },
                    { Transaction.Constants.Alienkey, comp.DrugId },
                    { Transaction.Constants.ServiceType, comp.DrugCategoryId },
                    { Transaction.Constants.RiskRed, comp.GetName() },
                    { Transaction.Constants.LosecsLink, Losecs }
                };
                var t = new Transaction(SiteId, patient, userId, Values, null);
                if (t.AddTransaction() == 0)
                {
                    success = false;
                }
            }

            return success;
        }

        /// <summary>
        /// Check required inputs for ordering the medication
        /// </summary>
        /// <param name="medName"></param>
        /// <param name="type"></param>
        /// <param name="overrides"></param>
        /// <returns>List of missing item strings</returns>
        public List<string> CheckRequired(string medName, string type, bool overrides)
        {
            var di = GetDataItems();
            var errors = new List<string>();

            if (Type.Equals(Constants.TYPE_FREE_TEXT))
            {
                if (string.IsNullOrWhiteSpace(medName))
                {
                    errors.Add("Name of medication for free text entry is required.");
                }

                if (string.IsNullOrWhiteSpace(Route) && di[Constants.DataItem_Route].Equals("R")) {
                    errors.Add("Route is required");
                }
            } else
            {
                if (string.IsNullOrWhiteSpace(Dose) && string.IsNullOrWhiteSpace(Unit) && di[Constants.DataItem_Dosage].Equals("R"))
                {
                    errors.Add("  Dosage is required");
                }
                if (string.IsNullOrWhiteSpace(Route) && di[Constants.DataItem_Route].Equals("R"))
                {
                    errors.Add("  Route is required");
                }
                if (string.IsNullOrWhiteSpace(Time) && di[Constants.DataItem_Schedule].Equals("R"))
                {
                    errors.Add("  Schedule is required");
                }
                if (string.IsNullOrWhiteSpace(Repeat) && di[Constants.DataItem_Repeat].Equals("R") && !type.Equals("mgp"))
                {
                    errors.Add("  Repeat is required");
                }
                if (di[Constants.DataItem_Rationale].Equals("R") && !overrides)
                {
                    //errors.Add("  Override is required for each interaction/reaction");
                }
                if (string.IsNullOrWhiteSpace(Notes) && di[Constants.DataItem_Notes].Equals("R"))
                {
                    errors.Add("  Notes is required");
                }

                if (errors.Count > 0)
                {
                    errors.Insert(0, medName);
                }
            }

            return errors;
        }

        /// <summary>
        /// Get the list of components
        /// </summary>
        /// <returns></returns>
        public List<IComponent> GetComponents()
        {
            var compList = new List<IComponent>();
            foreach(var c in Components)
            {
                compList.Add(c);
            }
            return compList;
        }

        /// <summary>
        /// Get the custom action information for this med using a particular code
        /// </summary>
        /// <param name="code">Action code</param>
        /// <returns>Action information Dictionary</returns>
        public Dictionary<string, string> GetCustomAction(string code)
        {
            var actions = GetCustomActions();
            if (actions.ContainsKey(code))
            {
                return actions[code];
            }

            return new Dictionary<string, string>();
        }

        /// <summary>
        /// Get a custom action using the action's color
        /// </summary>
        /// <param name="color">Action color</param>
        /// <returns>Dictionary of action information</returns>
        public Dictionary<string, string> GetCustomActionByColor(string color)
        {
            var action = new Dictionary<string, string>();
            var actions = GetCustomActions();
            foreach(var a in actions.Values)
            {
                if (a.ContainsKey("color") && !string.IsNullOrWhiteSpace(a["color"]) && a["color"].Equals(color))
                {
                    if (a.ContainsKey("entry_date") && action.ContainsKey("entry_date")) {
                        var aDate = PulseCheck.Utilities.Time.DateTimeFromString(a["entry_date"]);
                        var actionDate = PulseCheck.Utilities.Time.DateTimeFromString(a["entry_date"]);
                        if ((aDate.HasValue && actionDate.HasValue && aDate.Value > actionDate.Value) || (aDate.HasValue && !actionDate.HasValue))
                        {
                            action = a;
                        }
                    }
                }
            }

            return action;
        }

        /// <summary>
        /// Get the custom actions available for this medication
        /// </summary>
        /// <returns>Dictionary of custom actions</returns>
        public Dictionary<string, Dictionary<string, string>> GetCustomActions()
        {
            if (CustomActions.Keys.Count == 0)
            {
                LoadCustomActions();
            }

            return CustomActions;
        }

        /// <summary>
        /// Get information on the drug based on the NDC
        /// </summary>
        /// <param name="ndc">The NDC to get info on</param>
        /// <returns>Dictionary of drug information</returns>
        public Dictionary<string, string> GetDrugInfoFromNDC(string ndc)
        {
            if (MultumDenorm.ContainsKey("ndc") && MultumDenorm["ndc"].ContainsKey(ndc))
                return MultumDenorm["ndc"][ndc];

            var res = DrugDB.GetInstance().GetDrugInfoByNDC(ndc);
            if (res != null && DrugDB.GetInstance().GetDBType() == DrugDB.Constants.Vendors.MEDISPAN && res.Count > 0)
            {
                var info = new DB.Select
                {
                    Sql = "SELECT TOP 1 * FROM QCPR_product WHERE site=@site AND primary_drug_id=@ndc",
                    Parameters = new SqlParameter[]
                    {
                        new SqlParameter("@site", SqlDbType.TinyInt) { Value = SiteId },
                        new SqlParameter("@ndc", SqlDbType.VarChar) { Value = ndc }
                    }
                }.RunForDataRow();

                res["product_name"] = info["product_name"]?.ToString();
                res["product_code"] = info["product_code"]?.ToString();
            }

            if (!MultumDenorm.ContainsKey("ndc"))
                MultumDenorm["ndc"] = new Dictionary<string, Dictionary<string, string>>();

            MultumDenorm["ndc"][ndc] = res;

            return res;
        }

        /// <summary>
        /// Get the DTO form of this Medication and its components
        /// </summary>
        /// <param name="userDictionary">Dictionary of user ID -> User objects</param>
        /// <returns>Resulting MedicationDTO object</returns>
        public MedicationDTO GetDTO(Dictionary<int?, User> userDictionary)
        {
            var newMed = new MedicationDTO
            {
                Id = Id,
                Type = Type,
                Name = Name,
                Status = Status,
                Ibex = Ibex,
                Site = Site,
                Losecs = Losecs,
                Route = Route,
                Unit = Unit,
                Dose = Dose,
                Schedule = Schedule,
                Time = Time,
                Repeat = Repeat,
                Notes = Notes,
                IVType = IVType,
                IVSite = IVSite,
                IVLocation = IVLocation,
                Rate = Rate,
                RateUnit = RateUnit,
                Indication = Indication,
                OrderForUser = MedUser(userDictionary, OrderForUserId),
                OrderUser = MedUser(userDictionary, OrderUserId),
                AckUser = MedUser(userDictionary, AckUserId),
                HoldUser = MedUser(userDictionary, HoldUserId),
                UnholdUser = MedUser(userDictionary, UnholdUserId),
                CancelUser = MedUser(userDictionary, CancelUserId),
                DeleteUser = MedUser(userDictionary, DeleteUserId),
                GiveUser = MedUser(userDictionary, GiveUserId),
                StopUser = MedUser(userDictionary, StopUserId),
                DiscontinuedUser = MedUser(userDictionary, DiscontinuedUserId),
                DiscontinueUser = MedUser(userDictionary, DiscontinueUserId),
                ExcludeUser = MedUser(userDictionary, ExcludeUserId),
                OrderDate = PulseCheck.Utilities.Time.DateTimeOrNullFromString(OrderDate),
                AckDate = PulseCheck.Utilities.Time.DateTimeOrNullFromString(AckDate),
                HoldDate = PulseCheck.Utilities.Time.DateTimeOrNullFromString(HoldDate),
                HoldSysdate = PulseCheck.Utilities.Time.DateTimeOrNullFromString(HoldSysdate),
                UnholdDate = PulseCheck.Utilities.Time.DateTimeOrNullFromString(UnholdDate),
                UnholdSysdate = PulseCheck.Utilities.Time.DateTimeOrNullFromString(UnholdSysdate),
                CancelDate = PulseCheck.Utilities.Time.DateTimeOrNullFromString(CancelDate),
                CancelSysdate = PulseCheck.Utilities.Time.DateTimeOrNullFromString(CancelSysdate),
                DeleteDate = PulseCheck.Utilities.Time.DateTimeOrNullFromString(DeleteDate),
                GiveDate = PulseCheck.Utilities.Time.DateTimeOrNullFromString(GiveDate),
                GiveSysdate = PulseCheck.Utilities.Time.DateTimeOrNullFromString(GiveSysdate),
                StopDate = PulseCheck.Utilities.Time.DateTimeOrNullFromString(StopDate),
                StopSysdate = PulseCheck.Utilities.Time.DateTimeOrNullFromString(StopSysdate),
                DiscontinueDate = PulseCheck.Utilities.Time.DateTimeOrNullFromString(DiscontinueDate),
                DiscontinuedDate = PulseCheck.Utilities.Time.DateTimeOrNullFromString(DiscontinuedDate),
                DiscontinueSysdate = PulseCheck.Utilities.Time.DateTimeOrNullFromString(DiscontinueSysdate),
                DiscontinuedSysdate = PulseCheck.Utilities.Time.DateTimeOrNullFromString(DiscontinuedSysdate)
            };

            foreach (var comp in Components)
            {
                var newComp = new MedicationDTO.Component
                {
                    Id = comp.Id,
                    BrandName = comp.BrandName,
                    ActiveName = comp.ActiveName,
                    DrugRoute = comp.DrugRoute,
                    DrugForm = comp.DrugForm,
                    DrugStrength = comp.DrugStrength,
                    EnteredDose = comp.EnteredDose,
                    EnteredUnit = comp.EnteredUnit,
                    DrugDBType = comp.DrugDBType,
                    ActiveId = comp.ActiveId,
                    DrugId = comp.DrugId,
                    PackagingId = comp.PackagingId,
                    DrugCategoryId = comp.DrugCategoryId,
                    Type = comp.Type,
                    DrugFormId = comp.DrugFormId,
                    GroupName = comp.GroupName,
                    GroupType = comp.GroupType,
                    ProductCode = comp.ProductCode,
                    ProcedureCode = comp.ProcedureCode,
                    Interactions = comp.Interactions,
                    Reactions = comp.Reactions
                };

                newMed.Components.Add(newComp);
            }

            return newMed;
        }

        /// <summary>
        /// Get a list of unique user IDs associated with actions on this medication
        /// </summary>
        /// <returns>List of user IDs</returns>
        public List<int> GetUserLookupList()
        {
            var allUsers = new Dictionary<int?, int>();
            allUsers[OrderForUserId ?? 0] = 1;
            allUsers[OrderUserId ?? 0] = 1;
            allUsers[AckUserId ?? 0] = 1;
            allUsers[HoldUserId ?? 0] = 1;
            allUsers[UnholdUserId ?? 0] = 1;
            allUsers[CancelUserId ?? 0] = 1;
            allUsers[DeleteUserId ?? 0] = 1;
            allUsers[GiveUserId ?? 0] = 1;
            allUsers[StopUserId ?? 0] = 1;
            allUsers[DiscontinuedUserId ?? 0] = 1;
            allUsers[DiscontinueUserId ?? 0] = 1;
            allUsers[ExcludeUserId ?? 0] = 1;

            return allUsers.Keys.Select(x => (x > 0 ? (int)x : 0)).Where(x => x > 0).ToList();
        }

        /// <summary>
        /// Get an XElement representing this medication
        /// </summary>
        /// <returns>XElement for medication XML</returns>
        public XElement GetXML(Formulary formulary)
        {
            var xml = new XElement(
                "medication_service",
                new XAttribute("id", Losecs),
                new XAttribute("is_inactive", !IsActive()),
                new XElement("name", GetName()),
                new XElement("entry_timestamp", OrderDate),
                GetUserXML("entry_user", OrderUserId),
                GetUserXML("order_for_user", OrderForUserId),
                new XElement("order_number", Id)
            );

            var status = GetMedStatus();
            var verbiage = status.ContainsKey("verbiage") ? status["verbiage"].Replace(" ", "") : "";
            var statusAttribs = new Dictionary<string, string>();
            if (status.ContainsKey("custom") && (status["custom"] ?? "0").Equals("1"))
            {
                statusAttribs["custom"] = "1";
                statusAttribs["code"] = status.ContainsKey("code") ? status["code"] : "";
            }

            var medStatus = new XElement("med_status");
            medStatus.SetValue(verbiage);
            foreach(var k in statusAttribs.Keys)
            {
                medStatus.Add(new XAttribute(k, statusAttribs[k]));
            }
            xml.Add(medStatus);

            if (!IsActive())
            {
                var date = IsCancelled() ? CancelDate : DeleteDate;
                var user = IsCancelled() ? CancelUserId : DeleteUserId;
                xml.Add(new XElement("inactive_timestamp", date));
                xml.Add(GetUserXML("inactive_user", user));
            }

            xml.Add(GetActionXML("ack", AckUserId, AckDate));
            xml.Add(GetActionXML("hold", HoldUserId, HoldDate, HoldSysdate));
            xml.Add(GetActionXML("unhold", UnholdUserId, UnholdDate, UnholdSysdate));
            xml.Add(GetActionXML("give", GiveUserId, GiveDate, GiveSysdate));
            xml.Add(GetActionXML("discontinue", DiscontinueUserId, DiscontinueDate, DiscontinueSysdate));
            xml.Add(GetActionXML("discontinued", DiscontinuedUserId, DiscontinuedDate, DiscontinuedSysdate));

            var customActions = new List<XElement>();
            var actionList = GetCustomActions();
            foreach(KeyValuePair<string, Dictionary<string, string>> entry in actionList.OrderBy(x => x.Value.ContainsKey("sysdate") ? x.Value["sysdate"] : ""))
            {
                var custom = entry.Value;
                var elements = new List<XElement>
                {
                    GetUserXML("entry_user", (custom.ContainsKey("entry_user") && !string.IsNullOrWhiteSpace(custom["entry_user"]) ? Convert.ToInt32(custom["entry_user"]) : 0)),
                    new XElement("entry_date", (custom.ContainsKey("entry_date") && !string.IsNullOrWhiteSpace(custom["entry_date"]) ? custom["entry_date"] : "")),
                    new XElement("entry_sysdate", (custom.ContainsKey("entry_sysdate") && !string.IsNullOrWhiteSpace(custom["entry_sysdate"]) ? custom["entry_sysdate"] : ""))
                };

                var actionXML = new XElement(
                    "action",
                    new XAttribute("code", custom.ContainsKey("code") ? custom["code"] : ""),
                    new XAttribute("description", custom.ContainsKey("description") ? custom["description"] : ""),
                    new XAttribute("verbiage", custom.ContainsKey("verbiage") ? custom["verbiage"] : ""),
                    new XAttribute("color", custom.ContainsKey("color") ? custom["color"] : "")
                );
                actionXML.Add(elements);
                customActions.Add(actionXML);
            }

            xml.Add(
                new XElement(
                    "actions",
                    new XAttribute("count", customActions.Count),
                    customActions
                )
            );

            var dose = (!string.IsNullOrWhiteSpace(Dose) ? Dose : "*");

            xml.Add(
                new XElement("dosage", GetDoseUnitDescription()),
                new XElement("dose", dose),
                new XElement("unit", Unit),
                new XElement("route", Route),
                new XElement("time", Time),
                new XElement("repeat", Repeat),
                new XElement("notes", Notes),
                new XElement("rate", Rate),
                new XElement("rate_unit", RateUnit),
                new XElement("schedule", GetMedTimeDescription(), new XAttribute("code", Time)),
                new XElement("type", Type),
                new XElement("product_code", ProductCode),
                new XElement("procedure_code", ProcedureCode),
                new XElement("indication", Indication)
                //TODO: get the description
                //new XElement("indication_description", IndicationDescription)
            );

            if (IsGiven() && !string.IsNullOrWhiteSpace(IVType) && !string.IsNullOrWhiteSpace(StopSysdate) && StopSysdate.Length >= 12)
            {
                var IsInjection = (IVType.Equals("Injection"));
                var stopDate = IsInjection? GiveDate : StopDate;
                var stopSysdate = IsInjection ? GiveSysdate : StopSysdate;
                var stopUser = IsInjection ? GiveUserId : StopUserId;

                // Append the last two digits of the give date value to the stop date value
                // to calculate duration because give date is 14 digits and stop date is only
                // 12 and without matching seconds in the two values, the calculated duration
                // minutes may be slightly off.
                var _t = new Time();
                var durationMins = _t.DiffMinutes(GiveDate, StopDate.Substring(0, 12) + GiveDate.Substring(12, 2));

                xml.Add(
                    GetUserXML("stop_user", stopUser),
                    new XElement("stop_date", stopDate),
                    new XElement("stop_sysdate", stopSysdate),
                    new XElement("duration", durationMins)
                );
            }

            xml.Add(
                new XElement("route_description", GetRouteDescription()),
                new XElement("time_description", GetMedTimeDescription()),
                new XElement("unit_description", GetPrintedUnitDescription())
            );

            if (IsDrug() || IsCombo())
            {
                var formularies = new List<XElement>();
                foreach(var comp in Components)
                {
                    var componentDose = comp.EnteredDose;
                    if (string.IsNullOrWhiteSpace(dose))
                    {
                        componentDose = "*";
                    }
                    var categoryData = new List<object>();
                    var subCategoryData = new List<object>();
                    if (!string.IsNullOrWhiteSpace(comp.DrugCategoryId) && !comp.DrugCategoryId.Equals("0"))
                    {
                        // TODO: Something here with categoryData and subCategoryData
                    }

                    // Get formularies containing the component.
                    // Combo meds are never on formulary, so we'll only add them on the drug level.
                    if (IsDrug())
                    {
                        var packagingId = comp.PackagingId;
                        if (formulary.IsInpatient(packagingId))
                        {
                            formularies.Add(
                                new XElement(
                                    "formulary",
                                    new XAttribute("code", "i")
                                )
                            );
                            formularies.Last().SetValue("inpatient");
                        }
                        if (formulary.IsOutpatient(packagingId))
                        {
                            formularies.Add(
                               new XElement(
                                   "formulary",
                                   new XAttribute("code", "o")
                               )
                           );
                            formularies.Last().SetValue("outpatient");
                        }
                        if (formulary.IsMachine(packagingId))
                        {
                            formularies.Add(
                                new XElement(
                                    "formulary",
                                    new XAttribute("code", "m")
                                )
                            );
                            formularies.Last().SetValue("machine");
                        }
                    }

                    var component = new List<XElement>
                    {
                        new XElement("name",                 comp.GetFullName(this)),
                        new XElement("dosage",               comp.GetDoseUnitDescription()),
                        new XElement("dose",                 componentDose),
                        new XElement("dose_form",            comp.DrugForm),
                        new XElement("dose_form_code",       comp.DrugFormId),
                        new XElement("unit",                 comp.EnteredUnit),
                        // TODO: main_drug_code_mnemonic
                        // TODO: drug_cat
                        // TODO: drug_subcat
                        new XElement("active_ingredient_id", comp.ActiveId),
                        new XElement("frmcode",              formulary.GetHospIdByNDC(comp.PackagingId)),
                        new XElement("formulation_id",       comp.DrugId),
                        new XElement("unit_description",     comp.GetEnteredUnitPrintedDescription()),
                        new XElement("product_code",         comp.ProductCode),
                        new XElement("procedure_code",       comp.ProcedureCode),
                        new XElement("type",                 comp.Type)
                    };

                    xml.Add(
                        new XElement(
                            "component",
                            new XAttribute("id", comp.Id),
                            component
                        )
                    );
                }

                xml.Add(
                    new XElement(
                        "formularies",
                        formularies
                    )
                );
            }

            return xml;
        }

        /// <summary>
        /// Load a component into this Medication object
        /// </summary>
        /// <param name="comp">Medication.Component object</param>
        public void LoadComponent(Medication.Component comp)
        {
            Components.Add(comp);
        }

        /// <summary>
        /// Load custom actions available for this medication
        /// </summary>
        private void LoadCustomActions()
        {
            CustomActions.Clear();
            var res = new DB.Select
            {
                Sql = "SELECT * FROM med_actions WHERE ibex=@ibex AND site=@site AND losecs=@losecs ORDER BY sysdate",
                Parameters = new SqlParameter[]
                {
                    new SqlParameter("@ibex", SqlDbType.Char) { Value = Ibex },
                    new SqlParameter("@site", SqlDbType.TinyInt) { Value = SiteId },
                    new SqlParameter("@losecs", SqlDbType.Int) { Value = Losecs }
                }
            }.RunForListOfDictionaries();

            foreach (var action in res)
            {
                action["name"] = "c_" + action["code"];
                CustomActions[action["code"]] = action;
            }

            return;
        }

        /// <summary>
        /// Make a new Medication by loading from the medication 'group'. Either a single component medication or a combo medication is created based on the group 'type' definition.
        /// </summary>
        /// <param name="user">User object</param>
        /// <param name="ibex">Patient identifier</param>
        /// <param name="siteId">Site identifier</param>
        /// <param name="groupId">Group identifier</param>
        /// <param name="info">Dictionary of items to be populated in the object on creation</param>
        /// <returns>New Medication object</returns>
        public static Medication LoadFromGroup(User user, string ibex, byte siteId, int groupId, Dictionary<string, string> info)
        {
            var med = new Medication(siteId);
            med.Ibex = ibex;
            var groupData = new List<Dictionary<string, string>>();
            var sel = new DB.Select
            {
                Sql = "SELECT * FROM grp WHERE id=@groupId",
                Parameters = new SqlParameter[] {
                    new SqlParameter("@groupId", SqlDbType.Int) { Value = groupId }
                }
            }.RunForDictionary();

            if (sel.ContainsKey("type"))
            {
                var type = sel["type"];
                if (type.Equals("M"))
                {
                    groupData.Add(new Dictionary<string, string> {
                        { "id", groupId.ToString() },
                    });
                    med.Name = sel["name"]?.ToString();
                    med.Type = Constants.TYPE_MEDICATION;
                }
                else if (type.Equals("X"))
                {
                    med.Name = sel["name"]?.ToString();
                    var res = new DB.Select
                    {
                        Sql = "SELECT id, name, type, qcpr_product_code, qcpr_procedure_code FROM grp LEFT JOIN QCPR_combo_med qcm ON qcm.cde_num = grp.num WHERE type IN ('M','A','B') AND num=@num AND site=@site ORDER BY name",
                        Parameters = new SqlParameter[]
                        {
                            new SqlParameter("@num", SqlDbType.Int) { Value = sel["code"] },
                            new SqlParameter("@site", SqlDbType.TinyInt) { Value = siteId }
                        }
                    }.RunForListOfDictionaries();
                    foreach (var comp in res)
                    {
                        groupData.Add(comp);
                        med.ProcedureCode = Convert.ToInt32(string.IsNullOrWhiteSpace(comp["qcpr_procedure_code"].ToString()) ? "0" : comp["qcpr_procedure_code"]);
                        med.ProductCode = Convert.ToInt32(string.IsNullOrWhiteSpace(comp["qcpr_product_code"].ToString()) ? "0" : comp["qcpr_product_code"]);
                    }
                    med.Type = Constants.TYPE_COMBO;
                }
                else if (type.Equals("V"))
                {
                    var res = new DB.Select
                    {
                        Sql = "SELECT id, name, type FROM grp WHERE type IN('A','B') AND num=@num AND site=@site ORDER BY name",
                        Parameters = new SqlParameter[]
                        {
                            new SqlParameter("@num", SqlDbType.Int) { Value = sel["code"] },
                            new SqlParameter("@site", SqlDbType.TinyInt) { Value = siteId }
                        }
                    }.RunForListOfDictionaries();
                    foreach (var comp in res)
                    {
                        groupData.Add(comp);
                    }
                    med.Type = Constants.TYPE_IV;
                    med.Name = sel["name"]?.ToString();
                }
                else if (type.Equals("B"))
                {
                    med.Name = sel["name"];
                    med.Type = Constants.TYPE_IV_BASE;
                    groupData.Add(new Dictionary<string, string> {
                        { "id", groupId.ToString() },
                    });
                }

                foreach (var data in groupData)
                {
                    var component = new Medication.Component();
                    component.LoadFromGroup(siteId, Convert.ToInt32(data["id"]), type);
                    component.GroupType = data.ContainsKey("type") ? data["type"] : type;
                    component.GroupName = data.ContainsKey("name") ? data["name"] : med.Name;
                    med.LoadComponent(component);
                }

                // Parameters supplied when a medication is actually ordered.
                med.Dose = info.ContainsKey("dose") && !string.IsNullOrWhiteSpace(info["dose"]) ? info["dose"] : "*";
                med.Unit = info.ContainsKey("unit") ? info["unit"] : "";
                med.Route = info.ContainsKey("route") ? info["route"] : "";
                med.Time = info.ContainsKey("time") ? info["time"] : "";
                med.Notes = info.ContainsKey("notes") ? info["notes"] : "";
                med.Repeat = info.ContainsKey("repeat") ? info["repeat"] : "";
                med.Authentication = info.ContainsKey("authentication") ? info["authentication"] : "";
            }

            return med;
        }

        /// <summary>
        /// Make a new Medication by loading the user's quicklist
        /// </summary>
        /// <param name="user">User object</param>
        /// <param name="ibex">Patient identifier</param>
        /// <param name="site">ISite instance</param>
        /// <param name="qlId">Id of med in rxl table</param>
        /// <param name="db">Optional DrugDB instance</param>
        /// <returns>Dictionary of quicklist information</returns>
        public static Dictionary<string, string> LoadFromQuickList(User user, string ibex, ISite site, int qlId, DrugDB db = null)
        {
            if (db == null)
                db = new DrugDB(site);

            return db.LoadQuickListEntry(ibex, user.Id, qlId);
        }

        /// <summary>
        /// Load a medication group for a patient
        /// </summary>
        /// <param name="user">User object</param>
        /// <param name="ibex">Patient identifier</param>
        /// <param name="groupNum">Group number/identifier</param>
        /// <returns>Dictionary of Medication objects</returns>
        public Dictionary<string, Medication> LoadGroup(User user, string ibex, int groupNum)
        {
            Dictionary<string, Medication> GroupData = new Dictionary<string, Medication>();
            List<string> PrimaryDrugIds = new List<string>();
            Dictionary<string, List<Component>> ComponentPrimaryDrugIdMappings = new Dictionary<string, List<Component>>();

            var connection = DB.GetConnectionString();
            using (var con = new SqlConnection(connection))
            {
                con.Open();
                var orgInfo = new DB.Select
                {
                    Connection = con,
                    Sql = "SELECT svccs FROM org WHERE site=@site",
                    Parameters = new SqlParameter[]
                    {
                        new SqlParameter("@site", SqlDbType.TinyInt) { Value = user.SiteId }
                    }
                }.RunForDataRow();

                var site = Convert.ToByte(orgInfo["svccs"].ToString());

                using (SqlCommand cmd = new SqlCommand("[dbo].[pc_meds_load_group_data]", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@grp_num", SqlDbType.Int).Value = groupNum;
                    cmd.Parameters.Add("@site", SqlDbType.Int).Value = site;

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Medication med = null;
                            var id = reader["id"].ToString();
                            if (GroupData.ContainsKey(id))
                            {
                                med = GroupData[id];
                            }
                            else
                            {
                                med = new Medication(user);
                                DrugDB = med.DrugDB;
                                med.Ibex = ibex;
                                med.Id = Convert.ToInt32(reader["id"].ToString().Trim());
                                med.Site = user.SiteId;
                                med.DefaultDose = reader["medication_dose"]?.ToString().Trim();
                                med.DefaultUnit = reader["medication_unit"]?.ToString().Trim();
                                med.DefaultRoute = reader["medication_route"]?.ToString().Trim();
                                med.Route = reader["medication_route"]?.ToString().Trim();
                                med.Name = reader["medication_name"]?.ToString().Trim();
                                med.Notes = reader["medication_notes"]?.ToString().Trim();
                                med.Rate = reader["medication_dose"]?.ToString().Trim();
                                med.RateUnit = reader["medication_unit"]?.ToString().Trim();
                                med.Unit = reader["medication_unit"]?.ToString().Trim();
                                med.Code = reader["medication_primary_drug_id"]?.ToString().Trim();
                            }

                            var comp = new Component(site, med.DrugDB.DBType);
                            var compName = (!String.IsNullOrEmpty(reader["component_name"]?.ToString()) ?
                                reader["component_name"].ToString() : reader["medication_name"].ToString()).Split(new string[] { " : " }, StringSplitOptions.None)[0];
                            comp.BrandName = compName.Trim();
                            comp.GroupType = reader["component_type"]?.ToString().Trim();
                            comp.EnteredDose = reader["component_dose"]?.ToString().Trim();
                            comp.EnteredUnit = reader["component_unit"]?.ToString().Trim();
                            comp.ProductCode = reader["product_code"]?.ToString().Trim();
                            comp.ProcedureCode = reader["procedure_code"]?.ToString().Trim();

                            med.Components.Add(comp);

                            var medType = reader["medication_type"]?.ToString().Trim();
                            if (!medType.Equals("V") && !medType.Equals("I"))
                            {
                                med.Type = medType;
                                PrimaryDrugIds.Add(med.Code);
                                if (!ComponentPrimaryDrugIdMappings.ContainsKey(med.Code))
                                {
                                    ComponentPrimaryDrugIdMappings.Add(med.Code, new List<Component>());
                                }

                                // Non-combo, non-IV meds will have a medication with a single component
                                ComponentPrimaryDrugIdMappings[med.Code].Add(comp);
                            }
                            else if (medType.Equals("V"))
                            {
                                med.Type = Constants.TYPE_IV;
                            }
                            else
                            {
                                med.Type = Constants.TYPE_COMBO;
                            }

                            var componentPrimaryDrugId = reader["component_primary_drug_id"]?.ToString().Trim();
                            if (!String.IsNullOrWhiteSpace(componentPrimaryDrugId) && !componentPrimaryDrugId.Equals("0"))
                            {
                                PrimaryDrugIds.Add(componentPrimaryDrugId);
                                if (!ComponentPrimaryDrugIdMappings.ContainsKey(componentPrimaryDrugId))
                                    ComponentPrimaryDrugIdMappings.Add(componentPrimaryDrugId, new List<Component>());

                                ComponentPrimaryDrugIdMappings[componentPrimaryDrugId].Add(comp);
                            }

                            GroupData[id] = med;
                        }
                        reader.Close();

                        var drugInfo = DrugDB.GetInstance().GetDrugInfoByNDCs(PrimaryDrugIds);
                        foreach(var info in drugInfo)
                        {
                            var ndc = info["ndc"];
                            if (ComponentPrimaryDrugIdMappings.ContainsKey(ndc))
                            {
                                if (info.ContainsKey("component_name") && !String.IsNullOrWhiteSpace(info["component_name"]))
                                {
                                    var name = (info["component_name"].Split(new string[] { " : " }, StringSplitOptions.None))[0];
                                    info["brand_name"] = name.Trim();
                                }
                                foreach(var comp in ComponentPrimaryDrugIdMappings[ndc])
                                {
                                    comp.SetDrugInfo(info);
                                }
                            }
                        }
                    }

                    con.Close();
                }
            }

            return GroupData;
        }

        private MinimalUser MedUser(Dictionary<int?, User> userDictionary, int? userId)
        {
            if (userId != null && userId > 0)
            {
                var user = (userDictionary.ContainsKey(userId) ? userDictionary[userId] : null);
                if (user != null)
                {
                    return user.ToMinimalUser();
                }
            }

            return null;
        }

        /// <summary>
        /// Get the description for dose and unit of the Medication
        /// </summary>
        /// <returns>Dose and unit description string</returns>
        public string GetDoseUnitDescription()
        {
            var doseUnit = !string.IsNullOrWhiteSpace(Dose) ? Dose : DefaultDose;
            if (string.IsNullOrWhiteSpace(doseUnit))
            {
                doseUnit = "*";
            }
            var unit = !string.IsNullOrWhiteSpace(Unit) ? Unit : DefaultUnit;
            if (!string.IsNullOrWhiteSpace(unit))
            {
                doseUnit += " " + (!string.IsNullOrWhiteSpace(PrintedUnit) ? GetPrintedUnitDescription() : GetUnitDescription());
            }

            return doseUnit;
        }

        /// <summary>
        /// Get the medication time description
        /// </summary>
        /// <returns>The display of the medication time</returns>
        public string GetMedTimeDescription()
        {
            return GetDescription("BS", Time);
        }

        /// <summary>
        /// Get the rate unit description display
        /// </summary>
        /// <returns>The display of the rate unit</returns>
        public string GetRateUnitDescription()
        {
            return GetDescription("BE", RateUnit);
        }

        /// <summary>
        /// Get the printed route display
        /// </summary>
        /// <returns>The display of the route (for printing)</returns>
        public string GetRouteDescription()
        {
            return GetDescription("AC", (!string.IsNullOrWhiteSpace(Route) ? Route : DefaultRoute));
        }

        /// <summary>
        /// Get the full name of this medication
        /// </summary>
        /// <param name="noDose">Optional flag to hide dose information in the name</param>
        /// <returns>Full name string, without Dose: and Route: labels</returns>
        public string GetFullName(bool noDose = true, bool noRoute = true)
        {
            var name = GetName();
            var dose = GetDoseUnitDescription();
            var route = GetRouteDescription();
            var strengthForm = " ";
            if (IsFreeText())
            {
                strengthForm = "";
                noDose = true;
            }
            if (!IsCombo() && !IsFreeText())
            {
                var comp = Components.Count > 0 ? Components.First() : null;
                if (comp != null)
                {
                    strengthForm += ShowDrugForm ? comp.DrugForm : "";
                    strengthForm += ShowDrugStrength ? " " + comp.DrugStrength : "";
                    if (!string.IsNullOrWhiteSpace(strengthForm))
                    {
                        strengthForm = " " + strengthForm.Trim();
                    }
                }
            }

            // TODO: I don't like this bolding, and we should look into it
            return (name + strengthForm + (noDose ? "" : (!string.IsNullOrWhiteSpace(dose) ? "<b>Dose</b>: " + dose : "")) + " " +  (!string.IsNullOrWhiteSpace(route) ? (noRoute ? "" : "<b>Route</b>: " ) + route : "")).Trim();
        }

        /// <summary>
        /// Get the full name of the medication, suitable for adding to the patient's chart
        /// </summary>
        /// <returns>Full name string</returns>
        public string GetFullNameForChart()
        {
            return GetFullName(false, false);
        }

        /// <summary>
        /// Get the Medication Services status for this med
        /// </summary>
        /// <returns>Dictionary of Medication Services status information</returns>
        public Dictionary<string, string> GetMedStatus()
        {
            var status = new Dictionary<string, string>();
            var lookupList = MedicationActions.Constants.SORTED_STATUSES;
            lookupList.Reverse();
            foreach (var lookup in lookupList)
            {
                if (lookup.ContainsKey("for") && !string.IsNullOrWhiteSpace(lookup["for"]) && !lookup["for"].Equals("status"))
                    continue;

                if (lookup.ContainsKey("code") && !string.IsNullOrWhiteSpace(lookup["code"]))
                {
                    var code = lookup["code"];
                    var name = MedicationActions.Constants.NAME.ContainsKey(code) ? MedicationActions.Constants.NAME[code] : null;
                    if (name != null)
                    {
                        var field = (name.Equals("del") ? "delete" : name);
                        var date = "";
                        var sysdate = "";
                        int? usr = 0;
                        switch(field)
                        {
                            case "order":
                                date = OrderDate;
                                usr = OrderUserId;
                                break;
                            case "ack":
                                date = AckDate;
                                usr = AckUserId;
                                break;
                            case "hold":
                                date = HoldDate;
                                sysdate = HoldSysdate;
                                usr = HoldUserId;
                                break;
                            case "unhold":
                                date = UnholdDate;
                                sysdate = UnholdSysdate;
                                usr = UnholdUserId;
                                break;
                            case "give":
                                date = GiveDate;
                                sysdate = GiveSysdate;
                                usr = GiveUserId;
                                break;
                            case "cancel":
                                date = CancelDate;
                                sysdate = CancelSysdate;
                                usr = CancelUserId;
                                break;
                            case "delete":
                                date = DeleteDate;
                                usr = DeleteUserId;
                                break;
                            case "discontinue":
                                date = DiscontinueDate;
                                sysdate = DiscontinueSysdate;
                                usr = DiscontinueUserId;
                                break;
                            case "discontinued":
                                date = DiscontinuedDate;
                                sysdate = DiscontinuedSysdate;
                                usr = DiscontinuedUserId;
                                break;
                            default:
                                break;
                        }

                        if (usr != 0 && usr != null)
                        {
                            var verbiage = MedicationActions.Constants.VERBIAGE.ContainsKey(code) ? MedicationActions.Constants.VERBIAGE[code] : null;
                            var cls = verbiage.ToLowerInvariant().Replace(' ', '_');
                            var description = MedicationActions.Constants.DESCRIPTION.ContainsKey(code) ? MedicationActions.Constants.DESCRIPTION[code] : null;
                            status["entry_date"] = date;
                            status["entry_user"] = usr.ToString();
                            status["name"] = name;
                            status["description"] = description;
                            status["verbiage"] = verbiage;
                            status["class"] = cls;
                            status["sysdate"] = sysdate;
                            status["code"] = code;
                            status["color"] = lookup["color"];
                        }
                    }
                }
                else if (lookup.ContainsKey("color") && !string.IsNullOrWhiteSpace(lookup["color"]))
                {
                    var color = lookup["color"];
                    var action = GetCustomActionByColor(color);
                    if (action.Keys.Count > 0)
                    {
                        status = action;
                        status["class"] = color.Equals(MedicationActions.Constants.ORANGE) ? "status_orange" : "status_purple";
                        status["custom"] = "1";
                    }
                }
                else
                {
                    status["code"] = "";
                    status["color"] = "";
                }

                if (status.Keys.Count > 0)
                    break;
            }

            return status;
        }

        /// <summary>
        /// Get the name of this medication
        /// </summary>
        /// <returns>Medication name</returns>
        public string GetName()
        {
            if (IsCombo() || IsFreeText())
            {
                return Name;
            } else
            {
                return Components.First().BrandName;
            }
        }

        /// <summary>
        /// Get the printed unit display for the Medication
        /// </summary>
        /// <returns>Printed unit display string</returns>
        public string GetPrintedUnitDescription()
        {
            return GetPrintedDescription("BE", (!string.IsNullOrWhiteSpace(Unit) ? Unit : DefaultUnit));
        }

        /// <summary>
        /// Get the unit display for the Medication
        /// </summary>
        /// <returns>Unit display string</returns>
        public string GetUnitDescription()
        {
            return GetDescription("BE", (!string.IsNullOrWhiteSpace(Unit) ? Unit : DefaultUnit));
        }

        /// <summary>
        /// Determine if the med has been acknowledged
        /// </summary>
        /// <returns>A boolean indicating if the medication has been acknowledged</returns>
        public bool IsAcknowledged()
        {
            return (
                !String.IsNullOrWhiteSpace(AckDate) &&
                !IsOnDiscontinue() &&
                PulseCheck.Utilities.Time.DateTimeFromString(AckDate) > PulseCheck.Utilities.Time.DateTimeFromString(DiscontinuedDate)
            );
        }

        /// <summary>
        /// Determine if the drug is active
        /// </summary>
        /// <returns>A boolean indicating if the drug is active</returns>
        public bool IsActive()
        {
            return Status.Equals(Constants.ACTIVE);
        }

        /// <summary>
        /// Determine if the drug has been cancelled
        /// </summary>
        /// <returns>A boolean indicating if the drug has been cancelled</returns>
        public bool IsCancelled()
        {
            return !IsActive() && !String.IsNullOrWhiteSpace(CancelDate);
        }

        /// <summary>
        /// Determine if the drug is a combo drug (contains more than one Component)
        /// </summary>
        /// <returns>A boolean indicating if the drug is a combo drug</returns>
        public bool IsCombo()
        {
            return Type.Equals(Medication.Constants.TYPE_IV) || Components.Count > 1;
        }

        /// <summary>
        /// Determine if the drug has been deleted
        /// </summary>
        /// <returns>A boolean indiciating if the drug was deleted</returns>
        public bool IsDeleted()
        {
            return !IsActive() && !String.IsNullOrWhiteSpace(DeleteDate);
        }

        /// <summary>
        /// Determine if the drug is discontinued
        /// </summary>
        /// <returns>A boolean indicating if the drug has been discontinued</returns>
        public bool IsDiscontinued()
        {
            return (
                IsActive() &&
                !IsCancelled() &&
                !IsDeleted() &&
                PulseCheck.Utilities.Time.DateTimeFromString(DiscontinuedDate) > PulseCheck.Utilities.Time.DateTimeFromString(DiscontinueDate)
            );
        }

        /// <summary>
        /// Determine if the drug is a single component drug
        /// </summary>
        /// <returns>A boolean indicating if the drug is single component</returns>
        public bool IsDrug()
        {
            return Components.Count == 1;
        }

        /// <summary>
        /// Determine if the drug is a free text drug order (contains zero components)
        /// </summary>
        /// <returns>A boolean indicating if the drug is free text</returns>
        public bool IsFreeText()
        {
            return Components.Count == 0;
        }

        /// <summary>
        /// Determine if the drug has been given
        /// </summary>
        /// <returns>A boolean indicating if the drug has been given</returns>
        public bool IsGiven()
        {
            var giveDate = PulseCheck.Utilities.Time.DateTimeFromString(GiveDate);
            return (
                !String.IsNullOrWhiteSpace(GiveDate) &&
                giveDate > PulseCheck.Utilities.Time.DateTimeFromString(DiscontinueDate) &&
                giveDate > PulseCheck.Utilities.Time.DateTimeFromString(DiscontinuedDate)
            );
        }

        /// <summary>
        /// Determine if the drug is on discontinue
        /// </summary>
        /// <returns>A boolean indicating if the drug is currently on discontinue</returns>
        public bool IsOnDiscontinue()
        {
            return (
                IsActive() &&
                !IsCancelled() &&
                !IsDeleted() &&
                !String.IsNullOrWhiteSpace(DiscontinueDate) &&
                String.IsNullOrWhiteSpace(DiscontinuedDate)
            );
        }

        /// <summary>
        /// Determine if the drug is on hold
        /// </summary>
        /// <returns>A boolean indicating if the drug is currently on hold</returns>
        public bool IsOnHold()
        {
            var holdDate = PulseCheck.Utilities.Time.DateTimeFromString(HoldDate);
            return (
                IsActive() &&
                !IsGiven() &&
                holdDate > PulseCheck.Utilities.Time.DateTimeFromString(UnholdDate) &&
                !IsOnDiscontinue() &&
                holdDate > PulseCheck.Utilities.Time.DateTimeFromString(DiscontinuedDate)
            );
        }

        /// <summary>
        /// Set an object attribute
        /// </summary>
        /// <param name="key">Attribute key</param>
        /// <param name="value">Attribute value</param>
        public void set(string key, object value)
        {
            if (!DrugDB.Constants.rxl_obj_map.ContainsKey(key))
            {
                return;
            }

            var mapKey = DrugDB.Constants.rxl_obj_map[key];
            switch(mapKey)
            {
                case "ndc":
                    break;
                case "dose":
                    Dose = value.ToString();
                    break;
                case "unit":
                    Unit = value.ToString();
                    break;
                case "route":
                    Route = value.ToString();
                    break;
                case "med_notes":
                    Notes = value.ToString();
                    break;
                case "schedule":
                    Schedule = value.ToString();
                    break;
                case "med_repeat":
                    Repeat = value.ToString();
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Get a description for a drug detail (route, unit, etc.)
        /// </summary>
        /// <param name="idxType">The idx type</param>
        /// <param name="id">The id of the idx entry</param>
        /// <returns></returns>
        protected static string GetDescription(string idxType, string id)
        {
            var description = "";
            if (IdxInfo.ContainsKey(idxType) && IdxInfo[idxType].ContainsKey(id))
            {
                description = IdxInfo[idxType][id];
            }

            return !string.IsNullOrWhiteSpace(description) ? description : id;
        }

        /// <summary>
        /// Get a printed description for a route, unit, etc.
        /// </summary>
        /// <param name="idxType">The idx type</param>
        /// <param name="id">The id for the idx entry</param>
        /// <returns></returns>
        protected static string GetPrintedDescription(string idxType, string id)
        {
            var printed = "";
            if (IdxInfoPrinted.ContainsKey(idxType) && IdxInfoPrinted[idxType].ContainsKey(id))
            {
                printed = IdxInfoPrinted[idxType][id];
            }
            var standard = "";
            if (IdxInfo.ContainsKey(idxType) && IdxInfo[idxType].ContainsKey(id))
            {
                standard = IdxInfo[idxType][id];
            }

            return !string.IsNullOrWhiteSpace(printed) ? printed :
                   !string.IsNullOrWhiteSpace(standard) ? standard :
                   id;
        }

        /// <summary>
        /// Get an XElement with action details
        /// </summary>
        /// <param name="actionName">Action name</param>
        /// <param name="userId">ID of user that performed the action</param>
        /// <param name="actionDate">User-entered date (YYYYMMDDHHmm(ss)?) of action</param>
        /// <param name="actionSysdate">System date (YYYYMMDDHHmmss) of action</param>
        /// <returns>XElement with action details</returns>
        private List<XElement> GetActionXML(string actionName, int? userId, string actionDate, string actionSysdate = null)
        {
            var userNodeName = actionName + "_user";
            var dateNodeName = actionName + "_date";
            var sysdateNodeName = actionName + "_sysdate";

            if (!string.IsNullOrWhiteSpace(actionDate) && actionDate.Trim().Length >= 12)
            {
                return new List<XElement>
                {
                    GetUserXML(userNodeName, userId),
                    new XElement(dateNodeName, actionDate),
                    new XElement(sysdateNodeName, (!string.IsNullOrWhiteSpace(actionSysdate) ? actionSysdate : actionDate))
                };
            }
            else
            {
                return new List<XElement>
                {
                    GetUserXML(userNodeName, null),
                    new XElement(dateNodeName, ""),
                    new XElement(sysdateNodeName, "")
                };
            }
        }

        /// <summary>
        /// Get an XElement with information for the specified user
        /// </summary>
        /// <param name="nodeName">XML node name</param>
        /// <param name="userId">User ID</param>
        /// <returns>XElement with user information</returns>
        private XElement GetUserXML(string nodeName, int? userId)
        {
            if (userId == null)
            {
                userId = 0;
            }

            if (!StaffData.ContainsKey(userId))
            {
                StaffData[userId] = new DB.Select
                {
                    Sql = "SELECT last, first, num, hospid, npi, init, mln AS medical_license, dea, hospid2 AS Second_ID, type, ordonly FROM drs WHERE num = @num",
                    Parameters = new SqlParameter[]
                    {
                        new SqlParameter("@num", SqlDbType.Int) { Value = userId }
                    }
                }.RunForDictionary();
            }

            var data = StaffData[userId];
            var xml = new XElement(
                nodeName
            );

            foreach (var k in data.Keys)
            {
                xml.Add(new XAttribute(
                    k, data[k]
                ));
            }

            xml.SetValue(data["last"] + ", " + data["first"]);

            return xml;
        }

        /// <summary>
        /// Clone this Medication
        /// </summary>
        /// <returns>Medication clone</returns>
        public Medication Clone()
        {
            return (Medication)this.MemberwiseClone();
        }

        object ICloneable.Clone()
        {
            return Clone();
        }

        /// <summary>
        /// Medication Component class
        /// </summary>
        public class Component : IComponent
        {
            [Key]
            public int Id { get; set; }

            public virtual Medication Medication { get; set; }

            public string Ibex { get; set; }
            public Int16 Site { get; set; }
            public int Losecs { get; set; }

            public string BrandName { get; set; }
            public string ActiveName { get; set; }
            public string DrugRoute { get; set; }
            public string DrugForm { get; set; }
            public string DrugStrength { get; set; }

            private string  _enteredDose { get; set; }
            public string EnteredDose
            {
                get { return this._enteredDose != null ? this._enteredDose.Trim() : "";  }
                set { this._enteredDose = value?.Trim() ?? ""; }
            }

            private string _enteredUnit { get; set; }
            public string EnteredUnit {
                get { return this._enteredUnit != null ? this._enteredUnit.Trim() : ""; }
                set { this._enteredUnit = value?.Trim() ?? ""; }
            }

            public string DrugDBType { get; set; } = "";
            public string ActiveId { get; set; }
            public string DrugId { get; set; }
            public string PackagingId { get; set; }
            public string DrugCategoryId { get; set; }
            public string Type { get; set; } = Constants.TYPE_DRUG;
            public string DrugFormId { get; set; }

            [NotMapped]
            public string GroupName { get; set; }

            [NotMapped]
            public string GroupType { get; set; }

            [NotMapped]
            public string ProductCode { get; set; }

            [NotMapped]
            public string ProcedureCode { get; set; }

            [NotMapped]
            public List<Dictionary<string, string>> Interactions { get; set; } = new List<Dictionary<string, string>>();

            [NotMapped]
            public List<Dictionary<string, string>> Reactions { get; set; } = new List<Dictionary<string, string>>();

            private bool PrintedUnit { get; set; } = false;

            /// <summary>
            /// Default Medication Component constructor
            /// </summary>
            public Component()
            {

            }

            /// <summary>
            /// Get the brand name of the component medication
            /// </summary>
            /// <returns>Brand name string</returns>
            public string GetBrandName()
            {
                return BrandName;
            }

            /// <summary>
            /// Medication Component constructor with site and drug database type
            /// </summary>
            /// <param name="siteId">Site identifier</param>
            /// <param name="drugDBType">Drug DB type identifier</param>
            public Component(byte siteId, string drugDBType)
            {
                Site = siteId;
                DrugDBType = drugDBType;
            }

            /// <summary>
            /// Internal lookup of drug information regarding the current drug
            /// </summary>
            /// <param name="siteId">Site identifier</param>
            /// <param name="drugId">Drug identifier</param>
            /// <param name="packagingId">Packaging ID</param>
            /// <param name="noEquivalent">Flag for whether this drug has an equivalent</param>
            private void DrugLookup(byte siteId, string drugId, string packagingId = null, bool noEquivalent = true)
            {
                var res = new Dictionary<string, string>();
                var medication = new Medication(siteId);
                if (!string.IsNullOrWhiteSpace(packagingId) && !packagingId.Equals("0"))
                {
                    res = medication.GetDrugInfoFromNDC(packagingId);
                    if ((string.IsNullOrWhiteSpace(res["ndc"]) || res["ndc"].Equals("0")) && !noEquivalent)
                    {
                        // If the drug existed at one time, get the Multum ID and see if any equivalents are available
                        var equivalents = medication.DrugDB.GetInstance().GetDrugInfoByFormulationId(drugId);
                        if (equivalents != null && equivalents.Count > 0)
                        {
                            var equiv = equivalents[0];
                            if (equiv.ContainsKey("ndc") && !string.IsNullOrWhiteSpace(equiv["ndc"]) && !equiv["ndc"].Equals("0"))
                                equiv["equivalent"] = "1";

                            res = equiv;
                        } else
                        {
                            res = new Dictionary<string, string>();
                        }
                    }
                } else if (!string.IsNullOrWhiteSpace(drugId) && !drugId.Equals("0"))
                {
                    // Attempt to select a drug based on the Multum ID. This will grab the first returned name.
                    var info = medication.DrugDB.GetInstance().GetDrugInfoByFormulationId(drugId);
                    if (info != null && info.Count > 0)
                        res = info[0];
                }

                SetDrugInfo(res);
            }

            /// <summary>
            /// Get the medication component dose unit description
            /// </summary>
            /// <returns>Dose unit description string</returns>
            public string GetDoseUnitDescription()
            {
                var doseUnit = EnteredDose;
                if (string.IsNullOrWhiteSpace(doseUnit))
                {
                    doseUnit = "*";
                }
                if (!string.IsNullOrWhiteSpace(doseUnit) && !string.IsNullOrWhiteSpace(EnteredUnit))
                {
                    doseUnit += " " + (PrintedUnit ? GetEnteredUnitPrintedDescription() : GetEnteredUnitDescription());
                }

                return doseUnit;
            }

            /// <summary>
            /// Get the medication component's unit description
            /// </summary>
            /// <returns>Unit description string</returns>
            public string GetEnteredUnitDescription()
            {
                return GetDescription("BE", EnteredUnit);
            }

            /// <summary>
            /// Get the medication component's entered unit printed description
            /// </summary>
            /// <returns>Entered unit printed description string</returns>
            public string GetEnteredUnitPrintedDescription()
            {
                return GetDescription("BE", EnteredUnit);
            }

            /// <summary>
            /// Get the full name of the component medication
            /// </summary>
            /// <param name="parentMed">Parent/top-level medication object for this component</param>
            /// <returns>Full name string</returns>
            public string GetFullName(Medication parentMed)
            {
                if (parentMed.IsCombo())
                    return GetName() + " [" + GetDoseUnitDescription() + "]";

                var name = GetName();
                var strength = parentMed.ShowDrugStrength ? DrugStrength : "";
                var drugForm = parentMed.ShowDrugForm ? DrugForm : "";
                var route = GetRouteDescription();

                if (!string.IsNullOrWhiteSpace(strength) || !string.IsNullOrWhiteSpace(drugForm))
                {
                    name += " :";
                    if (!string.IsNullOrWhiteSpace(strength))
                    {
                        name += " " + strength;
                    }
                    if (!string.IsNullOrWhiteSpace(drugForm))
                    {
                        name += " " + drugForm;
                    }
                }

                if (!string.IsNullOrWhiteSpace(route))
                    name += " : " + route;

                name = System.Text.RegularExpressions.Regex.Replace(name, "( : ){2,}", " : ");

                return name;
            }

            /// <summary>
            /// Get the field name that a provided name maps to, through the Multum object mapping definition
            /// </summary>
            /// <param name="fld">Field name</param>
            /// <returns>Mapped field name</returns>
            private string GetMappedMultumField(string fld)
            {
                if (Medication.Constants.MULTUM_OBJ_MAP.ContainsKey(fld))
                {
                    return Medication.Constants.MULTUM_OBJ_MAP[fld];
                }
                return fld;
            }

            /// <summary>
            /// Get the drug name portion of the name
            /// </summary>
            /// <returns>String name of drug</returns>
            public string GetName()
            {
                if (BrandName.Equals(ActiveName) || String.IsNullOrWhiteSpace(ActiveName))
                {
                    return BrandName;
                } else
                {
                    return BrandName + " (" + ActiveName + ")";
                }
            }

            /// <summary>
            /// Get the medication component's route description
            /// </summary>
            /// <returns>Route description string</returns>
            public string GetRouteDescription()
            {
                return GetDescription("AC", DrugRoute);
            }

            /// <summary>
            /// Create a Medication Component from a group identifier
            /// </summary>
            /// <param name="siteId">Site identifier</param>
            /// <param name="groupId">Group identifier</param>
            /// <param name="type">Componentn/group type</param>
            public void LoadFromGroup(byte siteId, int groupId, string type)
            {
                var med = new DB.Select
                {
                    Sql = "[dbo].[pc_meds_load_group]",
                    IsStoredProcedure = true,
                    Parameters = new SqlParameter[]
                    {
                        new SqlParameter("@group_id", SqlDbType.Int) { Value = groupId }
                    }
                }.RunForDataRow();

                DrugId = med["code"].ToString().Trim();
                DrugLookup(siteId, null, DrugId);

                Type = med["type"].ToString();

                if (!string.IsNullOrWhiteSpace(med["product_code"].ToString()) && !med["product_code"].ToString().Equals("0"))
                {
                    ProductCode = med["product_code"].ToString();
                    ProcedureCode = med["procedure_code"].ToString();
                }

                if (type.Equals("X") || type.Equals("V"))
                {
                    EnteredDose = !string.IsNullOrWhiteSpace(med["dose"].ToString()) ? "*" : med["dose"].ToString();
                    EnteredUnit = med["unit"].ToString();
                }

                BrandName = med["name"].ToString().Split(new string[] { " : " }, StringSplitOptions.None)[0];
            }

            /// <summary>
            /// Populate a Medication.Component object with information using a packaging id (ndc code in multum)
            /// </summary>
            /// <param name="id">Packaging id</param>
            public void LoadFromPackagingId(string id)
            {
                DrugLookup(Convert.ToByte(Site), null, id);
            }

            /// <summary>
            /// Set the drug information for a Component
            /// </summary>
            /// <param name="drugInfo">Dictionary of drug information</param>
            public Dictionary<string, string> SetDrugInfo(Dictionary<string, string> drugInfo)
            {
                var packagingId = drugInfo.ContainsKey("ndc") && !String.IsNullOrEmpty(drugInfo["ndc"]) ? drugInfo["ndc"] : drugInfo["packaging_id"];
                var ndc = new Dictionary<string, string>();
                foreach(var afld in drugInfo.Keys)
                {
                    var mappedField = GetMappedMultumField(afld);
                    set(mappedField, drugInfo[afld]);
                    ndc.Add(mappedField, drugInfo[afld]);
                }

                if (!String.IsNullOrEmpty(packagingId))
                    AddNDCToCache(packagingId, ndc);

                return ndc;
            }

            /// <summary>
            /// Set an attribute on this object
            /// </summary>
            /// <param name="name">Attribute name</param>
            /// <param name="value">Attribute value</param>
            private void set(string name, string value)
            {
                name = name.ToLowerInvariant();
                if (name.Equals("active") || name.Equals("active_name"))
                {
                    ActiveName = value;
                } else if (name.Equals("route") || name.Equals("drug_route"))
                {
                    DrugRoute = value;
                } else if (name.Equals("dose_form") || name.Equals("drug_form"))
                {
                    DrugForm = value;
                } else if (name.Equals("dose_form_id") || name.Equals("drug_form_id"))
                {
                    DrugFormId = value;
                } else if (name.Equals("strength") || name.Equals("drug_strength"))
                {
                    DrugStrength = value;
                } else if (name.Equals("drug") || name.Equals("active_id"))
                {
                    ActiveId = value;
                } else if (name.Equals("multum") || name.Equals("drug_id"))
                {
                    DrugId = value;
                } else if (name.Equals("ndc") || name.Equals("packaging_id"))
                {
                    PackagingId = value;
                } else if (name.Equals("drugcat") || name.Equals("drug_category_id"))
                {
                    DrugCategoryId = value;
                } else if (name.Equals("brand") || name.Equals("brand_name"))
                {
                    BrandName = value;
                }
            }

            /// <summary>
            /// Component constants
            /// </summary>
            public class Constants
            {
                /// <summary>
                /// Multum drug DB identifier
                /// </summary>
                public const string MULTUM_DRUG_DB = "M";

                /// <summary>
                /// FDB drug DB identifier
                /// </summary>
                public const string FDB_DRUG_DB = "F";

                /// <summary>
                /// FDB Canada drug DB identifier
                /// </summary>
                public const string CAN_FDB_DRUG_DB = "1";

                /// <summary>
                /// Medispan drug DB identifier
                /// </summary>
                public const string MEDISPAN_DRUG_DB = "2";

                /// <summary>
                /// Default drug DB identifier
                /// </summary>
                public const string DEFAULT_DRUG_DB = MULTUM_DRUG_DB;

                /// <summary>
                /// Component is a drug
                /// </summary>
                public const string TYPE_DRUG = "D";

                /// <summary>
                /// Component is an additive
                /// </summary>
                public const string TYPE_ADDITIVE = "A";

                /// <summary>
                /// Component is a base
                /// </summary>
                public const string TYPE_BASE = "B";
            }
        }

        /// <summary>
        /// Constants used in medications
        /// </summary>
        public class Constants
        {
            #region Status constants
            public const string ACTIVE = "A";
            public const string INACTIVE = "I";
            #endregion

            #region Obsolete descriptions
            public const string DRUG_EQUIVALENT_AVAILABLE = "Drug is obsolete - an equivalent is available";
            public const string DRUG_BRAND_MATCH_AVAILABLE = "Drug is obsolete - other forms of this brand are available";
            public const string DRUG_NO_LONGER_AVAILABLE = "Drug no longer available";
            #endregion

            #region Action titles
            public const string ACK_TITLE = "Acknowledge";
            public const string CANCEL_TITLE = "Cancel";
            public const string DELETE_TITLE = "Delete";
            public const string COSIGN_TITLE = "Co-sign";
            public const string GIVE_TITLE = "Give";
            public const string HOLD_TITLE = "Hold";
            public const string ORDER_TITLE = "Order";
            public const string REPEAT_TITLE = "Repeat";
            public const string UNHOLD_TITLE = "Unhold";
            public const string DISCONTINUE_TITLE = "Discontinue";
            public const string DISCONTINUED_TITLE = "Discontinued";
            #endregion

            #region Medication types
            /// <summary>
            /// Combo medication identifier
            /// </summary>
            public const string TYPE_COMBO = "C";

            /// <summary>
            /// Regular medication identifier
            /// </summary>
            public const string TYPE_MEDICATION = "M";

            /// <summary>
            /// Free text medication identifier
            /// </summary>
            public const string TYPE_FREE_TEXT = "F";

            /// <summary>
            /// IV medication identifier
            /// </summary>
            public const string TYPE_IV = "I";

            /// <summary>
            /// IV base medication identifier
            /// </summary>
            public const string TYPE_IV_BASE = "B";
            #endregion

            /// <summary>
            /// Dictionary for mapping multum objects
            /// </summary>
            public static readonly Dictionary<string, string> MULTUM_OBJ_MAP = new Dictionary<string, string> {
                { "active"       , "active_name" },
                { "route"        , "drug_route" },
                { "dose_form"    , "drug_form" },
                { "dose_form_id" , "drug_form_id" },
                { "strength"     , "drug_strength" },
                { "drug"         , "active_id" },
                { "multum"       , "drug_id" },
                { "ndc"          , "packaging_id" },
                { "drugcat"      , "drug_category_id" },
                { "brand"        , "brand_name" }
            };

            public static readonly Dictionary<string, int> SVCTYPE = new Dictionary<string, int>
            {
                { "ALLERGY",         260 },
                { "MINOR",           270 },
                { "UNDETERMINED",    275 },
                { "MODERATE",        280 },
                { "SEVERE",          290 },
                { "CONTRAINDICATED", 295 }
            };

            #region Data Item name constants
            /// <summary>
            /// DDS override data item
            /// </summary>
            public const string DataItem_DDS_override = "dds_override";

            /// <summary>
            /// Dosage data item
            /// </summary>
            public const string DataItem_Dosage = "dosage";

            /// <summary>
            /// Indication data item
            /// </summary>
            public const string DataItem_Indication = "indication";

            /// <summary>
            /// Notes data item
            /// </summary>
            public const string DataItem_Notes = "notes";

            /// <summary>
            /// Rate data item
            /// </summary>
            public const string DataItem_Rate = "rate";

            /// <summary>
            /// Rationale data item
            /// </summary>
            public const string DataItem_Rationale = "rationale";

            /// <summary>
            /// Repeat data item
            /// </summary>
            public const string DataItem_Repeat = "repeat";

            /// <summary>
            /// Route data item
            /// </summary>
            public const string DataItem_Route = "route";

            /// <summary>
            /// Schedule data item
            /// </summary>
            public const string DataItem_Schedule = "schedule";
            #endregion
        }
    }
}
 