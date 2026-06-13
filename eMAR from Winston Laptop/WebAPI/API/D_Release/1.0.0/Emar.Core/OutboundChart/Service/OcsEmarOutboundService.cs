using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using Emar.Core.Helpers;
using Emar.Core.MedicationReactions;
using Emar.Core.Medications.Model;
using Emar.Core.OutboundChart.Model;
using Emar.Core.OutboundChart.Repository;
using Emar.Core.OutboundData.Model;
using Emar.Core.OutboundData.Repository;
using Emar.Core.Templates.Model;
using Emar.Data;
using HelperChart = Emar.Core.Helpers.Chart;
using HelperDB = Emar.Core.Helpers.DB;
using EntitiesOrderInteract = Emar.Data.Entities.OrderInteraction;
using EntitiesOrderReact = Emar.Data.Entities.OrderReaction;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace Emar.Core.OutboundChart.Service
{
    public class OcsEmarOutboundService : IOcsEmarOutboundService
    {
        private readonly IbexContext _ibexContext;
        private readonly IEmarOutboundChartRepository _emarOutboundChartRepository;
        private readonly IEmarOutboundDataRepository _emarOutboundDataRepository;

        public OcsEmarOutboundService(IbexContext ibexContext, IEmarOutboundChartRepository emarOutboundChartRepository, IEmarOutboundDataRepository emarOutboundDataRepository)
        {
            _ibexContext = ibexContext;
            _emarOutboundChartRepository = emarOutboundChartRepository;
            _emarOutboundDataRepository = emarOutboundDataRepository;
        }

        //        public async System.Threading.Tasks.Task<string> SendChartLinesAsync(OcsChartParameters ocsChartParams)
        public string SendChartLinesAsync(OcsChartParameters ocsChartParams)
        {

            //           if (!user.HasWritePermission(Permission.MED_SVC))
            //               return null;

            var patient = new Patient();
            patient.Ibex = _emarOutboundDataRepository.GetExternalPatientId((long)ocsChartParams.patiendId);
            var now = DateTime.Now.ToString("yyyyMMddHHmmss");
            var externalSiteId = (byte)_emarOutboundDataRepository.GetExternalSiteId(ocsChartParams.site);
            var site = new Site(externalSiteId); // need to define other site attrs?
            var user = new User();
            user.Id = _emarOutboundDataRepository.GetExternalUserId(ocsChartParams.user);
            user.SiteId = externalSiteId;
            var losecs = Int32.Parse(ocsChartParams.losecs.ToString("ddHHmmss")); // must match what is in ODS

            var notes = ocsChartParams.medNotes;
            //            if (serviceOptions != null && serviceOptions.Count > 0)
            //                notes += (!string.IsNullOrWhiteSpace(notes) ? "\n" : "") + string.Join(", ", serviceOptions);

            var meds = new List<Medication>();
            var medNames = new List<string>();
            var qlMedNums = new List<string>();
            var required = new List<string>();
            //            var drugDb = new DrugDB(site);

            // Create the medication
            var med = new Medication(site.Id);
            var order = new OrderMedication();
            order.Id = ""; // put something there for now
            var orders = new List<OrderMedication>();
            orders.Add(order);

            /* TODO: Figure out if IV drugs are orderable through the API and
             * update object to support them, if needed
             * /
            if (type.Equals("IV"))
            {

            }*/
            if (!order.Id.ToLowerInvariant().Equals("ft"))
            {
                // first, determine if combo med
                var name = _emarOutboundDataRepository.GetComboName(ocsChartParams.medicationId);
                var isComboMed = name.Length > 0;
                if (!isComboMed)
                {
                    var component = new Medication.Component();
                    OdsMedicationDetails medDetails = _emarOutboundDataRepository.GetMedicationDetails(ocsChartParams.medicationId);
                    component.Ibex = patient.Ibex;
                    component.Site = site.Id;
                    component.Losecs = losecs;
                    component.BrandName = medDetails.BrandName;
                    component.DrugCategoryId = medDetails.DrugCategoryId;
                    component.DrugStrength = medDetails.DrugStrength;
                    component.DrugForm = medDetails.DrugForm;
                    component.DrugId = medDetails.DrugId;
                    component.ActiveName = medDetails.ActiveName;
                    component.ActiveId = medDetails.ActiveId;
                    component.Interactions = null;
                    component.Reactions = null;
                    //                  component.DrugDBType = 
                    med.Type = Medication.Constants.TYPE_MEDICATION;
                    med.Components.Add(component);
                    med.Name = medDetails.BrandName;
                }
                else
                {
                    List<int> detailIds = _emarOutboundDataRepository.GetMedicationDetailsIds(ocsChartParams.medicationId);
                    foreach (int detailId in detailIds)
                    {
                        OdsMedicationDetails medDetails = _emarOutboundDataRepository.GetMedicationDetailsFromMedDetailsId(detailId);
                        var comboComponent = new Medication.Component();
                        comboComponent.Ibex = patient.Ibex;
                        comboComponent.Site = site.Id;
                        comboComponent.Losecs = losecs;
                        comboComponent.BrandName = medDetails.BrandName;
                        comboComponent.DrugCategoryId = medDetails.DrugCategoryId;
                        comboComponent.DrugStrength = medDetails.DrugStrength;
                        comboComponent.DrugForm = medDetails.DrugForm;
                        comboComponent.DrugId = medDetails.DrugId;
                        comboComponent.ActiveName = medDetails.ActiveName;
                        comboComponent.ActiveId = medDetails.ActiveId;
                        comboComponent.Interactions = null;
                        comboComponent.Reactions = null;
                        //                      component.DrugDBType = 
                        med.Components.Add(comboComponent);
                    }
                    med.Type = Medication.Constants.TYPE_COMBO;
                    med.Name = name;
                }

                // TODO: ibex4q was setting product code and procedure code here, using 'q_prod' and 'q_proc' inputs. See if we need those right now.
                // TODO: ibex4q was checking for the existence of $MED_NAME{''}, and using that for the name if it existed, else GetName(). Where does that come from?
            }
            else
            {
                med.Name = order.Name;
                med.Type = Medication.Constants.TYPE_FREE_TEXT;
            }
            med.Ibex = patient.Ibex;
            med.Losecs = losecs;
            med.Dose = string.IsNullOrWhiteSpace(ocsChartParams.Dose) ? "*" : ocsChartParams.Dose.Replace(".00", "");
            med.Unit = _emarOutboundChartRepository.GetUnit(ocsChartParams.Unit);
            med.Route = _emarOutboundChartRepository.GetRoute(ocsChartParams.Route);
            med.Frequency = _emarOutboundChartRepository.GetFrequencyNameFromId(ocsChartParams.FrequencyId);
            med.Duration = (_emarOutboundChartRepository.GetDurationUnitFromId(ocsChartParams.DurationId) == null) ? "" :
                            ocsChartParams.Duration + " " + _emarOutboundChartRepository.GetDurationUnitFromId(ocsChartParams.DurationId);
            med.OrderUserId = user.Id;
            var orderingPhysicianId = _emarOutboundDataRepository.GetExternalUserId(ocsChartParams.orderingPhysicianId);
            med.OrderForUserId = orderingPhysicianId;
            med.OrderDate = ocsChartParams.OrderDate;
            med.Time = order.Time;
            med.Notes = (order.Notes + (!string.IsNullOrWhiteSpace(order.Notes) && !string.IsNullOrWhiteSpace(notes) ? "\n" : "") + notes).Trim();
            med.Repeat = order.Repeat;
            //            med.Authentication = authType;
            // TODO: get the complete data for the med (ibex..emar_med_administrations) for use with the mtb 'M' color calculation

            meds.Add(med);
            //            required.AddRange(med.CheckRequired(med.Name, type, order.Overrides.Count > 0));

            if (orderingPhysicianId == 0)
                required.Insert(0, "Ordering Physician is required");

            if (required.Count > 0)
                return string.Join("\n", required);

            // Restore the interactions and reactions per medication, so we can match them up to override rationale
            foreach (Medication medication in meds)
            {
                AddInteractionsAndReactions(medication, ocsChartParams);
            }

            var ordersPlaced = false;

            // Either all the medications will be ordered or none of the medications will be ordered
            // First, store to the database. If any entry fails, don't write the chart and roll back.
            // Second, gather all the Chart entries into a single string, then perform a single write.
            // This simplifies the write operation to a single 'print' since it would be a pain to remove
            // any entries that had already been written.
            var connection = HelperDB.GetConnectionString();
            using (var con = new SqlConnection(connection))
            {
                con.Open();
                var transaction = con.BeginTransaction();
                var EMR = new EMR(site.Id, patient.Ibex, true);
                try
                {

                    var emrLines = new List<EMR.Line>();
                    var i = 0;
                    foreach (var medication in meds)
                    {
                        // 
                        // var line = await _emarOutboundChartRepository.ChartEntry(patient, medication, now, orders[i].Overrides);
                        // TODO - need db changes to handle more than one override?
                        // override reasons are presently being set inside AddInteractionsAndReactions()
                        var line = _emarOutboundChartRepository.ChartEntry(patient, medication, now);
                        emrLines.Add(line);
                        medNames.Add(medication.GetFullName());
                        i++;
                    }

                    if (EMR.WriteLines(emrLines.ToArray()))
                    {
                        HelperChart.OnChartWrite(site, patient.Ibex, user.Id);
                        transaction.Commit();
                        ordersPlaced = true;
                    }
                    else
                    {
                        transaction.Rollback();
                        return "F";
                    }
                }
                catch (SqlException ex)
                {
                    //Log the SQL exception.
                    using (EventLog eventLog = new EventLog("Application"))
                    {
                        string sSQLException = ex.Message + "\n";
                        sSQLException += "error number = " + ex.Number + "\n";
                        sSQLException += "source = " + ex.Source + "\n";
                        sSQLException += "Line Number = " + ex.LineNumber + "\n";
                        sSQLException += ex.StackTrace + "\n";

                        eventLog.Source = "PulseCheck EMAR API";
                        eventLog.WriteEntry(sSQLException, EventLogEntryType.Information, 101, 1);
                    } //end using.

                    transaction.Rollback();

                    DTFL.Write(site.Id, user.Id, ex, "Medication Order Save");
                    transaction.Rollback();
                    return "F";
                }
                catch (Exception ex)
                {
                    //Log the exception.
                    using (EventLog eventLog = new EventLog("Application"))
                    {
                        string sException = ex.Message + "\n";
                        sException += "source = " + ex.Source + "\n";
                        sException += ex.StackTrace + "\n";

                        eventLog.Source = "PulseCheck EMAR API";
                        eventLog.WriteEntry(sException, EventLogEntryType.Information, 101, 1);
                    } //end using.

                    DTFL.Write(site.Id, user.Id, "Medication Order Save failure - " + ex.ToString());
                    transaction.Rollback();
                    return "F";
                }
                finally
                {
                    con.Close();
                }
            }

            if (ordersPlaced)
            {
                // we need for MVP - don't have corresponding 'ACESSED' log though
                // await MeaningfulUse.LogCreation(user, ocsChartParams.ibex, "MEDICATION SERVICE");
                MeaningfulUse.LogCreation(user, patient.Ibex, "MEDICATION SERVICE");

                foreach (var medication in meds)
                {
                    TriggerFile(site, patient.Ibex, user.Id, "ENTERED", medication.Losecs);
                }

                // previously had the medication order indicator/acknowledgement code here
                // was setting pat.ord30 but code now exists in updatePatientMedicationIndicator()

                // If the user is not the ordering physician, mail the orderer (if they aren't ordering-only)
                if (user.Id != orderingPhysicianId && site.GetOrgOption("MED_SVC_NOTIFY_ORD_PHYS").Equals("Y"))
                {
                    var ordOnly = new HelperDB.Select
                    {
                        Sql = "SELECT ordonly FROM drs WHERE num=@num",
                        Parameters = new SqlParameter[]
                        {
                            new SqlParameter("@num", SqlDbType.Int) { Value = orderingPhysicianId }
                        }
                    }.RunForScalar().ToString();
                    if (!ordOnly.Equals("Y"))
                    {
                        var message = "You are the ordering physician listed for a medication order entered by " + user.GetName() + " for " + patient.GetName() + ".\n\n" + string.Join("\n", medNames);
                        var internalMail = new PulseMail(site);
                        if (internalMail != null)
                        {
                            internalMail.SendMessage(orderingPhysicianId, "Medication Services Order", message, 0);
                        }
                    }
                }
            }

            // Increment usage of non-freetext QL meds
            if (qlMedNums.Count > 0)
            {
                var medNumParams = HelperDB.GetParamsList(qlMedNums, SqlDbType.Int);
                var rxlParams = new List<SqlParameter> {
                    new SqlParameter("@usr", SqlDbType.Int) { Value = user.Id },
                    new SqlParameter("@site", SqlDbType.TinyInt) { Value = site.Id },
                    new SqlParameter("@type", SqlDbType.Char) { Value = "M" }
                };
                rxlParams.AddRange(medNumParams.Item1);
                new HelperDB.Update
                {
                    Sql = "UPDATE rxl SET usage=ISNULL(usage,0)+1 WHERE usr=@usr AND site=@site AND type=@type AND num IN(" + string.Join(",", medNumParams.Item2) + ")",
                    Parameters = rxlParams.ToArray()
                }.Run();
            }

            //            CreateTrigger(site, patient, ocsChartParams.user, "place", meds);
            return "";
        }

        private void CreateTrigger(ISite site, Patient patient, int userId, string action, List<Medication> meds)
        {
            var opt = site.GetOrgOption("TRIGGER_MED_HSF");
            if (opt.Equals("N"))
            {
                return;
            }

            var triggerSettings = new Dictionary<string, Dictionary<string, string>>
            {
                { Trigger.Constants.TRIGGER_MED_SVC_HL7, new Dictionary<string, string>() },
                { Trigger.Constants.TRIGGER_MED_SVC_IMAGE, new Dictionary<string, string>() }
            };

            var res = new HelperDB.Select
            {
                Sql = "SELECT field_name, field_val, field_num FROM site_preferences WHERE site=@site AND field_num IN(1,2) ORDER BY field_num, field_seq",
                Parameters = new SqlParameter[]
                {
                    new SqlParameter("@site", SqlDbType.TinyInt) { Value = site.Id }
                }
            }.RunForListOfDictionaries();

            foreach (var r in res)
            {
                if (r["field_num"].Equals("1"))
                {
                    triggerSettings[Trigger.Constants.TRIGGER_MED_SVC_HL7][r["field_name"]] = r["field_val"];
                }
                else
                {
                    triggerSettings[Trigger.Constants.TRIGGER_MED_SVC_IMAGE][r["field_name"]] = r["field_val"];
                }
            }

            var patAge = patient.Age;
            var patAgeUnit = patient.AgeUnit;

            // The age is compared in years, so if a patient is days or weeks old, they're less than 1.
            if (patAgeUnit == Patient.Constants.AGEUNIT_DAYS || patAgeUnit == Patient.Constants.AGEUNIT_WEEKS)
            {
                patAge = 0;
            }
            else if (patAgeUnit == Patient.Constants.AGEUNIT_MONTHS)
            {
                patAge /= 12;
            }

            foreach (var triggerId in triggerSettings.Keys)
            {
                var currentTriggerSettings = triggerSettings[triggerId];

                // Treat blank or missing values as extreme of range.
                if (!currentTriggerSettings.ContainsKey("min_age") || string.IsNullOrWhiteSpace(currentTriggerSettings["min_age"]))
                {
                    currentTriggerSettings["min_age"] = "0";
                }

                if (!currentTriggerSettings.ContainsKey("max_age") || string.IsNullOrWhiteSpace(currentTriggerSettings["max_age"]))
                {
                    currentTriggerSettings["max_age"] = "10000";
                }

                var minAge = Convert.ToInt32(currentTriggerSettings["min_age"]);
                var maxAge = Convert.ToInt32(currentTriggerSettings["max_age"]);

                //                if ((opt.Equals(Trigger.Constants.TRIGGER_MED_SVC_BOTH) || opt.Equals(triggerId)) && ((patAge == 0 && patAgeUnit == Patient.Constants.AGEUNIT_DAYS) || (minAge <= patAge && maxAge >= patAge)))
                //                {
                //                    var frmCSSite = GetFormularyShareSite(site.Id);
                //                    var formulary = new Formulary(frmCSSite, null, null, "med");

                //                    foreach (var med in meds)
                //                    {
                //                        if (CheckFormularyForTrigger(site, med, formulary, currentTriggerSettings) && CheckActionForTrigger(action, currentTriggerSettings))
                //                        {
                //                            var error = CreateDBTrigger(site, patient.Ibex, userId, triggerId, med, formulary);
                //                            if (!string.IsNullOrWhiteSpace(error))
                //                            {
                //                                break;
                //                            }
                //                        }
                //                   }
                //                }
            }
        }

        //        private bool CheckFormularyForTrigger(ISite site, Medication med, Formulary formulary, Dictionary<string, string> triggerSettings)
        //        {
        //            var triggerIn = triggerSettings.ContainsKey("in_form") ? triggerSettings["in_form"] : "N";
        //            var triggerOut = triggerSettings.ContainsKey("out_form") ? triggerSettings["out_form"] : "N";
        //            var triggerMach = triggerSettings.ContainsKey("machine_form") ? triggerSettings["machine_form"] : "N";

        // Don't send anything if they're all turned off.
        // Free text should be considered never on formulary
        //            if ((triggerIn.Equals("N") && triggerOut.Equals("N") && triggerMach.Equals("N")) || med.IsFreeText())
        //            {
        //                return false;

        // If the machine setting is Don't send exact match (the most powerful switch) and one of the others is on, we'll send.
        // Combo meds are always considered on formulary
        //            }
        //            else if ((!triggerMach.Equals("D") && (triggerIn.Equals("A") || triggerOut.Equals("A"))) || med.IsCombo())
        //            {
        //                return true;
        //            }

        //            var ndc = "";
        //            var components = med.GetComponents();
        //            if (components.Count > 0)
        //            {
        //                ndc = components[0].PackagingId;
        //                var flags = formulary.GetFlags(ndc);

        //                var inpat = flags[Formulary.Constants.INPAT_TYPE];
        //                var outpat = flags[Formulary.Constants.OUTPAT_TYPE];
        //                var pyxis = flags[Formulary.Constants.PYXIS_TYPE];

        //                if (triggerMach.Equals("D") && pyxis > Formulary.Constants.EQUIV_MATCH)
        //                {
        //                    return false;
        //                }

        //                return
        //                    (triggerIn.Equals("F") && inpat > Formulary.Constants.NON_MATCH) ||
        //                    (triggerOut.Equals("F") && outpat > Formulary.Constants.NON_MATCH) ||
        //                    ((triggerMach.Equals("F") || triggerMach.Equals("D")) && pyxis > Formulary.Constants.NON_MATCH) ||
        //                    (triggerIn.Equals("M") && inpat > Formulary.Constants.EQUIV_MATCH) ||
        //                    (triggerOut.Equals("M") && outpat > Formulary.Constants.EQUIV_MATCH);
        //            }

        //            return false;
        //        }

        //        private bool CheckActionForTrigger(string action, Dictionary<string, string> triggerSettings)
        //        {
        //            var opt = "";
        //            if (action.StartsWith("c_"))
        //            {
        //                opt = "order_custom_" + action.Substring(2);
        //            }
        //            else
        //            {
        //                opt = MedicationManager.Constants.OPTS_MAPPING.ContainsKey(action) ? MedicationManager.Constants.OPTS_MAPPING[action] : "";
        //            }

        //            return (!string.IsNullOrWhiteSpace(opt) && triggerSettings.ContainsKey(opt) && triggerSettings[opt].Equals("Y"));
        //        }

        //        private string CreateDBTrigger(ISite site, string patientId, int userId, string triggerId, Medication med, Formulary formulary)
        //        {
        //            var xml = new StringBuilder("<medication_services>")
        //                .Append(med.GetXML(formulary).ToString())
        //                .Append("</medication_services>");

        //            var interfaceName = triggerId.Equals(Trigger.Constants.TRIGGER_MED_SVC_IMAGE) ? Trigger.Constants.MEDICATION_SERVICE_IMAGE : Trigger.Constants.MEDICATION_SERVICE_HL7;

        //            var error = Trigger.Create(site, patientId, userId, xml.ToString(), interfaceName, "4q");

        //            if (error != null)
        //            {
        //                DTFL.Write(site.Id, userId, "Cannot create ensemble trigger for Medication Services interface. IBEX: " + patientId + " ERROR: " + error);
        //            }

        //            return error;
        //        }

        /// <summary>
        /// Write a trigger file for the med interface
        /// </summary>
        /// <param name="site">ISite instance</param>
        /// <param name="patientId">Patient identifiefr</param>
        /// <param name="userId">User identifier</param>
        /// <param name="msg">Message</param>
        /// <param name="losecs">Losecs value of med</param>
        public static void TriggerFile(ISite site, string patientId, int userId, string msg, int losecs)
        {
            if (site.GetOrgOption("TRIGGER_MED_CUSTOM").Equals("N"))
            {
                return;
            }

            var filePath = site.Root + "\\link\\med\\" + patientId + losecs + "_" + (new Time()).Timestamp();
            var line = userId + ":" + msg;

            FileWriter.Write(filePath, line);
        }

        /// <summary>
        /// Given a site ID, return a Site object for the ID'd site's Formulary Sharing site
        /// </summary>
        /// <param name="siteId">Current site identifier</param>
        /// <returns>Site object for Formulary Sharing site</returns>
        public static Site GetFormularyShareSite(int siteId)
        {
            return new Site(Convert.ToByte(
                new HelperDB.Select
                {
                    Sql = "SELECT frmcs FROM org WHERE site=@site",
                    Parameters = new SqlParameter[]
                    {
                        new SqlParameter("@site", SqlDbType.TinyInt) { Value = siteId }
                    }
                }.RunForScalar()
            ));
        }

        /// <summary>
        /// Add interactions and reactions to a Medication
        /// </summary>
        /// <param name="med">Medication object to check</param>
        /// <param name="ocsChartParams">OCS Chart Parameters</param>
        private void AddInteractionsAndReactions(Medication med, OcsChartParameters ocsChartParams)
        {

            var orderInteractions = ocsChartParams.orderInteractions;
            var orderReactions = ocsChartParams.orderReactions;
            var allergyReactions = ocsChartParams.allergyReactionView;

            foreach (var comp in med.Components)
            {
                var dnum = comp.ActiveId;
                if (string.IsNullOrWhiteSpace(dnum))
                {
                    continue;
                }

                var acc_inters = new List<Dictionary<string, string>>();
                var acc_reacts = new List<Dictionary<string, string>>();

                foreach (var interaction in orderInteractions)
                {
                    var rsel = new Dictionary<string, string>();
                    var view = interaction.DrugInteractionView;
                    //                    dnum = string.IsNullOrWhiteSpace(view.InteractionDrug1) ? dnum : view.InteractionDrug1;
                    var drug = view.InteractionOrderName;
                    var sev = ((FdbInteractionSeverity)view.Severity).ToString();
                    var d = !string.IsNullOrWhiteSpace(view.InteractionDrug2) ? view.InteractionDrug2 : null;
                    rsel["dnum"] = d;
                    rsel["drug"] = drug;
                    rsel["type"] = "drug";
                    rsel["interaction"] = sev + " INTERACTION";
                    rsel["sevtxt"] = sev;
                    rsel["override_reason"] = _emarOutboundChartRepository.GetOverrideReason(interaction.MedicationInteraction.OverrideReasonId); // should allow more than one?
                    acc_inters.Add(rsel);
                }

                if (allergyReactions != null)
                {
                    foreach (var reaction in allergyReactions)
                    {
                        var rsel = new Dictionary<string, string>();
                        //                    dnum = string.IsNullOrWhiteSpace(view.InteractionDrug1) ? dnum : view.InteractionDrug1;
                        var drug = reaction.OrderBrandName; // reaction.PatientAllergyName?
                        var sev = reaction.PatientAllergySeverity;
                        var d = _emarOutboundChartRepository.GetAllergyDrugIdFromPatientAllergyId(reaction.PatientAllergyId);
                        rsel["dnum"] = d;
                        rsel["drug"] = drug;
                        rsel["type"] = "alg";
                        rsel["interaction"] = "ALLERGY REACTION";
                        rsel["sevtxt"] = "ALLERGY";
                        // find the matching override reason Id from orderReactions
                        var reasonId = orderReactions.Where(order => order.PatientAllergyId == reaction.PatientAllergyId).Select(reason =>reason.OverrideReasonId).FirstOrDefault();
                        rsel["override_reason"] = _emarOutboundChartRepository.GetOverrideReason(reasonId); // should allow more than one?
                        acc_reacts.Add(rsel);
                    }
                }
                comp.Interactions = acc_inters;
                comp.Reactions = acc_reacts;
            }
        }

        public string SendChartTemplateMarkup(List<OcsPromptParameters> orderedPromptList, int siteId, long patientOrderId, int userId, ActionEnum action, long adminId, bool newOrderAdmin)
        {
            var templateMarkup = (string)null;
            var promptMarkupList = new List<string>();
            var enteredTime = (string)null;
            var isFollowup = action == ActionEnum.FollowUp;
            var updateVitals = false;
            var vitalObj = new OcsVitals();
            foreach (var orderedPrompt in orderedPromptList)
            {
                if (orderedPrompt == null)
                {
                    continue;
                }

                var promptMarkup = (string)null;
                switch (orderedPrompt.promptType)
                {
                    case PromptType.DropDownListBox: // 'S'
                    case PromptType.threeStateButton:
                        if (!string.IsNullOrWhiteSpace(orderedPrompt.chartMarkup))
                        {
                            promptMarkup = orderedPrompt.chartMarkup;
                        }
                        else
                        {
                            if (isFollowup && orderedPrompt.promptType == PromptType.DropDownListBox)
                            {
                                var vital = String.Join("", orderedPrompt.promptLabel.Split(' ', '(', ')', '~', '-'));
                                var vitalProp = vitalObj.GetType().GetProperty(vital);
                                if (vitalProp != null)
                                {
                                    vitalProp.SetValue(vitalObj, orderedPrompt.promptValue, null);
                                }
                            }
                            promptMarkup = "^S" + orderedPrompt.promptLabel + "=" + orderedPrompt.promptValue;
                        }
                        break;

                    case PromptType.CheckBox: // 'D'
                    // case PromptType.Information:
                        if (!string.IsNullOrWhiteSpace(orderedPrompt.chartMarkup))
                        {
                            promptMarkup = orderedPrompt.chartMarkup;
                        }
                        else
                        {
                            promptMarkup = "^D=" + orderedPrompt.promptLabel;
                        }
                        break;

                    case PromptType.Date: // 'C'
                    case PromptType.DateTime:
                        if (orderedPrompt.promptType == PromptType.DateTime)
                        {
                            if (orderedPrompt.promptLabel.Equals(OdsConstants.At) || orderedPrompt.promptLabel.Equals(OdsConstants.GivenAt)
                             || orderedPrompt.promptLabel.Equals(OdsConstants.DocumentedAt))
                            {
                                enteredTime = orderedPrompt.promptValue;
                                if (isFollowup && orderedPrompt.promptLabel.Equals(OdsConstants.DocumentedAt))
                                    vitalObj.enteredDatetime = GetIbexFormatDateTimeFromDTO(enteredTime);
                                // since this datetime value is already part of the chart (see newSegment below), no need to add it
                                continue;
                            }
                        }
                        // conversion from dto string to charting string.
                        orderedPrompt.promptValue = GetLongDatetimeFromDTO(orderedPrompt.promptValue);
                        if (!string.IsNullOrWhiteSpace(orderedPrompt.chartMarkup))
                        {
                            // manually update markup if IV discontinued or IV continued upon transfer type prompt
                            if (orderedPrompt.promptLabel.Equals(OdsConstants.IVDiscontinued))
                            {
                                promptMarkup = "^CMedication infusion discontinued, on=" + orderedPrompt.promptValue;
                            }
                            else if (orderedPrompt.promptLabel.Equals(OdsConstants.IVContinuedUponTransfer))
                            {
                                // TODO: add 'from <department name>' after 'transfer'
                                promptMarkup = "^CMedication infusion continued upon transfer, on=" + orderedPrompt.promptValue;
                            }
                            else
                            {
                                promptMarkup = orderedPrompt.chartMarkup + orderedPrompt.promptValue;
                            }
                        }
                        else
                        {
                            // manually update markup if IV discontinued or IV continued upon transfer type prompt
                            if (orderedPrompt.promptLabel.Equals(OdsConstants.IVDiscontinued))
                            {
                                promptMarkup = "^CMedication infusion discontinued, on=" + orderedPrompt.promptValue;
                            }
                            else if (orderedPrompt.promptLabel.Equals(OdsConstants.IVContinuedUponTransfer))
                            {
                                // TODO: add 'from <department name>' after 'transfer'
                                promptMarkup = "^CMedication infusion continued upon transfer, on=" + orderedPrompt.promptValue;
                            }
                            else
                            {
                                promptMarkup = "^C" + orderedPrompt.promptLabel + "=" + orderedPrompt.promptValue;
                            }
                        }
                        break;

                    case PromptType.FreeText: // 'C'
                    case PromptType.MultiLineFreeText:
                        if (!string.IsNullOrWhiteSpace(orderedPrompt.chartMarkup))
                        {
                            if (isFollowup && orderedPrompt.promptType == PromptType.FreeText)
                            {
                                if (orderedPrompt.promptLabel.Equals(OdsConstants.Pulse) || orderedPrompt.promptLabel.Equals(OdsConstants.BPSystolic)
                                 || orderedPrompt.promptLabel.Equals(OdsConstants.BPDiastolic) || orderedPrompt.promptLabel.Equals(OdsConstants.Temperature)
                                 || orderedPrompt.promptLabel.Equals(OdsConstants.O2Sat) || orderedPrompt.promptLabel.Equals(OdsConstants.Map)
                                 || orderedPrompt.promptLabel.Equals(OdsConstants.Respitory) || orderedPrompt.promptLabel.Equals(OdsConstants.Pain)
                                 || orderedPrompt.promptLabel.Equals(OdsConstants.EndTidal))
                                {
                                    var vital = String.Join("", orderedPrompt.promptLabel.Split(' ', '(', ')', '~', '-'));
                                    var vitalProp = vitalObj.GetType().GetProperty(vital);
                                    if (vitalProp != null)
                                    {
                                        vitalProp.SetValue(vitalObj, orderedPrompt.promptValue, null);
                                        updateVitals = true;
                                    }
                                }
                            }
                            promptMarkup = orderedPrompt.chartMarkup + orderedPrompt.promptValue;
                        }
                        else
                        {
                            promptMarkup = "^C" + orderedPrompt.promptLabel + "=" + orderedPrompt.promptValue;
                        }

                        break;

                    case PromptType.Notify: // 'C'
                        // currently only writing to chart and not actually notifying!
                        if (!int.TryParse(orderedPrompt.promptValue, out int notifyId))
                            throw new ArgumentException(
                                $"Found user ID ({orderedPrompt.promptValue}) that isn't an integer.",
                                nameof(orderedPrompt.promptType));
                        var username = _emarOutboundChartRepository.GetInternalUserName(notifyId);

                        if (!string.IsNullOrWhiteSpace(orderedPrompt.chartMarkup))
                        {
                            promptMarkup = orderedPrompt.chartMarkup + username;
                        }
                        else
                        {
                            promptMarkup = "^C" + orderedPrompt.promptLabel + "=" + username;
                        }
                        break;

                    default:
                        break;
                }
                if (!string.IsNullOrWhiteSpace(promptMarkup))
                {
                    promptMarkupList.Add(promptMarkup);
                }
            }

            if (promptMarkupList != null)
            {
                templateMarkup = String.Join("&", promptMarkupList);
            }

            var patIbex = _emarOutboundDataRepository.GetExternalPatientId(_emarOutboundChartRepository.GetPatientIdFromPatientOrderId(patientOrderId));
            var externalSiteId = (byte)_emarOutboundDataRepository.GetExternalSiteId(siteId);
            var externalUserId = _emarOutboundDataRepository.GetExternalUserId(userId);
            // update the vital data in the patients table if flag set
            if (updateVitals)
            {
                addPatientVitals(vitalObj, patIbex, externalSiteId, externalUserId, (byte)siteId);
            }

            // update the patient medication service indicator
            updatePatientMedicationIndicator(patIbex, externalSiteId);

            var emr = new EMR(externalSiteId, patIbex);
            var currentDatetime = (new Time(externalSiteId)).LongDateTime();
            var orderDatetime = !string.IsNullOrEmpty(enteredTime) ? (new Time()).LongDateTime(enteredTime) : (new Time()).LongDateTime();
            var inactive = new List<object>();
            var losecsFromOrder = GetLosecsFromPatientOrderId(patientOrderId);
            var losecsDerived = GetLosecsFromAdministrationId(adminId) ?? losecsFromOrder;

            var i = -1;
            var max_position = emr.Lines.Count - 1;
            EMR.Line newLine = null;
            foreach (EMR.Line line in emr.Lines)
            {
                i++;
                if (line.NCT() == EMR.Constants.NCT_MED_SVC)
                {

                    var losecs = line.Losecs().Split(new char[] { '&' }, StringSplitOptions.RemoveEmptyEntries);
                    if (losecs.Contains(losecsFromOrder) || losecsFromOrder.Equals(line.TableXRef()))
                    {

                        // if not active line, then skip
                        if (line.Status().Equals("I"))
                            continue;

                        // if not a new order admin and not the first administration and not the current administration, skip
                        if (newOrderAdmin == false && !losecsDerived.Equals(losecsFromOrder) && !losecsDerived.Equals(losecs[0]))
                            continue;

                        var signed = 0;
                        // if cancel or delete, determine if order signed or not and capture drug name
                        var cancelOrDelete = (action == ActionEnum.Cancel || action == ActionEnum.Delete);
                        var canDrug = (string)null;
                        if (cancelOrDelete)
                        {
                            foreach (EMR.Line sig_line in emr.Lines)
                            {
                                if (sig_line.NCT() == EMR.Constants.NCT_DIG_SIG && (String.Compare(sig_line.SysTime(), line.SysTime(), StringComparison.Ordinal) >= 0)
                                 && sig_line.User() == line.User())
                                {
                                    signed = 1;
                                    break;
                                }
                            }
                            string pattern = @"(\(.*\))*(?<drug>.*)";
                            if (Regex.IsMatch(line.PartName(), pattern))
                            {
                                Regex r = new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.Compiled);
                                canDrug = r.Match(line.PartName()).Result("${drug}");
                            }
                        }

                        newLine = line.Clone();
                        // newLine has the following three fields set so will unset them
                        newLine.LineHeader.status = null;
                        newLine.LineHeader.inactive_time = null;
                        newLine.LineHeader.inactive_user = 0;
                        // if not a new order admin, then use current data segment. Else start anew.
                        var lineData = newOrderAdmin == false ? newLine.data() : new List<EMR.Line.DataSegment>();
                        // create initial data segment based upon action, user, and documentation date
                        // action will be bolded if Follow Up
                        var actionTitle = Medication.ActionConstants.ACTION_MAP[action.ToString()];
                        if (action == ActionEnum.FollowUp)
                            actionTitle = "<b>" + actionTitle + "</b>";
                        var newSegment = new EMR.Line.DataSegment(
                            EMR.Line.DataSegment.Constants.TYPE_DROPDOWN,
                            string.Format("\n{0} by: {1} {2}", actionTitle, _emarOutboundChartRepository.GetFullNameFromUserId(userId), orderDatetime)
                        );
                        lineData.Add(newSegment);
                        // if there is template markup then create data segment using it
                        if (!string.IsNullOrWhiteSpace(templateMarkup))
                        {
                            var newSegment2 = new EMR.Line.DataSegment(
                                EMR.Line.DataSegment.Constants.TYPE_DROPDOWN,
                                string.Format("\n&{0}", templateMarkup)
                            );
                            lineData.Add(newSegment2);
                        }
                        newLine.DataSegments = lineData;
                        // if cancel or delete then do their specific handling
                        if (cancelOrDelete)
                        {
                            var operation = action == ActionEnum.Cancel ? "CANCELED" : "DELETED";
                            if (signed == 1 || action == ActionEnum.Cancel)
                            {
                                newLine.LinePart.part = "(" + operation + ")" + " " + canDrug;
                            }

                            if (signed == 0)
                            {
                                newLine.LineHeader.inactive_time = new Time().Timestamp();
                                newLine.LineHeader.inactive_user = externalUserId;
                                ////////////////////////////////////////////
                                /// if delete, see ibex4q lines 951-963 ////
                                /// UPDATE trx SET status='I',datechg='$TRXDATE' $WHERE AND status='A' AND LOSECS IN ('$losecs_sql')
                                ////////////////////////////////////////////
                                if (action == ActionEnum.Delete)
                                {
                                    newLine.LineHeader.status = "I"; // is there constant or create new one?
                                    foreach (EMR.Line follow_line in emr.Lines)
                                    {
                                        if ((String.Compare(follow_line.SysTime(), line.SysTime(), StringComparison.Ordinal) >= 0)
                                         && losecsFromOrder.Equals(follow_line.ChartXRef()))  // follow_line.TableXRef() ?
                                        {
                                            inactive.Add(follow_line.LineNumber.ToString());
                                            // also trx update, see ibex4q line 961
                                        }
                                    }
                                }
                            }
                            else
                            {
                                newLine.LineHeader.chart_xref = losecsFromOrder + "&1";  // newLine.LineHeader.table_xref ?
                            }
                        }
                        else if (action == ActionEnum.CoSign)
                        {
                            // file trx here, see ibex4q lines 1015-1025
                        }
                        // If this is not the first administration, set the table_xref to the original losecs
                        // which will allow the mapping of all related administrations during chart display
                        if (losecsFromOrder != losecsDerived)
                            newLine.LineHeader.table_xref = losecsFromOrder;
                        // Set the losecs to the proper value; original losecs if first admin otherwise the last losecs
                        // seen (from last administration)
                        newLine.LineHeader.losecs = losecsDerived;
                        // set the user time to be the time entered on the template
                        newLine.LineHeader.user_time = enteredTime != null ? enteredTime : new Time().Timestamp();
                        // inactivate the current line if not a new order admin or if from the first order administration
                        if (newOrderAdmin == false || losecsFromOrder == losecsDerived)
                            inactive.Add(line.LineNumber.ToString());
                        break;
                    }
                }
            }
            if (newLine != null)
            {
                var status = emr.WriteLine(newLine);
                if (status)
                {
                    emr.WriteLines(inactive.ToArray(), externalUserId);
                }
            }

            return "";
        }

        public string addPatientVitals(OcsVitals ocsVitals, string patIbex, byte externalSiteId, int externalUserId, byte internalSiteId)
        {
            try
            {
                var patient = _ibexContext.Patients.First(p => p.Ibex == patIbex && p.Site == externalSiteId);
                // set the tracking behavior to all
                _ibexContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.TrackAll;
                patient.VSDate = ocsVitals.enteredDatetime;
                patient.VSUser = externalUserId;
                patient.VSDia = !string.IsNullOrWhiteSpace(patient.VSDia) && string.IsNullOrWhiteSpace(ocsVitals.BPDiastolic) ?
                                "-" + patient.VSDia.Trim() + "-" : string.IsNullOrWhiteSpace(ocsVitals.BPDiastolic) ? "" : ocsVitals.BPDiastolic;
                patient.VSSys = !string.IsNullOrWhiteSpace(patient.VSSys) && string.IsNullOrWhiteSpace(ocsVitals.BPSystolic) ?
                                "-" + patient.VSSys.Trim() + "-" : string.IsNullOrWhiteSpace(ocsVitals.BPSystolic) ? "" : ocsVitals.BPSystolic;
                patient.VSPulse = !string.IsNullOrWhiteSpace(patient.VSPulse) && string.IsNullOrWhiteSpace(ocsVitals.PULSE) ?
                                  "-" + patient.VSPulse.Trim() + "-" : string.IsNullOrWhiteSpace(ocsVitals.PULSE) ? "" : ocsVitals.PULSE;
                patient.VSTemp = !string.IsNullOrWhiteSpace(patient.VSTemp) && string.IsNullOrWhiteSpace(ocsVitals.TEMPERATURE) ?
                                 "-" + patient.VSTemp.Trim() + "-" : string.IsNullOrWhiteSpace(ocsVitals.TEMPERATURE) ? "" : ocsVitals.TEMPERATURE;
                patient.VSResp = !string.IsNullOrWhiteSpace(patient.VSResp) && string.IsNullOrWhiteSpace(ocsVitals.RESPIRATORY) ?
                                 "-" + patient.VSResp.Trim() + "-" : string.IsNullOrWhiteSpace(ocsVitals.RESPIRATORY) ? "" : ocsVitals.RESPIRATORY;
                patient.VSPain = !string.IsNullOrWhiteSpace(patient.VSPain) && string.IsNullOrWhiteSpace(ocsVitals.PAIN) ?
                                 "-" + patient.VSPain.Trim() + "-" : string.IsNullOrWhiteSpace(ocsVitals.PAIN) ? "" : ocsVitals.PAIN;
                patient.VSO2 = !string.IsNullOrWhiteSpace(patient.VSO2) && string.IsNullOrWhiteSpace(ocsVitals.O2SAT) ?
                               "-" + patient.VSO2.Trim() + "-" : string.IsNullOrWhiteSpace(ocsVitals.O2SAT) ? "" : ocsVitals.O2SAT;
                patient.VSMap = !string.IsNullOrWhiteSpace(patient.VSMap) && string.IsNullOrWhiteSpace(ocsVitals.MAP) ?
                                "-" + patient.VSMap.Trim() + "-" : string.IsNullOrWhiteSpace(ocsVitals.MAP) ? "" : ocsVitals.MAP;
                patient.VSEndTidal = !string.IsNullOrWhiteSpace(patient.VSEndTidal) && string.IsNullOrWhiteSpace(ocsVitals.ENDTIDALCO2) ?
                                     "-" + patient.VSEndTidal.Trim() + "-" : string.IsNullOrWhiteSpace(ocsVitals.ENDTIDALCO2) ? "" : ocsVitals.ENDTIDALCO2;
                // indicators/levels
                // ord11, ord12, ord13, ord14, ord15, ord23, vsmaplevel, vsendtidallevel
                // IBEX::Patient::add_vitals() 
                // lib04.ibx - vscompare(), _vscheck()
                // IBEX::Vital_signs::get_vs_range_for_age()

                // get patient age in days
                if (patient.Age < 0)
                {
                    patient.Age = 0;
                }
                var UnitMultiplier = 0;
                switch (patient.AgeUnits)
                {
                    case "D":
                        UnitMultiplier = 1;
                        break;
                    case "W":
                        UnitMultiplier = 7;
                        break;
                    case "M":
                        UnitMultiplier = 30;
                        break;
                    case "Y":
                        UnitMultiplier = 365;
                        break;
                    default:
                        UnitMultiplier = 365;
                        break;
                }

                var ageInDays = (int)(patient.Age * UnitMultiplier);
                ageInDays = ageInDays == 0 ? 365 * 99 : ageInDays; // no age defaults to 999 years
                var vitalTypes = GetVitalTypes();
                // need code share site so will start with emar site_code_shares table and convert results to external site ID
                // consider rewrite to use function or perhaps create a view like what was done for medication_routes?
                var codeShareSiteId = _emarOutboundDataRepository.GetExternalSiteId(_emarOutboundChartRepository.GetCodeShareSite(internalSiteId));
                var vitalsRangeData = GetVitalSignRanges((byte)codeShareSiteId, ageInDays);
                int BPSys = -1;
                int BPDia = -1;

                if (vitalTypes.Count > 0 && vitalsRangeData.Count > 0) {
                    foreach (KeyValuePair<int, string> entry in vitalTypes)
                    {
                        if (!OdsConstants.VITALS_TO_OCS_MAP.ContainsKey(entry.Value))
                            continue;
                        var attrName = OdsConstants.VITALS_TO_OCS_MAP[entry.Value];
                        string vitalsValue;
                        var vitalProp = ocsVitals.GetType().GetProperty(attrName);
                        if (vitalProp != null)
                        {
                            vitalsValue = vitalProp.GetValue(ocsVitals)?.ToString().Trim();
                        }
                        else
                        {
                            continue; // what to do? Back to top
                        }
                        if (string.IsNullOrEmpty(vitalsValue))
                            continue;

                        decimal decimalVitalValue;
                        if (!decimal.TryParse(vitalsValue, out decimalVitalValue))
                        {
                            continue; // what to do? Back to top
                        }

                        var currentRangeData = vitalsRangeData.Where(c => c.typeName == entry.Value);
                        var panicLow = currentRangeData.Where(c => c.rangeTypeId == 1).Select(r => r.rangeValue).FirstOrDefault();
                        var normalLow = currentRangeData.Where(c => c.rangeTypeId == 2).Select(r => r.rangeValue).FirstOrDefault();
                        var normalHigh = currentRangeData.Where(c => c.rangeTypeId == 3).Select(r => r.rangeValue).FirstOrDefault();
                        var panicHigh = currentRangeData.Where(c => c.rangeTypeId == 4).Select(r => r.rangeValue).FirstOrDefault();

                        int indicatorValue;
                        if (panicLow != null && decimalVitalValue <= panicLow)
                        {
                            indicatorValue = 1;
                        }
                        else if (normalLow != null && decimalVitalValue < normalLow)
                        {
                            indicatorValue = 2;
                        }
                        else if (panicHigh != null && decimalVitalValue >= panicHigh)
                        {
                            indicatorValue = 4;
                        }
                        else if (normalHigh != null && decimalVitalValue > normalHigh)
                        {
                            indicatorValue = 3;
                        }
                        else
                        {
                            indicatorValue = 0;
                        }

                        if (!OdsConstants.VITALS_TO_PAT_MAP.ContainsKey(entry.Value))
                            continue;
                        var patAttrName = OdsConstants.VITALS_TO_PAT_MAP[entry.Value];
                        if (patAttrName.Equals(OdsConstants.BPPatIndicator))
                        {
                            if (entry.Value.Equals(OdsConstants.BPSys))
                            {
                                BPSys = indicatorValue;
                            }
                            else
                            {
                                BPDia = indicatorValue;
                            }
                            continue;
                        }

                        // Following code looks like it should work but was getting "object does not match target type" exception
                        // Therefore, replaced with switch statement block below
//                        var vitalsProp = patient.GetType().GetProperty(patAttrName);
//                        if (vitalsProp != null)
//                        {
//                            vitalProp.SetValue(patient, indicatorValue.ToString(), null);
//                        }
//                        else
//                        {
//                            continue; // what to do? Back to top
//                        }

                        // TODO : figure out why above code fails and fix
                        switch (patAttrName)
                        {
                            case "Ord12":
                                patient.Ord12 = indicatorValue.ToString();
                                break;
                            case "Ord13":
                                patient.Ord13 = indicatorValue.ToString();
                                break;
                            case "Ord14":
                                patient.Ord14 = indicatorValue.ToString();
                                break;
                            case "Ord15":
                                patient.Ord15 = indicatorValue.ToString();
                                break;
                            case "Ord23":
                                patient.Ord23 = indicatorValue.ToString();
                                break;
                            case "VSMapLevel":
                                patient.VSMapLevel = indicatorValue.ToString();
                                break;
                            case "VSEndTidalLevel":
                                patient.VSEndTidalLevel = indicatorValue.ToString();
                                break;
                            default:
                                break;
                        }
                    }
                }

                // Add constants for values 0-6
                if (BPSys >= 0 || BPDia >= 0)
                {
                    patient.Ord11 = "0";
                    if (BPSys == 1 || BPDia == 1)
                        patient.Ord11 = "5";
                    if (BPSys == 3 || BPDia == 3)
                        patient.Ord11 = "5";
                    if (BPSys == 2 || BPDia == 2)
                        patient.Ord11 = "6";
                    if (BPSys == 4 || BPDia == 4)
                        patient.Ord11 = "6";
                }

                _ibexContext.Entry(patient).Property(p => p.VSDate).IsModified = true;
                _ibexContext.Entry(patient).Property(p => p.VSUser).IsModified = true;
                _ibexContext.Entry(patient).Property(p => p.VSDia).IsModified = true;
                _ibexContext.Entry(patient).Property(p => p.VSSys).IsModified = true;
                _ibexContext.Entry(patient).Property(p => p.VSPulse).IsModified = true;
                _ibexContext.Entry(patient).Property(p => p.VSTemp).IsModified = true;
                _ibexContext.Entry(patient).Property(p => p.VSResp).IsModified = true;
                _ibexContext.Entry(patient).Property(p => p.VSPain).IsModified = true;
                _ibexContext.Entry(patient).Property(p => p.VSO2).IsModified = true;
                _ibexContext.Entry(patient).Property(p => p.VSMap).IsModified = true;
                _ibexContext.Entry(patient).Property(p => p.VSEndTidal).IsModified = true;
                // indicators/levels
                _ibexContext.Entry(patient).Property(p => p.Ord11).IsModified = true;
                _ibexContext.Entry(patient).Property(p => p.Ord12).IsModified = true;
                _ibexContext.Entry(patient).Property(p => p.Ord13).IsModified = true;
                _ibexContext.Entry(patient).Property(p => p.Ord14).IsModified = true;
                _ibexContext.Entry(patient).Property(p => p.Ord15).IsModified = true;
                _ibexContext.Entry(patient).Property(p => p.Ord23).IsModified = true;
                _ibexContext.Entry(patient).Property(p => p.VSMapLevel).IsModified = true;
                _ibexContext.Entry(patient).Property(p => p.VSEndTidalLevel).IsModified = true;
                // previously had SaveChanges() here but now will be done inside updatePatientMedicationIndicator()
            }
            catch (System.InvalidOperationException Ex)
            {
                //If there's an exception, then we'll need to pass this error up.
                throw new ArgumentException($"Patient with Id {patIbex} had an error in addPatientVitals.", nameof(patIbex));
            }
            catch (Exception Ex)
            {
                //Some other type of error.
                //Just pass the error message up the stack.
                throw new Exception(Ex.Message, Ex.InnerException);
            }   //end try/catch

            return "";
        }

        /// <summary>
        /// Update the patient medication indicator
        /// </summary>
        /// <param name="ibex">Patient ibex number</param>
        /// <param name="siteId">Site identifier</param>
        /// <returns>null on ? error, non-whitespace on execution error, empty string on success</returns>
        //
        // This method retrieves the patient medication data and converts it into Medication objects.
        // These objects then can use the GetMedStatus method and then apply the code to find the resultant
        // medication acknowledgement order status.
        // Additionally, it mimics the perl code library function get_emar_meds() in EMR.pm. 
        public string updatePatientMedicationIndicator(string ibex, int siteId)
        {
            try
            {
                var medications = new Dictionary<int, Medication>();
                var patorderids = new Dictionary<long, Medication>();
                var losecs = new List<int>();
                // get the data from the med table and save in medications, patorderids, and losecs
                var medData = GetMedications(ibex, siteId);
                foreach (var data in medData)
                {
                    var med = new Medication();
                    med.Losecs = data.losecs;
                    med.OrderDate = data.orderDate;
                    med.OrderUserId = data.orderUser;
                    med.GiveDate = data.giveDate;
                    med.GiveUserId = data.giveUser;
                    med.GiveSysdate = data.giveSysDate;
                    med.PatientOrderId = data.emarPatientOrderId;

                    medications.Add(data.losecs, med);
                    patorderids.Add(data.emarPatientOrderId, med);
                    losecs.Add(data.losecs);
                }

                var emar_medications = new Dictionary<int, List<Medication>>();
                var emar_losecs = new Dictionary<int, long>();
                var emar_cancels = new Dictionary<long, int>();
                var currentLosecs = 0;
                var emarmeds = new List<Medication>();

                // get the data from the emar_med_administrations table and save in emar_medications and emar_losecs
                var emarMedAdminData = GetEmarMedAdmins(ibex, siteId);
                foreach (var data in emarMedAdminData)
                {
                    var action = data.medAdminType;
                    if (!Medication.ActionConstants.MEDICATION_MAP.ContainsKey(action))
                        continue;

                    var med = new Medication();
                    med.Losecs = data.losecs;
                    med.PatientOrderId = data.patientOrderId;
                    med.MedAdminType = data.medAdminType;
                    med.MedAdminDate = data.medAdminDate;
                    med.MedAdminSysDate = data.medAdminSysDate;
                    med.MedAdminUser = data.medAdminUser;

                    var map = Medication.ActionConstants.MEDICATION_MAP[action];
                    foreach (string type in new[] { "Date", "UserId", "Sysdate" })
                    {
                        var medProp = med.GetType().GetProperty(map + type);
                        if (medProp != null)
                        {
                            switch (type)
                            {
                                case "Date":
                                    medProp.SetValue(med, data.medAdminDate, null);
                                    break;
                                case "UserId":
                                    medProp.SetValue(med, data.medAdminUser, null);
                                    break;
                                case "Sysdate":
                                    medProp.SetValue(med, data.medAdminSysDate, null);
                                    break;
                            }
                        }
                    }
                    if (currentLosecs != data.losecs)
                    {
                        emar_losecs.Add(data.losecs, data.patientOrderId);
                        if (currentLosecs != 0)
                        {
                            // clone since want to create new object list
                            var emarMedsNew = new List<Medication>(emarmeds.Select(x => x?.Clone()));
                            emar_medications.Add(currentLosecs, emarMedsNew);
                            emarmeds.Clear();
                        }
                    }
                    emarmeds.Add(med);
                    if (map.Equals("Cancel") || map.Equals("Delete"))
                        emar_cancels.Add(data.patientOrderId, 1);
                    currentLosecs = data.losecs;
                }
                if (currentLosecs > 0)
                    emar_medications.Add(currentLosecs, emarmeds);

                // build out second and subsequent administrations starting with base med values
                var temp_medications = new Dictionary<int, Medication>();
                foreach (var emarlosecs in emar_losecs.Keys)
                {
                    if (medications.ContainsKey(emarlosecs))
                        continue;

                    // clone since want to create new object
                    var temp_med = patorderids[emar_medications[emarlosecs][0].PatientOrderId].Clone();
                    temp_med.GiveDate = null;
                    temp_med.GiveSysdate = null;
                    temp_med.GiveUserId = 0;
                    temp_med.Losecs = emarlosecs;
                    foreach (var entry in emar_medications[emarlosecs])
                    {
                        var map = Medication.ActionConstants.MEDICATION_MAP[entry.MedAdminType];
                        foreach (string type in new[] { "Date", "UserId", "Sysdate" })
                        {
                            var medProp = temp_med.GetType().GetProperty(map + type);
                            if (medProp != null)
                            {
                                switch (type)
                                {
                                    case "Date":
                                        medProp.SetValue(temp_med, entry.MedAdminDate, null);
                                        break;
                                    case "UserId":
                                        medProp.SetValue(temp_med, entry.MedAdminUser, null);
                                        break;
                                    case "Sysdate":
                                        medProp.SetValue(temp_med, entry.MedAdminSysDate, null);
                                        break;
                                }
                            }
                        }

                        if (emar_cancels.ContainsKey(entry.PatientOrderId))
                            temp_med.Status = Medication.Constants.INACTIVE;
                    }

                    temp_medications.Add(emarlosecs, temp_med);
                }

                // build out first administrations using appropriate data from emar_med_administrations
                foreach (var losecsItem in losecs)
                {
                    if (!emar_losecs.ContainsKey(losecsItem))
                        continue;

                    // don't clone since want to modify medications object
                    var medsFromMed = medications[losecsItem];
                    foreach (var entry in emar_medications[losecsItem])
                    {
                        var map = Medication.ActionConstants.MEDICATION_MAP[entry.MedAdminType];
                        foreach (string type in new[] { "Date", "UserId", "Sysdate" })
                        {
                            var medProp = medsFromMed.GetType().GetProperty(map + type);
                            if (medProp != null)
                            {
                                switch (type)
                                {
                                    case "Date":
                                        medProp.SetValue(medsFromMed, entry.MedAdminDate, null);
                                        break;
                                    case "UserId":
                                        medProp.SetValue(medsFromMed, entry.MedAdminUser, null);
                                        break;
                                    case "Sysdate":
                                        medProp.SetValue(medsFromMed, entry.MedAdminSysDate, null);
                                        break;
                                }
                            }
                        }

                        if (emar_cancels.ContainsKey(entry.PatientOrderId))
                            medsFromMed.Status = Medication.Constants.INACTIVE;
                    }
                }

                // add the second and subsequent order administrations to medications
                foreach (var losecsItem in temp_medications.Keys)
                {
                    medications.Add(losecsItem, temp_medications[losecsItem]);
                }

                // finished with getting the medications from the med and emar_med_administrations tables
                var statuses = new Dictionary<string, Dictionary<string, int>>
                {
                    { "color", new Dictionary<string, int>() },
                    { "code", new Dictionary<string, int>() }
                };
                // get the medication status' for each medication
                foreach (var key in medications.Keys)
                {
                    var med = medications[key];
                    var medStatus = med.GetMedStatus();
                    if (medStatus.ContainsKey("code"))
                    {
                        if (!statuses["code"].ContainsKey(medStatus["code"]))
                        {
                            statuses["code"][medStatus["code"]] = 1;
                        }
                        else
                        {
                            statuses["code"][medStatus["code"]]++;
                        }
                    }
                }

                // get the patient indicator (color) by finding the highest priority status in the action list
                var index = "";
                foreach (var lookup in Medication.ActionConstants.SORTED_STATUSES)
                {
                    if (lookup.ContainsKey("for") && !string.IsNullOrWhiteSpace(lookup["for"]) && !lookup["for"].Equals("indicator"))
                        continue;

                    var color = lookup.ContainsKey("color") ? lookup["color"] : null;
                    if (color != null)
                    {
                        var code = lookup.ContainsKey("code") && !string.IsNullOrWhiteSpace(lookup["code"]) ? lookup["code"] : null;
                        if (code != null)
                        {
                            if (statuses["code"].ContainsKey(code) && statuses["code"][code] > 0)
                                index = color;
                        }
                        else if (statuses["color"].ContainsKey(color) && statuses["color"][color] > 0)
                        {
                            index = color;
                        }
                    }

                    if (!string.IsNullOrWhiteSpace(index))
                        break;
                }

                // determine if a modify was made in addPatientVitals() to Patients 
                var changeTrackingChanges = _ibexContext.ChangeTracker.HasChanges();
                // if index was set or there were changes detected, then update the patient record
                if (!string.IsNullOrWhiteSpace(index) || changeTrackingChanges) {
                    // find the patient record
                    var patient = _ibexContext.Patients.First(p => p.Ibex == ibex && p.Site == siteId);
                    // set the tracking behavior to all - no harm if set already
                    _ibexContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.TrackAll;
                    // if a value found above then use it else use previous value
                    patient.Ord30 = !string.IsNullOrWhiteSpace(index) ? index : patient.Ord30;
                    _ibexContext.Entry(patient).Property(p => p.Ord30).IsModified = true;
                    _ibexContext.SaveChanges();
                    // set the tracking behavior to none
                    _ibexContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
                }

            }
            catch (System.InvalidOperationException Ex)
            {
                //If there's an exception, then we'll need to pass this error up.
                throw new ArgumentException($"Patient with Id {ibex} had an error in updatePatientMedicationIndicator.", nameof(ibex));
            }
            catch (Exception Ex)
            {
                //Some other type of error.
                //Just pass the error message up the stack.
                throw new Exception(Ex.Message, Ex.InnerException);
            }   //end try/catch

            return "";
        }

        private Dictionary<int, string> GetVitalTypes()
        {
                var vtypes = _ibexContext.VitalTypes.Where(v => v.Name != "Weight (kg)");
                var ret = new Dictionary<int, string>();
                foreach (var vt in vtypes)
                    ret.Add(vt.Id, vt.Name);
                return ret;
        }

        public List<OcsVitalsRangeData> GetVitalSignRanges(byte siteId, int age)
        {
            var query = (from vr in _ibexContext.VitalRanges
                        join vt in _ibexContext.VitalTypes on vr.TypeId equals vt.Id
                        where vr.Site == siteId && vr.AgeStart <= age && vr.AgeEnd > age
                        select new OcsVitalsRangeData
                        {
                            rangeValue = vr.Value,
                            typeName = vt.Name,
                            rangeTypeId = vr.RangeTypeId
                        }).ToList();

            return query;
        }

        public string GetIbexFormatDateTimeFromDTO(string inputDTO)
        {
            DateTimeOffset offset;
            if (!DateTimeOffset.TryParse(inputDTO, out offset))
            {
                offset = DateTimeOffset.Now; // error handling here?
            }

            return offset.ToString("yyyyMMddHHmm") ?? new Time().Timestamp().Substring(0, 12);
        }

        public string GetLosecsFromPatientOrderId(long patientOrderId)
        {
            var query = from m in _ibexContext.Medications
                        where m.EmarPatientOrderId == patientOrderId
                        select m.Losecs.ToString();
            return query.FirstOrDefault();
        }

        public string GetLongDatetimeFromDTO(string inputDTO)
        {
            if (string.IsNullOrWhiteSpace(inputDTO))
                return null;

            DateTimeOffset offset;
            DateTimeOffset.TryParse(inputDTO, out offset);
            var longDatetimeStr = offset.ToString("yyyyMMddHHmmss");
            longDatetimeStr = (new Time()).LongDateTime(longDatetimeStr);
            return longDatetimeStr;
        }

        public string GetLosecsFromAdministrationId(long adminId)
        {
            if (adminId == 0)
                return null;
            var query = from m in _ibexContext.EmarMedicationAdministrations
                        where m.OrderAdministrationsId == adminId
                        select m.Losecs.ToString();
            return query.FirstOrDefault();
        }

        public List<OcsMedicationData> GetMedications(string ibex, int site)
        {
            var query = (from m in _ibexContext.Medications
                         where m.Site == site && m.Ibex.Equals(ibex)
                         select new OcsMedicationData
                         {
                             losecs = m.Losecs,
                             orderDate = m.OrderDate,
                             orderUser = m.OrderUser,
                             giveDate = m.GiveDate,
                             giveUser = m.GiveUser,
                             giveSysDate = m.GiveSysDate,
                             emarPatientOrderId = m.EmarPatientOrderId,
                             status = m.Status
                         }).ToList();

            return query;
        }

        public List<OcsEmarMedAdminData> GetEmarMedAdmins(string ibex, int site)
        {
            var query = (from e in _ibexContext.EmarMedicationAdministrations
                         where e.Site == site && e.Ibex.Equals(ibex)
                         select new OcsEmarMedAdminData
                         {
                             losecs = e.Losecs,
                             medAdminType = e.MedAdminType,
                             medAdminDate = e.MedAdminDate,
                             medAdminUser = e.MedAdminUser,
                             medAdminSysDate = e.MedAdminSysdate,
                             patientOrderId = e.PatientOrderId
                         }).ToList();

            return query;
        }
    }
}


