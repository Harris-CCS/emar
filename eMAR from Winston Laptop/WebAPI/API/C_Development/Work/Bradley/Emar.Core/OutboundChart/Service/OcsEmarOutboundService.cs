using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Emar.Core.Helpers;
using Emar.Core.Medications.Model;
using Emar.Core.OutboundChart.Model;
using Emar.Core.OutboundChart.Repository;
using Emar.Core.OutboundData.Model;
using Emar.Core.OutboundData.Repository;
using Emar.Core.Sites.Repository;
using Emar.Core.Templates.Model;
using Emar.Data;
using HelperChart = Emar.Core.Helpers.Chart;
using HelperDB = Emar.Core.Helpers.DB;
using System.Diagnostics;
using System.Text.RegularExpressions;
using Emar.Core.Orders.Repository;

namespace Emar.Core.OutboundChart.Service
{
    public class OcsEmarOutboundService : IOcsEmarOutboundService
    {
        private readonly IbexContext _ibexContext;
        private readonly IEmarOutboundChartRepository _emarOutboundChartRepository;
        private readonly IEmarOutboundDataRepository _emarOutboundDataRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly ISiteRepository _siteRepository;

        public OcsEmarOutboundService(IbexContext ibexContext, IEmarOutboundChartRepository emarOutboundChartRepository, IEmarOutboundDataRepository emarOutboundDataRepository, IOrderRepository orderRepository,
            ISiteRepository siteRepository)
        {
            _ibexContext = ibexContext;
            _emarOutboundChartRepository = emarOutboundChartRepository;
            _emarOutboundDataRepository = emarOutboundDataRepository;
            _orderRepository = orderRepository;
            _siteRepository = siteRepository;
        }

        //        public async System.Threading.Tasks.Task<string> SendChartLinesAsync(OcsChartParameters ocsChartParams)
        public string SendChartLinesAsync(OcsChartParameters ocsChartParams)
        {

            //           if (!user.HasWritePermission(Permission.MED_SVC))
            //               return null;

            var patient = new Patient();
            // fill out the patient object for use later
            patient.Ibex = _emarOutboundDataRepository.GetExternalPatientId((long)ocsChartParams.patiendId);
            var patientDataForIbex = _emarOutboundChartRepository.GetPatientDataForIbex((long)ocsChartParams.patiendId);
            patient = MapPatientDataForIbex(patient, patientDataForIbex);
            var now = DateTime.Now.ToString("yyyyMMddHHmmss"); // note: not part of CreateMeds
            var externalSiteId = (byte)_emarOutboundDataRepository.GetExternalSiteId(ocsChartParams.site);
            var site = new Site(externalSiteId); // need to define other site attrs?
            var user = new User();
            user.Id = _emarOutboundDataRepository.GetExternalUserId(ocsChartParams.user);
            user.SiteId = externalSiteId;
            // determine if active patient by trying to retrieve patient from pat table
            var activePatient = _ibexContext.Patients.FirstOrDefault(p => p.Ibex == patient.Ibex && p.Site == externalSiteId) != null;
            var losecs = Int32.Parse(ocsChartParams.losecs.ToString("ddHHmmss")); // must match what is in ODS

            var notes = ocsChartParams.medNotes;
            //            if (serviceOptions != null && serviceOptions.Count > 0)
            //                notes += (!string.IsNullOrWhiteSpace(notes) ? "\n" : "") + string.Join(", ", serviceOptions);

            var meds = new List<Medication>();
            var medNames = new List<string>(); // note: not part of CreateMeds
            var qlMedNums = new List<string>(); // note: not part of CreateMeds
            var required = new List<string>(); // note: not part of CreateMeds
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
                    component.DrugRoute = medDetails.DrugRoute;
                    component.PackagingId = ocsChartParams.Ndc ?? medDetails.PackagingId;
                    component.Interactions = null;
                    component.Reactions = null;
                    component.RXNorm = med.DrugDB.GetInstance().GetRxcuiByDrugId(medDetails.DrugId);
                    // use the NDC/PackagingId determined above to get the service
                    component.Service = _emarOutboundDataRepository.GetServiceByNdc(component.PackagingId);
                    component.Id = Convert.ToInt32(_emarOutboundChartRepository.GetMedDetailsId(patient.Ibex, site.Id, losecs, false, null));
                    // verify if correct component dose value or not
                    component.EnteredDose = string.IsNullOrWhiteSpace(ocsChartParams.Dose) ? "*" : ocsChartParams.Dose.Replace(".000", "");
                    // component.DrugFormId = // medispan only?
                    // component.DrugDBType = 
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
                        comboComponent.DrugRoute = medDetails.DrugRoute;
                        comboComponent.PackagingId = medDetails.PackagingId;
                        comboComponent.Interactions = null;
                        comboComponent.Reactions = null;
                        comboComponent.RXNorm = med.DrugDB.GetInstance().GetRxcuiByDrugId(medDetails.DrugId);
                        // combo meds will rely on medDetails packaging id's retrieved from the detailIds list for now
                        // should use _emarOutboundChartRepository.GetServiceCodesFromFormulary(int medicationId, int siteId) instead?
                        comboComponent.Service = _emarOutboundDataRepository.GetServiceByNdc(medDetails.PackagingId);
                        comboComponent.Id = Convert.ToInt32(_emarOutboundChartRepository.GetMedDetailsId(patient.Ibex, site.Id, losecs, false, medDetails.BrandName));
                        comboComponent.EnteredDose = medDetails.EnteredDose.Replace(".000", "");
                        comboComponent.EnteredUnit = medDetails.EnteredUnit;
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
            med.Dose = string.IsNullOrWhiteSpace(ocsChartParams.Dose) ? "*" : ocsChartParams.Dose.Replace(".000", "");
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
            med.Schedule = med.Frequency; // using frequency description for now
            med.Indication = ocsChartParams.AntiMicrobialIndication;
            med.IndicationDescription = ocsChartParams.AntiMicrobialIndicationText;
            //            med.Authentication = authType;
            // TODO?: get the complete data for the med (ibex..emar_med_administrations) for use with the mtb 'M' color calculation

            meds.Add(med);
            //            required.AddRange(med.CheckRequired(med.Name, type, order.Overrides.Count > 0));

            ////
            //// TODO: Substitute med creation block above for CreateMeds call like in SendChartTemplateMarkup ////
            ///        Why repeat the code and maintain in two places?
            ////

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
                var EMR = new EMR(site.Id, patient.Ibex, true, false, !activePatient);
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
                        var line = _emarOutboundChartRepository.ChartEntry(patient, medication, now, ocsChartParams.PharmVerifStatus, ocsChartParams.PRNIndication);
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
                        return "EMR.WriteLines failed in SendChartLinesAsync";
                    }
                }
                catch (SqlException ex)
                {
                    LogException(ex);
                    DTFL.Write(site.Id, user.Id, ex, "Medication Order Save");
                    transaction.Rollback();
                    return "SqlException in SendChartLinesAsync";
                }
                catch (Exception ex)
                {
                    LogException(ex);
                    DTFL.Write(site.Id, user.Id, "Medication Order Save failure - " + ex.ToString());
                    transaction.Rollback();
                    return "Exception in SendChartLinesAsync";
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
                        var ordererName = _emarOutboundChartRepository.GetFullNameFromUserId(ocsChartParams.user);
                        var message = "You are the ordering physician listed for a medication order entered by " + ordererName + " for " + patient.GetName() + ".\n\n" + string.Join("\n", medNames);
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

            CreateTrigger(site, patient, user.Id, "place", meds);
            return "";
        }

