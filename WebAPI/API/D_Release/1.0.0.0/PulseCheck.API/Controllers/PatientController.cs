using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;
using DomainModel;
using Interfaces.Services;
using PulseCheck.API.Models;
using System.Data;
using System.Data.SqlClient;
using PulseCheck.Utilities;
using System.IO;
using System.Text.RegularExpressions;
using System.Linq;

namespace PulseCheck.API.Controllers
{
    /// <summary>
    /// Patient controller for PulseCheck API
    /// </summary>
    public class PatientController : ApiController
    {
        private readonly IPatientService _patientService;
        private readonly ISiteService _siteService;
        private readonly IUserService _userService;
        private readonly IMedicationService _medicationService;
        private readonly Authentication _authUtil = new Authentication();

        /// <summary>
        /// PatientController constructor - empty
        /// </summary>
        public PatientController()
        {

        }

        /// <summary>
        /// PatientController constructor
        /// </summary>
        /// <param name="patientService">Patient service</param>
        /// <param name="siteService">Site service</param>
        /// <param name="userService">User service</param>
        /// <param name="medicationService">Medication service</param>
        public PatientController(IPatientService patientService, ISiteService siteService, IUserService userService, IMedicationService medicationService)
        {
            _patientService = patientService;
            _siteService = siteService;
            _userService = userService;
            _medicationService = medicationService;
        }

        /// <summary>
        /// Get patient information
        /// </summary>
        /// <remarks>Use the expand parameter to include additional information that is omitted by default</remarks>
        /// <returns></returns>
        /// <response code="200"></response>
        [VersionedRoute("api/patient/{patientId}", 1)]
        [Route("api/v1/patient/{patientId}")]
        [HttpGet]
        public async Task<IHttpActionResult> GetPatientV1(string patientId, string expand = "")
        {
            int userId = _authUtil.GetAuthenticatedUserId(User);
            User user = await _userService.GetUserByIdAsync(userId);
            var result = await _patientService.GetPatientByIdAsync(user.SiteId, patientId, user, expand);
            if (result == null)
            {
                return new WLRResponse(ErrorCodes.NOT_AUTHORIZED, Request);
            }

            var pat = DynamicExtensions.ToDynamic(result);
            pat.expand = DomainModel.Constants.Expando.GetPatientOptions();
            return Ok(pat);
        }

        /// <summary>
        /// Get patient allergy information
        /// </summary>
        /// <remarks></remarks>
        /// <returns>List of Allergy objects</returns>
        /// <response code="200"></response>
        [VersionedRoute("api/patient/{patientId}/allergies", 1)]
        [Route("api/v1/patient/{patientId}/allergies")]
        [HttpGet]
        public async Task<IHttpActionResult> GetPatientAllergiesV1(string patientId)
        {
            int userId = _authUtil.GetAuthenticatedUserId(User);
            User user = await _userService.GetUserByIdAsync(userId);
            await MeaningfulUse.LogAccess(user, patientId, "ALLERGIES");
            var result = await _patientService.GetPatientAllergies(user.SiteId, patientId, user);
            if (result == null)
            {
                return new WLRResponse(ErrorCodes.NOT_AUTHORIZED, Request);
            }
            return Ok(result);
        }

        /// <summary>
        /// Get patient current medication information
        /// </summary>
        /// <remarks></remarks>
        /// <returns>List of CurrentMedication objects</returns>
        /// <response code="200"></response>
        [VersionedRoute("api/patient/{patientId}/currentmeds", 1)]
        [Route("api/v1/patient/{patientId}/currentmeds")]
        [HttpGet]
        public async Task<IHttpActionResult> GetPatientCurrentMedicationsV1(string patientId)
        {
            int userId = _authUtil.GetAuthenticatedUserId(User);
            User user = await _userService.GetUserByIdAsync(userId);
            await MeaningfulUse.LogAccess(user, patientId, "CURRENT MEDICATIONS");
            var result = await _patientService.GetPatientCurrentMedications(user.SiteId, patientId, user);
            if (result == null)
            {
                return new WLRResponse(ErrorCodes.NOT_AUTHORIZED, Request);
            }
            return Ok(result);
        }

        /// <summary>
        /// Search for a medication
        /// </summary>
        /// <returns>A list of medications</returns>
        /// <response code="200"></response>
        [VersionedRoute("api/patient/{patientId}/orders/search", 1)]
        [Route("api/v1/patient/{patientId}/orders/search")]
        [HttpGet]
        public async Task<List<DomainModel.Group>> SearchOrdersV1(string patientId, [FromUri]string name)
        {
            var userId = _authUtil.GetAuthenticatedUserId(User);
            var user = await _userService.GetUserByIdAsync(userId);
            if (user.HasAtLeastReadPermission(Permission.ORDERS))
            {
                var services = await _siteService.SearchOrdersBySiteIdAsync(user.SiteId, name, userId);
                var groupedServices = await _patientService.CreateOrderServices(user, patientId, services);
                return groupedServices;
            }

            return null;
        }

        /// <summary>
        /// Send a list of non-medication orders for the patient
        /// </summary>
        /// <remarks></remarks>
        /// <returns>A string of error messages</returns>
        /// <response code="200"></response>
        [VersionedRoute("api/patient/{patientId}/orders", 1)]
        [Route("api/v1/patient/{patientId}/orders")]
        [HttpPost]
        public async Task<IHttpActionResult> PostPatientOrdersV1(string patientId, [FromBody]List<Order> orders)
        {
            int userId = _authUtil.GetAuthenticatedUserId(User);
            User user = await _userService.GetUserByIdAsync(userId);
            
            var result = await _patientService.PlaceOrder(user.SiteId, patientId, user, orders);
            if (result == null)
                return new WLRResponse(ErrorCodes.NOT_AUTHORIZED, Request);

            await MeaningfulUse.LogCreation(user, patientId, "ORDER");
            return Ok(result);
        }

        /// <summary>
        /// Send a list of non-medication orders for the patient
        /// </summary>
        /// <remarks></remarks>
        /// <returns>A string of error messages</returns>
        /// <response code="200"></response>
        [VersionedRoute("api/patient/{patientId}/orders/services/{serviceCode}/queries", 1)]
        [Route("api/v1/patient/{patientId}/orders/services/{serviceCode}/queries")]
        [HttpGet]
        public async Task<IHttpActionResult> GetServiceQueriesV1(string patientId, string serviceCode)
        {
            int userId = _authUtil.GetAuthenticatedUserId(User);
            User user = await _userService.GetUserByIdAsync(userId);

            var result = await _patientService.GetServiceQueries(user, patientId, serviceCode);
            if (result == null)
            {
                return new WLRResponse(ErrorCodes.NOT_AUTHORIZED, Request);
            }
            return Ok(result);
        }

        /// <summary>
        /// Get a list of non-medication orders for the patient
        /// </summary>
        /// <remarks></remarks>
        /// <returns>A string of error messages</returns>
        /// <response code="200"></response>
        [VersionedRoute("api/patient/{patientId}/orders", 1)]
        [Route("api/v1/patient/{patientId}/orders")]
        [HttpGet]
        public async Task<IHttpActionResult> GetPatientOrdersV1(string patientId, [FromUri]bool includeQueries=false)
        {
            int userId = _authUtil.GetAuthenticatedUserId(User);
            User user = await _userService.GetUserByIdAsync(userId);
            if (user.CanNavigateTo(Navigation.Constants.ORDERS))
            {
                var result = await _patientService.GetPatientOrders(user.SiteId, patientId, includeQueries);
                return Ok(result);
            }

            return new WLRResponse(ErrorCodes.NOT_AUTHORIZED, Request);
        }

        /// <summary>
        /// Get most used medications for the current user
        /// </summary>
        /// <returns>Most used meds</returns>
        [VersionedRoute("api/patient/{patientId}/meds/mostused", 1)]
        [Route("api/v1/patient/{patientId}/meds/mostused")]
        [HttpGet]
        public async Task<IHttpActionResult> GetMostUsedMedsV1(string patientId)
        {
            var currentUserId = _authUtil.GetAuthenticatedUserId(User);
            var user = await _userService.GetUserByIdAsync(currentUserId);
            if (user.HasAtLeastReadPermission(Permission.MED_SVC))
            {
                var site = await _siteService.GetSiteByIdAsync(user.SiteId);
                var qlData = await _medicationService.GetMedMostUsedList(user, site, patientId);
                return Ok(qlData);
            }

            return new WLRResponse(ErrorCodes.NOT_AUTHORIZED, Request);
        }

