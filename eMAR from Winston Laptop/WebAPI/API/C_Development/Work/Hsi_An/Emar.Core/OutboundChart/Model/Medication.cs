using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Emar.Core.Helpers;

namespace Emar.Core.OutboundChart.Model
{
    /// <summary>
    /// Medication object
    /// </summary>
    public class Medication : IMedication, ICloneable
    {
        public int Id { get; set; }
        public string Type { get; set; } = Constants.TYPE_MEDICATION;
        private string _name;
        public string Name
        {
            get { return _name; }
            set { _name = System.Text.RegularExpressions.Regex.Replace(value, "( : ){2,}", " : "); }
        }
        public string Status { get; set; }
        private string _ibex;
        public string Ibex
        {
            get { return _ibex; }
            set
            {
                _ibex = value;
                foreach (var component in Components)
                {
                    component.Ibex = _ibex;
                }
            }
        }
        private Int16 _site;
        public Int16 Site
        {
            get { return _site; }
            set
            {
                _site = value;
                foreach (var component in Components)
                {
                    component.Site = _site;
                }
            }
        }
        public int Losecs { get; set; }
        private string _route { get; set; }
        public string Route
        {
            get { return this._route != null ? this._route.Trim() : ""; }
            set { this._route = value?.Trim() ?? ""; }
        }
        private string _unit { get; set; }
        public string Unit
        {
            get { return this._unit != null ? this._unit.Trim() : ""; }
            set { this._unit = value?.Trim() ?? ""; }
        }
        private string _dose { get; set; }
        public string Dose
        {
            get { return this._dose != null ? this._dose.Trim() : ""; }
            set { this._dose = value?.Trim() ?? ""; }
        }
        private string _defaultRoute { get; set; }
        public string DefaultRoute
        {
            get { return this._defaultRoute != null ? this._defaultRoute.Trim() : ""; }
            set { this._defaultRoute = value?.Trim() ?? ""; }
        }
        private string _defaultUnit { get; set; }
        public string DefaultUnit
        {
            get { return this._defaultUnit != null ? this._defaultUnit.Trim() : ""; }
            set { this._defaultUnit = value?.Trim() ?? ""; }
        }
        private string _printedUnit { get; set; }
        public string PrintedUnit
        {
            get { return this._printedUnit != null ? this._printedUnit.Trim() : ""; }
            set { this._printedUnit = value?.Trim() ?? ""; }
        }
        private string _defaultDose { get; set; }
        public string DefaultDose
        {
            get { return this._defaultDose != null ? this._defaultDose.Trim() : ""; }
            set { this._defaultDose = value?.Trim() ?? ""; }
        }
        public string Frequency { get; set; }
        public string Duration { get; set; }
        public string Time { get; set; }
        public string Repeat { get; set; }
        public string Notes { get; set; }
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
        public int? OrderForUserId { get; set; }
        public int? OrderUserId { get; set; }
        public int? AckUserId { get; set; }
        public int? HoldUserId { get; set; }
        public int? UnholdUserId { get; set; }
        public int? CancelUserId { get; set; }
        public int? DeleteUserId { get; set; }
        public int? GiveUserId { get; set; }
        public int? StopUserId { get; set; }
        public int? ExcludeUserId { get; set; }
        public int? DiscontinueUserId { get; set; }
        public int? DiscontinuedUserId { get; set; }
        public string IVType { get; set; }
        public int? IVSite { get; set; }
        public string IVLocation { get; set; }
        public int? CPTLosecsLink { get; set; }
        public string Authentication { get; set; }
        private string _rate { get; set; }
        public string Rate
        {
            get { return this._rate != null ? this._rate.Trim() : ""; }
            set { this._rate = value?.Trim() ?? ""; }
        }
        private string _rateUnit { get; set; }
        public string RateUnit
        {
            get { return this._rateUnit != null ? this._rateUnit.Trim() : ""; }
            set { this._rateUnit = value?.Trim() ?? ""; }
        }
        public long PatientOrderId { get; set; }
        public string MedAdminType { get; set; }
        public string MedAdminDate { get; set; }
        public string MedAdminSysDate { get; set; }
        public int? MedAdminUser { get; set; }
        public virtual ICollection<Component> Components { get; set; }
        private byte SiteId { get; set; }
        private Dictionary<string, Dictionary<string, string>> CustomActions = new Dictionary<string, Dictionary<string, string>>();
        private String OrderDateString { get; set; }
        private static Dictionary<string, Dictionary<string, string>> IdxInfo = new Dictionary<string, Dictionary<string, string>>();
        private static Dictionary<string, string> IdxInfoIVType = new Dictionary<string, string>();
        private static Dictionary<string, Dictionary<string, string>> IdxInfoPrinted = new Dictionary<string, Dictionary<string, string>>();
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
            Status = Constants.ACTIVE;
            OrderDateString = (new Time()).Timestamp();
            Type = "";

