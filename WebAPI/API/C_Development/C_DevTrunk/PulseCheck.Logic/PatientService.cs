using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PulseCheck.Data;
using PulseCheck.Domain;
using PulseCheck.Domain.Options;
using PulseCheck.IDomain;
using PulseCheck.ILogic;
using PulseCheck.IRepository;
using PulseCheck.Utilities;
using Chart = PulseCheck.Utilities.Chart;

namespace PulseCheck.Logic
{
    public class PatientService : IPatientManager
    {
        private readonly IPatientRepository _patientRepository;
        private readonly ISiteRepository _siteRepository;
        private readonly IMedicationRepository _medicationRepository;
        private readonly IUserRepository _userRepository;

        static MedicationManager _medSvc;

        /// <summary>
        /// PatientService constructor - empty
        /// </summary>
        public PatientService()
        {

        }

        /// <summary>
        /// PatientService constructor
        /// </summary>
        /// <param name="patientRepository">IPatientRepository instance</param>
        /// <param name="siteRepository">ISiteRepository instance</param>
        /// <param name="medicationRepository">IMedicationRepository instance</param>
        public PatientService(IPatientRepository patientRepository, ISiteRepository siteRepository, IMedicationRepository medicationRepository, IUserRepository userRepository)
        {
            _patientRepository = patientRepository;
            _siteRepository = siteRepository;
            _medicationRepository = medicationRepository;
            _userRepository = userRepository;

            _medSvc = new MedicationManager(medicationRepository, userRepository);
        }

        /// <summary>
        /// Acknowledge a medication order
        /// </summary>
        /// <param name="siteId">Site identifier</param>
        /// <param name="patientId">Patient identifier</param>
        /// <param name="user">User object</param>
        /// <param name="orderId">Order identifier</param>
        /// <returns>null on permission error, non-whitespace on execution error, empty string on success</returns>
        public async Task<string> AcknowledgeMedOrder(byte siteId, string patientId, User user, int orderId)
        {
            if (!user.HasWritePermission(Permission.MED_SVC))
                return null;

            var med = await _medicationRepository.GetMedicationByIdAsync(siteId, patientId, orderId);
            var msg = "";
            if (med.IsAcknowledged())
            {
                msg = "Cannot acknowledge an order that was already acknowledged";
            }
            else if (med.IsGiven())
            {
                msg = "Cannot acknowledge an order that was already given";
            }
            else if (med.IsCancelled())
            {
                msg = "Cannot acknowledge an order that was already canceled";
            }

            if (!string.IsNullOrWhiteSpace(msg))
                return msg;

            med.Acknowledge(user.Id);
            var status = (await _medicationRepository.Save(med) == 1);
            if (status)
            {
                var emr = new EMR(siteId, patientId);
                var orderDate = (new Time(siteId)).LongDateTime();
                var inactive = new List<object>();
                EMR.Line newLine = null;
                foreach (EMR.Line line in emr.Lines)
                {
                    if (line.NCT() == EMR.Constants.NCT_MED_SVC)
                    {
                        var losecs = line.Losecs().Split(new char[] { '&' }, StringSplitOptions.RemoveEmptyEntries);
                        if (losecs.Contains(med.Losecs.ToString()))
                        {
                            newLine = line.Clone();
                            var lineData = newLine.data();
                            var newSegment = new EMR.Line.DataSegment(
                                EMR.Line.DataSegment.Constants.TYPE_DROPDOWN,
                                string.Format("\nAcknowledged by: {0} {1}", user.GetName(), orderDate)
                            );
                            lineData.Add(newSegment);
                            newLine.DataSegments = lineData;
                            inactive.Add(line.LineNumber.ToString());
                            break;
                        }
                    }
                }
                if (newLine != null)
                {
                    status = emr.WriteLine(newLine);
                    if (status)
                    {
                        emr.WriteLines(inactive.ToArray(), user.Id);
                        var Site = await _siteRepository.GetSiteByIdAsync(siteId);
                        if (Site.GetOrgOption("MED_INF").Equals("Y"))
                            MedicationManager.TriggerFile(Site, patientId, user.Id, "ACKNOWLEDGED", med.Losecs);

                        var patient = await _patientRepository.GetPatientByIdAsync(Site.Id, patientId, user);
                        var medList = new List<Medication>
                        {
                            med
                        };
                        CreateTrigger(Site, patient, user.Id, "ack", medList);
                    }
                }
                else
                {
                    status = false;
                }
            }

            if (!status)
                return "Order acknowledge failed";

            var site = new Site(siteId);
            var allMeds = await _medicationRepository.GetMedicationsByPatientIdAsync(site.Id, patientId);
            var noneInOrderStatus = true;
            foreach (var existingMed in allMeds)
            {
                if (!existingMed.IsAcknowledged() && !existingMed.IsCancelled() && !existingMed.IsGiven())
                {
                    noneInOrderStatus = false;
                    break;
                }
            }

            if (noneInOrderStatus)
            {
                var update = new DB.Update
                {
                    Sql = "UPDATE pat SET ord30=@ord30 WHERE ibex=@ibex AND site=@site",
                    Parameters = new SqlParameter[]
                    {
                        new SqlParameter("@ord30", SqlDbType.Char) { Value = MedicationActions.Constants.BLUE },
                        new SqlParameter("@ibex", SqlDbType.Char) { Value = patientId },
                        new SqlParameter("@site", SqlDbType.TinyInt) { Value = site.Id }
                    }
                }.Run();
            }

            return "";
        }

        /// <summary>
        /// Get a single medication order for a patient
        /// </summary>
        /// <param name="site">Site identifier</param>
        /// <param name="patientId">Patient identifier</param>
        /// <param name="user">User object</param>
        /// <param name="orderId">Medication order identifier</param>
        /// <returns>MedicationDTO object for order</returns>
        public async Task<MedicationDTO> GetMedOrder(ISite site, string patientId, User user, int orderId)
        {
            if (!user.HasAtLeastReadPermission(Permission.MED_SVC))
            {
                return null;
            }

            var med = await _medicationRepository.GetMedicationByIdAsync(site.Id, patientId, orderId);
            med.Name = med.GetName();
            var allUsers = med.GetUserLookupList();

            var users = await _userRepository.GetUsersByIdAsync(allUsers);
            var userDictionary = users.ToDictionary(x => (int?)x.Id, v => v);
            return med.GetDTO(userDictionary);
        }

        /// <summary>
        /// Get the list of medication orders for the patient
        /// </summary>
        /// <param name="site">ISite instance</param>
        /// <param name="patientId">Patient identifier</param>
        /// <param name="user">User object</param>
        /// <returns>List of MedicationDTO objects</returns>
        public async Task<List<MedicationDTO>> GetMedOrders(ISite site, string patientId, User user)
        {
            if (!user.HasAtLeastReadPermission(Permission.MED_SVC))
            {
                return null;
            }

            var result = await _medicationRepository.GetMedicationsByPatientIdAsync(site.Id, patientId);
            var allUsers = new Dictionary<int?, int>();
            foreach(var med in result)
            {
                med.Name = med.GetName();
                var userList = med.GetUserLookupList();
                foreach(var u in userList)
                {
                    allUsers[u] = 1;
                }
            }

            var finalMedList = new List<MedicationDTO>();
            var users = await _userRepository.GetUsersByIdAsync(allUsers.Keys.Select(x => (x > 0 ? (int)x : 0)).Where(x => x > 0).ToList());
            var userDictionary = users.ToDictionary(x => (int?)x.Id, v => v);
            foreach(var med in result)
            {
                finalMedList.Add(med.GetDTO(userDictionary));
            }

            return finalMedList;
        }