        /// <summary>
        /// Get quick list medications for the current user
        /// </summary>
        /// <returns></returns>
        [VersionedRoute("api/patient/{patientId}/meds/quicklist", 1)]
        [Route("api/v1/patient/{patientId}/meds/quicklist")]
        [HttpGet]
        public async Task<IHttpActionResult> GetQuickListMedsV1(string patientId)
        {
            var currentUserId = _authUtil.GetAuthenticatedUserId(User);
            var user = await _userService.GetUserByIdAsync(currentUserId);
            if (user.HasAtLeastReadPermission(Permission.MED_SVC))
            {
                var site = await _siteService.GetSiteByIdAsync(user.SiteId);
                var qlData = await _medicationService.GetMedQuickList(user, site, patientId);
                return Ok(qlData);
            }

            return new WLRResponse(ErrorCodes.NOT_AUTHORIZED, Request);
        }

        /// <summary>
        /// Get quick list medications for the current user
        /// </summary>
        /// <returns></returns>
        [VersionedRoute("api/patient/{patientId}/meds/brands/{*brand}", 1)]
        [Route("api/v1/patient/{patientId}/meds/brands/{*brand}")]
        [HttpGet, System.Web.Mvc.ValidateInput(false)]
        public async Task<IHttpActionResult> GetBrandMedsV1(string patientId, string brand)
        {
            var currentUserId = _authUtil.GetAuthenticatedUserId(User);
            var user = await _userService.GetUserByIdAsync(currentUserId);
            if (user.HasAtLeastReadPermission(Permission.MED_SVC))
            {
                var site = await _siteService.GetSiteByIdAsync(user.SiteId);
                var qlData = await _medicationService.GetBrandMeds(user, site, patientId, brand);
                return Ok(qlData);
            }

            return new WLRResponse(ErrorCodes.NOT_AUTHORIZED, Request);
        }

        /// <summary>
        /// Sign the patient's chart
        /// </summary>
        /// <param name="signInfo">PatientSign object</param>
        /// <returns></returns>
        /// <response code="200"></response>
        [VersionedRoute("api/patient/sign", 1)]
        [Route("api/v1/patient/sign")]
        [HttpPost]
        public async Task<IHttpActionResult> SignChartV1([FromBody]PatientSign signInfo)
        {
            int userId = _authUtil.GetAuthenticatedUserId(User);
            User user = await _userService.GetUserByIdAsync(userId);
            if (!user.CanNavigateTo(Navigation.Constants.SIGN_CHART))
                return new WLRResponse(ErrorCodes.NOT_AUTHORIZED, Request);

            var result = await _patientService.SignChart(user.SiteId, signInfo.Ibex, user);
            if (result != null)
                return new ErrorResponse(result, 500, Request);

            return Ok();
        }

        /// <summary>
        /// Get medication group for a patient
        /// </summary>
        /// <remarks></remarks>
        /// <param name="patientId">Patient identifier</param>
        /// <param name="groupNum">Group identifier/number</param>
        /// <returns>List of Medication objects in the group</returns>
        /// <response code="200"></response>
        [VersionedRoute("api/patient/{patientId}/orders/group/{groupNum}", 1)]
        [Route("api/v1/patient/{patientId}/orders/group/{groupNum}")]
        [HttpGet]
        public async Task<IHttpActionResult> GetPatientMedicationGroupV1(string patientId, int groupNum)
        {
            int userId = _authUtil.GetAuthenticatedUserId(User);
            User user = await _userService.GetUserByIdAsync(userId);
            if (user.HasAtLeastReadPermission(Permission.MED_SVC))
            {
                var site = await _siteService.GetSiteByIdAsync(user.SiteId);
                var groups = await _siteService.GetMedPathwaysBySiteIdAsync(site.Id).ConfigureAwait(false);
                var group = groups.Find(o => o.Num == groupNum);
                if (group == null)
                {
                    return new WLRResponse(ErrorCodes.PARAMETER_FAULT, "Medication group not found", Request);
                }
                var result = await _medicationService.GetMedicationGroup(user, site, group, patientId);
                if (result == null)
                {
                    return new WLRResponse(ErrorCodes.NOT_AUTHORIZED, Request);
                }
                return Ok(result);
            }
            return new WLRResponse(ErrorCodes.NOT_AUTHORIZED, Request);
        }

        /// <summary>
        /// Acknowledge a medication order
        /// </summary>
        /// <param name="patientId">Patient identifier</param>
        /// <param name="MedOrder">PatientMedOrder object</param>
        /// <returns></returns>
        [VersionedRoute("api/patient/{patientId}/order/acknowledge", 1)]
        [Route("api/v1/patient/{patientId}/order/acknowledge")]
        [HttpPost]
        public async Task<IHttpActionResult> OrderAcknowledgeV1(string patientId, [FromBody]PatientMedOrder MedOrder)
        {
            int userId = _authUtil.GetAuthenticatedUserId(User);
            User user = await _userService.GetUserByIdAsync(userId);
            var result = await _patientService.AcknowledgeMedOrder(user.SiteId, patientId, user, MedOrder.Id);
            if (result == null)
            {
                return new WLRResponse(ErrorCodes.NOT_AUTHORIZED, Request);
            }
            else if (string.IsNullOrWhiteSpace(result)) {
                var site = await _siteService.GetSiteByIdAsync(user.SiteId);
                var updatedMed = await _patientService.GetMedOrder(site, patientId, user, MedOrder.Id);
                return Ok(updatedMed);
            }
            else
            {
                return BadRequest(result);
            }
        }

        /// <summary>
        /// Record an authentication failure 
        /// </summary>
        /// <param name="patientId">Patient identifier</param>
        /// <param name="authFailure">MedAuthFailure object</param>
        /// <returns></returns>
        [VersionedRoute("api/patient/{patientId}/order/authfailure", 1)]
        [Route("api/v1/patient/{patientId}/order/authfailure")]
        [HttpPost]
        public async Task<IHttpActionResult> PostAuthFailureV1(string patientId, [FromBody]MedAuthFailure authFailure)
        {
            int userId = _authUtil.GetAuthenticatedUserId(User);
            User user = await _userService.GetUserByIdAsync(userId);

            if (!user.HasWritePermission(Permission.MED_SVC))
            {
                return new WLRResponse(ErrorCodes.NOT_AUTHORIZED, Request);
            }

            var medObj = await _medicationService.GetMedicationByLosecsAsync(user.SiteId, patientId, authFailure.Losecs);
            if (medObj != null)
            {
                Events.AuthenticationFailure(user.SiteId, user.Id, patientId, authFailure.Losecs, authFailure.Action, Events.ID_MEDSVC, "");
                var site = await _siteService.GetSiteByIdAsync(user.SiteId);
                var internalMail = new PulseMail(site);
                if (internalMail != null)
                {
                    var patient = await _patientService.GetPatientByIdAsync(user.SiteId, patientId, user);
                    var medActions = new MedicationActions(site);
                    var details = medActions.GetActionByName(authFailure.Action);
                    if (!details.ContainsKey("description"))
                    {
                        details["description"] = "action";
                    }

                    var patientName = patient.GetName();
                    var medName = medObj.GetName();
                    var subject = "Medication Service Failed Authentication";
                    var message = "Since you were unable to authenticate the " + details["description"] + " of " + medName + " on " + patientName + ", follow hospital policy to manually sign.";
                    internalMail.SendMessage(user.Id, subject, message, 0);
                }
            }

            return Ok();
        }

        /// <summary>
        /// Get a single medication order for the patient
        /// </summary>
        /// <param name="patientId">Patient identifier</param>
        /// <param name="orderId">Medication order identifier</param>
        /// <returns>Medication object for patient med order</returns>
        [VersionedRoute("api/patient/{patientId}/order/meds/{orderId}", 1)]
        [Route("api/v1/patient/{patientId}/order/meds/{orderId}")]
        [HttpGet]
        public async Task<IHttpActionResult> GetMedOrderV1(string patientId, int orderId)
        {
            int userId = _authUtil.GetAuthenticatedUserId(User);
            User user = await _userService.GetUserByIdAsync(userId);
            Site site = await _siteService.GetSiteByIdAsync(user.SiteId);
            var result = await _patientService.GetMedOrder(site, patientId, user, orderId);
            if (result == null)
            {
                return new WLRResponse(ErrorCodes.NOT_AUTHORIZED, Request);
            }

            return Ok(result);
        }