            // retrieve site specific settings here?
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
        /// Get the list of components
        /// </summary>
        /// <returns></returns>
        public List<IComponent> GetComponents()
        {
            var compList = new List<IComponent>();
            foreach (var c in Components)
            {
                compList.Add(c);
            }
            return compList;
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
            foreach (var a in actions.Values)
            {
                if (a.ContainsKey("color") && !string.IsNullOrWhiteSpace(a["color"]) && a["color"].Equals(color))
                {
                    if (a.ContainsKey("entry_date") && action.ContainsKey("entry_date"))
                    {
                        var aDate = Emar.Core.Helpers.Time.DateTimeFromString(a["entry_date"]);
                        var actionDate = Emar.Core.Helpers.Time.DateTimeFromString(a["entry_date"]);
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
            return (name + strengthForm + (noDose ? "" : (!string.IsNullOrWhiteSpace(dose) ? "<b>Dose</b>: " + dose : "")) + " " + (!string.IsNullOrWhiteSpace(route) ? (noRoute ? "" : "<b>Route</b>: ") + route : "")).Trim();
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
            // applying a Reverse() to SORTED_STATUSES appeared to be causing the LookupList to alternate between reverse
            // order and normal order after each GetMedStatus() invocation. Added new constant to keep consistent.
            var lookupList = Medication.ActionConstants.REVERSE_SORTED_STATUSES;
            foreach (var lookup in lookupList)
            {
                if (lookup.ContainsKey("for") && !string.IsNullOrWhiteSpace(lookup["for"]) && !lookup["for"].Equals("status"))
                    continue;

                if (lookup.ContainsKey("code") && !string.IsNullOrWhiteSpace(lookup["code"]))
                {
                    var code = lookup["code"];
                    var name = Medication.ActionConstants.NAME.ContainsKey(code) ? Medication.ActionConstants.NAME[code] : null;
                    if (name != null)
                    {
                        var field = (name.Equals("del") ? "delete" : name);
                        var date = "";
                        var sysdate = "";
                        int? usr = 0;
                        switch (field)
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
                            var verbiage = Medication.ActionConstants.VERBIAGE.ContainsKey(code) ? Medication.ActionConstants.VERBIAGE[code] : null;
                            var cls = verbiage.ToLowerInvariant().Replace(' ', '_');
                            var description = Medication.ActionConstants.DESCRIPTION.ContainsKey(code) ? Medication.ActionConstants.DESCRIPTION[code] : null;
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
                // comment out until custom actions are used - will cause exception due to missing ibex
   //             else if (lookup.ContainsKey("color") && !string.IsNullOrWhiteSpace(lookup["color"]))
   //             {
   //                 var color = lookup["color"];
   //                 var action = GetCustomActionByColor(color);
   //                 if (action.Keys.Count > 0)
   //                 {
   //                     status = action;
   //                     status["class"] = color.Equals(Medication.ActionConstants.ORANGE) ? "status_orange" : "status_purple";
   //                     status["custom"] = "1";
   //                 }
   //             }
   //             else
   //             {
   //                 status["code"] = "";
   //                 status["color"] = "";
   //             }

                if (status.Keys.Count > 0)
                    // had a 'break' here but appeared to be working like 'continue' instead
                    return status;
            }

            // shouldn't happen that returns w/o status being assigned - if so, add empty string for code & color?
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
            }
            else
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
        /// Determine if the drug is a combo drug (contains more than one Component)
        /// </summary>
        /// <returns>A boolean indicating if the drug is a combo drug</returns>
        public bool IsCombo()
        {
            return Type.Equals(Medication.Constants.TYPE_IV) || Components.Count > 1;
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
            var giveDate = Emar.Core.Helpers.Time.DateTimeFromString(GiveDate);
            return (
                !String.IsNullOrWhiteSpace(GiveDate) // removed discontinue(d) checks
            );
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
            private string _enteredDose { get; set; }
            public string EnteredDose
            {
                get { return this._enteredDose != null ? this._enteredDose.Trim() : ""; }
                set { this._enteredDose = value?.Trim() ?? ""; }
            }

            private string _enteredUnit { get; set; }
            public string EnteredUnit
            {
                get { return this._enteredUnit != null ? this._enteredUnit.Trim() : ""; }
                set { this._enteredUnit = value?.Trim() ?? ""; }
            }

            public string ActiveId { get; set; }
            public string DrugId { get; set; }
            public string PackagingId { get; set; }
            public string DrugCategoryId { get; set; }
            public string Type { get; set; } = Constants.TYPE_DRUG;
            public string DrugFormId { get; set; }

            public List<Dictionary<string, string>> Interactions { get; set; } = new List<Dictionary<string, string>>();

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
            /// Get the drug name portion of the name
            /// </summary>
            /// <returns>String name of drug</returns>
            public string GetName()
            {
                if (BrandName.Equals(ActiveName) || String.IsNullOrWhiteSpace(ActiveName))
                {
                    return BrandName;
                }
                else
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
        }

        /// <summary>
        /// Medication Action constants
        /// </summary>
        public static class ActionConstants
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
                { "O", "Authenticated using password or biometrics." },
                { "B", "Authenticated using biometrics." },
                { "N", "No Authentication." }
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
            /// Available statuses, sorted in reverse order
            /// </summary>
            public static readonly List<Dictionary<string, string>> REVERSE_SORTED_STATUSES = new List<Dictionary<string, string>>
            {
                new Dictionary<string, string> { { "code", DISCONTINUED }, { "color", GREEN },    { "for", "" } },
                new Dictionary<string, string> { { "code", GIVE },         { "color", GREEN },    { "for", "indicator" } },
                new Dictionary<string, string> { { "code", DISCONTINUE },  { "color", GRAY  },    { "for", "" } },
                new Dictionary<string, string> { { "code", GIVE },         { "color", GREEN },    { "for", "status" } },
                new Dictionary<string, string> { { "code", CANCEL },       { "color", GRAY },     { "for", "status" } },
                new Dictionary<string, string> { { "code", DELETE },       { "color", NO_COLOR }, { "for", "status" } },
                new Dictionary<string, string> { { "code", HOLD },         { "color", YELLOW},    { "for", "" } },
                new Dictionary<string, string> { { "code", "" },           { "color", ORANGE },   { "for", "" } },
                new Dictionary<string, string> { { "code", ACKNOWLEDGE },  { "color", BLUE },     { "for", "" } },
                new Dictionary<string, string> { { "code", "" },           { "color", PURPLE },   { "for", "" } },
                new Dictionary<string, string> { { "code", GIVE },         { "color", GREEN },    { "for", "text" } },
                new Dictionary<string, string> { { "code", ORDER },        { "color", RED },      { "for", "" } }
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
            /// Action mapping from ActionEnum string to charting action label
            /// </summary>
            public static readonly Dictionary<string, string> ACTION_MAP = new Dictionary<string, string>
            {
                { ORDER, "Ordered" }, // unused
                { "Acknowledge", "Acknowledged" },
                { "Hold", "Held" },
                { "UnHold", "Hold Canceled" },
                { "Give", "Documented as given" },
                { "Cancel", "Canceled" },
                { "Delete", "Deleted" },
                { "CoSign", "Co-signed" },
                { "OrderDiscontinue", "Discontinue Ordered" },
                { "CompleteDiscontinue", "Discontinued" },
                { "MissedDose", "Missed dose noted" },
                { "Reschedule", "Rescheduled" },
                { "FollowUp", "Follow Up" },
//                { "Complete", "Complete" }, // unused
//                { "Repeat", "Repeat" }, // unused
//                { "Modify", "Modify" }, // unused
                { "PharmVerification", "Pharmacist Verified" },
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

            /// <summary>
            /// Medication mapping from ActionEnum string to medication label
            /// </summary>
            public static readonly Dictionary<string, string> MEDICATION_MAP = new Dictionary<string, string>
            {
                { "Acknowledge", "Ack" },
                { "Hold", "Hold" },
                { "UnHold", "Unhold" },
                { "Give", "Give" },
                { "Cancel", "Cancel" },
                { "Delete", "Delete" },
                { "OrderDiscontinue", "Discontinue" }, // right mapping?
                { "CompleteDiscontinue", "Discontinued" } // right mapping?
            };
        }
    }
}