        private void CreateTrigger(ISite site, Patient patient, int userId, string action, List<Medication> meds, string opt = null)
        {
            try
            {
                if (opt == null)
                {
                    opt = site.GetOrgOption("TRIGGER_MED_HSF");
                }
                if (opt.Equals("N"))
                {
                    return;
                }

                // Delete action currently does not map to existing constant - needed in CheckActionForTrigger() below
                if (action.Equals("delete"))
                    action = "del";

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

                    if ((opt.Equals(Trigger.Constants.TRIGGER_MED_SVC_BOTH) || opt.Equals(triggerId)) && ((patAge == 0 && patAgeUnit == Patient.Constants.AGEUNIT_DAYS) || (minAge <= patAge && maxAge >= patAge)))
                    {
                        var frmCSSite = GetFormularyShareSite(site.Id); // currently looking at org.frmcs and not emar.site_code_share
                        var formulary = new Formulary(frmCSSite, null, null, "med");

                        foreach (var med in meds)
                        {
                            if (CheckFormularyForTrigger(site, med, formulary, currentTriggerSettings) && CheckActionForTrigger(action, currentTriggerSettings))
                            {
                                var error = CreateDBTrigger(site, patient.Ibex, userId, triggerId, med, formulary);
                                if (!string.IsNullOrWhiteSpace(error))
                                {
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogException(ex);
                //Just pass the error message up the stack.
                throw new Exception(ex.Message, ex.InnerException);
            }   //end try/catch
        }

        private bool CheckFormularyForTrigger(ISite site, Medication med, Formulary formulary, Dictionary<string, string> triggerSettings)
        {
            var triggerIn = triggerSettings.ContainsKey("in_form") ? triggerSettings["in_form"] : "N";
            var triggerOut = triggerSettings.ContainsKey("out_form") ? triggerSettings["out_form"] : "N";
            var triggerMach = triggerSettings.ContainsKey("machine_form") ? triggerSettings["machine_form"] : "N";

            // Don't send anything if they're all turned off.
            // Free text should be considered never on formulary
            if ((triggerIn.Equals("N") && triggerOut.Equals("N") && triggerMach.Equals("N")) || med.IsFreeText())
            {
                return false;

                // If the machine setting is Don't send exact match (the most powerful switch) and one of the others is on, we'll send.
                // Combo meds are always considered on formulary
            }
            else if ((!triggerMach.Equals("D") && (triggerIn.Equals("A") || triggerOut.Equals("A"))) || med.IsCombo())
            {
                return true;
            }

            var ndc = "";
            var components = med.GetComponents();
            if (components.Count > 0)
            {
                ndc = components[0].PackagingId;
                var flags = formulary.GetFlags(ndc);

                var inpat = flags[Formulary.Constants.INPAT_TYPE];
                var outpat = flags[Formulary.Constants.OUTPAT_TYPE];
                var pyxis = flags[Formulary.Constants.PYXIS_TYPE];

                if (triggerMach.Equals("D") && pyxis > Formulary.Constants.EQUIV_MATCH)
                {
                    return false;
                }

                return
                    (triggerIn.Equals("F") && inpat > Formulary.Constants.NON_MATCH) ||
                    (triggerOut.Equals("F") && outpat > Formulary.Constants.NON_MATCH) ||
                    ((triggerMach.Equals("F") || triggerMach.Equals("D")) && pyxis > Formulary.Constants.NON_MATCH) ||
                    (triggerIn.Equals("M") && inpat > Formulary.Constants.EQUIV_MATCH) ||
                    (triggerOut.Equals("M") && outpat > Formulary.Constants.EQUIV_MATCH);
            }

            return false;
        }

        private bool CheckActionForTrigger(string action, Dictionary<string, string> triggerSettings)
        {
            var opt = "";
            if (action.StartsWith("c_"))
            {
                opt = "order_custom_" + action.Substring(2);
            }
            else
            {
                opt = Medication.Constants.OPTS_MAPPING.ContainsKey(action) ? Medication.Constants.OPTS_MAPPING[action] : "";
            }

            return (!string.IsNullOrWhiteSpace(opt) && triggerSettings.ContainsKey(opt) && triggerSettings[opt].Equals("Y"));
        }

        private string CreateDBTrigger(ISite site, string patientId, int userId, string triggerId, Medication med, Formulary formulary)
        {
            var xml = med.GetXML(formulary);
            var interfaceName = triggerId.Equals(Trigger.Constants.TRIGGER_MED_SVC_IMAGE) ? Trigger.Constants.MEDICATION_SERVICE_IMAGE : Trigger.Constants.MEDICATION_SERVICE_HL7;

            var error = Trigger.Create(site, patientId, userId, xml.ToString(), interfaceName, "emar");
            if (error != null)
            {
                DTFL.Write(site.Id, userId, "Cannot create ensemble trigger for Medication Services interface. IBEX: " + patientId + " ERROR: " + error);
            }

            return error;
        }

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
                        //Not drug, since drug is what we ordered.
                        //PatientAllergyName seems to be the allergy.
                        //Winston Murdock, 03/04/2022.
                        rsel["drug"] = reaction.PatientAllergyName;
                        rsel["type"] = "alg";
                        rsel["interaction"] = "ALLERGY REACTION";
                        rsel["sevtxt"] = "ALLERGY";
                        // find the matching override reason Id from orderReactions
                        var reasonId = orderReactions.Where(order => order.PatientAllergyId == reaction.PatientAllergyId).Select(reason => reason.OverrideReasonId).FirstOrDefault();
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
            try
            {
                //Move the siteNow calculation up to the top of this method so that
                //we can use it throughout the whole thing.
                ///Winston Murdock, 03/30/2022.  PC-27144
                ///
                //Get the timezone of the current site.
                string timeZoneName = _siteRepository.GetSiteTimeZone(siteId);

                //Get now in the current site's time zone.
                var siteNow = timeZoneName.NowWithTimeZoneOffset();
                // Get the ibex string format of the siteNow DateTimeOffset
                var siteNowIbex = siteNow.ToString("yyyyMMddHHmmss");

                var promptMarkupList = new List<string>();
                var enteredTime = (string)null;
                var isFollowup = action == ActionEnum.FollowUp;
                var updateVitals = false;
                var vitalObj = new OcsVitals();
                var notifyUsers = false;
                var notifyIds = new List<int>();
                Site site = null;

                //In case this is a template with no prompts (for example
                //pharmacy verification), set enteredTime = siteNowIbex.
                enteredTime = siteNowIbex;

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
                            // need to revisit these cases
                            if (!string.IsNullOrWhiteSpace(orderedPrompt.chartMarkup) && (!isFollowup || orderedPrompt.promptType != PromptType.DropDownListBox))
                            {
                                promptMarkup = orderedPrompt.chartMarkup;
                            }
                            else
                            {
                                var promptLabel = orderedPrompt.promptLabel;
                                // include a check for vitals specific labels. Currently, they all start with ' ~~('.
                                // Note: if the prompt text/labels change for vitals in the template standard data loader,
                                //         the check will need to make a corresponding change here.
                                if (isFollowup && orderedPrompt.promptType == PromptType.DropDownListBox
                                 && promptLabel.StartsWith(" ~~("))
                                {
                                    var vital = String.Join("", orderedPrompt.promptLabel.Split(' ', '(', ')', '~', '-'));
                                    var vitalProp = vitalObj.GetType().GetProperty(vital);
                                    if (vitalProp != null)
                                    {
                                        vitalProp.SetValue(vitalObj, orderedPrompt.promptValue, null);
                                    }
                                    // empty out the label cause we do not want to see it in the chart
                                    promptLabel = "";
                                }
                                // need promptMarkup when for a vitals followup or a bag number hung?
                                promptMarkup = "^S" + promptLabel + "=" + orderedPrompt.promptValue;
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

                                    //convert enteredTime to the current time zone.
                                    //Needed so that any of the entries to the patient's chart
                                    //show in the correct time zone.
                                    //Winston Murdock, 03/30/2022.  PC=-27144
                                    enteredTime = GetIbexFormatDateTimeFromDTO(enteredTime, siteNow);

                                    if (isFollowup && orderedPrompt.promptLabel.Equals(OdsConstants.DocumentedAt))
                                        //We've already converted enteredTime to a DateTimeOffset above.
                                        //Just use it here.
                                        //vitalObj.enteredDatetime = GetIbexFormatDateTimeFromDTO(enteredTime, siteNow);
                                        vitalObj.enteredDatetime = enteredTime;
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
                                     || orderedPrompt.promptLabel.Equals(OdsConstants.EndTidal) || orderedPrompt.promptLabel.Equals(OdsConstants.On))
                                    {
                                        var vital = String.Join("", orderedPrompt.promptLabel.Split(' ', '(', ')', '~', '-'));
                                        var vitalProp = vitalObj.GetType().GetProperty(vital);
                                        if (vitalProp != null)
                                        {
                                            vitalProp.SetValue(vitalObj, orderedPrompt.promptValue, null);
                                            if (!orderedPrompt.promptLabel.Equals(OdsConstants.On))
                                                updateVitals = true;
                                        }
                                    }
                                }
                                // Need to handle prompt label OdsConstants.On differently
                                promptMarkup = orderedPrompt.chartMarkup + orderedPrompt.promptValue;
                            }
                            else
                            {
                                promptMarkup = "^C" + orderedPrompt.promptLabel + "=" + orderedPrompt.promptValue;
                            }

                            break;

                        case PromptType.Notify: // 'C'
                            var notifyNums = orderedPrompt.promptValue.Split(',');
                            foreach (string userNum in notifyNums)
                            {
                                if (!int.TryParse(userNum, out int notifyId))
                                    throw new ArgumentException(
                                        $"Found user ID ({orderedPrompt.promptValue}) that isn't an integer.",
                                        nameof(orderedPrompt.promptType));
                                notifyIds.Add(notifyId);
                                notifyUsers = true;
                            }
                            break;

                        default:
                            break;
                    }

                    if (!string.IsNullOrWhiteSpace(promptMarkup))
                    {
                        promptMarkupList.Add(promptMarkup);
                    }
                } //end loop through all prompts

                var patId = _emarOutboundChartRepository.GetPatientIdFromPatientOrderId(patientOrderId);
                var patIbex = _emarOutboundDataRepository.GetExternalPatientId(patId);
                var externalSiteId = (byte)_emarOutboundDataRepository.GetExternalSiteId(siteId);
                var externalUserId = _emarOutboundDataRepository.GetExternalUserId(userId);
                // determine if active patient by trying to retrieve patient from pat table
                var activePatient = _ibexContext.Patients.FirstOrDefault(p => p.Ibex == patIbex && p.Site == externalSiteId) != null;
                var ordererName = _emarOutboundChartRepository.GetFullNameFromUserId(userId);
                List<Medication> meds = null;
                PatientOrderDataForMeds patientOrderParams = null;

                // add trx entry if give action and there is a service code in the associated formulary
                if (action == ActionEnum.Give)
                {
                    var ret = addTrxEntryForFormulary(patientOrderId, patId, patIbex, externalSiteId, externalUserId, enteredTime, siteId);
                    if (!string.IsNullOrWhiteSpace(ret))
                    {
                        LogError(patIbex, externalSiteId, patientOrderId, adminId, ret, action.ToString());
                    }
                }

                // All pharmacy verification handling inside of this file will use the current value of PharmacyVerificationStatus
                //   of the PatientOrder entity as it sole check. It is essential that it be set beforehand and that it be correct. 
                var pharmVerificationStatus = _emarOutboundChartRepository.GetPharmVerificationStatus(patientOrderId);

                // Important Note:
                // The following section of code expects that only one of the methods within is called.
                // If this is not the case, then an error due to multiple modifications of the same IbexPatient object may result
                // Therefore, changed code to if/else design
                //
                // BEGIN SECTION
                // First, handle the pharmacy verification template case first - other pharmacy verification handling will occur
                //   inside the updatePatientMedicationIndicator() call below.
                if (action == ActionEnum.PharmVerification && activePatient)
                {
                    var ret = updatePatientPharmacyVerification(patId, patIbex, externalSiteId);
                    if (!string.IsNullOrWhiteSpace(ret))
                    {
                        LogError(patIbex, externalSiteId, patientOrderId, adminId, ret, action.ToString());
                    }
                }
                // update the vital data in the patients table if flag set - can only be from a followup action (implicit check)
                else if (updateVitals && activePatient)
                {
                    var ret = addPatientVitals(vitalObj, patIbex, externalSiteId, externalUserId, (byte)siteId);
                    if (!string.IsNullOrWhiteSpace(ret))
                    {
                        LogError(patIbex, externalSiteId, patientOrderId, adminId, ret, action.ToString());
                    }
                }
                // update the patient medication service indicator for all other action types
                else if (activePatient)
                {
                    var ret = updatePatientMedicationIndicator(patId, patIbex, externalSiteId);
                    if (!string.IsNullOrWhiteSpace(ret))
                    {
                        LogError(patIbex, externalSiteId, patientOrderId, adminId, ret, action.ToString());
                    }
                }
                // END SECTION

                var losecsFromOrder = GetLosecsFromPatientOrderId(patientOrderId);
                var losecsDerived = GetLosecsFromAdministrationId(adminId) ?? losecsFromOrder;

                // Trigger XML creation
                // Process only the eight actions contained in MEDICATION_MAP
                if (Medication.ActionConstants.MEDICATION_MAP.ContainsKey(action.ToString()))
                {
                    site = new Site(externalSiteId);
                    var opt = site.GetOrgOption("TRIGGER_MED_HSF");
                    // check if valid option value before any additional processing takes place
                    if (!opt.Equals("N"))
                    {
                        var patient = new Patient();
                        patient.Ibex = patIbex;
                        var patientDataForIbex = _emarOutboundChartRepository.GetPatientDataForIbex(patId);
                        patient = MapPatientDataForIbex(patient, patientDataForIbex);
                        patientOrderParams = _emarOutboundChartRepository.GetPatientOrderDataForMeds(patientOrderId);
                        patientOrderParams.PatientOrderAdminId = adminId;

                        // create the medication list for use by the interface trigger creation method
                        meds = CreateMeds(site, patient, externalUserId, patientOrderParams, losecsDerived, losecsFromOrder != losecsDerived);

                        CreateTrigger(site, patient, externalUserId, Medication.ActionConstants.MEDICATION_MAP[action.ToString()].ToLower(), meds, opt);
                    }
                }

                var emr = new EMR(externalSiteId, patIbex, false, false, !activePatient);
                if (emr == null)
                {
                    // failed to create emr object so log error
                    var errMsg = "new EMR failed - could not create object.";
                    LogError(patIbex, externalSiteId, patientOrderId, adminId, errMsg, action.ToString());
                }
                //We've already converted enteredTime from a string (yyyyMMddhhmmss) to a DateTimeOffset.
                //Don't need to do any other operations on it here.
                //Not sure why this line is here.
                //We don't seem to use this variable anywhere.
                //Leaving it for now.
                var currentDatetime = (new Time()).LongDateTime(siteNowIbex);

                //Yes, the mixed casing is incorrect here.
                //It's easier to leave it than fix it all places throughout this method.
                var orderDatetime = !string.IsNullOrEmpty(enteredTime) ? (new Time()).LongDateTime(enteredTime) : (new Time()).LongDateTime();

                var inactive = new List<object>();

                var i = -1;
                var max_position = emr.Lines.Count - 1;
                var foundLosecs = false;
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

                            foundLosecs = true;
                            var signed = 0;
                            // if cancel or delete, determine if order signed or not and capture drug name
                            var cancelOrDelete = (action == ActionEnum.Cancel || action == ActionEnum.Delete);
                            // if discontinue or discontinued, capture drug name
                            var discontinueOrDiscontinued = (action == ActionEnum.OrderDiscontinue || action == ActionEnum.CompleteDiscontinue);
                            var canDrug = (string)null;
                            if (cancelOrDelete || discontinueOrDiscontinued)
                            {
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
                                }
                                string pattern = @"(\(.*\))*(?<drug>.*)";
                                if (Regex.IsMatch(line.PartName(), pattern))
                                {
                                    Regex r = new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.Compiled);
                                    canDrug = r.Match(line.PartName()).Result("${drug}");
                                }
                            }

                            // make a clone of the original line - currently shallow since don't have a proper deep copy method
                            newLine = line.Clone();
                            // until a deep copy can be performed, create a new LineHeader object and set it in newLine
                            // w/o this new object, a reference would be used instead which caused unwanted side effects
                            var header = new EMR.Line.Header {
                                audio = line.Audio(),
                                chart_xref = line.ChartXRef(),
                                doc_id = line.DocId(),
                                inactive_time = "",
                                inactive_user = 0,
                                level = line.Level(),
                                losecs = line.Losecs(),
                                no_link = line.NoLink() ? "1" : "0", // doesn't seem to follow into chart
                                status = "",
                                sys_time = siteNowIbex,
                                table_xref = line.TableXRef(),
                                user = line.User(),
                                user_time = line.UserTime()
                            };
                            newLine.LineHeader = header;

                            // similar to above, create a new DataSegments object (list of DataSegment objects)
                            // since shallow copy in Clone makes a reference
                            var dataPieces = newLine.Data.Split(EMR.Constants.DelimiterData.ToString());
                            List<EMR.Line.DataSegment> parsedData = new List<EMR.Line.DataSegment> ();
                            foreach (string dataPiece in dataPieces)
                            {
                                parsedData.Add(new EMR.Line.DataSegment(dataPiece));
                            }
                            newLine.DataSegments = parsedData;

                            var pharmVerificationAction = action == ActionEnum.PharmVerification;
                            // if not a new order admin or if a pharm verif action, then use current data segment. Else create a new list of DataSegment objects
                            var lineData = (newOrderAdmin == false || pharmVerificationAction) ? newLine.data() : new List<EMR.Line.DataSegment>();
                            // pharmacy verification requires special chart text manipulation. The "needed" text line
                            // will be replaced with the "verified" text line downstream
                            if (pharmVerificationAction)
                            {
                                // start with a fresh data segment list
                                var pharmVerifDataSegs = new List<EMR.Line.DataSegment>();
                                foreach (var segment in lineData)
                                {
                                    var lineString = segment.ValueSegments.value;
                                    // check if this string contains the string to replace
                                    var startpos = lineString.IndexOf("Pharmacist Verification Needed");
                                    if (startpos == -1)
                                    {
                                        // doesn't contain the string so add to list and continue
                                        pharmVerifDataSegs.Add(segment);
                                        continue;
                                    }
                                    // look for the line ending newline char
                                    string newlinechar = "\n";
                                    char[] chars = newlinechar.ToCharArray();
                                    var endpos = lineString.IndexOfAny(chars, startpos);
                                    if (endpos == -1)
                                        // should not happen
                                        continue;
                                    // save the resultant line with the found ("needed") string removed
                                    var replacedLine = lineString.Remove(startpos, endpos - startpos + 1);
                                    // create new data segment using replacedLine and add to list
                                    var pharmVerifSeg = new EMR.Line.DataSegment(
                                        EMR.Line.DataSegment.Constants.TYPE_DROPDOWN,
                                        replacedLine);
                                    pharmVerifDataSegs.Add(pharmVerifSeg);
                                }
                                // replace the old list of data segments with the new list
                                lineData = pharmVerifDataSegs;
                            }

                            // use the ACTION_MAP to convert action to action text
                            var actionTitle = Medication.ActionConstants.ACTION_MAP[action.ToString()];
                            // action will be bolded if Follow Up
                            if (action == ActionEnum.FollowUp)
                                actionTitle = "<b>" + actionTitle + "</b>";

                            // create initial data segment based upon action, user, and documentation date
                            var newSegment = new EMR.Line.DataSegment(
                                EMR.Line.DataSegment.Constants.TYPE_DROPDOWN,
                                string.Format("\n{0} by: {1} {2}", actionTitle, ordererName, orderDatetime)
                            );
                            lineData.Add(newSegment);

                            // if there is template markup then create data segments using it
                            if (promptMarkupList != null && promptMarkupList.Count > 0)
                            {
                                foreach (var promptMarkup in promptMarkupList)
                                {
                                    lineData.Add(new EMR.Line.DataSegment(promptMarkup));
                                }
                            }

                            // per requirements for trigger/re-trigger pharmacy verification, set variable to proper status
                            var resendPharmVerif = pharmVerificationStatus == 1 ? true : false;
                            // if determined that pharmacy verification needs to be sent to chart, then do so
                            if (resendPharmVerif)
                            {
                                var pharmVerifText = "Pharmacist Verification Needed";
                                var pharmVerifSegment = new EMR.Line.DataSegment(
                                    EMR.Line.DataSegment.Constants.TYPE_DROPDOWN,
                                    string.Format("\n{0}: Entered By {1} {2}", pharmVerifText, ordererName, orderDatetime)
                                );
                                lineData.Add(pharmVerifSegment);
                            }

                            newLine.DataSegments = lineData;
                            // update Data attribute also since used in duplicate line check
                            newLine.Data = String.Join(EMR.Constants.DelimiterData.ToString(), newLine.DataSegments);

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
                                    //Use site Now.
                                    //newLine.LineHeader.inactive_time = new Time().Timestamp();
                                    newLine.LineHeader.inactive_time = siteNowIbex;
                                    newLine.LineHeader.inactive_user = externalUserId;
                                    if (action == ActionEnum.Delete)
                                    {
                                        newLine.LineHeader.status = "I"; // is there constant or create new one?
                                        var trxDate = (new Time()).TimestampNoSeconds();
                                        // update the status ('I') of the trx record(s) associated with this order
                                        var ret = updateTrxEntryForDelete(patIbex, externalSiteId, trxDate, losecsFromOrder);
                                        if (!string.IsNullOrWhiteSpace(ret))
                                        {
                                            LogError(patIbex, externalSiteId, patientOrderId, adminId, ret, action.ToString());
                                        }
                                        foreach (EMR.Line follow_line in emr.Lines)
                                        {
                                            if ((String.Compare(follow_line.SysTime(), line.SysTime(), StringComparison.Ordinal) >= 0)
                                             && losecsFromOrder.Equals(follow_line.ChartXRef()))
                                            {
                                                inactive.Add(follow_line.LineNumber.ToString());
                                                // update the status ('I') of the trx record(s) that are followups to this order
                                                ret = updateTrxEntryForDelete(patIbex, externalSiteId, trxDate, follow_line.Losecs());
                                                if (!string.IsNullOrWhiteSpace(ret))
                                                {
                                                    LogError(patIbex, externalSiteId, patientOrderId, adminId, ret, action.ToString());
                                                }
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    newLine.LineHeader.chart_xref = losecsFromOrder + "&1";  // newLine.LineHeader.table_xref ?
                                }
                            }
                            else if (discontinueOrDiscontinued)
                            {
                                var operation = action == ActionEnum.OrderDiscontinue ? "DISCONTINUE" : "DISCONTINUED";
                                newLine.LinePart.part = "(" + operation + ")" + " " + canDrug;
                                //Use siteNowIbex.
                                //newLine.LineHeader.inactive_time = new Time().Timestamp();
                                newLine.LineHeader.inactive_time = siteNowIbex;
                                newLine.LineHeader.inactive_user = externalUserId;
                            }
                            else if (action == ActionEnum.CoSign)
                            {
                                var ret = addTrxEntryForCoSign(patId, patIbex, externalSiteId, externalUserId, losecsDerived);
                                if (!string.IsNullOrWhiteSpace(ret))
                                {
                                    LogError(patIbex, externalSiteId, patientOrderId, adminId, ret, action.ToString());
                                }
                            }
                            // If this is not the first administration, set the table_xref to the original losecs
                            // which will allow the mapping of all related administrations during chart display
                            if (losecsFromOrder != losecsDerived)
                                newLine.LineHeader.table_xref = losecsFromOrder;
                            // Set the losecs to the proper value; original losecs if first admin otherwise the last losecs
                            // seen (from last administration)
                            newLine.LineHeader.losecs = losecsDerived;
                            // set the user time to be the time entered on the template
                            // use siteNowIbex if we don't have a value for enteredTime.
                            newLine.LineHeader.user_time = enteredTime != null ? enteredTime : siteNowIbex;
                            // inactivate the current line if not a new order admin or if from the first order administration
                            if (newOrderAdmin == false || losecsFromOrder == losecsDerived)
                                inactive.Add(line.LineNumber.ToString());
                            break;
                        }
                    }
                }
                if (newLine != null)
                {
                    // need to check for the duplicate line case so set dup_found flag initially to false
                    var dup_found = false;
                    // check line to be added against existing lines for duplicates
                    // trying to fix the case where the user presses the Enter button multiple times and the chart is duplicated
                    foreach (EMR.Line dup_line in emr.Lines)
                    {
                        // set the checking order with performance in mind
                        if (dup_line.NCT() == EMR.Constants.NCT_MED_SVC && dup_line.User() == newLine.User()
                         && dup_line.PartName().Equals(newLine.PartName()) && dup_line.Data.Equals(newLine.Data))
                        {
                            // if it gets here, most likely a duplicate chart entry but check one last thing
                            // is the system time of the current line (dup_line) within one minute of now?
                            // the thought is that the user may be on the page for up to one minute while trying to submit it 
                            // note: may want to adjust this time offset value in the future
                            var siteNowLessMinute = siteNow.AddMinutes(-1);
                            var siteNowLessMinuteIbex = siteNowLessMinute.ToString("yyyyMMddHHmmss");
                            if (String.Compare(dup_line.SysTime(), siteNowLessMinuteIbex) >= 0)
                            {
                                // found a duplicate so set flag to true
                                dup_found = true;
                                break;
                            }
                        }
                    }
                    if (!dup_found)
                    {
                        // no duplicate line found so write line
                        var status = emr.WriteLine(newLine);
                        if (status)
                        {
                            emr.WriteLines(inactive.ToArray(), externalUserId);
                        }
                        else
                        {
                            // failed to write out charting line so log error
                            var errMsg = "emr.WriteLine failed - could not document line: " + String.Join(EMR.Constants.DelimiterData.ToString(), newLine.DataSegments);
                            LogError(patIbex, externalSiteId, patientOrderId, adminId, errMsg, action.ToString());
                        }
                    }
                    else
                    {
                        // duplicate charting line found so log error
                        var errMsg = "emr.WriteLine failed - duplicate line encountered.";
                        LogError(patIbex, externalSiteId, patientOrderId, adminId, errMsg, action.ToString());
                    }
                }
                else
                {
                    // did not create newLine so log error
                    var errMsg = "newLine is null - nothing to document." + (!foundLosecs ? " Could not find losecs." : "");
                    LogError(patIbex, externalSiteId, patientOrderId, adminId, errMsg, action.ToString());
                }
                // if there are vitals entered on followup, perform appropriate charting
                if (updateVitals)
                {
                    //Add siteNow to the parameters list so that we can use now in the site's time zone
                    //rather than Now.Time.Timestamp when saving the system time for vital signs entries.
                    //Winston Murdock, 03/30/2022.  PC-27144
                    var vitalsLines = _emarOutboundChartRepository.ChartVitalsEntries(externalUserId, losecsFromOrder, enteredTime, vitalObj, siteNow);
                    if (vitalsLines != null)
                        emr.WriteLines(vitalsLines.ToArray());
                }
                // if a notify users selection was made from a give or followup template, then send the PulseMail 
                if (notifyUsers)
                {
                    if (site == null)
                    {
                        site = new Site(externalSiteId);
                    }
                    var patient = new Patient();
                    var patientDataForIbex = _emarOutboundChartRepository.GetPatientDataForIbex(patId);
                    patient = MapPatientDataForIbex(patient, patientDataForIbex);
                    var message = ordererName + " has made an entry on the chart of patient: " + patient.GetName() + " " + (new Time()).LongDateTime() + "\n";
                    message += "Medication Services: " + action.ToString() + "\n";
                    if (meds == null || patientOrderParams == null)
                    {
                        patientOrderParams = _emarOutboundChartRepository.GetPatientOrderDataForMeds(patientOrderId);
                        patientOrderParams.PatientOrderAdminId = adminId;
                        meds = CreateMeds(site, patient, externalUserId, patientOrderParams, losecsDerived, losecsFromOrder != losecsDerived);
                    }
                    // meds defined as List though there is only one item in the list
                    foreach (var med in meds)
                    {
                        message += string.Format("Order: {0}", med.GetFullNameForChart()) + "\n";
                    }
                    if (promptMarkupList != null && promptMarkupList.Count > 0)
                    {
                        var foundMarkup = false;
                        foreach (var promptMarkup in promptMarkupList)
                        {
                            // only include specific markup at this time - for instance, no vital data in followup
                            if (promptMarkup.StartsWith("^S^") || promptMarkup.StartsWith("^D=") || promptMarkup.StartsWith("^C="))
                            {
                                var markupList = promptMarkup.Split('=');
                                if (markupList.Length > 1)
                                {
                                    // remove potential markup at end? (like ^^U)
                                    message += markupList[1] + ", ";
                                    foundMarkup = true;
                                }
                            }
                        }
                        if (foundMarkup)
                            message = message.Remove(message.Length - 2, 2);
                    }
                    var internalMail = new PulseMail(site);
                    if (internalMail != null)
                    {
                        foreach (int notifyId in notifyIds)
                        {
                            var extNotifyId = _emarOutboundDataRepository.GetExternalUserId(notifyId);
                            var ret = internalMail.SendMessage(extNotifyId, "Medication Services Notification", message, 0);
                            if (ret == false)
                            {
                                LogError(patIbex, externalSiteId, patientOrderId, adminId, "PulseMail send error", action.ToString());
                            }
                        }
                    }
                    else
                    {
                        LogError(patIbex, externalSiteId, patientOrderId, adminId, "PulseMail constructor error", action.ToString());
                    }
                }

                return "";
            }
            catch (Exception ex)
            {
                LogException(ex);

                // send dtfl?
                //                var externalSiteId = (byte)_emarOutboundDataRepository.GetExternalSiteId(siteId);
                //                var externalUserId = _emarOutboundDataRepository.GetExternalUserId(userId);
                //                DTFL.Write(externalSiteId, externalUserId, "SendChartTemplateMarkup failure - " + ex.ToString());
                return "";
            }
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
                var codeShareSiteId = _emarOutboundDataRepository.GetExternalSiteId(_emarOutboundChartRepository.GetCodeShareSite(internalSiteId, EmarOutboundChartRepository.Constants.CODE_SHARE_VITAL_SIGNS));
                var vitalsRangeData = GetVitalSignRanges((byte)codeShareSiteId, ageInDays);
                int BPSys = -1;
                int BPDia = -1;

                if (vitalTypes.Count > 0 && vitalsRangeData.Count > 0)
                {
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
                _ibexContext.SaveChanges();
                // set the tracking behavior to none
                _ibexContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
            }
            catch (System.InvalidOperationException ex)
            {
                LogException(ex);
                return "InvalidOperationException in addPatientVitals.";
            }
            catch (System.ArgumentNullException ex)
            {
                LogException(ex);
                return "ArgumentNullException in addPatientVitals.";
            }
            catch (DbUpdateConcurrencyException ex)
            {
                LogException(ex);
                return "DbUpdateConcurrencyException in addPatientVitals.";
            }
            catch (DbUpdateException ex)
            {
                LogException(ex);
                return "DbUpdateException in addPatientVitals.";
            }
            catch (Exception ex)
            {
                LogException(ex);
                return "Exception in addPatientVitals.";
            }   //end try/catch

            return "";
        }

        /// <summary>
        /// Update the patient medication indicator
        /// </summary>
        /// <param name="patientId">Patient emar identifier</param>
        /// <param name="ibex">Patient ibex number</param>
        /// <param name="siteId">Site identifier</param>
        /// <returns>null on ? error, non-whitespace on execution error, empty string on success</returns>
        //
        // This method retrieves the patient medication data and converts it into Medication objects.
        // These objects then can use the GetMedStatus method and then apply the code to find the resultant
        // medication acknowledgement order status.
        // Additionally, it mimics the perl code library function get_emar_meds() in EMR.pm. 
        public string updatePatientMedicationIndicator(long patientId, string ibex, int siteId)
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

                //Use the ibex siteId to get the eMAR siteId.
                //Then use the eMAR siteId to get now in the site's timezone.
                //Winston Murdock, 03/28/2022.  PC-27069
                var emarSiteId = _siteRepository.GetInternalSiteId(siteId);
                var siteNow = _siteRepository.GetSiteTimeZone(emarSiteId).NowWithTimeZoneOffset();

                // Grab patient orders from eMAR so we can identify PRN orders.
                var patient_orders = new Dictionary<long, Data.Entities.PatientOrder>();
                if (medData.Count > 0)
                {
                    var patId = _emarOutboundChartRepository.GetPatientIdFromPatientOrderId(medData[0].emarPatientOrderId);
                    List<Data.Entities.PatientOrder> patOrdersList = _orderRepository.GetOrders(patId).ToList();
                    if (patOrdersList != null && patOrdersList.Count > 0)
                    {
                        foreach (Data.Entities.PatientOrder p in patOrdersList)
                        {
                            if (!patient_orders.ContainsKey(p.Id))
                            {
                                patient_orders.Add(p.Id, p);
                            }
                        }
                    }
                }

                var emar_medications = new Dictionary<int, List<Medication>>();
                var emar_losecs = new Dictionary<int, long>();
                var emar_cancels = new Dictionary<long, int>();
                var emarmeds = new List<Medication>(); // consider its necessity
                                                       // commenting out missed dose related code under need dictates it's use
                                                       //                var emar_missed = new Dictionary<long, int>();

                // get the data from the emar_med_administrations table and save in emar_medications and emar_losecs
                var emarMedAdminData = GetEmarMedAdmins(ibex, siteId);
                foreach (var data in emarMedAdminData)
                {
                    var action = data.medAdminType;
                    if (!Medication.ActionConstants.MEDICATION_MAP.ContainsKey(action))
                        //                    {
                        //                        if (action.Equals("MissedDose"))
                        //                        {
                        //                            if (emar_medications.ContainsKey(data.losecs))
                        //                                emar_medications.Remove(data.losecs);
                        //                            if (emar_losecs.ContainsKey(data.losecs))
                        //                                emar_losecs.Remove(data.losecs);
                        //                            if (!emar_missed.ContainsKey(data.losecs))
                        //                                emar_missed.Add(data.losecs, 1);
                        //                        }
                        continue;
                    //                    }

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
                    if (data.losecs != 0)
                    {
                        if (!emar_losecs.ContainsKey(data.losecs))
                            emar_losecs.Add(data.losecs, data.patientOrderId);

                        if (!emar_medications.ContainsKey(data.losecs))
                        {
                            // Need to revisit this block and the use/need of emarmeds
                            emarmeds.Add(med);
                            var emarMedsNew = new List<Medication>(emarmeds.Select(x => x?.Clone()));
                            emar_medications.Add(data.losecs, emarMedsNew);
                            emarmeds.Clear();
                        }
                        else
                        {
                            var emarMedsNew = new List<Medication>(emar_medications[data.losecs].Select(x => x?.Clone()));
                            emarMedsNew.Add(med);
                            emar_medications[data.losecs] = emarMedsNew;
                        }
                    }

                    if (map.Equals("Cancel") || map.Equals("Delete"))
                    {
                        if (!emar_cancels.ContainsKey(data.patientOrderId))
                        {
                            emar_cancels.Add(data.patientOrderId, 1);
                        }
                    }
                }

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
                        //                    {
                        //                        if (emar_missed.ContainsKey(losecsItem))
                        //                        {
                        //                            if (emar_medications.ContainsKey(losecsItem))
                        //                                medications.Remove(losecsItem);
                        //                        }
                        continue;
                    //                    }

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

                // Keep track of the earliest admin scheduled for the future, so we can know when to change the indicator on the MTB.
                DateTimeOffset? earliestFutureAdmin = null;

                // Keep track of the count of open PRN orders
                int openPRNCount = 0;

                // get the medication status' for each medication
                foreach (var key in medications.Keys)
                {
                    var med = medications[key];
                    var patientOrder = patient_orders.ContainsKey(med.PatientOrderId) ? patient_orders[med.PatientOrderId] : null;
                    var isPrnOrder = (patientOrder != null && patientOrder.Prn);
                    var isHeldOrder = (patientOrder != null && patientOrder.OrderStatus.Equals("OnHold"));

                    // skip the 'order' portion of the modify
                    // REVISIT - this assumes that the losecs for the cancel is +1 over the order which may not be true
                    if (medications.ContainsKey(key + 1) && medications[key].PatientOrderId == medications[key + 1].PatientOrderId && !string.IsNullOrEmpty(medications[key + 1].CancelDate))
                        continue;

                    var medStatus = med.GetMedStatus();
                    if (medStatus.ContainsKey("code"))
                    {
                        // Do not add cancelled meds' earlier statuses to this list.
                        if (emar_cancels.ContainsKey(med.PatientOrderId) && !(medStatus["code"].Equals("C") || medStatus["code"].Equals("D")))
                        {
                            continue;
                        }

                        // If this is a PRN order, make sure to treat the med as given.
                        // If this order is on hold, make sure to treat the med as held.
                        // In both cases, ignore administrations associated with the order
                        // when determining future actions.
                        if (isPrnOrder)
                        {
                            medStatus["code"] = Medication.ActionConstants.GIVE;

                            string orderStatus = patientOrder.OrderStatus;
                            if (!(orderStatus.Equals("Completed") || orderStatus.Equals("Deleted") || orderStatus.Equals("Discontinued") || orderStatus.Equals("Cancelled")))
                            {
                                openPRNCount++;
                            }
                        }
                        else if (isHeldOrder)
                        {
                            medStatus["code"] = Medication.ActionConstants.HOLD;
                        }

                        if (!statuses["code"].ContainsKey(medStatus["code"]))
                        {
                            statuses["code"][medStatus["code"]] = 1;
                        }
                        else
                        {
                            statuses["code"][medStatus["code"]]++;
                        }

                        if (!isPrnOrder && !isHeldOrder)
                        {
                            DateTimeOffset? medBeginDateTime = patientOrder.BeginDatetime;
                            DateTimeOffset? addDateTime = patientOrder.AddDatetime;
                            foreach (Data.Entities.OrderAdministration o in patientOrder.OrderAdministrations)
                            {
                                DateTimeOffset? adminScheduledDateTime = o.AdministrationScheduledDatetime;
                                //Use AddDateTime instead of BeginDateTime since a user can list the start
                                //time for an order as being in the future and then reschedule it backwards
                                //towards now.  In this case, I created an order at 10:43 AM central but
                                //started it at 2:43 PM central.  Any time I reschedule it to now (even when
                                //now is before the original begin time of 2:43 PM), we use the value of
                                //BeginDateTime rather than the administration's scheduled date time
                                //becuase of this check here.
                                //Winston Murdock, 03/28/2022.  PC-27069
                                //We don't need this check at all.  The UI/API won't let you initially schedule
                                //an administration to be earlier than the time the order was added.
                                //And they don't let you reschedule it to be earlier than now.
                                //So we'll never have an issue here.
                                //Winston Murdock, 03/29/2022.  PC-27069
                                //if (adminScheduledDateTime != null && adminScheduledDateTime < medBeginDateTime)
                                //if (adminScheduledDateTime != null && adminScheduledDateTime. < addDateTime)
                                //{
                                //    adminScheduledDateTime = addDateTime;
                                //}

                                //Skip over administrations that happened in the past or have already been done, acknowledged, or stopped.
                                //This ensures that we only account for future administrations when calculating
                                //the "M" on the tracking board.
                                //In case the user leaves the window open for a few minutes and then clicks the submit/go button,
                                //I'm going to subtract five minutes from now.
                                //II'm also changing this to use siteNow isntead of Now.
                                //Winston Murdock, 03/29/2022.  PC-27609
                                if (adminScheduledDateTime < siteNow.AddMinutes(-5) ||
                                    o.AdministrationDatetime != null ||
                                    o.AcknowledgeDatetime != null ||
                                    o.StopDatetime != null)
                                {
                                    continue;
                                }

                                //This was giving us issues where the earliestFutureAdmin is null since we were
                                //attempting to see if a DateTimeOffeset was less than null.
                                //So I unrolled it into the nested if statement below.
                                //Winston Murdock, 03/28/2022.  PC-27069
                                //if (earliestFutureAdmin == null || (adminScheduledDateTime != null && adminScheduledDateTime < earliestFutureAdmin))
                                //{
                                //    earliestFutureAdmin = adminScheduledDateTime;
                                //}

                                //If earliest future admin time is null, set it to the admin's scheduled date time if it also not null.
                                if (earliestFutureAdmin == null)
                                {
                                    //We don't have an earliest future admin time yet.
                                    //If the admin's scheduled date time is not null,
                                    //set the earliest future admin time to it.
                                    if (adminScheduledDateTime != null)
                                    {
                                        //Admin's scheduled date time is not null.
                                        //Set the earliest future admin time to it.
                                        earliestFutureAdmin = adminScheduledDateTime;
                                    } //end if
                                }
                                else
                                {
                                    //Earliest future admin time has been set.
                                    //If it's earlier than the current earliest future admin time,
                                    //then set the variable to it.
                                    //Else, leave the variable alone.
                                    if (adminScheduledDateTime < earliestFutureAdmin)
                                    {
                                        earliestFutureAdmin = adminScheduledDateTime;
                                    } //end if
                                } //end if
                            }
                        }
                    }
                }

                // get the patient indicator (color) by finding the highest priority status in the action list
                // Loops over:
                //      Red / Ordered
                //      Blue / Acknowledged
                //      Yellow / Held
                //      Gray / Discontinue
                //      Green / Given
                //      Green / Discontinued
                var index = "";
                var altIndex = "";
                foreach (var lookup in Medication.ActionConstants.SORTED_STATUSES)
                {
                    if (lookup.ContainsKey("for") && !string.IsNullOrWhiteSpace(lookup["for"]) && !lookup["for"].Equals("indicator"))
                        continue;

                    var color = lookup.ContainsKey("color") ? lookup["color"] : null;
                    if (color != null)
                    {
                        var code = lookup.ContainsKey("code") && !string.IsNullOrWhiteSpace(lookup["code"]) ? lookup["code"] : null;
                        var currentIndex = "";
                        if (code != null)
                        {
                            if (statuses["code"].ContainsKey(code) && statuses["code"][code] > 0)
                            {
                                currentIndex = color;
                            }
                        }
                        else if (statuses["color"].ContainsKey(color) && statuses["color"][color] > 0)
                        {
                            currentIndex = color;
                        }

                        // If this check produced an index...
                        if (!string.IsNullOrWhiteSpace(currentIndex))
                        {
                            // Only set index the first time we encounter it, so we store the highest one.
                            if (string.IsNullOrWhiteSpace(index))
                            {
                                index = currentIndex;

                                // If the index we've found is not red, then we don't need to continue the loop to find
                                // the next highest-priority status. We can break out here.
                                if (!index.Equals(Medication.ActionConstants.RED))
                                {
                                    break;
                                }

                                // If we've already found index, then store the next highest-priority status.
                            }
                            else if (string.IsNullOrWhiteSpace(altIndex))
                            {
                                altIndex = currentIndex;
                            }

                            // If both indexes have been set, then we can break out of the loop.
                            if (!string.IsNullOrWhiteSpace(index) && !string.IsNullOrWhiteSpace(altIndex))
                            {
                                break;
                            }
                        }
                    }
                }

                // find the patient record
                var patient = _ibexContext.Patients.FirstOrDefault(p => p.Ibex == ibex && p.Site == siteId);
                if (patient != null)
                {
                    // set the tracking behavior to all - no harm if set already
                    _ibexContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.TrackAll;

                    // Set open PRN count
                    patient.Ord58 = openPRNCount;

                    /////////////////////////////////
                    //Move the calculation of medDueTime up above where we set ord30_alt and ord30.
                    //Jim Hoos, 03/28/2022.  PC=27069
                    int medDueTime = 0;
                    if (earliestFutureAdmin != null || (index != null && index.Equals(Medication.ActionConstants.RED)))
                    {
                        try
                        {
                            medDueTime = Convert.ToInt32(
                                new HelperDB.Select
                                {
                                    Sql = "SELECT med_due_time FROM org WHERE site=@site",
                                    Parameters = new SqlParameter[]
                                    {
                                        new SqlParameter("@site", SqlDbType.TinyInt) { Value = siteId }
                                    }
                                }.RunForScalar()
                            );
                        }
                        catch (Exception ex)
                        {
                            LogException(ex);
                        }
                    }
                    /////////////////////////////////

                    // If we have a future administration, then we know the 'M' should turn red in the future, and 
                    // we want to display the calculated index until then. Store the calculated current index and the future red.
                    DateTimeOffset? futureStatusTime = null;
                    if (earliestFutureAdmin != null)
                    {
                        /////////////////////////////i
                        //If the difference between the first future administration and now (in the site's time zone)
                        //is more than medDueTime (which is 30 minutes on 57c), then set ord30_alt = altIndex.
                        //Else, set ord30_alt = index.
                        //Jim Hoos and Winston Murdock, 03/28/2022.  PC-27069
                        TimeSpan earliestFutureDiff = (DateTimeOffset)earliestFutureAdmin - siteNow;

                        //If I reschedule an order five hours out into the future, .Minutes is going to be 0, which is less than 30.
                        //So we'll have the red "M" appear on the tracking board.
                        //Instead, I need to use .TotalMinutes (which will be 300 minutes) for the comparison.
                        //I've tested this in Postman and showed it to Romel.  I'll push this up to 57c on Monday morning.
                        //Winston Murdock, 04/29/2022.  PC-27069
                        //patient.Ord30Alternate = int.Parse(earliestFutureDiff.Minutes.ToString()) > medDueTime ? altIndex : index;
                        patient.Ord30Alternate = double.Parse(earliestFutureDiff.TotalMinutes.ToString()) > medDueTime ? altIndex : index;

                        // patient.Ord30Alternate = index;
                        /////////////////////////////
                        patient.Ord30 = Medication.ActionConstants.RED;
                        futureStatusTime = earliestFutureAdmin;

                        // Otherwise just set the new index
                    }
                    else
                    {
                        patient.Ord30 = index;

                        // And if the new index is a red, then we also have to update the alternate index and the future time.
                        if (index != null && index.Equals(Medication.ActionConstants.RED))
                        {
                            patient.Ord30Alternate = altIndex;
                            futureStatusTime = DateTimeOffset.Now;

                            // If the new index is not a red, clear out the alternate status
                        }
                        else
                        {
                            patient.Ord30Alternate = null;
                        }
                    }

                    // If we made it here with a time set for a future status, pull the site setting for due time
                    // and subtract it from that time, so we know when to show the future status.
                    if (futureStatusTime != null)
                    {
                        ///////////////////////////////////////
                        /// The call to calculate medDueTime was moved higher up, above
                        /// the logic that sets ord30_alt and ord30.
                        /// Jim Hoos, 03/28/2022.  OC-27069
                        //////////////////////////////////////
                        patient.Ord30DateTime = ((DateTimeOffset)futureStatusTime).AddMinutes(medDueTime * -1);
                    }
                    else
                    {
                        patient.Ord30DateTime = null;
                    }

                    // get the pharmacy verification required status
                    var pharmVerificationStatus = _emarOutboundChartRepository.GetPharmVerificationReqStatus(patientId);
                    patient.Ord57 = pharmVerificationStatus ? "Y" : "N";
                    // update the pharmacy verification status column
                    _ibexContext.Entry(patient).Property(p => p.Ord57).IsModified = true;
                    // Update all the indicator columns
                    _ibexContext.Entry(patient).Property(p => p.Ord30).IsModified = true;
                    _ibexContext.Entry(patient).Property(p => p.Ord30Alternate).IsModified = true;
                    _ibexContext.Entry(patient).Property(p => p.Ord30DateTime).IsModified = true;
                    _ibexContext.Entry(patient).Property(p => p.Ord58).IsModified = true;
                    _ibexContext.SaveChanges();

                    // set the tracking behavior to none
                    _ibexContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
                }
            }
            // start try/catch updatePatientMedicationIndicator exceptions here
            catch (System.InvalidOperationException ex)
            {
                LogException(ex);
                return "InvalidOperationException in updatePatientMedicationIndicator.";
            }
            catch (System.ArgumentNullException ex)
            {
                LogException(ex);
                return "ArgumentNullException in updatePatientMedicationIndicator.";
            }
            catch (System.Reflection.AmbiguousMatchException ex)
            {
                LogException(ex);
                return "AmbiguousMatchException in updatePatientMedicationIndicator.";
            }
            catch (DbUpdateConcurrencyException ex)
            {
                LogException(ex);
                return "DbUpdateConcurrencyException in updatePatientMedicationIndicator.";
            }
            catch (DbUpdateException ex)
            {
                LogException(ex);
                return "DbUpdateException in updatePatientMedicationIndicator.";
            }
            catch (SqlException ex)
            {
                LogException(ex);
                return "SqlException in updatePatientMedicationIndicator.";
            }
            catch (Exception ex)
            {
                LogException(ex);
                return "Exception in updatePatientMedicationIndicator.";
            }   //end try/catch

            return "";
        }

        public string addTrxEntryForCoSign(long patId, string patExtId, byte extSiteId, int userId, string losecs)
        {
            try
            {
                // create patient object for use in Transaction constructor
                var patient = new Patient();
                patient.Ibex = patExtId;
                var patientDataForIbex = _emarOutboundChartRepository.GetPatientDataForIbex(patId);
                patient = MapPatientDataForIbex(patient, patientDataForIbex);

                var Values = new Dictionary<string, object>
                {
                    { Transaction.Constants.Name, "Co-sign" },
                    { Transaction.Constants.Service, 303 },
                    { Transaction.Constants.Type, "I" },
                    { Transaction.Constants.Quantity, "1" },
                    { Transaction.Constants.Alienkey, "" },
                    { Transaction.Constants.ServiceType, 0 },
                    { Transaction.Constants.Face, "" },
                    { Transaction.Constants.APC, "" },
                    { Transaction.Constants.CPT, "" },
                    { Transaction.Constants.LosecsLink, losecs }
                };

                var t = new Transaction(extSiteId, patient, userId, Values, null);
                // add the new trx entry using the Values data
                if (t.AddTransaction() == 0)
                {
                    return "Add transaction failure in addTrxEntryForCoSign for patient " + patExtId + ".";
                }
            }
            catch (System.InvalidOperationException ex)
            {
                LogException(ex);
                //If there's an exception, then we'll need to pass this error up.
                throw new ArgumentException($"Patient with Id {patId} had an error in addTrxEntryForCoSign.", nameof(patId));
            }
            catch (SqlException ex)
            {
                LogException(ex);
                //If there's an exception, then we'll need to pass this error up.
                throw new ArgumentException($"Patient with Id {patId} had an error in addTrxEntryForCoSign.", nameof(patId));
            }
            catch (Exception ex)
            {
                LogException(ex);
                //Just pass the error message up the stack.
                throw new Exception(ex.Message, ex.InnerException);
            }   //end try/catch

            return "";
        }

        public string addTrxEntryForFormulary(long patientOrderId, long patId, string patExtId, byte extSiteId, int userId, string enteredTime, int intSiteId)
        {
            try
            {
                var patient = new Patient();
                // fill out the patient object for use later
                patient.Ibex = patExtId;
                var patientDataForIbex = _emarOutboundChartRepository.GetPatientDataForIbex(patId);
                patient = MapPatientDataForIbex(patient, patientDataForIbex);
                var now = DateTime.Now.ToString("yyyyMMddHHmm");
                var medId = _emarOutboundChartRepository.GetMedicationIdFromPatientOrderId(patientOrderId);
                var baseNdc = _emarOutboundChartRepository.GetNDCFromPatientOrderId(patientOrderId);
                var serviceCodesList = new List<string>();
                var drugName = _emarOutboundDataRepository.GetComboName(medId);
                var isComboMed = drugName.Length > 0;
                if (!isComboMed)
                {
                    // not combo med so potentially only one service code
                    // currently not using formulary code sharing

                    // ndc should not be empty but if it is, no need to try and get the service code
                    if (string.IsNullOrEmpty(baseNdc))
                        return "";
                    // since this is the base ndc we need to get all the individual ndcs
                    var ndcs = _emarOutboundChartRepository.GetNDCsFromBaseNDC(baseNdc, extSiteId);
                    // loop through ndc results list to try to find a match in the formulary
                    foreach (var ndc in ndcs)
                    {
                        var serviceCode = _emarOutboundChartRepository.GetServiceCodesFromFormulary(ndc, intSiteId);
                        if (string.IsNullOrEmpty(serviceCode))
                            continue;

                        serviceCodesList.Add(serviceCode);
                        break;
                    }
                }
                else
                {
                    //combo med so get the drug Ids from medication_details
                    var drugIds = _emarOutboundChartRepository.GetMedicationDetailsDrugIds(medId);
                    foreach (var drugId in drugIds)
                    {
                        if (string.IsNullOrEmpty(drugId))
                            continue;
                        // get the medication Ids from the medications table using the drug_id
                        var medicationId = _emarOutboundChartRepository.GetMedicationMedicationIds(drugId);
                        if (medicationId == 0)
                            continue;
                        // currently not using formulary code sharing
                        // TODO: at proper time, change to use ndc's when DB can support multiple ndc's per order
                        var serviceCode = _emarOutboundChartRepository.GetServiceCodesFromFormulary(medicationId, intSiteId);
                        if (string.IsNullOrEmpty(serviceCode))
                            continue;

                        serviceCodesList.Add(serviceCode);
                    }
                }

                var serviceCodeShare = (byte)0;
                if (serviceCodesList.Count > 0)
                {
                    // get the code share site for service
                    // could use _emarOutboundChartRepository.GetCodeShareSite() here instead
                    var svccs = new HelperDB.Select
                    {
                        Sql = "SELECT svccs FROM org WHERE site=@site",
                        Parameters = new SqlParameter[]
                        {
                            new SqlParameter("@site", SqlDbType.TinyInt) { Value = extSiteId }
                        }
                    }.RunForScalar();
                    serviceCodeShare = Convert.ToByte(svccs);
                }

                // loop through each of the service codes to get the service data and then use that data in
                // inserting a new entry in the trx table
                foreach (var svc in serviceCodesList)
                {
                    // get the data from the service table based upon the code
                    var info = new HelperDB.Select
                    {
                        Sql = "SELECT name,amt,svctype,face,apc,lvlpts,cpt FROM svc WHERE site=@site AND code=@code",
                        Parameters = new SqlParameter[]
                        {
                            new SqlParameter("@site", SqlDbType.TinyInt) { Value = serviceCodeShare },
                            new SqlParameter("@code", SqlDbType.VarChar) { Value = svc }
                        }
                    }.RunForDataRow();

                    if (info == null)
                        continue;

                    var name = info["name"]?.ToString().Trim();
                    name = name.Length > 80 ? name.Substring(0, 80) : name;
                    var amt = info["amt"]?.ToString().Trim();
                    var svctype = info["svctype"]?.ToString().Trim();
                    var face = info["face"]?.ToString().Trim();
                    var apc = info["apc"]?.ToString().Trim();
                    var lvlpts = info["lvlpts"]?.ToString().Trim();
                    var cpt = info["cpt"]?.ToString().Trim();
                    cpt = cpt.Length > 5 ? cpt.Substring(0, 5) : cpt;

                    var Values = new Dictionary<string, object>
                    {
                        { Transaction.Constants.Name, name },
                        { Transaction.Constants.Service, 210 },
                        { Transaction.Constants.Type, "Q" },
                        { Transaction.Constants.Quantity, "1" },
                        { Transaction.Constants.Alienkey, svc },
                        { Transaction.Constants.ServiceType, svctype },
                        { Transaction.Constants.Amount, amt },
                        { Transaction.Constants.Face, face },
                        { Transaction.Constants.APC, apc },
                        { Transaction.Constants.LevelPoints, lvlpts },
                        { Transaction.Constants.CPT, cpt },
                        { Transaction.Constants.Date, enteredTime ?? now }
                    };
                    var t = new Transaction(extSiteId, patient, userId, Values, null);
                    // add the new trx entry using the svc data
                    if (t.AddTransaction() == 0)
                    {
                        return "Add transaction failure in addTrxEntryForFormulary for patient " + patExtId + ".";
                    }
                }
            }
            catch (System.InvalidOperationException ex)
            {
                LogException(ex);
                //If there's an exception, then we'll need to pass this error up.
                throw new ArgumentException($"Patient with Id {patId} had an error in addTrxEntryForFormulary.", nameof(patId));
            }
            catch (SqlException ex)
            {
                LogException(ex);
                //If there's an exception, then we'll need to pass this error up.
                throw new ArgumentException($"Patient with Id {patId} had an error in addTrxEntryForFormulary.", nameof(patId));
            }
            catch (Exception ex)
            {
                LogException(ex);
                //Just pass the error message up the stack.
                throw new Exception(ex.Message, ex.InnerException);
            }   //end try/catch

            return "";
        }

        public string updateTrxEntryForDelete(string patExtId, byte extSiteId, string trxDate, string losecs)
        {
            try
            {
                var trxParams = new List<SqlParameter> {
                    new SqlParameter("@status", SqlDbType.Char) { Value = "I" },
                    new SqlParameter("@datechg", SqlDbType.VarChar) { Value = trxDate },
                    new SqlParameter("@ibex", SqlDbType.Char) { Value = patExtId },
                    new SqlParameter("@site", SqlDbType.SmallInt) { Value = extSiteId },
                    new SqlParameter("@statusold", SqlDbType.Char) { Value = "A" },
                    new SqlParameter("@losecs", SqlDbType.Int) { Value = losecs }
                };

                new HelperDB.Update
                {
                    Sql = "UPDATE trx SET status = @status, datechg = @datechg WHERE ibex = @ibex AND site = @site AND status = @statusold AND losecslink IN (@losecs)",
                    Parameters = trxParams.ToArray()
                }.Run();
            }
            catch (System.InvalidOperationException ex)
            {
                LogException(ex);
                //If there's an exception, then we'll need to pass this error up.
                throw new ArgumentException($"Patient with Ibex {patExtId} had an error in updateTrxEntryForDelete.", nameof(patExtId));
            }
            catch (SqlException ex)
            {
                LogException(ex);
                //If there's an exception, then we'll need to pass this error up.
                throw new ArgumentException($"Patient with Ibex {patExtId} had an error in updateTrxEntryForDelete.", nameof(patExtId));
            }
            catch (Exception ex)
            {
                LogException(ex);
                //Just pass the error message up the stack.
                throw new Exception(ex.Message, ex.InnerException);
            }   //end try/catch

            return "";
        }

        public string updatePatientPharmacyVerification(long patientId, string extPatientId, byte externalSiteId)
        {
            try
            {
                // find the patient record
                var patient = _ibexContext.Patients.First(p => p.Ibex == extPatientId && p.Site == externalSiteId);
                // set the tracking behavior to all - no harm if set already
                _ibexContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.TrackAll;
                // get the pharmacy verification required status
                var pharmVerificationStatus = _emarOutboundChartRepository.GetPharmVerificationReqStatus(patientId);
                patient.Ord57 = pharmVerificationStatus ? "Y" : "N";
                _ibexContext.Entry(patient).Property(p => p.Ord57).IsModified = true;
                _ibexContext.SaveChanges();
                // set the tracking behavior to none
                _ibexContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
            }
            catch (System.InvalidOperationException ex)
            {
                LogException(ex);
                return "InvalidOperationException in updatePatientPharmacyVerification.";
            }
            catch (System.ArgumentNullException ex)
            {
                LogException(ex);
                return "ArgumentNullException in updatePatientPharmacyVerification.";
            }
            catch (DbUpdateConcurrencyException ex)
            {
                LogException(ex);
                return "DbUpdateConcurrencyException in updatePatientPharmacyVerification.";
            }
            catch (DbUpdateException ex)
            {
                LogException(ex);
                return "DbUpdateException in updatePatientPharmacyVerification.";
            }
            catch (Exception ex)
            {
                LogException(ex);
                return "Exception in updatePatientPharmacyVerification.";
            }   //end try/catch

            return "";
        }

        public List<Medication> CreateMeds(ISite site, Patient patient, int externalUserId, PatientOrderDataForMeds patientOrderParams, string losecsStr, bool isEmarMedAdmin)
        {
            //           if (!user.HasWritePermission(Permission.MED_SVC))
            //               return null;
            try
            {
                var user = new User();
                user.Id = externalUserId;
                user.SiteId = site.Id;
                var losecs = Convert.ToInt32(losecsStr);

                var notes = patientOrderParams.medNotes;
                //            if (serviceOptions != null && serviceOptions.Count > 0)
                //                notes += (!string.IsNullOrWhiteSpace(notes) ? "\n" : "") + string.Join(", ", serviceOptions);

                var meds = new List<Medication>();
                // var drugDb = new DrugDB(site);

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
                    var name = _emarOutboundDataRepository.GetComboName(patientOrderParams.medicationId);
                    var isComboMed = name.Length > 0;
                    if (!isComboMed)
                    {
                        var component = new Medication.Component();
                        OdsMedicationDetails medDetails = _emarOutboundDataRepository.GetMedicationDetails(patientOrderParams.medicationId);
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
                        component.DrugRoute = medDetails.DrugRoute;
                        component.PackagingId = patientOrderParams.Ndc ?? medDetails.PackagingId;
                        component.Interactions = null;
                        component.Reactions = null;
                        component.RXNorm = med.DrugDB.GetInstance().GetRxcuiByDrugId(medDetails.DrugId);
                        // use the NDC/PackagingId determined above to get the service
                        component.Service = _emarOutboundDataRepository.GetServiceByNdc(component.PackagingId);
                        component.Id = Convert.ToInt32(_emarOutboundChartRepository.GetMedDetailsId(patient.Ibex, site.Id, losecs, isEmarMedAdmin, null));
                        // verify if correct component dose value or not
                        component.EnteredDose = string.IsNullOrWhiteSpace(patientOrderParams.Dose) ? "*" : patientOrderParams.Dose.Replace(".000", "");
                        // component.DrugFormId = // medispan only?
                        //                  component.DrugDBType = 
                        med.Type = Medication.Constants.TYPE_MEDICATION;
                        med.Components.Add(component);
                        med.Name = medDetails.BrandName;
                    }
                    else
                    {
                        List<int> detailIds = _emarOutboundDataRepository.GetMedicationDetailsIds(patientOrderParams.medicationId);
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
                            comboComponent.DrugRoute = medDetails.DrugRoute;
                            comboComponent.PackagingId = medDetails.PackagingId;
                            comboComponent.Interactions = null;
                            comboComponent.Reactions = null;
                            comboComponent.RXNorm = med.DrugDB.GetInstance().GetRxcuiByDrugId(medDetails.DrugId);
                            // combo meds will rely on medDetails packaging id's retrieved from the detailIds list for now
                            // should use _emarOutboundChartRepository.GetServiceCodesFromFormulary(int medicationId, int siteId) instead?
                            comboComponent.Service = _emarOutboundDataRepository.GetServiceByNdc(medDetails.PackagingId);
                            comboComponent.Id = Convert.ToInt32(_emarOutboundChartRepository.GetMedDetailsId(patient.Ibex, site.Id, losecs, isEmarMedAdmin, medDetails.BrandName));
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
                med.Dose = string.IsNullOrWhiteSpace(patientOrderParams.Dose) ? "*" : patientOrderParams.Dose.Replace(".000", "");
                med.Unit = _emarOutboundChartRepository.GetUnit(patientOrderParams.Unit);
                med.Route = _emarOutboundChartRepository.GetRoute(patientOrderParams.Route);
                med.Frequency = _emarOutboundChartRepository.GetFrequencyNameFromId(patientOrderParams.FrequencyId);
                med.Duration = (_emarOutboundChartRepository.GetDurationUnitFromId(patientOrderParams.DurationId) == null) ? "" :
                                patientOrderParams.Duration + " " + _emarOutboundChartRepository.GetDurationUnitFromId(patientOrderParams.DurationId);
                med.OrderUserId = user.Id;
                var orderingPhysicianId = _emarOutboundDataRepository.GetExternalUserId(patientOrderParams.orderingPhysicianId);
                med.OrderForUserId = orderingPhysicianId;
                med.OrderDate = patientOrderParams.OrderDate;
                med.Time = order.Time;
                med.Notes = (order.Notes + (!string.IsNullOrWhiteSpace(order.Notes) && !string.IsNullOrWhiteSpace(notes) ? "\n" : "") + notes).Trim();
                med.Repeat = order.Repeat;
                med.Schedule = med.Frequency; // using frequency description for now
                med.Indication = patientOrderParams.AntiMicrobialIndication;
                med.IndicationDescription = patientOrderParams.AntiMicrobialIndicationText;
                //            med.Authentication = authType;
                med = AddActionMedData(med, patientOrderParams.PatentOrderId, patientOrderParams.PatientOrderAdminId);

                meds.Add(med);

                return meds;
            }
            catch (Exception ex)
            {
                LogException(ex);
                //Just pass the error message up the stack.
                throw new Exception(ex.Message, ex.InnerException);
            }   //end try/catch
        }

        private Medication AddActionMedData(Medication med, long orderId, long adminId)
        {
            try
            {
                // get the data from the med table based upon the order Id
                // Convert to use _ibexContext?
                var info = new HelperDB.Select
                {
                    Sql = @"SELECT give_date,give_sysdate,give_usr,iv_type FROM med WHERE emar_patient_order_id=@orderid",
                    Parameters = new SqlParameter[]
                    {
                        new SqlParameter("@orderid", SqlDbType.BigInt) { Value = orderId }
                    }
                }.RunForDataRow();

                if (info == null)
                    return med;

                if (info["give_usr"] != null && !string.IsNullOrEmpty(info["give_usr"].ToString()))
                {
                    med.GiveUserId = (int)info["give_usr"];
                    med.GiveDate = info["give_sysdate"].ToString();
                    med.GiveSysdate = info["give_sysdate"].ToString();
                }
                if (!string.IsNullOrWhiteSpace(info["iv_type"].ToString()))
                {
                    med.IVType = info["iv_type"].ToString();
                }

                // get the data from the emar_med_administrations table based upon the order Id and admin Id
                // Convert to use _ibexContext?
                var res = new HelperDB.Select
                {
                    Sql = @"SELECT med_admin_type,med_admin_date,med_admin_sysdate,med_admin_user,stop_user,stop_date,stop_sysdate "
                        + @"FROM emar_med_administrations WHERE patient_order_id=@orderid AND order_administrations_id=@adminid "
                        + @"ORDER BY med_admin_sysdate",
                    Parameters = new SqlParameter[]
                    {
                        new SqlParameter("@orderid", SqlDbType.BigInt) { Value = orderId },
                        new SqlParameter("@adminid", SqlDbType.BigInt) { Value = adminId }
                    }
                }.RunForListOfDictionaries();

                if (res == null || res.Count() < 1)
                    return med;

                foreach (var r in res)
                {
                    // if there is a stop_date value, grab the stop data and continue - it's a followup
                    if (!string.IsNullOrWhiteSpace(r["stop_date"]))
                    {
                        med.StopDate = r["stop_date"];
                        med.StopSysdate = r["stop_sysdate"];
                        med.StopUserId = !string.IsNullOrWhiteSpace(r["stop_user"]) ? Convert.ToInt32(r["stop_user"]) : 0;
                        // !string.IsNullOrWhiteSpace(r["stop_user"]) ? Convert.ToInt32(r["stop_user"]) : null;
                        continue;
                    }

                    // only care about actions that are mapped - will have corresponding date and userId values
                    if (!Medication.ActionConstants.MEDICATION_MAP.ContainsKey(r["med_admin_type"]))
                        continue;

                    var map = Medication.ActionConstants.MEDICATION_MAP[r["med_admin_type"]];
                    // set the corresponding three values for each action and skip any undefined ones
                    foreach (string type in new[] { "Date", "UserId", "Sysdate" })
                    {
                        var medProp = med.GetType().GetProperty(map + type);
                        if (medProp != null)
                        {
                            switch (type)
                            {
                                case "Date":
                                    medProp.SetValue(med, r["med_admin_date"], null);
                                    break;
                                case "UserId":
                                    medProp.SetValue(med, Convert.ToInt32(r["med_admin_user"]), null);
                                    break;
                                case "Sysdate":
                                    medProp.SetValue(med, r["med_admin_sysdate"], null);
                                    break;
                            }
                        }
                    }
                }

                // if the delete date or cancel date has been set, then set the medication status to inactive
                if (!string.IsNullOrEmpty(med.DeleteDate) || !string.IsNullOrEmpty(med.CancelDate))
                    med.Status = Medication.Constants.INACTIVE;

                return med;
            }
            catch (Exception ex)
            {
                LogException(ex);
                //Just pass the error message up the stack.
                throw new Exception(ex.Message, ex.InnerException);
            }   //end try/catch
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

        public string GetIbexFormatDateTimeFromDTO(string inputDTO, DateTimeOffset siteNow)
        {
            //Former logic.
            //DateTimeOffset offset;
            //if (!DateTimeOffset.TryParse(inputDTO, out offset))
            //{
            //    offset = DateTime.Now; // error handling here?
            //}

            //return offset.ToString("yyyyMMddHHmm") ?? new Time().Timestamp().Substring(0, 12);

            //The value always comes in as yyyyMMddhhmmss.
            //I added siteNow so that we can get the time zone offset.
            //Then I convert the stirng value to a DateTime.
            //Lastly, I convert the DateTime to a DateTimeOffset (using the offset from siteNow).
            //If we fail anywhere along the way, we just use siteNow (which already has the offset).
            //Winston Murdock, 03/30/2022.  PC-27144.
            DateTimeOffset offset;

            string formatString;
            if (inputDTO.Length == 14)
            {
                //YYYYMMddHHmmss
                formatString = "yyyyMMddHHmmss";
            }
            else
            {
                //yyyyMMddHHmmssfff
                formatString = "yyyyMMddHHmmssfff";
            } //end if

            DateTime dtTemp;
            try
            {
                //Convert the string to a DateTime.
                dtTemp = DateTime.ParseExact(inputDTO, formatString, null);
            }
            catch (Exception ex)
            {
                //If we can't convert it to a DateTime, use siteNow.
                dtTemp = siteNow.DateTime;
            } //end try/catch.

            try
            {
                //Convert the DateTime to a DateTimeOffset (passing in the DateTime and the Offset).
                offset = new DateTimeOffset(dtTemp, siteNow.Offset);
            }
            catch (Exception ex)
            {
                //If we can't convert the DateTime to a DateTimeOffset, use siteNow.
                offset = siteNow;
            } //end try/catch.
            
            //Return the DateTimeOffset converted to this format.
            return offset.ToString("yyyyMMddHHmm");
        } //end GetIbexFormatDateTimeFromDTO

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

        public Patient MapPatientDataForIbex(Patient patient, PatientDataForIbex missingData)
        {
            patient.Bed = missingData.Bed;
            patient.Department = missingData.Department;
            patient.FirstName = missingData.FirstName;
            patient.MiddleName = missingData.MiddleName;
            patient.LastName = missingData.LastName;
            patient.Suffix = missingData.NameSuffix;
            patient.Ward = missingData.Ward;
            patient.Age = missingData.Age != null ? (byte)missingData.Age : (byte)0;
            patient.AgeUnit = missingData.AgeUnits;

            return patient;
        }

        private void LogException(Exception ex)
        {   //Log the exception.
            using (EventLog eventLog = new EventLog("Application"))
            {
                string sException = ex.Message + "\n";
                if (ex is SqlException)
                {
                    sException += "error number = " + ((SqlException)ex).Number + "\n";
                }
                sException += "source = " + ex.Source + "\n";
                if (ex is SqlException)
                {
                    sException += "Line Number = " + ((SqlException)ex).LineNumber + "\n";
                }
                sException += ex.StackTrace + "\n";

                if (!(ex is SqlException))
                {
                    sException += ex.InnerException + "\n"; // added inner exception
                }

                eventLog.Source = "PulseCheck EMAR API";
                eventLog.WriteEntry(sException, EventLogEntryType.Error, 101, 1);
            } //end using.
        }
        private void LogError(string patIbex, byte externalSiteId, long patientOrderId, long adminId, string errMessage, string action = null)
        {
            //Log any error.
            using (EventLog eventLog = new EventLog("Application"))
            {
                string sErrMessage = "Patient ibex: " + patIbex + "\n";
                sErrMessage += "Patient site: " + externalSiteId + "\n";
                sErrMessage += "Patient order id: " + patientOrderId + "\n";
                sErrMessage += "Order admin id: " + adminId + "\n";
                sErrMessage += "Order action: " + action + "\n";
                sErrMessage += "Error message: " + errMessage;

                eventLog.Source = "PulseCheck EMAR API";
                eventLog.WriteEntry(sErrMessage, EventLogEntryType.Error, 101, 1);
            } //end using.
        }

        public DateTimeOffset TimeAdjustedForTimeZone(string siteTimeZone, DateTimeOffset tzo)
        {
            var tz = TimeZoneInfo.GetSystemTimeZones().FirstOrDefault(z =>
                z.DisplayName == siteTimeZone
                || z.DaylightName == siteTimeZone
                || z.StandardName == siteTimeZone);
            if (tz == null)
                throw new ArgumentException(
                    "Invalid Timezone passed to Emar.Core.Templates.Repository.TimeAdjustedForTimeZone()",
                    nameof(siteTimeZone));

            var siteTzOffset = tz.BaseUtcOffset;
            if (tz.IsDaylightSavingTime(tzo))
                siteTzOffset = siteTzOffset.Add(new TimeSpan(0, 60, 0));
            return (int)(siteTzOffset - tzo.Offset).TotalMinutes == 0 ? tzo : tzo.ToOffset(siteTzOffset);
        }
    }
}