        /// <summary>
        /// Get the list of medication orders for the patient
        /// </summary>
        /// <param name="patientId">Patient identifier</param>
        /// <returns>List of Medication objects for patient med orders</returns>
        [VersionedRoute("api/patient/{patientId}/order/meds", 1)]
        [Route("api/v1/patient/{patientId}/order/meds")]
        [HttpGet]
        public async Task<IHttpActionResult> GetMedOrdersV1(string patientId)
        {
            int userId = _authUtil.GetAuthenticatedUserId(User);
            User user = await _userService.GetUserByIdAsync(userId);
            Site site = await _siteService.GetSiteByIdAsync(user.SiteId);
            var result = await _patientService.GetMedOrders(site, patientId, user);
            if (result == null)
                return new WLRResponse(ErrorCodes.NOT_AUTHORIZED, Request);

            await MeaningfulUse.LogAccess(user, patientId, "MEDICATION SERVICE");
            return Ok(result);
        }

        /// <summary>
        /// Order one or more medications for a patient
        /// </summary>
        /// <param name="patientId">Patient identifier</param>
        /// <param name="order">MedSvcOrder object</param>
        /// <returns></returns>
        [VersionedRoute("api/patient/{patientId}/order/meds", 1)]
        [Route("api/v1/patient/{patientId}/order/meds")]
        [HttpPost]
        public async Task<IHttpActionResult> PostMedOrdersV1(string patientId, [FromBody]MedSvcOrder order)
        {
            int userId = _authUtil.GetAuthenticatedUserId(User);
            User user = await _userService.GetUserByIdAsync(userId);
            Site site = await _siteService.GetSiteByIdAsync(user.SiteId);
            var result = await _patientService.PostMedOrders(site, patientId, user, order.Type, order.OrderingPhysician, order.Notes, order.ServiceOptions, order.AuthType, order.MedOrders);
            if (result == null)
            {
                return new WLRResponse(ErrorCodes.NOT_AUTHORIZED, Request);
            }
            else if (string.IsNullOrWhiteSpace(result))
            {
                return Ok();
            }
            else
            {
                if (result.Length == 1)
                {
                    return new WLRResponse(result.ToCharArray()[0], Request);
                }
                return BadRequest(result);
            }
        }

        /// <summary>
        /// Get a list of late order results for a particular patient
        /// </summary>
        /// <param name="patientId">Patient identifier</param>
        /// <returns>List of order information</returns>
        [VersionedRoute("api/patient/{patientId}/lateresults", 1)]
        [Route("api/v1/patient/{patientId}/lateresults")]
        [HttpGet]
        public async Task<IHttpActionResult> GetPatientLateResultsV1(string patientId)
        {
            int userId = _authUtil.GetAuthenticatedUserId(User);
            User user = await _userService.GetUserByIdAsync(userId);
            var result = await _patientService.GetPatientLateResults(user.SiteId, patientId, user);
            if (result == null)
            {
                return new WLRResponse(ErrorCodes.NOT_AUTHORIZED, Request);
            }
            return Ok(result);
        }

        /// <summary>
        /// Get the results for a particular patient
        /// </summary>
        /// <param name="patientId">Patient identifier</param>
        /// <param name="mostCurrentOnly">Optional Y/N flag to set view to "Most Current" or "All". If not provided, defaults to user's current stored setting</param>
        /// <returns>List of results</returns>
        [VersionedRoute("api/patient/{patientId}/results", 1)]
        [Route("api/v1/patient/{patientId}/results")]
        [HttpGet]
        public async Task<IHttpActionResult> GetPatientResultsV1(string patientId, string mostCurrentOnly = "")
        {
            int userId = _authUtil.GetAuthenticatedUserId(User);
            User user = await _userService.GetUserByIdAsync(userId);
            var result = await _patientService.GetPatientResults(user.SiteId, patientId, user, mostCurrentOnly);
            if (result == null)
            {
                return new WLRResponse(ErrorCodes.NOT_AUTHORIZED, Request);
            }

            // Write a "Results viewed" entry to the chart if we ended up with any results to return
            if (result.Count > 0)
            {
                Results.AuditView(user.SiteId, patientId, user.Id);
            }

            return Ok(result);
        }

        /// <summary>
        /// Post patient results to the chart
        /// </summary>
        /// <param name="patientId">Patient identifier</param>
        /// <param name="resultLines">Line numbers of results to post</param>
        /// <returns>Success/failure response code</returns>
        [VersionedRoute("api/patient/{patientId}/results", 1)]
        [Route("api/v1/patient/{patientId}/results")]
        [HttpPost]
        public async Task<IHttpActionResult> PostPatientResultsV1(string patientId, [FromBody]List<int> resultLines) {
            int userId = _authUtil.GetAuthenticatedUserId(User);
            User user = await _userService.GetUserByIdAsync(userId);
            var result = await _patientService.PostPatientResults(user.SiteId, patientId, user, resultLines);
            return Ok(result);
        }

        /// <summary>
        /// Get order pathway information for a patient
        /// </summary>
        /// <param name="patientId">Patient identifier</param>
        /// <param name="pathwayNum">Pathway identifier</param>
        /// <returns>Pathway information containing groups within the pathway and queries within the groups</returns>
        [VersionedRoute("api/patient/{patientId}/pathway/{pathwayNum}", 1)]
        [Route("api/v1/patient/{patientId}/pathway/{pathwayNum}")]
        [HttpGet]
        public async Task<IHttpActionResult> GetPatientPathwayV1(string patientId, int pathwayNum)
        {
            int userId = _authUtil.GetAuthenticatedUserId(User);
            User user = await _userService.GetUserByIdAsync(userId);
            if (!user.CanNavigateTo(Navigation.Constants.ORDERS))
            {
                return new WLRResponse(ErrorCodes.NOT_AUTHORIZED, Request);
            }

            var result = await _patientService.GetPatientPathway(user.SiteId, patientId, pathwayNum, user);

            if (result == null)
            {
                return new WLRResponse(ErrorCodes.UNEXPECTED_ERROR_CONDITION, Request);
            }

            return Ok(result);
        }

        /// <summary>
        /// Assign a patient to a particular area
        /// </summary>
        /// <remarks></remarks>
        /// <returns></returns>
        /// <response code="200"></response>
        [VersionedRoute("api/patient/area", 1)]
        [Route("api/v1/patient/area")]
        [HttpPost]
        public async Task<IHttpActionResult> PostPatientAreaAssignmentV1(PatientLocation loc)
        {
            var userId = _authUtil.GetAuthenticatedUserId(User);
            var user = await _userService.GetUserByIdAsync(userId);
            byte siteId = user.SiteId;
            var res = await PatientLocationAssignmentV1(loc, siteId);
            if (res == null)
            {
                var newPat = await _patientService.GetPatientByIdAsync(siteId, loc.Ibex, user);
                return Ok(newPat);
            }
            return res;
        }

        /// <summary>
        /// Assign a patient to a particular bed
        /// </summary>
        /// <remarks></remarks>
        /// <returns></returns>
        /// <response code="200"></response>
        [VersionedRoute("api/patient/bed", 1)]
        [Route("api/v1/patient/bed")]
        [HttpPost]
        public async Task<IHttpActionResult> PostPatientBedAssignmentV1(PatientLocation loc)
        {
            int userId = _authUtil.GetAuthenticatedUserId(User);
            User user = await _userService.GetUserByIdAsync(userId);
            byte siteId = user.SiteId;
            var res = await PatientLocationAssignmentV1(loc, siteId);
            if (res == null)
            {
                var newPat = await _patientService.GetPatientByIdAsync(siteId, loc.Ibex, user);
                return Ok(newPat);
            }
            return res;
        }