        /// <summary>
        /// Order medications for a patient
        /// </summary>
        /// <param name="site">ISite instance</param>
        /// <param name="patientId">Patient identifier</param>
        /// <param name="user">User object</param>
        /// <param name="type">Order type</param>
        /// <param name="orderingPhysicianId">Ordering physician identifier</param>
        /// <param name="notes">Notes applied to all medication orders</param>
        /// <param name="serviceOptions">Service option selections</param>
        /// <param name="authType">Authentication type used for ordering</param>
        /// <param name="orders">List of medication orders</param>
        /// <returns>null on permission error, non-whitespace on execution error, empty string on success</returns>
        public async Task<string> PostMedOrders(ISite site, string patientId, User user, string type, int orderingPhysicianId, string notes, List<string> serviceOptions, string authType, List<OrderMedication> orders)
        {
            if (!user.HasWritePermission(Permission.MED_SVC))
                return null;

            var patient = await _patientRepository.GetPatientByIdAsync(site.Id, patientId, user);
            var _time = new Time();
            var now = _time.Timestamp();

            var InterOverList = new Dictionary<string, string>();
            var res = new DB.Select
            {
                Sql = "SELECT name, alienkey FROM trx WHERE ibex=@ibex AND site=@site AND type='P' AND status<>'I'",
                Parameters = new SqlParameter[]
                {
                    new SqlParameter("@ibex", SqlDbType.Char) { Value = patientId },
                    new SqlParameter("@site", SqlDbType.TinyInt) { Value = site.Id }
                }
            }.RunForDataSet();

            if (res != null && res.Tables.Count > 0 && res.Tables[0].Rows.Count > 0)
            {
                foreach(DataRow dr in res.Tables[0].Rows)
                {
                    var name = dr["name"].ToString();
                    InterOverList[name.Substring(0, name.IndexOf(':') - 1)] = dr["alienkey"].ToString().Substring(11, 6);
                }
            }

            var losecsList = new DB.Select
            {
                Sql = "SELECT losecs FROM med WHERE ibex=@ibex AND site=@site AND status<>'I'",
                Parameters = new SqlParameter[]
                {
                    new SqlParameter("@ibex", SqlDbType.Char) { Value = patientId },
                    new SqlParameter("@site", SqlDbType.TinyInt) { Value = site.Id }
                }
            }.RunForListOfStrings("losecs");

            var parameters = new List<SqlParameter>
            {
                new SqlParameter("@ibex", SqlDbType.Char) { Value = patientId },
                new SqlParameter("@site", SqlDbType.TinyInt) { Value = site.Id }
            };
            var losecsParams = DB.GetParamsList(losecsList, SqlDbType.Int);

            if (losecsParams.Item1.Any())
            {
                parameters.AddRange(losecsParams.Item1);

                res = new DB.Select
                {
                    Sql = "SELECT brand_name, active_id FROM med_details WHERE site=@site AND ibex=@ibex AND losecs IN(" + string.Join(",", losecsParams.Item2) + ")",
                    Parameters = parameters.ToArray()
                }.RunForDataSet();

                if (res != null && res.Tables.Count > 0 && res.Tables[0].Rows.Count > 0)
                {
                    foreach (DataRow dr in res.Tables[0].Rows)
                    {
                        InterOverList[dr["brand_name"].ToString()] = dr["active_id"].ToString();
                    }
                }
            }

            if (serviceOptions != null && serviceOptions.Count > 0)
                notes += (!string.IsNullOrWhiteSpace(notes) ? "\n" : "") + string.Join(", ", serviceOptions);

            var meds = new List<Medication>();
            var medNames = new List<string>();
            var qlMedNums = new List<string>();
            var required = new List<string>();

            var drugDb = new DrugDB(site);

            // Ordering from a group
            if (type.Equals("mgp"))
            {
                foreach(var order in orders)
                {
                    var medInfo = new Dictionary<string, string>
                    {
                        { "dose", string.IsNullOrWhiteSpace(order.Dose) ? "*" : order.Dose },
                        { "unit", order.Unit },
                        { "route", order.Route },
                        { "time", order.Time },
                        { "notes", (order.Notes + (!string.IsNullOrWhiteSpace(order.Notes) && !string.IsNullOrWhiteSpace(notes) ? "\n" : "") + notes).Trim() },
                        { "repeat", order.Repeat },
                        { "authentication", authType },
                        { "drug_db_type", drugDb.DBType }
                    };

                    var med = Medication.LoadFromGroup(user, patientId, site.Id, Convert.ToInt32(order.Id), medInfo);

                    /* TODO: Does the API need to handle IV orders right now?
                    if (med.Type.Equals(Medication.Constants.TYPE_IV))
                    {
                        med.Rate = order.Rate;
                        med.RateUnit = order.RateUnit;
                        foreach(var comp in med.Components)
                        {
                            comp.Type = comp.GroupType;
                            ...
                        }
                    }*/

                    meds.Add(med);
                    required.AddRange(med.CheckRequired(med.Name, type, order.Overrides.Count > 0));
                }
            }
            // Ordering from a quick list
            else if (type.Equals("ql"))
            {
                foreach(var order in orders)
                {
                    var medData = new List<Dictionary<string, string>> {
                        Medication.LoadFromQuickList(user, patientId, site, Convert.ToInt32(order.Id), drugDb)
                    };
                    var med = ((new MedicationManager(_medicationRepository, _userRepository)).GetExtraQuickListData(drugDb, user, site, medData))[0];
                    if (medData[0].ContainsKey("ndc") && medData[0]["ndc"].Equals("ft"))
                    {
                        med.Name = medData[0]["brand"];
                    }
                    else {
                        med.Name = order.Name;
                        qlMedNums.Add(order.Id);
                    }

                    med.Dose = (string.IsNullOrWhiteSpace(order.Dose) ? "*" : order.Dose);
                    med.Unit = order.Unit;
                    med.Route = order.Route;
                    med.Time = order.Time;
                    med.Schedule = order.Time;
                    if (med.Schedule.Length > 10)
                    {
                        med.Schedule = med.Schedule.Substring(0, 10);
                    }
                    med.Repeat = order.Repeat;
                    med.Notes = (order.Notes + (!string.IsNullOrWhiteSpace(order.Notes) && !string.IsNullOrWhiteSpace(notes) ? "\n" : "") + notes).Trim();
                    med.Authentication = authType;
                    med.Type = Medication.Constants.TYPE_MEDICATION;

                    meds.Add(med);
                    required.AddRange(med.CheckRequired(med.Name, type, order.Overrides.Count > 0));
                }
            }
            // Single, other order type
            else
            {
                // Create the medication for the single order
                var med = new Medication(user);
                var order = orders[0];

                /* TODO: Figure out if IV drugs are orderable through the API and
                 * update object to support them, if needed
                 * /
                if (type.Equals("IV"))
                {

                }*/
                if (!order.Id.ToLowerInvariant().Equals("ft"))
                {
                    var component = new Medication.Component(site.Id, null);
                    component.LoadFromPackagingId(order.Id);
                    component.Ibex = patient.Ibex;
                    component.Site = site.Id;
                    component.DrugDBType = med.DrugDB.GetInstance().GetDBType();

                    // TODO: ibex4q was setting product code and procedure code here, using 'q_prod' and 'q_proc' inputs. See if we need those right now.

                    med.Components.Add(component);
                    med.Type = Medication.Constants.TYPE_MEDICATION;

                    // TODO: ibex4q was checking for the existence of $MED_NAME{''}, and using that for the name if it existed, else GetName(). Where does that come from?
                    med.Name = med.GetName();
                }
                else
                {
                    med.Name = order.Name;
                    med.Type = Medication.Constants.TYPE_FREE_TEXT;
                }

                med.Dose = string.IsNullOrWhiteSpace(order.Dose) ? "*" : order.Dose;
                med.Unit = order.Unit;
                med.Route = order.Route;
                med.Time = order.Time;
                med.Notes = (order.Notes + (!string.IsNullOrWhiteSpace(order.Notes) && !string.IsNullOrWhiteSpace(notes) ? "\n" : "") + notes).Trim();
                med.Repeat = order.Repeat;
                med.Authentication = authType;

                meds.Add(med);
                required.AddRange(med.CheckRequired(med.Name, type, order.Overrides.Count > 0));
            }

            if (orderingPhysicianId == 0)
                required.Insert(0, "Ordering Physician is required");

            if (required.Count > 0)
                return string.Join("\n", required);

            // Restore the interactions and reactions per medication, so we can match them up to override rationale
            MedicationManager.AddInteractionsAndReactionsToMedications(site, meds, patientId, _medicationRepository);

            var ordersPlaced = false;

            // Either all the medications will be ordered or none of the medications will be ordered
            // First, store to the database. If any entry fails, don't write the chart and roll back.
            // Second, gather all the Chart entries into a single string, then perform a single write.
            // This simplifies the write operation to a single 'print' since it would be a pain to remove
            // any entries that had already been written.
            var connection = DB.GetConnectionString();
            using (var con = new SqlConnection(connection))
            {
                con.Open();
                var transaction = con.BeginTransaction();
                var EMR = new EMR(site.Id, patientId, true);
                try
                {
                    using (var context = new IbexContext(con))
                    {
                        context.Database.UseTransaction(transaction);

                        var rand = new Random();

                        foreach (var med in meds)
                        {
                            med.Ibex = patient.Ibex;
                            med.Site = site.Id;
                            med.OrderForUserId = orderingPhysicianId;
                            med.OrderUserId = user.Id;
                            med.Losecs =  _time.DiffSeconds(patient.Ibex) + rand.Next(1, 50000);
                            if (med.OrderDate == null)
                                med.OrderDate = (new Time()).Timestamp();

                            context.Entry(med).State = EntityState.Added;
                        }

                        await context.SaveChangesAsync();
                    }

                    var emrLines = new List<EMR.Line>();
                    var i = 0;
                    foreach(var med in meds)
                    {
                        var line = await _medSvc.ChartEntry(patient, med, now, InterOverList, orders[i].Overrides);
                        emrLines.Add(line);
                        medNames.Add(med.GetFullName());
                        i++;
                    }

                    if (EMR.WriteLines(emrLines.ToArray()))
                    {
                        Chart.OnChartWrite(site, patientId, user.Id);
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
                    DTFL.Write(site.Id, user.Id, ex, "Medication Order Save");
                    transaction.Rollback();
                    return "F";
                }
                catch (Exception ex)
                {
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
                await MeaningfulUse.LogCreation(user, patientId, "MEDICATION SERVICE");
                var statuses = new Dictionary<string, Dictionary<string, int>>
                {
                    { "color", new Dictionary<string, int>() },
                    { "code", new Dictionary<string, int>() }
                };

                foreach(var med in meds)
                {
                    MedicationManager.TriggerFile(site, patientId, user.Id, "ENTERED", med.Losecs);

                    var medStatus = med.GetMedStatus();
                    if (medStatus.ContainsKey("custom") && !string.IsNullOrWhiteSpace(medStatus["custom"]))
                    {
                        if (medStatus.ContainsKey("color"))
                        {
                            if (!statuses["color"].ContainsKey(medStatus["color"]))
                            {
                                statuses["color"][medStatus["color"]] = 1;
                            }
                            else
                            {
                                statuses["color"][medStatus["color"]]++;
                            }
                        }
                    }
                    else
                    {
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
                }

                var index = "";
                foreach(var lookup in MedicationActions.Constants.SORTED_STATUSES)
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

                var update = new DB.Update
                {
                    Sql = "UPDATE pat SET ord30=@ord30 WHERE ibex=@ibex AND site=@site",
                    Parameters = new SqlParameter[]
                    {
                        new SqlParameter("@ord30", SqlDbType.Char) { Value = index },
                        new SqlParameter("@ibex", SqlDbType.Char) { Value = patientId },
                        new SqlParameter("@site", SqlDbType.TinyInt) { Value = site.Id }
                    }
                }.Run();

                // If the user is not the ordering physician, mail the orderer (if they aren't ordering-only)
                if (user.Id != orderingPhysicianId && site.GetOrgOption("MED_SVC_NOTIFY_ORD_PHYS").Equals("Y"))
                {
                    var ordOnly = new DB.Select
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
            if (qlMedNums.Count > 0) {
                var medNumParams = DB.GetParamsList(qlMedNums, SqlDbType.Int);
                var rxlParams = new List<SqlParameter> {
                    new SqlParameter("@usr", SqlDbType.Int) { Value = user.Id },
                    new SqlParameter("@site", SqlDbType.TinyInt) { Value = site.Id },
                    new SqlParameter("@type", SqlDbType.Char) { Value = "M" }
                };
                rxlParams.AddRange(medNumParams.Item1);
                new DB.Update
                {
                    Sql = "UPDATE rxl SET usage=ISNULL(usage,0)+1 WHERE usr=@usr AND site=@site AND type=@type AND num IN(" + string.Join(",", medNumParams.Item2) + ")",
                    Parameters = rxlParams.ToArray()
                }.Run();
            }

            CreateTrigger(site, patient, user.Id, "place", meds);

            return "";
        }

        public async Task<List<Order>> GetPatientOrders(byte siteId, string patientId, bool includeQueries = false)
        {
            var orders = await _patientRepository.GetPatientOrders(siteId, patientId);
            if (includeQueries)
            {
                //TODO: do something here for queries
            }
            return orders;
        }

        /// <summary>
        /// Acknowledge a medication order
        /// </summary>
        /// <param name="siteId">Site identifier</param>
        /// <param name="patientId">Patient identifier</param>
        /// <param name="user">User object</param>
        /// <param name="orderId">Order identifier</param>
        /// <returns>null on permission error, non-whitespace on execution error, empty string on success</returns>
        public async Task<string> PlaceOrder(byte siteId, string patientId, User user, List<Order> orders)
        {
            if (!user.HasWritePermission(Permission.ORDERS))
                return null;

            var connection = DB.GetConnectionString();
            using (var con = new SqlConnection(connection))
            {
                con.Open();

                var root = new DB.Select
                {
                    Sql = "SELECT root FROM org WHERE site=@site",
                    Connection = con,
                    Parameters = new SqlParameter[]
                        {
                            new SqlParameter("@site", SqlDbType.TinyInt) { Value = siteId }
                        }
                }.RunForScalar();

                var patient = await _patientRepository.GetPatientByIdAsync(siteId, patientId, user);
                var chart = new EMR(siteId, patientId, true);

                var orderSvcKeys = orders.Select(x => x.ServiceCode).ToList();

                var customDepts = OrderEntry.LoadCustomDepartmentIndicators(siteId);
                var indicatorUpdates = new Dictionary<string, OrderEntry.DepartmentIndicator>();

                var orderSettings = OrderEntry.LoadOrderSettings(siteId, orderSvcKeys);
                var queryDefaults = orderSvcKeys.Any() ? OrderEntry.LoadQueryDefaults(siteId, orderSvcKeys) : new Dictionary<string, Dictionary<string, Dictionary<string, string>>>();

                var mnemonicList = new List<string>();
                foreach (var svc in queryDefaults.Keys)
                {
                    mnemonicList.AddRange(queryDefaults[svc].Keys.ToList());
                }
                mnemonicList = mnemonicList.Distinct().ToList();

                var queryInfo = OrderEntry.LoadQueryInfo(siteId, mnemonicList);
                var instructions = OrderEntry.LoadQueryInstructions(siteId, mnemonicList);

                var gender = patient.Demographics.Gender;
                var genderCode =
                    gender == Gender.Male ? "M" :
                    gender == Gender.Female ? "F" :
                    gender == Gender.Unknown ? "U" : "O";

                var _time = new Time();
                var now = _time.Timestamp();
                var createQueueFile = false;
                var createdPendingOrders = false;
                var hasIdsForInterface = !string.IsNullOrWhiteSpace(patient.AcctNum) && !string.IsNullOrWhiteSpace(patient.MedicalRecordNumber);
                var failedOrders = new List<Order>();
                foreach (var order in orders)
                {
                    var orderingPhysician = await _userRepository.GetUserByIdAsync(order.OrderingPhysician);
                    var queries = order.Queries.ToDictionary(q => q.Mnemonic, q => q);

                    if (order.Events == null || order.Events.Count == 0)
                    {
                        order.Events = new List<Event>
                        {
                            new Event
                            {
                               User = user.ToMinimalUser(),
                               Type = "ORDERED",
                               DateTime = DateTime.Now
                            }
                        };
                    }

                    var orderEvent = new Event();
                    var startEvent = new Event();
                    var sendEvent = new Event();

                    foreach (var theEvent in order.Events)
                    {
                        switch (theEvent.Type)
                        {
                            case "SENT":
                                sendEvent = theEvent;
                                break;
                            case "STARTED":
                                startEvent = theEvent;
                                break;
                            case "ORDERED":
                            default:
                                orderEvent = theEvent;
                                break;
                        }
                    }

                    var settings = orderSettings[order.ServiceCode];
                    var sendWhenOrdered = settings["task"] == "N";
                    var values = new Dictionary<string, object>
                                {
                                    { Transaction.Constants.Name, !string.IsNullOrWhiteSpace(order.Name) ? order.Name : settings["name"] },
                                    { Transaction.Constants.Service, 210 },
                                    { Transaction.Constants.Type, "Q" },
                                    { Transaction.Constants.Date, orderEvent.DateTime.ToString(Time.Constants.FORMAT_TIMESTAMP) },
                                    { Transaction.Constants.Alienkey, order.ServiceCode },
                                    { Transaction.Constants.Quantity, order.Quantity },
                                    { Transaction.Constants.CPT, settings["cpt"] },
                                    { Transaction.Constants.Tid, settings["department"] },
                                    { Transaction.Constants.ServiceType, settings["svctype"] },
                                    { Transaction.Constants.LevelPoints, settings["lvlpts"] },
                                    { Transaction.Constants.Amount, settings["amt"] },
                                    { Transaction.Constants.APC, settings["apc"] },
                                    { Transaction.Constants.Face, settings["face"] },
                                    { Transaction.Constants.RiskRed, queries.ContainsKey("COMMENT") ? queries["COMMENT"].Value : "" },
                                    { Transaction.Constants.RiskGreen, queries.ContainsKey("REASON") ? queries["REASON"].Value : "" },
                                };

                    var t = new Transaction(siteId, patient, user.Id, values);
                    var losecs = order.Losecs = t.AddTransaction();

                    var orderStatus = OrderEntry.Constants.PENDING_ORDER;
                    if (sendWhenOrdered && hasIdsForInterface)
                    {
                        if (order.Direction == OrderEntry.Constants.INBOUND_ORDER)
                        {
                            // TODO: do stuff so interfaces calling this work properly
                        }
                        sendEvent.DateTime = orderEvent.DateTime;
                        sendEvent.User.Id = user.Id;

                        startEvent.DateTime = orderEvent.DateTime;
                        startEvent.User.Id = user.Id;

                        order.SendMinutes = 0;
                        orderStatus = OrderEntry.Constants.SENT_ORDER;

                        // Create the interface trigger
                        var triggerFile = root.ToString().Trim() + "link\\snd\\" + patient.Ibex;
                        if (!String.IsNullOrWhiteSpace(patient.AcctNum) && !String.IsNullOrWhiteSpace(patient.MedicalRecordNumber) && !File.Exists(triggerFile))
                            File.Create(triggerFile);
                    }

                    createdPendingOrders = true;

                    if (customDepts.ContainsKey(settings["department"]))
                    {
                        if (!indicatorUpdates.ContainsKey(settings["department"]))
                        {
                            var deptIndicator = customDepts[settings["department"]];
                            indicatorUpdates.Add(settings["department"], new OrderEntry.DepartmentIndicator
                            {
                                Status = orderStatus,
                                DepartmentLetter = deptIndicator.DepartmentLetter,
                                PatientColumn = deptIndicator.PatientColumn,
                            });
                        }
                    }
                    else if (!indicatorUpdates.ContainsKey(settings["face"]))
                    {
                        indicatorUpdates.Add(settings["face"], new OrderEntry.DepartmentIndicator
                        {
                            Status = orderStatus,
                            PatientColumn = OrderEntry.Constants.INDICATOR_COLUMNS[settings["face"]]
                        });
                    }

                    for (int i = 0; i < order.Quantity; i++)
                    {
                        var transaction = con.BeginTransaction();
                        var goodSoFar = true;
                        try
                        {
                            var sqlCols = new List<string>
                            {
                                "site", "ibex", "losecs", "dteorder", "face", "status", "flag", "alienkey", "name", "doctor", "hospid", "unit", "usrorder", "department", "repeat", "task", "prompt", "promptreq", "data_source"
                            };

                            var sqlVals = new List<string>
                            {
                                "@site", "@ibex", "@losecs", "@dteorder", "@face", "@status", "@flag", "@alienkey", "@name", "@doctor", "@hospid", "@unit", "@usrorder", "@department", "@repeat", "@task", "@prompt", "@promptreq", "@data_source"
                            };

                            var sqlParams = new List<SqlParameter> {
                                    new SqlParameter("@site", SqlDbType.SmallInt) { Value = siteId },
                                    new SqlParameter("@ibex", SqlDbType.Char) { Value = patientId },
                                    new SqlParameter("@losecs", SqlDbType.Int) { Value = order.Losecs },
                                    new SqlParameter("@dteorder", SqlDbType.VarChar) { Value = orderEvent.DateTime.ToString(Time.Constants.FORMAT_TIMESTAMP_NO_SECONDS) },
                                    new SqlParameter("@face", SqlDbType.Char) { Value = settings["face"] },
                                    new SqlParameter("@status", SqlDbType.Char) { Value = orderStatus },
                                    new SqlParameter("@flag", SqlDbType.Char) { Value = order.Direction },
                                    new SqlParameter("@alienkey", SqlDbType.VarChar) { Value = order.ServiceCode },
                                    new SqlParameter("@name", SqlDbType.VarChar) { Value = !string.IsNullOrWhiteSpace(order.Name) ? order.Name : settings["name"] },
                                    new SqlParameter("@doctor", SqlDbType.Int) { Value = orderingPhysician.Id },
                                    new SqlParameter("@hospid", SqlDbType.Char) { Value = orderingPhysician.HospitalId },
                                    new SqlParameter("@unit", SqlDbType.VarChar) { Value = order.Id },
                                    new SqlParameter("@usrorder", SqlDbType.Int) { Value = user.Id },
                                    new SqlParameter("@department", SqlDbType.VarChar) { Value = settings["department"] },
                                    new SqlParameter("@repeat", SqlDbType.Char) { Value = settings["repeat"] },
                                    new SqlParameter("@task", SqlDbType.Char) { Value = settings["task"] },
                                    new SqlParameter("@prompt", SqlDbType.Char) { Value = settings["prompt"] },
                                    new SqlParameter("@promptreq", SqlDbType.Char) { Value = settings["promptreq"] },
                                    new SqlParameter("@data_source", SqlDbType.Char) { Value = Domain.Constants.Data_Source_Mobile },
                            };

                            if (orderStatus == OrderEntry.Constants.SENT_ORDER)
                            {
                                sqlCols.AddRange(new[] { "dtesend", "usrsend", "dtestart", "sendminutes" });
                                sqlVals.AddRange(new[] { "@dtesend", "@usrsend", "@dtestart", "@sendminutes" });
                                sqlParams.AddRange(new[] {
                                    new SqlParameter("@dtesend", sendEvent.DateTime.ToString(Time.Constants.FORMAT_TIMESTAMP_NO_SECONDS)),
                                    new SqlParameter("@usrsend", sendEvent.User.Id),
                                    new SqlParameter("@dtestart", startEvent.DateTime.ToString(Time.Constants.FORMAT_TIMESTAMP_NO_SECONDS)),
                                    new SqlParameter("@sendminutes", SqlDbType.Int) { Value = order.SendMinutes },
                                });
                            }

                            var result = new DB.Insert
                            {
                                Connection = con,
                                Transaction = transaction,
                                Sql = "INSERT INTO ord_info(" + string.Join(",", sqlCols) + ") " +
                                "VALUES (" + string.Join(",", sqlVals) + ")",
                                Parameters = sqlParams.ToArray(),
                            }.Run();
                        }
                        catch (Exception ex)
                        {
                            goodSoFar = false;
                            order.AddError(ex.Message);
                        }

                        if (!string.IsNullOrEmpty(order.ServiceCode) && queryDefaults.ContainsKey(order.ServiceCode))
                        {
                            foreach (var mnemonic in queryDefaults[order.ServiceCode].Keys)
                            {
                                // If we don't have a specific query (either missing or not present on the Orders page), add it with the default values
                                if (!queries.ContainsKey(mnemonic))
                                {
                                    var currentQuery = queryDefaults[order.ServiceCode][mnemonic];

                                    // This should only happen with messed-up data, where there's a default for a query, but no settings for it
                                    if (!queryInfo.ContainsKey(mnemonic))
                                        continue;

                                    var currentQueryInfo = queryInfo[mnemonic];
                                    order.Queries.Add(new Query
                                    {
                                        Mnemonic = mnemonic,
                                        Value = genderCode == "M" && !String.IsNullOrWhiteSpace(currentQuery["default_value_male"]) ? currentQuery["default_value_male"] :
                                                genderCode == "F" && !String.IsNullOrWhiteSpace(currentQuery["default_value_female"]) ? currentQuery["default_value_female"] :
                                                currentQuery["default_value"],
                                        Description = queryInfo[mnemonic]["name"],
                                        Type = currentQueryInfo["type"],
                                        Required = currentQueryInfo["req"].ToString() == "Y",
                                        Order = order,
                                    });
                                }
                                else
                                {
                                    queries[mnemonic].Type = queryInfo[mnemonic]["type"];
                                    queries[mnemonic].Order = order;
                                    queries[mnemonic].Description = queryInfo[mnemonic]["name"];
                                }
                            }
                        }

                        var queryCounter = 1;
                        foreach (var query in order.Queries)
                        {
                            if (goodSoFar)
                            {
                                var convertedQuery = Query.ConvertToProperType(query);

                                try
                                {
                                    var querySaved = false;
                                    if (convertedQuery.Validate())
                                    {
                                        // TODO: I really hate how the ord_queries table uses ibex, site, and losecs, instead of just something like order_id to match on.
                                        var queryResult = new DB.Insert
                                        {
                                            Connection = con,
                                            Transaction = transaction,
                                            Sql = "INSERT INTO ord_queries(site, ibex, losecs, status, mnemonic, description, value, display_value, req, usr, query_ts, sequence, display_on_chart, query_num) " +
                                                   "VALUES (@site, @ibex, @losecs, @status, @mnemonic, @description, @value, @display_value, @req, @usr, @query_ts, @sequence, @display_on_chart, @query_num)",
                                            Parameters = new SqlParameter[] {
                                                new SqlParameter("@site", SqlDbType.TinyInt) { Value = siteId },
                                                new SqlParameter("@ibex", SqlDbType.Char) { Value = patientId },
                                                new SqlParameter("@losecs", SqlDbType.Int) { Value = order.Losecs },
                                                new SqlParameter("@status", SqlDbType.Char) { Value = Domain.Constants.ACTIVE_STATUS.ToString() },
                                                new SqlParameter("@mnemonic", SqlDbType.Char) { Value = convertedQuery.Mnemonic },
                                                new SqlParameter("@description", SqlDbType.VarChar) { Value = convertedQuery.Description },
                                                new SqlParameter("@value", SqlDbType.VarChar) { Value = convertedQuery.Value },
                                                new SqlParameter("@display_value", SqlDbType.VarChar) { Value = convertedQuery.DisplayValue },
                                                new SqlParameter("@req", SqlDbType.Char) { Value = queryDefaults[order.ServiceCode][convertedQuery.Mnemonic]["required"] },
                                                new SqlParameter("@usr", SqlDbType.Int) { Value = user.Id },
                                                new SqlParameter("@query_ts", SqlDbType.Char) { Value = now },
                                                new SqlParameter("@sequence", SqlDbType.SmallInt) { Value = queryInfo[convertedQuery.Mnemonic]["sequence"] },
                                                new SqlParameter("@display_on_chart", SqlDbType.Char) { Value = queryDefaults[order.ServiceCode][convertedQuery.Mnemonic]["default_display_on_chart"] },
                                                new SqlParameter("@query_num", SqlDbType.SmallInt) { Value = queryCounter++ },
                                            },
                                        }.Run();
                                        querySaved = queryResult == 1;
                                    }

                                    if (!querySaved)
                                    {
                                        order.AddError(convertedQuery.Error);
                                        goodSoFar = false;
                                    }
                                }
                                catch (Exception ex)
                                {
                                    order.AddError(ex.Message);
                                    goodSoFar = false;
                                }

                                // If a query can't do its action (like alpha paging failing), don't stop the order from proceeding
                                try
                                {
                                    convertedQuery.Action();
                                }
                                catch (Exception ex)
                                {
                                    order.AddError(ex.Message);
                                }
                            }
                        }
                        if (goodSoFar)
                        {
                            transaction.Commit();
                        }
                        else
                        {
                            failedOrders.Add(order);
                            transaction.Rollback();
                        }
                    }

                    var viewLine = new EMR.Line
                    {
                        LineHeader = new EMR.Line.Header
                        {
                            sys_time = now,
                            user = user.Id,
                            losecs = order.Losecs.ToString(),
                            chart_xref = order.Losecs.ToString(),
                        },
                        LinePart = new EMR.Line.Part
                        {
                            nct = EMR.Constants.NCT_HIDDEN_SECTION,
                            section = EMR.Constants.SECT_ORDERS,
                        },
                        DataSegments = new List<EMR.Line.DataSegment> {
                            new EMR.Line.DataSegment(EMR.Line.DataSegment.Constants.TYPE_TEXT, order.Name)
                        }
                    };

                    chart.WriteLine(viewLine, user.Id);

                    // Orders require signatures for the person who placed it, the ordering physician, and the attending
                    (new Signatures.AuditEntry(siteId, patientId, user.Id)).Save();

                    foreach (MinimalProvider p in patient.Providers)
                    {
                        if (p.Role.Id.Equals(Domain.Constants.Id_Doctor) && p.User != null && p.User.Id > 0)
                        {
                            if (orderingPhysician.Id != p.User.Id && user.Id != p.User.Id)
                            {
                                (new Signatures.AuditEntry(siteId, patientId, user.Id)).Save();
                            }
                            break;
                        }
                    }

                    // Inbound orders shouldn't require a signature from the ordering physician
                    if (!orderingPhysician.IsOrderingOnly() && orderingPhysician.Id != user.Id && order.Direction != OrderEntry.Constants.INBOUND_ORDER)
                    {
                        (new Signatures.AuditEntry(siteId, patientId, orderingPhysician.Id)).Save();
                    }
                }

                // Update the flag that shows we've got new orders
                if (createdPendingOrders)
                {
                    new DB.Update
                    {
                        Sql = "UPDATE pat set ord47 = 'Y' where site=@site and ibex=@ibex",
                        Parameters = new SqlParameter[]
                        {
                            new SqlParameter("@ibex", SqlDbType.Char) { Value = patientId },
                            new SqlParameter("@site", SqlDbType.TinyInt) { Value = siteId }
                        }
                    }.Run();
                }

                if (createQueueFile && hasIdsForInterface)
                {
                    OrderEntry.CreateQueueFile(siteId, patientId);
                }

                OrderEntry.UpdateIndicators(siteId, patientId, indicatorUpdates);

                if (failedOrders.Any())
                    return "The following services were not ordered: " + String.Join("; ", failedOrders.Select(x => x.Name + ": " + String.Join(", ", x.Errors)));
            }
            return "";
        }

        /// <summary>
        /// Get a patient from a particular site, by identifier
        /// </summary>
        /// <param name="siteId">Site identifier</param>
        /// <param name="patientId">Patient identifier</param>
        /// <param name="user">User object</param>
        /// <param name="expand">Optional data expansion parameter</param>
        /// <returns>Patient object</returns>
        public async Task<Patient> GetPatientByIdAsync(byte siteId, string patientId, User user, string expand = "")
        {
            var result = await _patientRepository.GetPatientByIdAsync(siteId, patientId, user, expand);
            return result;
        }

        /// <summary>
        /// Get the list of allergies for a particular patient
        /// </summary>
        /// <param name="siteId">Site identifier</param>
        /// <param name="patientId">Patient identifier</param>
        /// <param name="user">User object</param>
        /// <returns>List of Allergy objects</returns>
        public async Task<List<Allergy>> GetPatientAllergies(byte siteId, string patientId, User user)
        {
            if (!user.CanNavigateTo(Navigation.Constants.ALLERGIES))
            {
                return null;
            }
            var result = await _patientRepository.GetPatientAllergies(siteId, patientId, user);
            return result;
        }

        /// <summary>
        /// Get the list of current medications for a particular patient
        /// </summary>
        /// <param name="siteId">Site identifier</param>
        /// <param name="patientId">Patient identifier</param>
        /// <param name="user">User object</param>
        /// <returns>List of Current Medication objects</returns>
        public async Task<List<CurrentMedication>> GetPatientCurrentMedications(byte siteId, string patientId, User user)
        {
            if (!user.CanNavigateTo(Navigation.Constants.CURRENT_MEDS))
            {
                return null;
            }
            var result = await _patientRepository.GetPatientCurrentMedications(siteId, patientId, user);
            return result;
        }

        /// <summary>
        /// Get a list of orders with late results for a particular patient
        /// </summary>
        /// <param name="siteId">Site identifier</param>
        /// <param name="patientId">Patient identifier</param>
        /// <param name="user">User object</param>
        /// <returns>List of Order objects reprsenting orders with late results</returns>
        public async Task<List<Order>> GetPatientLateResults(byte siteId, string patientId, User user)
        {
            // TODO: Probably need some permission check here. But what?
            var result = await _patientRepository.GetPatientLateResults(siteId, patientId, user);
            return result;
        }

        /// <summary>
        /// Get a clinical pathway, with its group and query information included and checked against the patient
        /// </summary>
        /// <param name="siteId">Site identifier</param>
        /// <param name="patientId">Patient identifier</param>
        /// <param name="pathwayNum">Pathway identifier/number</param>
        /// <param name="user">User object</param>
        /// <returns>Clinical pathway with groups and queries populated</returns>
        public async Task<ClinicalPathway> GetPatientPathway(byte siteId, string patientId, int pathwayNum, User user)
        {
            var clinicalPathway = await _siteRepository.GetOrderPathwayByIdAsync(siteId, pathwayNum);

            var sql = @"
                SELECT
                    svc.code, svc.svctype, svc.name, svc.face, svc.maxqty, svc.svc, case when f.favoritenum is not null then 1 else 0 end as is_favorite
                FROM
                    svc
                    INNER JOIN grp ON grp.code = svc.code and svc.site = grp.site
                    inner join org on org.svccs = grp.site
                    left join favorites f on f.type = 'O' and f.favoritenum = svc.svc and f.drsnum = @user
                WHERE
                        grp.num = @num
                    AND grp.type = 'S'
                    AND org.site = @site
                    AND svc.status = 'A'
                ORDER BY
                    checkde desc, svc.name";

            var serviceInfo = new DB.Select
            {
                Sql = sql,
                Parameters = new SqlParameter[]
                {
                    new SqlParameter("@num", SqlDbType.Int) { Value = pathwayNum },
                    new SqlParameter("@site", SqlDbType.TinyInt) { Value = siteId },
                    new SqlParameter("@user", SqlDbType.Int) { Value = user.Id },
                }
            }.RunForListOfDictionaries();

            var services = serviceInfo.Select(s => new Service
            {
                Number = Convert.ToInt32(s["svc"]),
                Code = s["code"],
                Name = s["name"],
                Type = Convert.ToInt32(s["svctype"]),
                InterfaceType = s["face"],
                MaxQuantity = Convert.ToInt32(s["maxqty"]),
                IsUserFavorite = Convert.ToByte(s["is_favorite"]) == 1
            }).ToList();

            var groups = await CreateOrderServices(user, patientId, services);
            clinicalPathway.Groups.AddRange(groups);

            return clinicalPathway;
        }

        public async Task<List<Query>> GetServiceQueries(User user, string patientId, string serviceCode)
        {
            var queries = new List<Query>();

            var siteId = user.SiteId;
            var queryDefaults = OrderEntry.LoadQueryDefaults(siteId, new List<string> { serviceCode });

            // If we don't have any queries attached to this service, return
            if (!queryDefaults.ContainsKey(serviceCode))
                return queries;

            var mnemonicList = queryDefaults[serviceCode].Keys.ToList();

            var orderDefaults = OrderEntry.LoadQueryInfo(siteId, mnemonicList, true);
            var instructions = OrderEntry.LoadQueryInstructions(siteId, mnemonicList);


            var patient = await _patientRepository.GetPatientByIdAsync(siteId, patientId, user);

            var gender = patient.Demographics.Gender;
            var genderCode =
                gender == Gender.Male ? "M" :
                gender == Gender.Female ? "F" :
                gender == Gender.Unknown ? "U" : "O";

            var restricted = new List<Dictionary<string, string>>();
            foreach (var svc in queryDefaults.Keys)
            {
                var restrictedKeys = queryDefaults[svc].Keys;
                foreach (var k in restrictedKeys)
                {
                    var type = orderDefaults.ContainsKey(k) && orderDefaults[k].ContainsKey("type") ? orderDefaults[k]["type"] : "";
                    if (type.Equals("code") || type.Equals("other"))
                    {
                        var restrictedInfo = new Dictionary<string, string>
                        {
                            { "mnemonic", k },
                            { "svccode", svc },
                            { "gender", genderCode.Equals("U") || !orderDefaults[k]["gender_defaults"].Equals("Y") ? "O" : genderCode}
                        };

                        restricted.Add(restrictedInfo);
                    }
                }
            }
            var restrictedLists = OrderEntry.GetRestrictedCodesetList(siteId, restricted);

            var keys = queryDefaults[serviceCode].Keys.ToList();
            keys.Sort(delegate (string x, string y)
            {
                if (!orderDefaults.ContainsKey(x))
                    return 1;
                if (!orderDefaults.ContainsKey(y))
                    return -1;

                return Convert.ToInt32(orderDefaults[x]["sequence"]) - Convert.ToInt32(orderDefaults[y]["sequence"]);
            });

            foreach (var field in keys)
            {
                if (!orderDefaults.ContainsKey(field))
                    continue;

                var type = orderDefaults[field].ContainsKey("type") && !string.IsNullOrWhiteSpace(orderDefaults[field]["type"]) ? orderDefaults[field]["type"] : "";
                if (type.Equals(Query.Constants.TYPE_HIDDEN) || type.Equals(Query.Constants.TYPE_ALPHA))
                    continue;

                var dispOnce = (orderDefaults[field].ContainsKey("display_once") && !string.IsNullOrWhiteSpace(orderDefaults[field]["display_once"])) ? orderDefaults[field]["display_once"].Equals("Y") : false;

                // Assign default value
                var df = queryDefaults[serviceCode][field]["default_value"];
                if (orderDefaults[field]["gender_defaults"].Equals("Y"))
                {
                    if (genderCode.Equals("M"))
                    {
                        df = queryDefaults[serviceCode][field]["default_value_male"];
                    }
                    else if (genderCode.Equals("F"))
                    {
                        df = queryDefaults[serviceCode][field]["default_value_female"];
                    }
                }

                var query = new Query
                {
                    Description = orderDefaults[field]["name"],
                    Type = type,
                    Mnemonic = field,
                    Required = queryDefaults[serviceCode][field]["required"].Equals("Y"),
                    Value = df,
                    DisplayOnce = dispOnce,
                    Sequence = Convert.ToInt32(orderDefaults[field]["sequence"]),
                };

                if (type.Equals(Query.Constants.TYPE_TEXT))
                {
                    var maxLength = Convert.ToInt32(orderDefaults[field]["max_length"]);
                    var convertedQuery = (TextQuery)Query.ConvertToProperType(query);

                    if (maxLength > 0)
                        convertedQuery.MaxLength = maxLength;

                    queries.Add(convertedQuery);
                }
                else if (type.Equals(Query.Constants.TYPE_NUMERIC))
                {
                    var maxValue = Convert.ToInt32(orderDefaults[field]["max_length"]);
                    var convertedQuery = (NumericQuery)Query.ConvertToProperType(query);

                    if (maxValue > 0)
                        convertedQuery.MaxValue = maxValue;

                    queries.Add(convertedQuery);
                }
                else if (type.Equals(Query.Constants.TYPE_DROPDOWN))
                {
                    var maxValue = Convert.ToInt32(orderDefaults[field]["max_length"]);
                    var convertedQuery = (DropdownQuery)Query.ConvertToProperType(query);

                    var queryCodes = OrderEntry.Codeset.GetValues(siteId, orderDefaults[field]["type_options"]);
                    convertedQuery.Codes.AddRange(
                        queryCodes
                            .Select(c => new Code { Display = c.Value["name"], Value = c.Key })
                            .Where(x => restrictedLists.ContainsKey(serviceCode) && restrictedLists[serviceCode].ContainsKey(field) ? restrictedLists[serviceCode][field].Contains(x.Value) : true )
                            .OrderBy(x => x.Display)
                    );

                    queries.Add(convertedQuery);
                }
                else if (type.Equals(Query.Constants.TYPE_DROPDOWNOO))
                {
                    var maxLength = Convert.ToInt32(orderDefaults[field]["max_length"]);
                    var convertedQuery = (DropdownOrOtherQuery)Query.ConvertToProperType(query);

                    if (maxLength > 0)
                        convertedQuery.MaxLength = maxLength;

                    var queryCodes = OrderEntry.Codeset.GetValues(siteId, orderDefaults[field]["type_options"]);
                    convertedQuery.Codes.AddRange(
                        queryCodes
                            .Select(c => new Code { Display = c.Value["name"], Value = c.Key })
                            .Where(x => restrictedLists.ContainsKey(serviceCode) && restrictedLists[serviceCode].ContainsKey(field) ? restrictedLists[serviceCode][field].Contains(x.Value) : true)
                            .OrderBy(x => x.Display)
                    );

                    queries.Add(convertedQuery);
                }
                else if (type.Equals(Query.Constants.TYPE_INSTRUCTION))
                {
                    if (instructions.ContainsKey(field))
                        query.Value = instructions[field];

                    queries.Add(query);
                }
                else
                {
                    queries.Add(query);
                }
            }

            queries.Sort(delegate(Query a, Query b)
            {
                return a.Sequence - b.Sequence;
            });

            return queries;
        }

        public async Task<List<Group>> CreateOrderServices(User user, string patientId, List<Service> services)
        {
            var siteId = user.SiteId;
            var svcList = services.Where(s => s.InterfaceType != "S").Select(s => s.Code).ToList();

            var ordDefaults = OrderEntry.LoadQueryDefaults(siteId, svcList);
            var mnemonicList = new List<string>();
            foreach (var svc in ordDefaults.Keys)
            {
                mnemonicList.AddRange(ordDefaults[svc].Keys);
            }
            mnemonicList = mnemonicList.Distinct().ToList();

            var orderDefaults = OrderEntry.LoadQueryInfo(siteId, mnemonicList, true);
            var instructions = OrderEntry.LoadQueryInstructions(siteId, mnemonicList);

            var patient = await _patientRepository.GetPatientByIdAsync(siteId, patientId, user);

            var gender = patient.Demographics.Gender;
            var genderCode =
                gender == Gender.Male ? "M" :
                gender == Gender.Female ? "F" :
                gender == Gender.Unknown ? "U" : "O";

            var restricted = new List<Dictionary<string, string>>();
            foreach (var svc in ordDefaults.Keys)
            {
                var restrictedKeys = ordDefaults[svc].Keys;
                foreach (var k in restrictedKeys)
                {

                    var type = orderDefaults.ContainsKey(k) && orderDefaults[k].ContainsKey("type") ? orderDefaults[k]["type"] : "";
                    if (type.Equals("code") || type.Equals("other"))
                    {
                        var restrictedInfo = new Dictionary<string, string>
                        {
                            { "mnemonic", k },
                            { "svccode", svc },
                            { "gender", genderCode.Equals("U") || !orderDefaults[k]["gender_defaults"].Equals("Y") ? "O" : genderCode}
                        };

                        restricted.Add(restrictedInfo);
                    }
                }
            }
            var restrictedLists = OrderEntry.GetRestrictedCodesetList(siteId, restricted);

            var groupNames = new DB.Select
            {
                Sql = "SELECT cde.num,cde.name FROM cde inner join org on org.svccs=cde.site WHERE type = @type AND org.site = @site AND cde.status = 'A'",
                Parameters = new SqlParameter[]
                {
                    new SqlParameter("@type", SqlDbType.Char) { Value = "S"},
                    new SqlParameter("@site", SqlDbType.TinyInt) { Value = siteId }
                }
            }.RunForDictionary("num");

            var groups = new Dictionary<int, Group>();
            foreach (var service in services)
            {
                if (!groupNames.ContainsKey(service.Type.ToString()))
                    continue;

                if (!groups.ContainsKey(service.Type))
                    groups.Add(service.Type, new Group { Name = service.Type == 0 ? "Common" : groupNames[service.Type.ToString()]["name"] });

                if (ordDefaults.ContainsKey(service.Code) && !service.InterfaceType.Equals("S"))
                {
                    var keys = ordDefaults[service.Code].Keys.ToList();
                    foreach (var field in keys)
                    {
                        if (!orderDefaults.ContainsKey(field))
                            continue;

                        var type = orderDefaults[field].ContainsKey("type") && !string.IsNullOrWhiteSpace(orderDefaults[field]["type"]) ? orderDefaults[field]["type"] : "";
                        if (type.Equals(Query.Constants.TYPE_HIDDEN) || type.Equals(Query.Constants.TYPE_ALPHA))
                            continue;

                        var dispOnce = (orderDefaults[field].ContainsKey("display_once") && !string.IsNullOrWhiteSpace(orderDefaults[field]["display_once"])) ? orderDefaults[field]["display_once"].Equals("Y") : false;

                        // Assign default value
                        var df = ordDefaults[service.Code][field]["default_value"];
                        if (orderDefaults[field]["gender_defaults"].Equals("Y"))
                        {
                            if (genderCode.Equals("M"))
                            {
                                df = ordDefaults[service.Code][field]["default_value_male"];
                            }
                            else if (genderCode.Equals("F"))
                            {
                                df = ordDefaults[service.Code][field]["default_value_female"];
                            }
                        }

                        var query = new Query
                        {
                            Description = orderDefaults[field]["name"],
                            Type = type,
                            Mnemonic = field,
                            Required = ordDefaults[service.Code][field]["required"].Equals("Y"),
                            Value = df,
                            DisplayOnce = dispOnce,
                            Sequence = Convert.ToInt32(orderDefaults[field]["sequence"]),
                        };

                        if (type.Equals(Query.Constants.TYPE_TEXT))
                        {
                            var maxLength = Convert.ToInt32(orderDefaults[field]["max_length"]);
                            var convertedQuery = (TextQuery)Query.ConvertToProperType(query);

                            if (maxLength > 0)
                                convertedQuery.MaxLength = maxLength;

                            service.Queries.Add(convertedQuery);
                        }
                        else if (type.Equals(Query.Constants.TYPE_NUMERIC))
                        {
                            var maxValue = Convert.ToInt32(orderDefaults[field]["max_length"]);
                            var convertedQuery = (NumericQuery)Query.ConvertToProperType(query);

                            if (maxValue > 0)
                                convertedQuery.MaxValue = maxValue;

                            service.Queries.Add(convertedQuery);
                        }
                        else if (type.Equals(Query.Constants.TYPE_DROPDOWN))
                        {
                            var maxValue = Convert.ToInt32(orderDefaults[field]["max_length"]);
                            var convertedQuery = (DropdownQuery)Query.ConvertToProperType(query);

                            var queryCodes = OrderEntry.Codeset.GetValues(siteId, orderDefaults[field]["type_options"]);
                            convertedQuery.Codes.AddRange(
                                queryCodes
                                    .Select(c => new Code { Display = c.Value["name"], Value = c.Key })
                                    .Where(x => restrictedLists.ContainsKey(service.Code) && restrictedLists[service.Code].ContainsKey(field) ? restrictedLists[service.Code][field].Contains(x.Value) : true)
                                    .OrderBy(x => x.Display));

                            service.Queries.Add(convertedQuery);
                        }
                        else if (type.Equals(Query.Constants.TYPE_DROPDOWNOO))
                        {
                            var maxLength = Convert.ToInt32(orderDefaults[field]["max_length"]);
                            var convertedQuery = (DropdownOrOtherQuery)Query.ConvertToProperType(query);

                            if (maxLength > 0)
                                convertedQuery.MaxLength = maxLength;

                            var queryCodes = OrderEntry.Codeset.GetValues(siteId, orderDefaults[field]["type_options"]);
                            convertedQuery.Codes.AddRange(
                                queryCodes
                                    .Select(c => new Code { Display = c.Value["name"], Value = c.Key })
                                    .Where(x => restrictedLists.ContainsKey(service.Code) && restrictedLists[service.Code].ContainsKey(field) ? restrictedLists[service.Code][field].Contains(x.Value) : true)
                                    .OrderBy(x => x.Display)
                            );

                            service.Queries.Add(convertedQuery);
                        }
                        else if (type.Equals(Query.Constants.TYPE_INSTRUCTION))
                        {
                            if (instructions.ContainsKey(field))
                                query.Value = instructions[field];

                            service.Queries.Add(query);
                        }
                        else
                        {
                            service.Queries.Add(query);
                        }
                    }
                }

                service.Queries.Sort(delegate (Query a, Query b)
                {
                    return a.Sequence - b.Sequence;
                });

                groups[service.Type].Services.Add(service);
            }

            return groups.Select(g => g.Value).OrderBy(g => g.Name).ToList();
        }

        /// <summary>
        /// Post results to a patient's chart
        /// </summary>
        /// <param name="siteId">Site identifier</param>
        /// <param name="patientId">Patient identifier</param>
        /// <param name="user">User object</param>
        /// <param name="lineNums">Result line numbers to post</param>
        /// <returns>Boolean success/failure flag</returns>
        public async Task<bool> PostPatientResults(byte siteId, string patientId, User user, List<int> lineNums)
        {
            var patient = await _patientRepository.GetPatientByIdAsync(siteId, patientId, user);
            if (patient == null)
                return false;

            var results = await GetPatientResults(siteId, patientId, user, null);
            var postedResults = Results.GetPostedResults(siteId, patient);
            lineNums = lineNums.Where(x => !postedResults.ContainsKey(x)).ToList();

            var site = await _siteRepository.GetSiteByIdAsync(siteId);
            var config = new Results.Config(site);

            int maxSize = 20;
            var maxSizeSetting = config.GetHl7Entry("tests^result", "max_size");
            if (!string.IsNullOrWhiteSpace(maxSizeSetting))
                maxSize = Convert.ToInt32(maxSizeSetting);

            var spaceLen = maxSize > 0 ? maxSize : 40;
            var spaces = new String(' ', spaceLen);
            var useTable = (config.GetEntry("print table format") ?? "").Equals("1");
            var labels = new List<string>
            {
                null,
                config.GetHl7Entry("tests^name", "column_name"),
                config.GetHl7Entry("tests^result", "column_name"),
                config.GetHl7Entry("tests^units",  "column_name"),
                config.GetHl7Entry("tests^range",  "column_name")
            };
            var printStatus = (config.GetEntry("PRINTSTATUS") ?? "").Equals("Y");
            if (printStatus)
                labels.Add(config.GetHl7Entry("tests^status", "column_name"));

            var _t = new Time(site.Id);
            var postResults = new List<OrderResult.ResultForChart>();

            foreach(var postLine in lineNums)
            {
                foreach(var result in results)
                {
                    var testType = (config.GetEntry("lab sources", result.Source) ?? "").Equals("LAB") ? "LAB" : "RAD";
                    var addPre = ((config.GetEntry("preserve fixed format") ?? "").Equals("1") && testType.Equals("RAD"));

                    if (result.FirstLineNum < postLine && result.LastLineNum < postLine)
                    {
                        continue;
                    }
                    else if (result.FirstLineNum > postLine)
                    {
                        break;
                    }

                    var i = 0;
                    var allContent = "";
                    var currentName = "";
                    var multi = false;
                    OrderResult.Component postComponent = null;
                    foreach (var component in result.Components)
                    {
                        if (component.Fields != null)
                        {
                            if (i == 0)
                            {
                                currentName = component.Fields.Name;
                                multi = (result.FirstLineNum != result.LastLineNum);
                            }
                            else if (!component.Fields.Name.Equals(currentName))
                            {
                                allContent = "";
                            }
                        }

                        if (component.LastLineNum == postLine)
                            postComponent = component.Clone();

                        if (component.Fields.Name.Equals(currentName))
                        {
                            allContent +=
                                (string.IsNullOrWhiteSpace(component.Fields.Result)) ? "" :
                                (!string.IsNullOrWhiteSpace(allContent)) ? "\\n" :
                                "";
                            allContent += component.Fields.Result;
                        }

                        i++;
                    }

                    var DTE = _t.LongDateTime(_t.DateTimeToString(result.SpecimenDate));
                    var deptName = config.GetEntry("dept names", result.Source);
                    var resultText = "";
                    if (postComponent != null && !multi)
                    {
                        resultText = postComponent.Fields.Result;
                    }
                    // Post a multiline entry (allContent)
                    else if (multi && !string.IsNullOrWhiteSpace(allContent))
                    {
                        if (addPre)
                            allContent = "<pre>" + allContent + "</pre>";
                        resultText = allContent;
                    }

                    if (postComponent != null)
                    {
                        if (postComponent.Fields == null)
                            postComponent.Fields = new OrderResult.Component.TestFields();

                        postResults.Add(new OrderResult.ResultForChart
                        {
                            ComponentName = postComponent.Fields.Name,
                            ResultText = resultText,
                            Units = postComponent.Fields.Units,
                            Range = postComponent.Fields.Range,
                            ParentName = result.Name,
                            LineCT = postLine,
                            IsAbnormal = postComponent.IsAbnormal,
                            DateString = DTE,
                            Comment = string.IsNullOrEmpty(postComponent.Fields.Comment) ? postComponent.Notes : postComponent.Fields.Comment,
                            TestType = testType,
                            DeptName = deptName,
                            OrderNumber = result.OrderNumber,
                            Status = postComponent.Fields.Status
                        });
                    }
                }
            }

            var oldTestName = "";
            var oldOrderNumber = "";
            var oldDate = "";
            var oldTestType = "";
            List<EMR.Line> lines = new List<EMR.Line>();
            var j = 0;
            var isLab = false;
            foreach (var result in postResults)
            {
                var OBRTestName = result.ParentName;
                var OrdNum = result.OrderNumber;
                isLab = (result.TestType.Equals("LAB"));
                j++;

                if (!(OBRTestName ?? "").Equals(oldTestName) || !(OrdNum ?? "").Equals(oldOrderNumber) || !(result.DateString ?? "").Equals(oldDate))
                {
                    if (j > 1 && oldTestType.Equals("LAB") && useTable)
                        lines.Last().DataSegments.Add(Results.GetTableStartDataSegment());

                    lines.Add(new EMR.Line
                    {
                        LineHeader = new EMR.Line.Header
                        {
                            sys_time = _t.Timestamp(),
                            user = user.Id
                        },
                        LinePart = new EMR.Line.Part
                        {
                            nct = EMR.Constants.NCT_RESULTS,
                            section = "RESULTS",
                            part = result.DeptName
                        },
                        DataSegments = new List<EMR.Line.DataSegment>()
                    });

                    if (isLab && useTable)
                    {
                        var label = labels[2] + spaces;
                        var headerSegment = new EMR.Line.DataSegment(
                            EMR.Line.DataSegment.Constants.TYPE_TABLE + EMR.Line.DataSegment.Constants.MODIFIER_TABLE_HEADER,
                            ""
                        );
                        headerSegment.KeepTrailingDelimiter = true;

                        lines.Last().DataSegments.AddRange(new List<EMR.Line.DataSegment> {
                            headerSegment,
                            Results.GetTableCellDataSegment(labels[1]),
                            Results.GetTableCellDataSegment(label),
                            Results.GetTableCellDataSegment(labels[3]),
                            Results.GetTableCellDataSegment(labels[4])
                        });
                        if (printStatus)
                            lines.Last().DataSegments.Add(Results.GetTableCellDataSegment(labels[5]));

                        lines.Last().DataSegments.AddRange(new List<EMR.Line.DataSegment> {
                            Results.GetTableStartDataSegment(),
                            Results.GetTableCellDataSegment(OBRTestName + " " + result.DateString)
                        });
                    }

                    lines.Last().DataSegments.Add(Results.GetTableText(OBRTestName + " " + result.DateString, true));
                }

                lines.Last().DataSegments.AddRange(Results.GetResultMarkup(result.ResultText, result.TestType, result.IsAbnormal, useTable, spaceLen, result.ComponentName, result.Units));

                if (isLab && useTable)
                    lines.Last().DataSegments.Add(Results.GetTableCellDataSegment(result.Range));

                if (!string.IsNullOrWhiteSpace(result.Range))
                    lines.Last().DataSegments.Add(Results.GetTableText(" Range (" + result.Range + ")"));

                if (printStatus)
                {
                    if (isLab && useTable)
                        lines.Last().DataSegments.Add(Results.GetTableCellDataSegment(result.Status));

                    if (!string.IsNullOrWhiteSpace(result.Status))
                        lines.Last().DataSegments.Add(Results.GetTableText(" Status (" + result.Status + ")"));
                }

                if (!string.IsNullOrWhiteSpace(result.Comment))
                {
                    var commentLen = 78;
                    if (isLab && useTable)
                    {
                        var commentList = result.Comment.Split(new string[] { "<LF>", "\n" }, StringSplitOptions.None);
                        foreach (var comment in commentList)
                        {
                            if (comment.Length > commentLen)
                            {
                                var tempLine = Results.WrapText(comment, commentLen);
                                var parts = tempLine.Split(new char[] { '~' });
                                foreach (var part in parts)
                                {
                                    lines.Last().DataSegments.AddRange(new List<EMR.Line.DataSegment> {
                                        Results.GetTableStartDataSegment(),
                                        Results.GetTableCellDataSegment(part),
                                        Results.GetTableText(part)
                                    });
                                }
                            }
                            else
                            {
                                lines.Last().DataSegments.AddRange(new List<EMR.Line.DataSegment>
                                {
                                    Results.GetTableStartDataSegment(),
                                    Results.GetTableCellDataSegment(comment),
                                    Results.GetTableText(comment)
                                });
                            }
                        }
                    }
                    else
                    {
                        lines.Last().DataSegments.Add(Results.GetTableText(result.Comment));
                    }
                }

                lines.Last().DataSegments.Add(new EMR.Line.DataSegment("M", "1-" + result.LineCT));

                oldTestName = result.ParentName;
                oldOrderNumber = result.OrderNumber;
                oldTestType = result.TestType;
                oldDate = result.DateString;

                Results.StoreResultsPostChart(siteId, patientId, result.LineCT, result.OrderNumber);
            }

            if (isLab && useTable)
                lines.Last().DataSegments.Add(Results.GetTableStartDataSegment());

            if (lines.Count > 0)
            {
                var emr = new EMR(siteId, patientId, true);
                if (emr != null && emr.WriteLines(lines.ToArray()))
                {
                    await MeaningfulUse.LogCreation(user, patientId, "RESULTS");
                    Chart.OnChartWrite(site, patientId, user.Id);
                    return true;
                }

                return false;
            }

            return false;
        }

        /// <summary>
        /// Get the results for a patient
        /// </summary>
        /// <param name="siteId">Site identifier</param>
        /// <param name="patientId">Patient identifier</param>
        /// <param name="user">User object</param>
        /// <param name="mostCurrentOnly">Y/N flag for whether this should only return the most recent results</param>
        /// <returns>List of Order objects with Results populated</returns>
        public async Task<List<OrderResult>> GetPatientResults(byte siteId, string patientId, User user, string mostCurrentOnly = "")
        {
            bool canPostToChart = true;
            if(!user.HasAtLeastReadPermission(Permission.ORDERS))
            {
                return null;
            } else if (!user.HasWritePermission(Permission.ORDERS))
            {
                canPostToChart = false;
            }

            var site = await _siteRepository.GetSiteByIdAsync(siteId);
            var config = new Results.Config(site);
            var enterRad = !(config.GetEntry("ENTERRAD") ?? "").Equals("N");

            bool filterToMostCurrent = false;
            if (!string.IsNullOrWhiteSpace(mostCurrentOnly))
            {
                filterToMostCurrent = (mostCurrentOnly.ToUpperInvariant().Equals("Y"));
            } else
            {
                filterToMostCurrent = (config.GetEntry("default checkbox") ?? "").Equals("Y");
            }

            // TODO: This doesn't support archived results.
            var patient = await _patientRepository.GetPatientByIdAsync(siteId, patientId, user);
            if (patient != null)
            {
                // Clear ord27 so results not viewed flag is cleared
                foreach (MinimalProvider p in patient.Providers)
                {
                    if (p.Role.Id.Equals(Domain.Constants.Id_Doctor))
                    {
                        if (p.User != null && p.User.Id == user.Id)
                        {
                            new DB.Update
                            {
                                Sql = "UPDATE pat SET ord27='' WHERE ibex=@ibex AND site=@site",
                                Parameters = new SqlParameter[]
                                {
                                    new SqlParameter("@ibex", SqlDbType.Char) { Value = patientId },
                                    new SqlParameter("@site", SqlDbType.TinyInt) { Value = siteId }
                                }
                            }.Run();
                        }
                        break;
                    }
                }
            }

            var postedResults = Results.GetPostedResults(siteId, patient);
            var results = ParseResults(site, patient);
            var abnormalCodes = Results.GetAbnormalCodes(siteId);
            var levels = new Results.Levels(abnormalCodes);
            var lateResults = await GetPatientLateResults(siteId, patient.Ibex, user);

            var finalResultList = new List<OrderResult>();
            var seenOrders = new HashSet<string>();
            var removeOld = (mostCurrentOnly != null && mostCurrentOnly.ToUpperInvariant().Equals("Y"));
            foreach(var result in results)
            {
                if (result.FirstLineNum == 0)
                    continue;

                // Skip over order numbers we have already seen, if requested.
                if (removeOld && !string.IsNullOrWhiteSpace(result.OrderNumber) && !seenOrders.Add(result.OrderNumber))
                    continue;

                var translatedSource = (config.GetEntry("lab sources", result.Source) ?? "").Equals("LAB") ? "LAB" : "RAD";
                var entireResultPosted = (postedResults.ContainsKey(result.LineNum) || postedResults.ContainsKey(result.LastLineNum) || !canPostToChart);
                foreach (var comp in result.Components)
                {
                    comp.CanPostToChart = ((!postedResults.ContainsKey(comp.LineNum)) && canPostToChart && !entireResultPosted);
                    if (translatedSource.Equals("RAD")) {
                        if (!enterRad) {
                            comp.CanPostToChart = false;
                        }

                        // TODO: MOB-131
                        // RAD results stink, and numbers on the front-end and back-end aren't matching, but should
                        if (postedResults.ContainsKey(comp.FirstLineNum))
                            comp.CanPostToChart = false;
                    }
                    var level = levels.GetLevel(comp.Fields);
                    if (level != null)
                    {
                        if (level == Results.Levels.Constants.ABNORMAL_RESULT_FLAG || level == Results.Levels.Constants.LOW_RESULT_FLAG || level == Results.Levels.Constants.HIGH_RESULT_FLAG)
                        {
                            comp.IsAbnormal = true;
                            if (level != Results.Levels.Constants.ABNORMAL_RESULT_FLAG)
                                comp.AbnormalType = level == Results.Levels.Constants.HIGH_RESULT_FLAG ? "HIGH" : "LOW";
                        }
                        else
                        {
                            comp.IsCritical = true;
                            comp.CriticalType = (level.IndexOf("high") > 0) ? "HIGH" : "LOW";
                        }
                        comp.LevelClass = levels.GetStyleClass(level);
                    }
                }

                finalResultList.Add(result);
            }

            return finalResultList;
        }

        /// <summary>
        /// Post a comment to a patient
        /// </summary>
        /// <param name="siteId">Site identifier</param>
        /// <param name="patientId">Patient identifier</param>
        /// <param name="userId">User identifier</param>
        /// <param name="commentId">Structured comment identifier</param>
        /// <param name="commentName">Freetext comment name</param>
        /// <param name="removeComment">Boolean flag for whether the comment should be removed from the MTB</param>
        /// <returns>Integer for number of comments affected</returns>
        public int PostComment(byte siteId, string patientId, int userId, Int32? commentId, string commentName = null, bool removeComment = false)
        {
            var result = _patientRepository.PostCommentByIdAsync(siteId, patientId, userId, commentId, commentName, removeComment);
            return result;
        }

        /// <summary>
        /// Sign a patient chart
        /// </summary>
        /// <param name="siteId">Site identifier</param>
        /// <param name="patientId">Patient identifier</param>
        /// <param name="user">User object</param>
        /// <returns></returns>
        public async Task<string> SignChart(byte siteId, string patientId, User user)
        {
            var result = await _patientRepository.SignChart(siteId, patientId, user);
            return result;
        }

        /// <summary>
        /// Parse results stored in the database
        /// </summary>
        /// <param name="siteId">Site identifier</param>
        /// <param name="patient">Patient object</param>
        /// <returns>List of OrderResult objects</returns>
        private List<OrderResult> ParseResults(Site site, Patient patient)
        {
            var results = new List<OrderResult>();
            var resultData = new OrderResult();
            var tempComp = new OrderResult.Component();

            var siteId = site.Id;

            if (patient != null && !string.IsNullOrWhiteSpace(patient.AcctNum)) {
                var config = new Results.Config(site);
                var resultsData = new DB.Select
                {
                    Sql = "SELECT * FROM ord_results WHERE account_number=@acctnum AND site=@site ORDER BY line_num",
                    Parameters = new SqlParameter[]
                    {
                        new SqlParameter("@acctnum", SqlDbType.VarChar) { Value = patient.AcctNum },
                        new SqlParameter("@site", SqlDbType.TinyInt) { Value = siteId }
                    }
                }.RunForListOfDictionaries();

                var testFields = new List<string>();
                int lineNum = 0;
                string messageType = "";
                foreach (var result in resultsData)
                {
                    lineNum = Convert.ToInt32(result["line_num"]);
                    var segment = result["segment"];

                    // Start of a message
                    if (segment.Equals("MSH"))
                    {
                        tempComp = null;
                        testFields = new List<string>();
                        testFields.Add("line_num");
                        testFields.AddRange(result.Keys.Where(x => x.StartsWith("tests_")).ToList());

                        if (results.Count > 0 || lineNum > 1)
                        {
                            if (resultData.Components != null && resultData.Components.Count > 0)
                            {
                                resultData.LastLineNum = resultData.Components.Last().LineNum;
                                if (!string.IsNullOrEmpty(resultData.Components[0].Fields.Text))
                                {
                                    resultData.Components[0].Fields.Result = resultData.Components[0].Fields.Text;
                                }
                                else
                                {
                                    resultData.AddText(resultData.Components[0].Notes + resultData.LineFeed);
                                    resultData.Components[0].Fields.Result = resultData.Text;
                                }
                            }

                            resultData.FirstLineNum = resultData.LineNum;
                            if (!string.IsNullOrWhiteSpace(messageType) && messageType.Equals(OrderResult.Constants.ORDER_RESULT_MESSAGE))
                                results.Add(resultData);
                        }

                        resultData = new OrderResult
                        {
                            Source = result["source"],
                            LineFeed = result["line_feed"],
                            Components = new List<OrderResult.Component>()
                        };

                        messageType = result["message_type"];
                    }

                    // Component results
                    else if (segment.Equals("OBX"))
                    {
                        var sourceToUse = !string.IsNullOrWhiteSpace(result["source"]) ? result["source"] : resultData.Source;
                        var translatedSource = (config.GetEntry("lab sources", sourceToUse) ?? "").Equals("LAB") ? "LAB" : "RAD";
                        // Radiology results have text that spans multiple segments, and that's all we need.
                        if (translatedSource.Equals("RAD"))
                        {
                            resultData.AddText(result["tests_text"] + result["line_feed"]);
                            resultData.LastLineNum = lineNum;
                        }

                        // Other results have differing values across multiple segments
                        else
                        {
                            var newComp = (tempComp != null) ? tempComp.Clone() : new OrderResult.Component();
                            // TODO: One of these has got to be wrong.
                            newComp.FirstLineNum = lineNum;
                            newComp.LastLineNum = lineNum;
                            newComp.LineNum = lineNum;
                            if (newComp.Fields == null)
                            {
                                newComp.Fields = new OrderResult.Component.TestFields();
                            }
                            foreach (var k in testFields)
                            {
                                if (result.ContainsKey(k))
                                {
                                    newComp.Fields.set(k, result[k]);
                                }
                            }
                            if (resultData.Components == null)
                            {
                                resultData.Components = new List<OrderResult.Component>();
                            }
                            resultData.Components.Add(newComp);
                            tempComp = new OrderResult.Component();
                        }
                    }

                    // Notes/Comments
                    else if (segment.Equals("NTE"))
                    {
                        if (resultData.Components == null)
                        {
                            resultData.Components = new List<OrderResult.Component>();
                        }
                        if (resultData.Components.Count == 0)
                        {
                            resultData.Components.Add(new OrderResult.Component
                            {
                                LineNum = lineNum,
                                // Look, RAD results are dumb and there is so much hacking that has to go on.
                                // We need to set the line number to match the main result's line numbers, but the main result starts one before the actual data
                                // Yes, this is annoying and dumb.
                                FirstLineNum = resultData.FirstLineNum + 1,
                                LastLineNum = lineNum
                            });
                        }
                        var last = resultData.Components.Last();
                        last.LineNum = lineNum;
                        last.LastLineNum = lineNum;
                        last.AddNotes(result["tests_comment"] + result["line_feed"]);
                    }

                    // Everything else
                    else
                    {
                        if (segment.Equals("OBR"))
                        {
                            var rClone = resultData.Clone();
                            if (resultData.Components != null && resultData.Components.Count > 0)
                            {
                                rClone.LastLineNum = rClone.Components.Last().LineNum;
                                rClone.FirstLineNum = rClone.LineNum;

                                if (!string.IsNullOrWhiteSpace(messageType) && messageType.Equals(OrderResult.Constants.ORDER_RESULT_MESSAGE))
                                    results.Add(rClone);

                                resultData.Components.Clear();
                            }
                            resultData = new OrderResult
                            {
                                Source = rClone.Source,
                                LineFeed = rClone.LineFeed,
                                FirstLineNum = lineNum,
                                Status = rClone.Status,
                            };
                            tempComp = new OrderResult.Component();
                        }
                        if (OrderResult.Constants.AssignmentFields.ContainsKey(segment))
                        {
                            foreach (var field in OrderResult.Constants.AssignmentFields[segment])
                            {
                                if (!result.ContainsKey(field) || string.IsNullOrWhiteSpace(result[field]))
                                {
                                    continue;
                                }
                                if (segment.Equals("OBR"))
                                {
                                    resultData.set(field, result[field]);
                                }
                                else
                                {
                                    if (field.Equals("status"))
                                        resultData.set(field, result[field]);

                                    if (resultData.Components == null)
                                    {
                                        resultData.Components = new List<OrderResult.Component>();
                                    }

                                    if (tempComp == null)
                                    {
                                        tempComp = new OrderResult.Component();
                                    }
                                    tempComp.set(field, result[field]);
                                }
                            }
                        }
                    }
                }
                if (lineNum > 0 && !string.IsNullOrWhiteSpace(resultData.Source))
                {
                    if (resultData.Components != null && resultData.Components.Count > 0)
                        resultData.LastLineNum = resultData.Components.Last().LineNum;

                    resultData.FirstLineNum = resultData.LineNum;

                    // Don't add status updates to the list of results
                    if (!string.IsNullOrWhiteSpace(messageType) && messageType.Equals(OrderResult.Constants.ORDER_RESULT_MESSAGE))
                        results.Add(resultData.Clone());
                }

                // Some results (like Radiology) may not really match up the results to the component, so
                // handle that situation here
                if (resultData.Components.Count == 1 && string.IsNullOrEmpty(resultData.Components[0].Text))
                {
                    if (!string.IsNullOrEmpty(resultData.Components[0].Fields.Text))
                    {
                        resultData.Components[0].Fields.Result = resultData.Components[0].Fields.Text;
                    }
                    else
                    {
                        resultData.AddText(resultData.Components[0].Notes + resultData.LineFeed);
                        resultData.Components[0].Fields.Result = resultData.Text;
                    }
                }






                // TODO: This is put in to match a bug on the front-end.  Both should be fixed at the same time to use the FirstLineNum when posting.
                // MOB-131
                resultData.Components.ForEach(c => c.LastLineNum = c.FirstLineNum);
            }

            return results;
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

            var res = new DB.Select
            {
                Sql = "SELECT field_name, field_val, field_num FROM site_preferences WHERE site=@site AND field_num IN(1,2) ORDER BY field_num, field_seq",
                Parameters = new SqlParameter[]
                {
                    new SqlParameter("@site", SqlDbType.TinyInt) { Value = site.Id }
                }
            }.RunForListOfDictionaries();

            foreach(var r in res)
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

            var patAge = patient.Demographics.Age.Value;
            var patAgeUnit = patient.Demographics.Age.Unit;

            // The age is compared in years, so if a patient is days or weeks old, they're less than 1.
            if (patAgeUnit == AgeUnit.Day || patAgeUnit == AgeUnit.Week)
            {
                patAge = 0;
            } else if (patAgeUnit == AgeUnit.Month)
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

                if ((opt.Equals(Trigger.Constants.TRIGGER_MED_SVC_BOTH) || opt.Equals(triggerId)) && ((patAge == 0 && patAgeUnit == AgeUnit.Day) || (minAge <= patAge && maxAge >= patAge)))
                {
                    var frmCSSite = MedicationManager.GetFormularyShareSite(site.Id);
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
            } else if ((!triggerMach.Equals("D") && (triggerIn.Equals("A") || triggerOut.Equals("A"))) || med.IsCombo())
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
            } else
            {
                opt = MedicationManager.Constants.OPTS_MAPPING.ContainsKey(action) ? MedicationManager.Constants.OPTS_MAPPING[action] : "";
            }

            return (!string.IsNullOrWhiteSpace(opt) && triggerSettings.ContainsKey(opt) && triggerSettings[opt].Equals("Y"));
        }

        private string CreateDBTrigger(ISite site, string patientId, int userId, string triggerId, Medication med, Formulary formulary)
        {
            var xml = new StringBuilder("<medication_services>")
                .Append(med.GetXML(formulary).ToString())
                .Append("</medication_services>");

            var interfaceName = triggerId.Equals(Trigger.Constants.TRIGGER_MED_SVC_IMAGE) ? Trigger.Constants.MEDICATION_SERVICE_IMAGE : Trigger.Constants.MEDICATION_SERVICE_HL7;

            var error = Trigger.Create(site, patientId, userId, xml.ToString(), interfaceName, "4q");

            if (error != null)
            {
                DTFL.Write(site.Id, userId, "Cannot create ensemble trigger for Medication Services interface. IBEX: " + patientId + " ERROR: " + error);
            }

            return error;
        }
    }
}