        // TODO: Move this logic out of the controller and in to the model.
        private async Task<IHttpActionResult> PatientLocationAssignmentV1(PatientLocation loc, byte siteId)
        {
            int userId = _authUtil.GetAuthenticatedUserId(User);
            User user = await _userService.GetUserByIdAsync(userId);
            if (!user.HasWritePermission(DomainModel.Permission.TRANSFER))
            {
                return new WLRResponse(ErrorCodes.NOT_AUTHORIZED, Request);
            }

            Time _t = new Time();
            string date = _t.TimestampNoSeconds();
            string dept = "", ward = "", bed = "";
            bool holdBed = false;
            bool returnToBed = false;
            bool shareBed = false;

            string[] locationInformation = loc.LocationId.Split('|');

            dept = locationInformation[0].Trim();

            // Return to bed, or Out of department (TODO: not implemented in V1).
            if (locationInformation.Length == 1)
            {
                var action = locationInformation[0].ToUpperInvariant();
                if (action.Equals("**RTB**"))
                {
                    returnToBed = true;
                } else
                {
                    throw new NotImplementedException("Transfer out of department is not implemented");
                }
            }

            // Area, with or without hold bed.
            else if (locationInformation.Length == 2)
            {
                string[] areaInformation = locationInformation[1].Split('^');
                ward = areaInformation[0].Trim();
                if (areaInformation.Length == 2 && areaInformation[1].ToUpperInvariant().Equals("HB"))
                {
                    holdBed = true;
                }
            }

            // Bed
            else if (locationInformation.Length >= 3)
            {
                ward = locationInformation[1].Trim();
                bed = locationInformation[2].Trim();

                // If sharing a bed there will be extra pieces of information in this string: monitor, name, commment, and URL.
                // As of 07/17/2017, it doesn't look like the Perl code actually uses that data for anything. Hooray!
                if (locationInformation.Length > 3)
                {
                    shareBed = true;
                }
            }

            var oldPatient = await _patientService.GetPatientByIdAsync(siteId, loc.Ibex, user);

            var newPatient = (Patient)oldPatient.Clone();
            newPatient.Department = dept;
            newPatient.Ward = ward;
            newPatient.Bed = bed;

            var connection = DB.GetConnectionString();
            using (var con = new SqlConnection(connection))
            {
                con.Open();
                var transaction = con.BeginTransaction();
                var deptName = "";
                var bedName = "";

                if (!returnToBed)
                {
                    var deptInfo = new DB.Select
                    {
                        Sql = "SELECT * FROM dept WHERE site=@site AND dept=@dept",
                        Connection = con,
                        Transaction = transaction,
                        Parameters = new SqlParameter[]
                        {
                        new SqlParameter("@site", SqlDbType.TinyInt) { Value = siteId },
                        new SqlParameter("@dept", SqlDbType.VarChar) { Value = dept }
                        }
                    }.RunForDataRow();

                    deptName = deptInfo["name"].ToString().Trim();
                    bedName = (deptName + " - " + ward + " " + bed).Trim();
                }

                if (holdBed)
                {
                    // Cannot transfer to hold area if already there.
                    var result = new DB.Select
                    {
                        Sql = "SELECT * FROM [api].[occupied_beds_vw] WHERE ibex=@ibex AND site=@site",
                        Connection = con,
                        Transaction = transaction,
                        Parameters = new SqlParameter[]
                        {
                            new SqlParameter("@ibex", SqlDbType.Char, 14) { Value = loc.Ibex },
                            new SqlParameter("@site", SqlDbType.TinyInt) { Value = siteId }
                        }
                    }.RunForDataSet();

                    if (result != null && result.Tables.Count > 0)
                    {
                        foreach (DataRow dr in result.Tables[0].Rows) {
                            if (dr["dept"].ToString().Trim().Equals(dept) && dr["ward"].ToString().Trim().Equals(ward) && dr["bed"].ToString().Trim().Equals(bed))
                            {
                                transaction.Rollback();
                                return new ErrorResponse("This patient is already in " + dr["ward"].ToString().Trim(), 409, Request);
                            }
                        }
                    }

                    // Update pat table setting the patient's ward2 field indicating that they are in a holding area.
                    var updateResult = new DB.Update
                    {
                        Sql = "UPDATE pat SET ward2=@ward2 WHERE site=@site AND ibex=@ibex",
                        Connection = con,
                        Transaction = transaction,
                        Parameters = new SqlParameter[]
                        {
                            new SqlParameter("@ibex", SqlDbType.Char, 14) { Value = loc.Ibex },
                            new SqlParameter("@site", SqlDbType.TinyInt) { Value = siteId },
                            new SqlParameter("@ward2", SqlDbType.VarChar) { Value = ward }
                        }
                    }.Run();

                    newPatient.Ward2 = ward;
                }
                else if (returnToBed)
                {
                    // Update pat table, clearing out the patient's ward2 field indicating that they have return to the bed or area.
                    var updateResult = new DB.Update
                    {
                        Sql = "UPDATE pat SET ward2='' WHERE site=@site AND ibex=@ibex",
                        Connection = con,
                        Transaction = transaction,
                        Parameters = new SqlParameter[]
                        {
                            new SqlParameter("@ibex", SqlDbType.Char, 14) { Value = loc.Ibex },
                            new SqlParameter("@site", SqlDbType.TinyInt) { Value = siteId }
                        }
                    }.Run();

                    newPatient.Ward2 = "";
                } else
                {
                    var oldPatientDepartment = oldPatient.Department.Trim();
                    bool setNewDeptDate = (!oldPatientDepartment.Equals(dept));

                    var interfaceCheck = new DB.Select
                    {
                        Sql = "SELECT gottriadt, root FROM org WHERE site=@site",
                        Connection = con,
                        Transaction = transaction,
                        Parameters = new SqlParameter[]
                        {
                            new SqlParameter("@site", SqlDbType.TinyInt) { Value = siteId }
                        }
                    }.RunForDataRow();

                    if (interfaceCheck["gottriadt"].ToString().Equals("Y"))
                    {
                        var root = interfaceCheck["root"].ToString().Trim();
                        var filePath = root + "link\\tri\\A02" + loc.Ibex + ".txt";
                        var msg = userId + "|" + oldPatient.Department + "^" + oldPatient.Ward + "^" + oldPatient.Bed + "|" + dept + "^" + ward + "^" + bed + "|\n";
                        FileWriter.Write(filePath, msg);

                        filePath = root + "link\\tri\\88_" + loc.Ibex;
                        FileWriter.Write(filePath, msg);
                    }

                    // Make sure new location is still valid.
                    if (!String.IsNullOrWhiteSpace(bed))
                    {
                        var defaultBedParams = new SqlParameter[]
                        {
                            new SqlParameter("@dept", SqlDbType.VarChar) { Value = dept },
                            new SqlParameter("@ward", SqlDbType.VarChar) { Value = ward },
                            new SqlParameter("@bed",  SqlDbType.VarChar) { Value = bed },
                            new SqlParameter("@site", SqlDbType.TinyInt) { Value = siteId }
                        };

                        var bedSql = "SELECT 1 AS [result] FROM bed WHERE dept=@dept AND ward=@ward AND bed=@bed AND site=@site AND status='E'";
                        try
                        {
                            var result = new DB.Select
                            {
                                Sql = bedSql,
                                Connection = con,
                                Transaction = transaction,
                                Parameters = defaultBedParams
                            }.RunForInt();

                            if (result != 1)
                            {
                                transaction.Rollback();
                                return new ErrorResponse("This bed (" + bedName + ") is not available.", 409, Request);
                            }
                        } catch (SqlException ex)
                        {
                            transaction.Rollback();
                            DTFL.Write(siteId, userId, ex, bedSql, defaultBedParams);
                            return new WLRResponse(ErrorCodes.DATA_WRITE_FAILED, Request);
                        }

                        if (!shareBed)
                        {
                            var transferSql = "SELECT COUNT(ibex) AS [result] FROM [api].[occupied_beds_vw] WHERE site=@site AND dept=@dept AND ward=@ward AND bed=@bed";
                            var transferParams = new SqlParameter[]
                            {
                                new SqlParameter("@dept", SqlDbType.VarChar) { Value = dept },
                                new SqlParameter("@ward", SqlDbType.VarChar) { Value = ward },
                                new SqlParameter("@bed",  SqlDbType.VarChar) { Value = bed },
                                new SqlParameter("@site", SqlDbType.TinyInt) { Value = siteId }
                            };

                            try
                            {
                                var shareResult = new DB.Select
                                {
                                    Sql = transferSql,
                                    Connection = con,
                                    Transaction = transaction,
                                    Parameters = transferParams
                                }.RunForInt();

                                if (shareResult != 0)
                                {
                                    transaction.Rollback();
                                    return new ErrorResponse("This bed (" + bedName + ") is already occupied.", 409, Request);
                                }
                            }
                            catch (SqlException ex)
                            {
                                transaction.Rollback();
                                DTFL.Write(siteId, userId, ex, transferSql, transferParams);
                                return new WLRResponse(ErrorCodes.DATA_WRITE_FAILED, Request);
                            }
                        }
                    } else
                    {
                        var shareSql = "SELECT 1 AS [result] FROM ward WHERE site=@site AND dept=@dept AND ward=@ward AND status IN('A','D')";
                        var shareParams = new SqlParameter[]
                        {
                            new SqlParameter("@site", SqlDbType.TinyInt) { Value = siteId },
                            new SqlParameter("@dept", SqlDbType.VarChar) { Value = dept },
                            new SqlParameter("@ward", SqlDbType.VarChar) { Value = ward }
                        };

                        try
                        {
                            var wardResult = new DB.Select
                            {
                                Sql = shareSql,
                                Connection = con,
                                Transaction = transaction,
                                Parameters = shareParams
                            }.RunForInt();

                            if (wardResult != 1)
                            {
                                transaction.Rollback();
                                return new ErrorResponse(deptName + " - " + ward + " is not a valid location.", 409, Request);
                            }
                        }
                        catch (SqlException ex)
                        {
                            transaction.Rollback();
                            DTFL.Write(siteId, userId, ex, shareSql, shareParams);
                            return new WLRResponse(ErrorCodes.DATA_WRITE_FAILED, Request);
                        }
                    }

                    // Update pat table with new location of patient and clear out the patient's ward2 field
                    // if the patient was in holding area before transfer.
                    var patUpdateParameters = new List<SqlParameter>
                    {
                        new SqlParameter("@dept", SqlDbType.Char)     { Value = dept },
                        new SqlParameter("@ward", SqlDbType.Char)     { Value = ward },
                        new SqlParameter("@bed",  SqlDbType.Char)     { Value = String.IsNullOrWhiteSpace(bed) ? "" : bed},
                        new SqlParameter("@ward2", SqlDbType.Char)    { Value = "" },
                        new SqlParameter("@roomtimer", SqlDbType.Int) { Value = _t.time() },
                        new SqlParameter("@site", SqlDbType.TinyInt)  { Value = siteId },
                        new SqlParameter("@ibex", SqlDbType.Char)     { Value = loc.Ibex }
                    };

                    var patUpdateSql = "UPDATE pat SET dept=@dept, ward=@ward, bed=@bed, ward2=@ward2, roomtimer=@roomtimer";
                    if (setNewDeptDate)
                    {
                        patUpdateSql += ", deptdate=@deptdate";
                        patUpdateParameters.Add(new SqlParameter("@deptdate", SqlDbType.Char) { Value = date });
                    }
                    patUpdateSql += " WHERE site=@site AND ibex=@ibex";

                    try
                    {
                        var patUpdateResult = new DB.Update
                        {
                            Sql = patUpdateSql,
                            Connection = con,
                            Transaction = transaction,
                            Parameters = patUpdateParameters.ToArray()
                        }.Run();

                        if (patUpdateResult < 1)
                        {
                            transaction.Rollback();
                            return new ErrorResponse("Patient details could not be updated.", 500, Request);
                        }
                    }
                    catch (SqlException ex)
                    {
                        transaction.Rollback();
                        DTFL.Write(siteId, userId, ex, patUpdateSql, patUpdateParameters.ToArray());
                        return new WLRResponse(ErrorCodes.DATA_WRITE_FAILED, Request);
                    }
                }

                // Patient is being assigned to a bed for the first time.
                var losecs = 0;
                if (String.IsNullOrWhiteSpace(oldPatient.Bed) && !String.IsNullOrWhiteSpace(bed) && !returnToBed)
                {
                    var trxResult = new DB.Select
                    {
                        Sql = "SELECT COUNT(ibex) AS [result] FROM trx WHERE ibex=@ibex AND site=@site AND type=@type AND service=@service",
                        Connection = con,
                        Transaction = transaction,
                        Parameters = new SqlParameter[]
                        {
                            new SqlParameter("@ibex", SqlDbType.Char) { Value = loc.Ibex },
                            new SqlParameter("@site", SqlDbType.TinyInt) { Value = siteId },
                            new SqlParameter("@type", SqlDbType.Char) { Value = "S" },
                            new SqlParameter("@service", SqlDbType.Int) { Value = 10 }
                        }
                    }.RunForInt();

                    if (trxResult < 1)
                    {
                        // TODO: Can the Timers class handle this for us?
                        Dictionary<string, object> Values = new Dictionary<string, object>
                        {
                            { Transaction.Constants.Name, "Time to Bed" },
                            { Transaction.Constants.Service, 10 },
                            { Transaction.Constants.Minutes, _t.DiffMinutes(loc.Ibex) },
                            { Transaction.Constants.Type, "S" },
                            { Transaction.Constants.Date, loc.Ibex },
                            { Transaction.Constants.ThruDate, date }
                        };
                        var t = new Transaction(siteId, newPatient, userId, Values, null);
                        losecs = t.AddTransaction();

                        if (losecs < 1)
                        {
                            return new ErrorResponse("Transaction for \"Time to Bed\" was not able to be recorded!", 500, Request);
                        }
                    }
                }

                if (!holdBed)
                {
                    var oldWard = oldPatient.Ward;
                    var oldBed = oldPatient.Bed;

                    if (!returnToBed)
                    {
                        oldWard = newPatient.Ward2;
                        oldBed = "";
                    }

                    var trxResult = new DB.Select
                    {
                        Sql = "SELECT trxdate, losecs FROM trx WHERE ibex=@ibex AND type=@type AND thrudate=@thrudate AND site=@site AND dept=@dept AND ward=@ward AND bed=@bed",
                        Connection = con,
                        Transaction = transaction,
                        Parameters = new SqlParameter[]
                        {
                            new SqlParameter("@ibex", SqlDbType.Char) { Value = loc.Ibex },
                            new SqlParameter("@type", SqlDbType.Char) { Value = "B" },
                            new SqlParameter("@thrudate", SqlDbType.VarChar) { Value = " " },    // Yes this was a space in a varchar in the Perl code. Scared to change it.
                            new SqlParameter("@site", SqlDbType.TinyInt) { Value = siteId },
                            new SqlParameter("@dept", SqlDbType.Char) { Value = oldPatient.Department },
                            new SqlParameter("@ward", SqlDbType.Char) { Value = oldWard },
                            new SqlParameter("@bed", SqlDbType.VarChar) { Value = oldBed }
                        }
                    }.RunForDataRow();

                    var oldTrxDate = ""; 
                    var oldLosecsValue = "";

                    if (trxResult != null)
                    {
                        trxResult["trxdate"].ToString();
                        trxResult["losecs"].ToString();
                    }

                    if (!String.IsNullOrWhiteSpace(oldTrxDate))
                    {
                        var trxMins = _t.DiffMinutes(oldTrxDate, date);
                        var updateResult = new DB.Update
                        {
                            Sql = "UPDATE trx SET mins=@mins, thrudate=@thrudate WHERE ibex=@ibex AND site=@site AND losecs=@losecs",
                            Connection = con,
                            Transaction = transaction,
                            Parameters = new SqlParameter[]
                            {
                                new SqlParameter("@mins", SqlDbType.Int) { Value = trxMins },
                                new SqlParameter("@thrudate", SqlDbType.VarChar) { Value = date },
                                new SqlParameter("@ibex", SqlDbType.Char) { Value = loc.Ibex },
                                new SqlParameter("@site", SqlDbType.TinyInt) { Value = siteId },
                                new SqlParameter("@losecs", SqlDbType.Int) { Value = oldLosecsValue }
                            }
                        }.Run();

                        if (updateResult < 1)
                        {
                            transaction.Rollback();
                            return new ErrorResponse("Transaction B entry could not be updated", 500, Request);
                        }
                    }
                }

                // Patient is transferring to a new bed or area
                if (!returnToBed)
                {
                    var trxName = "Transfer to " + bedName;
                    var Values = new Dictionary<string, object>
                    {
                        { Transaction.Constants.Name, trxName },
                        { Transaction.Constants.Service, 20 },
                        { Transaction.Constants.Type, "B" }
                    };
                    var t = new Transaction(siteId, newPatient, userId, Values, null);
                    if (t.AddTransaction() == 0)
                    {
                        transaction.Rollback();
                        return new ErrorResponse("Transaction for '" + trxName + "' was not able to be recorded!", 500, Request);
                    }
                }

                // Write the transfer to the chart
                Dictionary<string, string> fullNames = new Dictionary<string, string>();
                var deptInfoResult = new DB.Select
                {
                    Sql = "SELECT dept,name FROM dept WHERE site=@site AND (dept=@olddept OR dept=@newdept)",
                    Connection = con,
                    Transaction = transaction,
                    Parameters = new SqlParameter[]
                    {
                        new SqlParameter("@site", SqlDbType.TinyInt) { Value = siteId },
                        new SqlParameter("@olddept", SqlDbType.VarChar) { Value = oldPatient.Department },
                        new SqlParameter("@newdept", SqlDbType.VarChar) { Value = newPatient.Department }
                    }
                }.RunForDataSet();

                if (deptInfoResult != null && deptInfoResult.Tables.Count > 0)
                {
                    foreach (DataRow dr in deptInfoResult.Tables[0].Rows)
                    {
                        fullNames.Add("D|" + dr["dept"].ToString(), dr["name"].ToString());
                    }
                }

                var wardInfoResult = new DB.Select
                {
                    Sql = "SELECT dept,ward,name FROM ward WHERE site=@site AND ((dept=@olddept AND ward=@oldward) OR (dept=@newdept AND ward=@newward))",
                    Connection = con,
                    Transaction = transaction,
                    Parameters = new SqlParameter[]
                    {
                        new SqlParameter("@site", SqlDbType.TinyInt) { Value = siteId },
                        new SqlParameter("@olddept", SqlDbType.VarChar) { Value = oldPatient.Department },
                        new SqlParameter("@oldward", SqlDbType.VarChar) { Value = oldPatient.Ward },
                        new SqlParameter("@newdept", SqlDbType.VarChar) { Value = newPatient.Department },
                        new SqlParameter("@newward", SqlDbType.VarChar) { Value = newPatient.Ward }
                    }
                }.RunForDataSet();

                if (wardInfoResult != null && wardInfoResult.Tables.Count > 0)
                {
                    foreach (DataRow dr in wardInfoResult.Tables[0].Rows)
                    {
                        fullNames.Add("A|" + dr["dept"].ToString() + dr["ward"].ToString(), dr["name"].ToString());
                    }
                }

                List<string> oldLocation = new List<string>();
                List<string> newLocation = new List<string>();
                var oldDept = oldPatient.Department;
                if (fullNames.ContainsKey("D|" + oldDept))
                {
                    oldDept = fullNames["D|" + oldDept];
                }
                oldLocation.Add(oldDept);

                if (!oldPatient.Department.Equals(newPatient.Department))
                {
                    var newDept = newPatient.Department;
                    if (fullNames.ContainsKey("D|" + newDept))
                    {
                        newDept = fullNames["D|" + newDept];
                    }
                    newLocation.Add(newDept);
                }

                var pOldWard = oldPatient.Ward;
                if (fullNames.ContainsKey("A|" + oldPatient.Department + pOldWard))
                {
                    pOldWard = fullNames["A|" + oldPatient.Department + pOldWard];
                }
                oldLocation.Add(pOldWard);

                if (newLocation.Count > 0 || !oldPatient.Ward.Equals(newPatient.Ward))
                {
                    var newWard = newPatient.Ward;
                    if (fullNames.ContainsKey("A|" + newPatient.Department + newWard))
                    {
                        newWard = fullNames["A|" + newPatient.Department + newWard];
                    }
                    newLocation.Add(newWard);
                }

                if (!String.IsNullOrEmpty(oldPatient.Bed.Trim()))
                {
                    oldLocation.Add(oldPatient.Bed.Trim());
                }

                if(holdBed)
                {
                    newLocation.Add("(Hold Bed)");
                }

                if (!String.IsNullOrEmpty(newPatient.Bed.Trim())) {
                    newLocation.Add(newPatient.Bed.Trim());
                }

                string transferInfo = "";
                if (returnToBed)
                {
                    if (!String.IsNullOrEmpty(oldPatient.Bed.Trim()))
                    {
                        oldLocation.Add(oldPatient.Bed.Trim());
                    }
                    transferInfo = "Return to " + String.Join(" ", oldLocation);
                } else
                {
                    transferInfo = String.Join(" ", oldLocation) + " to " + String.Join(" ", newLocation);
                }

                // Update bed status of old bed if transferring from old bed to new bed, or to holding area without selcting Hold bed.
                if (!String.IsNullOrEmpty(oldPatient.Bed.Trim()) && (!String.IsNullOrEmpty(newPatient.Bed.Trim()) || (!holdBed || !returnToBed)))
                {
                    var updateResult = new DB.Update
                    {
                        Sql = "[api].[UpdateEmptiedBed]",
                        IsStoredProcedure = true,
                        Connection = con,
                        Transaction = transaction,
                        Parameters = new SqlParameter[]
                        {
                            new SqlParameter("@site", SqlDbType.TinyInt) { Value = siteId },
                            new SqlParameter("@user", SqlDbType.Int) { Value = userId },
                            new SqlParameter("@dept", SqlDbType.VarChar) { Value = oldPatient.Department.Trim() },
                            new SqlParameter("@ward", SqlDbType.VarChar) { Value = oldPatient.Ward.Trim() },
                            new SqlParameter("@bed", SqlDbType.VarChar) { Value = oldPatient.Bed.Trim() },
                        }
                    }.Run();
                }

                var transferData = new EMR.Line.DataSegment(EMR.Line.DataSegment.Constants.TYPE_TEXT, Escape.ChartEscape(transferInfo));
                var transferEntry = new EMR.Line
                {
                    LineHeader = new EMR.Line.Header
                    {
                        sys_time = _t.Timestamp(),
                        user = userId
                    },
                    LinePart = new EMR.Line.Part
                    {
                        nct = EMR.Constants.NCT_EVENTS,
                        section = EMR.Constants.SECT_EVENTS,
                        part = "TRANSFER"
                    },
                    DataSegments = new List<EMR.Line.DataSegment> {
                        transferData
                    }
                };

                var chart = new EMR(siteId, loc.Ibex, true);
                if (!chart.WriteLine(transferEntry))
                {
                    transaction.Rollback();
                    return new ErrorResponse("Error updating chart file", 500, Request);
                }

                transaction.Commit();
                con.Close();
            }

            return null;
        }

        /// <summary>
        /// Add or remove comments for a particular patient
        /// </summary>
        /// <remarks></remarks>
        /// <returns></returns>
        /// <response code="200"></response>
        [VersionedRoute("api/patient/comment", 1)]
        [Route("api/v1/patient/comment")]
        [HttpPost]
        public async Task<IHttpActionResult> PostCommentV1([FromBody]PatientComments commentsInfo)
        {
            int userId = _authUtil.GetAuthenticatedUserId(User);
            User user = await _userService.GetUserByIdAsync(userId);

            if (!user.CanNavigateTo(Navigation.Constants.PATIENT_COMMENTS_WRITE))
            {
                return new WLRResponse(ErrorCodes.NOT_AUTHORIZED, Request);
            }

            string ibex = commentsInfo.Ibex;
            int failures = 0;

            var comments = await _siteService.GetCommentsBySiteIdAsync(user.SiteId);
            var patient = await _patientService.GetPatientByIdAsync(user.SiteId, ibex, user);

            foreach(PatientComment p in commentsInfo.Comments) {
                var result = _patientService.PostComment(user.SiteId, ibex, userId, p.Id, p.Name, p.Remove);
                if (result != 1)
                {
                    if (result < 0)
                    {
                        return new WLRResponse(result, Request);
                    }
                    failures++;
                }
                else if (!p.Remove)
                {
                    var postedComment = comments.Find(c => c.Id.ToString().Equals(p.Id.ToString()));
                    var commentName = postedComment != null ? postedComment.Name : p.Name;
                    var commentAlienKey = postedComment != null ? String.IsNullOrWhiteSpace(postedComment.Code) ? "A" : postedComment.Code : "";

                    var Values = new Dictionary<String, object>
                    {
                        { Transaction.Constants.Service, 10 },
                        { Transaction.Constants.Type, "N" },
                        { Transaction.Constants.Name, commentName },
                        { Transaction.Constants.Alienkey, commentAlienKey }
                    };
                    var t = new Transaction(user.SiteId, patient, userId, Values, null);
                    if (t.AddTransaction() == 0)
                    {
                        failures++;
                    }
                }
            }
            if (failures > 0)
            {
                return new WLRResponse(ErrorCodes.DATA_WRITE_FAILED, Request);
            }

            return Ok();
        }

        /// <summary>
        /// Sign up a user for a particular patient
        /// </summary>
        /// <remarks></remarks>
        /// <returns></returns>
        /// <response code="200"></response>
        [VersionedRoute("api/patient/signup", 1)]
        [Route("api/v1/patient/signup")]
        [HttpPost]
        public async Task<IHttpActionResult> PostSignUpV1(SignupInfo info)
        {
            Dictionary<string, string> Events = new Dictionary<string, string>
            {
                { DomainModel.Constants.Id_Resident, "RESIDENT" },
                { DomainModel.Constants.Id_Doctor, "ATTENDING" },
                { DomainModel.Constants.Id_DoctorExtender, "DOCTOR EXTENDER" }
            };

            int userId = _authUtil.GetAuthenticatedUserId(User);
            User user = await _userService.GetUserByIdAsync(userId);
            if (user.IsOrderingOnly() || !user.IsActive())
                return new WLRResponse(ErrorCodes.NOT_AUTHORIZED, Request);

            var role = info.RoleId;
            var chargeNurse = user.HasWritePermission(Permission.CHARGE_NURSE_VIEW);
            if (user.IsNurse())
            {
                if (!role.Equals("primarynurse") && !role.Equals("extender"))
                {
                    var shw = new DB.Select
                    {
                        Sql = "SELECT shw FROM drs WHERE num=@num",
                        Parameters = new SqlParameter[]
                        {
                            new SqlParameter("@num", SqlDbType.Int) { Value = user.Id }
                        }
                    }.RunForScalar().ToString();
                    if (!chargeNurse || !shw.Substring(16, 1).Equals("1"))
                        return new WLRResponse(ErrorCodes.PARAMETER_FAULT, Request);
                }
            }
            else if (!Events.ContainsKey(role))
            {
                return new WLRResponse(ErrorCodes.PARAMETER_FAULT, Request);
            }

            var providerType = "";
            var oldProviderText = "(none)";
            var newProviderText = user.FirstName + " " + user.LastName;
            var currentPat = await _patientService.GetPatientByIdAsync(user.SiteId, info.Ibex, user);
            var oldProvider = 0;
            foreach(MinimalProvider p in currentPat.Providers)
            {
                if (p.Role.Id.Equals(info.RoleId))
                {
                    providerType = p.Role.Description;
                    if (p.User != null && p.User.Id > 0)
                        oldProvider = p.User.Id;
                }
            }

            // User is already signed up for this role. We're done.
            if (oldProvider == user.Id)
                return Ok(currentPat);

            var updateFields = new List<string> { info.RoleId + "=@provider1" };
            var updateParams = new List<SqlParameter> {
                new SqlParameter("@site", SqlDbType.TinyInt) { Value = user.SiteId },
                new SqlParameter("@ibex", SqlDbType.Char) { Value = info.Ibex },
                new SqlParameter("@provider1", SqlDbType.Int) { Value = user.Id }
            };

            if (role.Equals(DomainModel.Constants.Id_Doctor))
            {
                if (currentPat.FirstDoctor == 0)
                {
                    updateFields.Add("firstdoctor=@provider2");
                    updateParams.Add(new SqlParameter("@provider2", SqlDbType.Int) { Value = user.Id });
                }

                // Doctor signup needs to handle possible First Doctor and Second Doctor timers.
                var lo_count = new DB.Select
                {
                    Sql = "SELECT COUNT(losecs) AS lo_count FROM trx WHERE ibex=@ibex AND site=@site AND service IN(20,16614) AND type='S'",
                    Parameters = new SqlParameter[]
                    {
                        new SqlParameter("@ibex", SqlDbType.Char) { Value = info.Ibex },
                        new SqlParameter("@site", SqlDbType.TinyInt) { Value = user.SiteId }
                    }
                }.RunForInt();

                if (lo_count == 1)
                {
                    Timers.CreateTimer(Timers.Constants.SECOND_DOCTOR, currentPat, user);
                } else if (currentPat.FirstDoctor == 0)
                {
                    Timers.CreateTimer(Timers.Constants.DOCTOR, currentPat, user);
                }
            } else if (role.Equals(DomainModel.Constants.Id_Resident))
            {
                if (oldProvider == 0)
                {
                    Timers.CreateTimer(Timers.Constants.RESIDENT, currentPat, user);
                }
            } else if (role.Equals(DomainModel.Constants.Id_DoctorExtender))
            {
                if (oldProvider == 0)
                {
                    Timers.CreateTimer(Timers.Constants.DR_EXTENDER, currentPat, user);
                }
            } else if (role.Equals(DomainModel.Constants.Id_PrimaryNurse))
            {
                if (oldProvider == 0)
                {
                    Timers.CreateTimer(Timers.Constants.NURSE, currentPat, user);
                }
            } else if (role.Equals(DomainModel.Constants.Id_Extender))
            {
                if (oldProvider == 0)
                {
                    Timers.CreateTimer(Timers.Constants.EXTENDER, currentPat, user);
                }
            } else if (role.Equals(DomainModel.Constants.Id_Scribe))
            {

            } else if (role.Equals(DomainModel.Constants.Id_CareCoordinator))
            {
                if (oldProvider == 0)
                    Timers.CreateTimer(Timers.Constants.CARE_COORDINATOR, currentPat, user);

                // TODO: The Perl code in ibex65 was doing something here with checking care_coordinator_status and flipping it to 'A' if needed.
            }

            if (oldProvider > 0)
            {
                var oldUser = await _userService.GetUserByIdAsync(oldProvider);
                oldProviderText = oldUser.FirstName + " " + oldUser.LastName;
            }

            // Write change type to chart
            var _t = new Time();
            var sysDate = _t.Timestamp();
            var emr = new EMR(user.SiteId, info.Ibex, true);
            var changeLine = Escape.ChartEscape(providerType + " changed from " + oldProviderText + " to " + newProviderText);
            var newLines = new List<EMR.Line> { 
                new EMR.Line
                {
                    LineHeader = new EMR.Line.Header
                    {
                        sys_time = sysDate,
                        user = userId
                    },
                    LinePart = new EMR.Line.Part
                    {
                        nct = EMR.Constants.NCT_PATIENT_DATA_CHANGE,
                        section = EMR.Constants.SECT_ADMIN,
                        part = "PATIENT DATA CHANGE"
                    },
                    DataSegments = new List<EMR.Line.DataSegment>
                    {
                        new EMR.Line.DataSegment(EMR.Line.DataSegment.Constants.TYPE_DROPDOWN, changeLine)
                    }
                }
            };

            var orgInfo = new DB.Select
            {
                Sql = "SELECT gottriadt,root,nctcs FROM org WHERE site = @site",
                Parameters = new SqlParameter[]
                {
                        new SqlParameter("@site", SqlDbType.TinyInt) { Value = user.SiteId }
                }
            }.RunForDataRow();

            Patient newPat = null;
            var connection = DB.GetConnectionString();
            using (var con = new SqlConnection(connection))
            {
                con.Open();
                var patUpdate = new DB.Update
                {
                    Connection = con,
                    Sql = "UPDATE pat SET " + String.Join(",", updateFields) + " WHERE site=@site AND ibex=@ibex",
                    Parameters = updateParams.ToArray()
                }.Run();

                if (patUpdate < 1)
                    return new ErrorResponse("Error setting new provider", 500, Request);

                await MeaningfulUse.LogModification(user, currentPat.Ibex, "PATIENT DATA");

                newPat = await _patientService.GetPatientByIdAsync(user.SiteId, info.Ibex, user);

                var transaction = con.BeginTransaction();

                if (orgInfo["gottriadt"].ToString().Equals("Y"))
                {
                    var root = orgInfo["root"].ToString().Trim();
                    var filePath = root + "link\\tri\\A08" + info.Ibex;
                    FileWriter.Write(filePath, "");

                    filePath = root + "link\\tri\\65_" + info.Ibex;
                    FileWriter.Write(filePath, "");
                }

                // TimeSeen was provided by a doctor, so the user is saying they saw the patient. Need to do some extra DB chart writes.
                if (user.IsPhysician() && info.TimeSeen != null)
                {
                    // Check trx entries
                    var trxService = 0;
                    switch (role)
                    {
                        case DomainModel.Constants.Id_Doctor:
                            trxService = 50;
                            break;
                        case DomainModel.Constants.Id_Resident:
                            trxService = 51;
                            break;
                        case DomainModel.Constants.Id_DoctorExtender:
                            trxService = 52;
                            break;
                        default:
                            break;
                    }

                    if (trxService > 0)
                    {
                        var trxName = "Seen by " + providerType;
                        var trxDate = info.Ibex.Substring(0, 12);
                        var trxThruDate = _t.DateTimeToStringNoSeconds(info.TimeSeen);
                        var entrySql = "SELECT ibex,sysdate,thrudate,usr FROM trx WHERE ibex=@ibex AND site=@site AND type='S' AND service=@service";
                        var currentEntry = new DB.Select
                        {
                            Sql = entrySql,
                            Connection = con,
                            Transaction = transaction,
                            Parameters = new SqlParameter[]
                            {
                                new SqlParameter("@ibex", SqlDbType.Char) { Value  = info.Ibex },
                                new SqlParameter("@site", SqlDbType.TinyInt) { Value = user.SiteId },
                                new SqlParameter("@service", SqlDbType.Int) { Value = trxService }
                            }
                        }.RunForDataRow();

                        // TODO: There is a lot of logic in ibex4c for this that was not implemented in the API because it didn't
                        // seem like the API would be allowing users to un-see patients or edit their seen documentation.
                        if (currentEntry == null || info.TimeSeen < Time.DateTimeFromString(currentEntry["thrudate"].ToString()))
                        {
                            Dictionary<string, object> Values = new Dictionary<string, object>
                            {
                                { Transaction.Constants.Type, "S" },
                                { Transaction.Constants.Name, trxName },
                                { Transaction.Constants.Service, trxService },
                                { Transaction.Constants.Date, trxDate },
                                { Transaction.Constants.ThruDate, trxThruDate },
                                { Transaction.Constants.User, user.Id },
                                { Transaction.Constants.SystemDate, sysDate }
                            };

                            // There is a trx entry already...
                            if (currentEntry != null)
                            {
                                // Remove the existing trx entry
                                new DB.Update
                                {
                                    Sql = "DELETE FROM trx WHERE ibex=@ibex AND site=@site AND type='S' AND service=@service",
                                    Connection = con,
                                    Transaction = transaction,
                                    Parameters = new SqlParameter[]
                                    {
                                        new SqlParameter("@ibex", SqlDbType.Char) { Value  = info.Ibex },
                                        new SqlParameter("@site", SqlDbType.TinyInt) { Value = user.SiteId },
                                        new SqlParameter("@service", SqlDbType.Int) { Value = trxService }
                                    }
                                }.Run();
                            }

                            var t = new Transaction(user.SiteId, newPat, userId, Values, null);
                            var losecs = t.AddTransaction();

                            if (losecs < 1)
                            {
                                transaction.Rollback();
                                return new ErrorResponse("Error storing transaction record", 500, Request);
                            }
                        }

                        // Update seen time for patient in pat table.
                        var updateSql = "UPDATE pat SET " + info.RoleId + "_seen=@seen WHERE ibex=@ibex AND site=@site";
                        var update = new DB.Update
                        {
                            Sql = updateSql,
                            Connection = con,
                            Transaction = transaction,
                            Parameters = new SqlParameter[]
                            {
                                new SqlParameter("@seen", SqlDbType.Char) { Value = sysDate },
                                new SqlParameter("@ibex", SqlDbType.Char) { Value  = info.Ibex },
                                new SqlParameter("@site", SqlDbType.TinyInt) { Value = user.SiteId }
                            }
                        }.Run();
                    }

                    var signupInfo = await _siteService.GetSignupInfo(user.SiteId, user);

                    var filePath = orgInfo["root"].ToString().Trim() + "htdocs\\" + orgInfo["nctcs"] + "\\parts\\seen_time.prt";
                    var HNAME = Events[info.RoleId].Replace(" ", "");
                    var replaceDocumentation = "I saw the patient";
                    var replaceDocumentationEsc = replaceDocumentation;
                    var col = info.RoleId + "_seen_text";
                    var text = signupInfo["SeenTextRaw_" + info.RoleId];
                    var replaceFormat = "";
                    var seenDateTime = (new Time(user.SiteId)).LongDateTime(_t.DateTimeToString(info.TimeSeen));
                    if (!String.IsNullOrWhiteSpace(text))
                    {
                        replaceFormat = text;
                        replaceDocumentation = EMR.FormatSeenTimeInfo(text, user, seenDateTime);
                        replaceDocumentationEsc = replaceDocumentation;     // No idea why this isn't actually escaped. It wasn't in the Perl code either.
                    }

                    List<EMR.Line.DataSegment> chartData = new List<EMR.Line.DataSegment>();
                    var markupMatcher = new Regex("name\\s*=\\s*\"" + @"(\^[a-z]\^" + "[^\"]+)\"", RegexOptions.IgnoreCase);
                    var HNAMEReplace = new Regex("<HNAME>", RegexOptions.IgnoreCase);
                    if (File.Exists(filePath))
                    {
                        foreach (string line in File.ReadLines(filePath))
                        {
                            foreach (Match m in markupMatcher.Matches(line))
                            {
                                var name = m.Groups[1].Value;
                                name = HNAMEReplace.Replace(name, HNAME);
                                if (name.IndexOf("^fmt") > 0)
                                {
                                    chartData.Add(new EMR.Line.DataSegment(name + "=" + Escape.ChartEscape(replaceFormat)));
                                }
                                else if (name.IndexOf("^seen") > 0)
                                {
                                    chartData.Add(new EMR.Line.DataSegment(name + "=" + Escape.ChartEscape(replaceDocumentation)));
                                }
                            }
                        }
                    }

                    if (chartData.Count > 0)
                    {
                        newLines.Add(new EMR.Line
                        {
                            LineHeader = new EMR.Line.Header
                            {
                                sys_time = sysDate,
                                user_time = _t.DateTimeToString(info.TimeSeen),
                                user = user.Id
                            },
                            LinePart = new EMR.Line.Part
                            {
                                nct = EMR.Constants.NCT_EVENTS,
                                part = Events[role],
                                section = EMR.Constants.SECT_EVENTS
                            },
                            DataSegments = chartData
                        });
                    }
                }

                if (!emr.WriteLines(newLines.ToArray())) {
                    transaction.Rollback();
                    return new ErrorResponse("Error updating chart file", 500, Request);
                }

                transaction.Commit();
                con.Close();
            }

            return Ok(newPat);
        }
    }

    /// <summary>
    /// Object containing information for posting a medication action authentication failure
    /// </summary>
    public class MedAuthFailure
    {
        /// <summary>
        /// Action that failed authentication
        /// </summary>
        public string Action { get; set; }

        /// <summary>
        /// Losecs of med that failed authentication
        /// </summary>
        public int Losecs { get; set; }
    }

    /// <summary>
    /// Object containing information for posting an order of one or more medications for a patient
    /// </summary>
    public class MedSvcOrder
    {
        /// <summary>
        /// Order type (quick list, group, etc)
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// The ordering physician identifier
        /// </summary>
        public int OrderingPhysician { get; set; }

        /// <summary>
        /// Notes to apply to all med orders in this set
        /// </summary>
        public string Notes { get; set; }

        /// <summary>
        /// Service options to apply to all med orders in this set
        /// </summary>
        public List<string> ServiceOptions { get; set; }

        /// <summary>
        /// Authentication type used for order set
        /// </summary>
        public string AuthType { get; set; }

        /// <summary>
        /// The list of meds being ordered
        /// </summary>
        public List<OrderMedication> MedOrders { get; set; }
    }

    /// <summary>
    /// Object containing information for a particular comment
    /// </summary>
    public class PatientComment
    {
        /// <summary>
        /// Comment Id. 1, 2, or 3 = affect existing comment on tracking board (should only be used for removing a comment)
        /// Other value = add new structured comment to patient.
        /// Empty = add unstructured comment to patient.
        /// </summary>
        public Int32? Id { get; set; }

        /// <summary>
        /// Comment text. Can come from structured value associated with Id, or free text from user. Note that if Id is provided,
        /// Name value will be looked up anyway, and therefore cannot be overridden when using structured comments.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Flag to remove comment. Ignored unless Id = 1, 2, or 3.
        /// </summary>
        public bool Remove { get; set; }
    }

    /// <summary>
    /// Object containing information to post for changing comments on a patient
    /// </summary>
    public class PatientComments
    {
        /// <summary>
        /// Patient identifier
        /// </summary>
        public string Ibex { get; set; }

        /// <summary>
        /// List of PatientComment objects to post to patient.
        /// </summary>
        public List<PatientComment> Comments { get; set; }
    }

    /// <summary>
    /// Object containing information to post for assigning a patient to a new location
    /// </summary>
    public class PatientLocation
    {
        /// <summary>
        /// Patient identifier
        /// </summary>
        public string Ibex { get; set; }

        /// <summary>
        /// ID of new location
        /// </summary>
        public string LocationId { get; set; }
    }

    /// <summary>
    /// Object containing information to post for med order actions
    /// </summary>
    public class PatientMedOrder
    {
        /// <summary>
        /// Medication order ID
        /// </summary>
        public int Id { get; set; }
    }

    /// <summary>
    /// Object containing information to post for non-med order actions
    /// </summary>
    public class PatientOrder
    {
        /// <summary>
        /// Order ID
        /// </summary>
        public int Id { get; set; }
    }

    /// <summary>
    /// Object containing information to post for signing a patient's chart
    /// </summary>
    public class PatientSign
    {
        /// <summary>
        /// Patient identifier
        /// </summary>
        public string Ibex { get; set; }
    }

    /// <summary>
    /// Object containing information to post for a user signing up for a patient
    /// </summary>
    public class SignupInfo
    {
        /// <summary>
        /// Patient identifier
        /// </summary>
        public string Ibex { get; set; }

        /// <summary>
        /// Identifier of role that user is signing up for (doctor, resident, drextender, primarynurse, extender)
        /// </summary>
        public string RoleId { get; set; }

        /// <summary>
        /// DateTime for when the user saw the patient. Leave null for standard sign up if patient was not seen.
        /// </summary>
        public DateTime? TimeSeen { get; set; }
    }
}
