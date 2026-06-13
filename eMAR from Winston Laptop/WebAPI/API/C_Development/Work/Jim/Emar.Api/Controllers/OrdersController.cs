using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Emar.Api.Helpers;
using Emar.Core.Helpers;
using Emar.Core.Orders.Model;
using Emar.Core.Orders.Service;
using Emar.Core.ResourceParameters;
using Emar.Core.Templates.Model;
using Emar.Core.Templates.Service;
using Marvin.Cache.Headers;
using Microsoft.AspNetCore.Mvc;

namespace Emar.Api.Controllers
{
    /// <summary>
    /// Patient Orders Controller
    /// </summary>
    [ApiController]
    [Route("api/orders")]
    [HttpCacheExpiration(CacheLocation = CacheLocation.Public)]
    [HttpCacheValidation(MustRevalidate = true)]
    [Produces(MediaTypes.Json)]
    [Consumes(MediaTypes.Json)]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;
        //private readonly IPropertyMappingService _propertyMappingService;
        private readonly IPropertyCheckerService _propertyCheckerService;
        private readonly ITemplateService _templateService;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="orderService"></param>
        /// <param name="propertyCheckerService"></param>
        /// <param name="templateService"></param>
        public OrdersController(IOrderService orderService,
                                  //IPropertyMappingService propertyMappingService,
                                  IPropertyCheckerService propertyCheckerService,
                                  ITemplateService templateService)
        {
            _orderService = orderService ?? throw new ArgumentNullException(nameof(orderService));
            //_propertyMappingService = propertyMappingService ?? throw new ArgumentNullException(nameof(propertyMappingService));
            _propertyCheckerService = propertyCheckerService ?? throw new ArgumentNullException(nameof(propertyCheckerService));
            _templateService = templateService;
        }

        /// <summary>
        /// Get a list of orders for a patient.
        /// </summary>
        /// <param name="mediaType">
        /// Media type from Accept header.
        /// </param>
        /// <param name="patientId">
        /// eMAR unique patient identifier.  If it is passed in then this API call will return all the orders for this patient.
        /// </param>
        /// <returns>An ActionResult of IEnumerable collection of type OrderDto</returns>
        /// <remarks>
        /// </remarks>
        [HttpGet(Name = nameof(GetOrders))]
        [ProducesResponseType(typeof(IEnumerable<PatientOrderDto>), 200)] // (OK) - the resource is sent in the response
        [ProducesResponseType(400)] // (bad request) - indicates a bad request (e.g. wrong parameter)
        [ProducesResponseType(404)] // (not found) - the resource does not exits
        [ProducesResponseType(406)] // (not acceptable) - the server does not support the required representation
        public ActionResult<IEnumerable<PatientOrderDto>> GetOrders(
            [FromHeader(Name = "Accept")] string mediaType,
            [FromHeader(Name = "EMAR-Patient")] long? patientId
            )
        {
            if (!MediaTypes.IsValidMediaType(mediaType))
            {
                return BadRequest("Unsupported media type header provided.");
            }

            if (patientId == null)
            {
                return BadRequest("Patient id is missing.");
            }

            var resource = new BaseLinkResource
            {
                PatientId = patientId.Value,
                LinkExecuteOrderAction = Url.Link(nameof(ExecuteOrderAction), new { orderId = -99, actionId = "-98" }).Replace("-99", "{0}").Replace("-98", "{1}"),
                LinkExecuteAdministrationAction = Url.Link(nameof(ExecuteAdministrationAction), new { administrationId = -99, actionId = "-98" }).Replace("-99", "{0}").Replace("-98", "{1}"),
                LinkGetPatientOrder = Url.Link(nameof(OrdersController.GetOrder), new { orderId = -99 }),
                LinkGetCartOrder = Url.Link(nameof(CartOrdersController.GetCartOrder), new { cartOrderId = -99 }),
                LinkGetHomeMedication = Url.Link(nameof(HomeMedicationsController.GetHomeMedication), new { homeMedicationId = -99 })
            };

            var orders = _orderService.GetOrders(resource).ToList();

            if (!orders.Any())
                return NotFound($"No orders found for patient with id '{patientId}'.");

            var shapedOrders = ((IEnumerable<PatientOrderDto>)orders).ShapeData(null);
            var shapedOrdersWithLinks = shapedOrders.Select(order =>
            {
                var orderAsDictionary = order as IDictionary<string, object>;
                var orderLinks = CreateHateOasLinksForOrder((long)orderAsDictionary["Id"]);

                orderAsDictionary.Add("links", orderLinks);

                return orderAsDictionary;
            });

            return Ok(shapedOrdersWithLinks);
        }

        /// <summary>
        /// Get an order by order id.
        /// </summary>
        /// <param name="mediaType">
        /// Media type from Accept header.
        /// </param>
        /// <param name="orderId">
        /// Unique order identifier.
        /// </param>
        /// <param name="fields">
        /// *Optional.* \
        /// Comma delimited list of order elements to be returned.  If omitted, all order elements are returned.
        /// </param>
        /// <returns>An ActionResult of type OrderDto</returns>
        /// <remarks>
        /// </remarks>
        [HttpGet("{orderId}", Name = nameof(GetOrder))]
        [ProducesResponseType(200)] // (OK) - the resource is sent in the response
        [ProducesResponseType(400)] // (bad request) - indicates a bad request (e.g. wrong parameter)
        [ProducesResponseType(404)] // (not found) - the resource does not exits
        [ProducesResponseType(406)] // (not acceptable) - the server does not support the required representation
        public ActionResult<PatientOrderDto> GetOrder(
            [FromHeader(Name = "Accept")] string mediaType,
            [FromRoute(Name = "orderId")] long orderId
            )
        {
            if (!MediaTypes.IsValidMediaType(mediaType))
            {
                return BadRequest("Unsupported media type header provided.");
            }

            var resource = new BaseLinkResource
            {
                LinkExecuteOrderAction = Url.Link(nameof(ExecuteOrderAction), new { orderId = -99, actionId = "-98" }).Replace("-99", "{0}").Replace("-98", "{1}"),
                LinkExecuteAdministrationAction = Url.Link(nameof(ExecuteAdministrationAction), new { administrationId = -99, actionId = "-98" }).Replace("-99", "{0}").Replace("-98", "{1}"),
                LinkGetPatientOrder = Url.Link(nameof(OrdersController.GetOrder), new { orderId = -99 }),
                LinkGetCartOrder = Url.Link(nameof(CartOrdersController.GetCartOrder), new { cartOrderId = -99 }),
                LinkGetHomeMedication = Url.Link(nameof(HomeMedicationsController.GetHomeMedication), new { homeMedicationId = -99 })
            };

            var order = _orderService.GetOrder(orderId, resource);

            if (order == null)
            {
                return NotFound($"Patient order with id {orderId} was not found.");
            }

            var links = CreateHateOasLinksForOrder(orderId);
            var linkedResourceToReturn = order.ShapeData("") as IDictionary<string, object>;

            linkedResourceToReturn.Add("links", links);

            return Ok(linkedResourceToReturn);
        }

        ///// <summary>
        ///// Get the administrations of an order by order id.
        ///// </summary>
        ///// <param name="mediaType">
        ///// Media type from Accept header.
        ///// </param>
        ///// <param name="orderId">
        ///// Unique order identifier.
        ///// </param>
        ///// <returns>An ActionResult of IEnumerable collection of type OrderAdministrationDto</returns>
        ///// <remarks>
        ///// </remarks>
        //[HttpGet("{orderId}/administrations", Name = nameof(GetAdministrations))]
        //[ProducesResponseType(200)] // (OK) - the resource is sent in the response
        //[ProducesResponseType(400)] // (bad request) - indicates a bad request (e.g. wrong parameter)
        //[ProducesResponseType(404)] // (not found) - the resource does not exits
        //[ProducesResponseType(406)] // (not acceptable) - the server does not support the required representation
        //public ActionResult<IEnumerable<OrderAdministrationDto>> GetAdministrations(
        //    [FromHeader(Name = "Accept")] string mediaType,
        //    [FromRoute(Name = "orderId")] int orderId
        //    )
        //{
        //    if (!MediaTypes.IsValidMediaType(mediaType))
        //    {
        //        return BadRequest("Unsupported media type header provided.");
        //    }

        //    var administrationLinkBase = Url.Link(nameof(ExecuteAdministrationAction),
        //            new { administrationId = -99, actionCode = "XAction" })
        //        .Replace("-99", "{0}")
        //        .Replace("XAction", "{1}");

        //    var administrations = _orderService.GetAdministrations(orderId, administrationLinkBase);

        //    if (administrations == null)
        //    {
        //        return NotFound($"Patient order administrations for patient order with id {orderId} were not found");
        //    }

        //    return Ok(administrations);
        //}

        /// <summary>
        /// Get an administration by administration id of an order by order id.
        /// </summary>
        /// <param name="orderId">
        /// Unique order identifier.
        /// </param>
        /// <param name="mediaType">
        /// Media type from Accept header.
        /// </param>
        /// <param name="administrationId">
        /// Unique order administration identifier.
        /// </param>
        /// <returns>An ActionResult of type OrderAdministrationDto</returns>
        /// <remarks>
        /// </remarks>
        [HttpGet("{orderId}/administrations/{administrationId}", Name = nameof(GetAdministration))]
        [ProducesResponseType(200)] // (OK) - the resource is sent in the response
        [ProducesResponseType(400)] // (bad request) - indicates a bad request (e.g. wrong parameter)
        [ProducesResponseType(404)] // (not found) - the resource does not exits
        [ProducesResponseType(406)] // (not acceptable) - the server does not support the required representation
        public ActionResult<OrderAdministrationDto> GetAdministration(
            [FromHeader(Name = "Accept")] string mediaType,
            [FromRoute(Name = "orderId")] int orderId,
            [FromRoute(Name = "administrationId")] int administrationId
            )
        {
            if (!MediaTypes.IsValidMediaType(mediaType))
            {
                return BadRequest("Unsupported media type header provided.");
            }

            var administration = _orderService.GetAdministration(administrationId);

            if (administration == null)
            {
                return NotFound($"Patient order administration with id {administrationId} was not found");
            }

            if (!administration.OrderId.Equals(orderId))
            {
                return NotFound($"Patient order administration with id {administrationId} is not part of patient order with id {orderId}");
            }

            return Ok(administration);
        }

        /// <summary>
        /// Get the events of an administration by administration id of an order by order id.
        /// </summary>
        /// <param name="mediaType">
        /// Media type from Accept header.
        /// </param>
        /// <param name="orderId">
        /// Unique order identifier.
        /// </param>
        /// <param name="administrationId">
        /// Unique order administration identifier.
        /// </param>
        /// <returns>An ActionResult of IEnumerable collection of type OrderEventDto</returns>
        /// <remarks>
        /// </remarks>
        [HttpGet("{orderId}/administrations/{administrationId}/events", Name = nameof(GetAdministrationEvents))]
        [ProducesResponseType(200)] // (OK) - the resource is sent in the response
        [ProducesResponseType(400)] // (bad request) - indicates a bad request (e.g. wrong parameter)
        [ProducesResponseType(404)] // (not found) - the resource does not exits
        [ProducesResponseType(406)] // (not acceptable) - the server does not support the required representation
        public ActionResult<IEnumerable<OrderEventDto>> GetAdministrationEvents(
            [FromHeader(Name = "Accept")] string mediaType,
            [FromRoute(Name = "orderId")] int orderId,
            [FromRoute(Name = "administrationId")] int administrationId
            )
        {
            if (!MediaTypes.IsValidMediaType(mediaType))
            {
                return BadRequest("Unsupported media type header provided.");
            }

            var events = _orderService.GetAdministrationEvents(administrationId);

            if (events == null)
            {
                return NotFound($"Patient order administration events for order administration with id {administrationId} were not found");
            }

            return Ok(events);
        }

        /// <summary>
        /// Get the events of an order by order id.
        /// </summary>
        /// <param name="mediaType">
        /// Media type from Accept header.
        /// </param>
        /// <param name="orderId">
        /// Unique order identifier.
        /// </param>
        /// <returns>An ActionResult of IEnumerable collection of type OrderEventDto</returns>
        /// <remarks>
        /// </remarks>
        [HttpGet("{orderId}/events", Name = nameof(GetEvents))]
        [ProducesResponseType(200)] // (OK) - the resource is sent in the response
        [ProducesResponseType(400)] // (bad request) - indicates a bad request (e.g. wrong parameter)
        [ProducesResponseType(404)] // (not found) - the resource does not exits
        [ProducesResponseType(406)] // (not acceptable) - the server does not support the required representation
        public ActionResult<IEnumerable<OrderEventDto>> GetEvents(
            [FromHeader(Name = "Accept")] string mediaType,
            [FromRoute(Name = "orderId")] int orderId
            )
        {
            if (!MediaTypes.IsValidMediaType(mediaType))
            {
                return BadRequest("Unsupported media type header provided.");
            }

            var events = _orderService.GetEvents(orderId);

            if (events == null)
            {
                return NotFound($"Patient order events for order with id {orderId} were not found");
            }

            events = events.Where(@event => @event.AdministrationId != null);

            return Ok(events.OrderBy(@event => @event.SystemDatetime));
        }

        /// <summary>
        /// Get the event by event id of an order by order id.
        /// </summary>
        /// <param name="mediaType">
        /// Media type from Accept header.
        /// </param>
        /// <param name="eventId">
        /// Unique order event identifier.
        /// </param>
        /// <returns>An ActionResult of type OrderEventDto</returns>
        /// <remarks>
        /// </remarks>
        [HttpGet("/events/{eventId}", Name = nameof(GetEvent))]
        [ProducesResponseType(200)] // (OK) - the resource is sent in the response
        [ProducesResponseType(400)] // (bad request) - indicates a bad request (e.g. wrong parameter)
        [ProducesResponseType(404)] // (not found) - the resource does not exits
        [ProducesResponseType(406)] // (not acceptable) - the server does not support the required representation
        public ActionResult<OrderEventDto> GetEvent(
            [FromHeader(Name = "Accept")] string mediaType,
            [FromRoute(Name = "eventId")] int eventId
            )
        {
            if (!MediaTypes.IsValidMediaType(mediaType))
            {
                return BadRequest("Unsupported media type header provided.");
            }

            var @event = _orderService.GetEvent(eventId);

            if (@event == null)
            {
                return NotFound($"Patient order event with id {eventId} was not found");
            }

            return Ok(@event);
        }

        /// <summary>
        /// Executes one of the standards actions against an Order
        /// </summary>
        /// <param name="mediaType">Media type from Accept header.</param>
        /// <param name="siteId">Unique site identifier</param>
        /// <param name="userId">User who clicked the action button</param>
        /// <param name="orderId">ID of the Order to fire the action against</param>
        /// <param name="actionId">ID (from actions.id) of the Action to fire</param>
        /// <returns></returns>
        [HttpPost("{orderId}/actions/{actionId}", Name = nameof(ExecuteOrderAction))]
        [ProducesResponseType(200)] // (OK) - the resource is sent in the response
        [ProducesResponseType(400)] // (bad request) - indicates a bad request (e.g. wrong parameter)
        [ProducesResponseType(404)] // (not found) - the resource does not exits
        [ProducesResponseType(406)] // (not acceptable) - the server does not support the required representation
        public ActionResult<ActionResultDto> ExecuteOrderAction(
            [FromHeader(Name = "Accept")] string mediaType,
            [FromHeader(Name = "EMAR-Site")] int? siteId,
            [FromHeader(Name = "EMAR-User")] int? userId,
            [FromRoute(Name = "orderId")] int orderId,
            [FromRoute(Name = "actionId")] int actionId)
        {
            if (!MediaTypes.IsValidMediaType(mediaType))
                return BadRequest("Unsupported media type header provided.");

            if (siteId == null)
                return BadRequest("Site id is missing.");

            if (userId == null)
                return BadRequest("User id is missing.");

            if (!Enum.IsDefined(typeof(ActionEnum), actionId))
                return BadRequest($"Action id '{actionId}' is invalid.");

            ActionResultDto actionReturn;
            ActionEnum action = (ActionEnum)actionId;

            try
            {
                var hateoasHelper = new HateoasLinkHelper();

                hateoasHelper.SetOrderActionTemplateResultLink(Url.Link(nameof(FileOrderActionTemplateResult),
                    new { orderId = -99, actionId = -98, templateId = -97 }));

                BaseLinkResource resource = new BaseLinkResource
                {
                    LinkExecuteOrderAction = Url.Link(nameof(ExecuteOrderAction), new { orderId = -99, actionId = "-98" }).Replace("-99", "{0}").Replace("-98", "{1}"),
                    LinkExecuteAdministrationAction = Url.Link(nameof(ExecuteAdministrationAction), new { administrationId = -99, actionId = "-98" }).Replace("-99", "{0}").Replace("-98", "{1}"),
                    LinkGetPatientOrder = Url.Link(nameof(OrdersController.GetOrder), new { orderId = -99 }),
                    LinkGetCartOrder = Url.Link(nameof(CartOrdersController.GetCartOrder), new { cartOrderId = -99 }),
                    LinkGetHomeMedication = Url.Link(nameof(HomeMedicationsController.GetHomeMedication), new { homeMedicationId = -99 }),
                };

                actionReturn = _templateService.FireActionAgainstOrder(userId.Value, orderId, action,
                    siteId.Value, hateoasHelper, resource);

                //I've written a method to handle the interactions in TemplateRepository
                //When we delete or cancel an order, it removes both order interaction rows
                //for each of the interactions this order has.
                //The one where this order interacts to another order (Aspirin -> Warfarin)
                //and the one where another other interacts to this order (Warfarin -> Aspirin).
                //We were getting a null patient_order_id in order_interactions for one side of the
                //interaction when cancelling/deleting an order.
                //I've written logic to handle the null value and not cause the API call to error out.
                //But this will prevent the data from getting messed up in the first place.
                //I'm not sure what causes this.  Perhaps it's something related to my change
                //to do target recalculations versus recalculating everything?  I'm not sure.
                //But I've handled removing both sides of all order interactions for the
                //cancelled/deleted medication order in TemplateRepository, and we're good now.
                //So I'm commenting out this call since it's not needed.
                //Winston Murdock, 05/06/2022.  PC-27153
                //// On cancel or delete, recalculate interaction/reaction flags.
                //if (action == ActionEnum.Cancel || action == ActionEnum.Delete)
                //{
                //    var patientId = _orderService.GetOrder(orderId, resource).PatientId;
                //    _orderService.UpdatePatientOrderInteractionsAndReactions(patientId);
                //}
            }
            catch (ArgumentException e)
            {
                return BadRequest(Emar.Core.Helpers.Utilities.ExtractExceptionMessages(e));
            }
            catch (Exception e)
            {
                return Problem(Utilities.ExtractExceptionMessages(e), statusCode: (int)HttpStatusCode.InternalServerError);
            }

            //if (@event == null)
            //{
            //    return NotFound($"Patient order event with id {eventId} was not found");
            //}

            //if (!@event.OrderId.Equals(orderId))
            //{
            //    return NotFound($"Patient order event with id {eventId} is not part of patient order with id {orderId}");
            //}
            if (actionReturn.NewEvent != null)
            {
                var uri = Url.Link(nameof(GetEvent), new { eventId = actionReturn.NewEvent.Id });

                return Created(uri, actionReturn);
            }

            return Ok(actionReturn);
        }

        /// <summary>
        /// Executes one of the standards actions against an Administration
        /// </summary>
        /// <param name="siteId">Unique site identifier</param>
        /// <param name="userId">User who clicked the action button</param>
        /// <param name="administrationId">ID of the Order Administration to fire the action against</param>
        /// <param name="actionId">The Id (from actions.id) of the Action that is being fired</param>
        /// <param name="mediaType">Media type from Accept header.</param>
        /// <returns></returns>
        [HttpPost("administrations/{administrationId}/actions/{actionId}", Name = nameof(ExecuteAdministrationAction))]
        [ProducesResponseType(200)] // (OK) - the resource is sent in the response
        [ProducesResponseType(400)] // (bad request) - indicates a bad request (e.g. wrong parameter)
        [ProducesResponseType(404)] // (not found) - the resource does not exits
        [ProducesResponseType(406)] // (not acceptable) - the server does not support the required representation
        public ActionResult<ActionResultDto> ExecuteAdministrationAction(
            [FromHeader(Name = "Accept")] string mediaType,
            [FromHeader(Name = "EMAR-Site")] int? siteId,
            [FromHeader(Name = "EMAR-User")] int? userId,
            [FromRoute(Name = "administrationId")] int administrationId,
            [FromRoute(Name = "actionId")] int actionId
        )
        {
            if (!MediaTypes.IsValidMediaType(mediaType))
            {
                return BadRequest("Unsupported media type header provided.");
            }

            if (siteId == null)
            {
                return BadRequest("Site id is missing.");
            }

            if (userId == null)
            {
                return BadRequest("User id is missing.");
            }

            if (!Enum.IsDefined(typeof(ActionEnum), actionId))
            {
                return BadRequest($"Action id '{actionId}' is invalid.");
            }

            ActionResultDto actionReturn;

            try
            {
                var hateoasHelper = new HateoasLinkHelper();
                hateoasHelper.SetAdministrationActionTemplateResultLink(
                    Url.Link(nameof(FileAdministrationActionTemplateResult), new { administrationId = -99, actionId = -98, templateId = -97 }));
                BaseLinkResource resource = new BaseLinkResource
                {
                    LinkExecuteOrderAction = Url.Link(nameof(ExecuteOrderAction), new { orderId = -99, actionId = "-98" }).Replace("-99", "{0}").Replace("-98", "{1}"),
                    LinkExecuteAdministrationAction = Url.Link(nameof(ExecuteAdministrationAction), new { administrationId = -99, actionId = "-98" }).Replace("-99", "{0}").Replace("-98", "{1}"),
                    LinkGetPatientOrder = Url.Link(nameof(OrdersController.GetOrder), new { orderId = -99 }),
                    LinkGetCartOrder = Url.Link(nameof(CartOrdersController.GetCartOrder), new { cartOrderId = -99 }),
                    LinkGetHomeMedication = Url.Link(nameof(HomeMedicationsController.GetHomeMedication), new { homeMedicationId = -99 }),
                };

                actionReturn = _templateService.FireActionAgainstAdministration(userId.Value, administrationId,
                    (ActionEnum) actionId, siteId.Value, hateoasHelper, resource);
            }
            catch (ArgumentException e)
            {
                return BadRequest(Utilities.ExtractExceptionMessages(e));
            }
            catch (Exception e)
            {
                return Problem(Utilities.ExtractExceptionMessages(e), statusCode: (int)HttpStatusCode.InternalServerError);
            }

            if (actionReturn.NewEvent != null)
            {
                var uri = Url.Link(nameof(GetEvent), new {eventId = actionReturn.NewEvent.Id});

                return Created(uri, actionReturn);
            }

            return Ok(actionReturn);
        }

        /// <summary>
        /// Executes one of the standards actions against an Order
        /// </summary>
        /// <param name="userId">User who clicked the action button</param>
        /// <param name="siteId">Unique site identifier</param>
        /// <param name="orderId">ID of the Order to the template response against</param>
        /// <param name="actionId">The Id (from actions.id) of the Action that is being fired</param>
        /// <param name="mediaType">Media type from Accept header.</param>
        /// <param name="templateId">ID of the Template the user was filling out</param>
        /// <param name="templateResponses">Data harvested from the user's input to the template</param>
        /// <returns></returns>
        [HttpPost("{orderId}/actions/{actionId}/templates/{templateId}", Name = nameof(FileOrderActionTemplateResult))]
        [ProducesResponseType(200)] // (OK) - the resource is sent in the response
        [ProducesResponseType(400)] // (bad request) - indicates a bad request (e.g. wrong parameter)
        [ProducesResponseType(404)] // (not found) - the resource does not exits
        [ProducesResponseType(406)] // (not acceptable) - the server does not support the required representation
        public ActionResult<ActionResultDto> FileOrderActionTemplateResult(
        [FromHeader(Name = "Accept")] string mediaType,
        [FromHeader(Name = "EMAR-User")] int? userId,
        [FromHeader(Name = "EMAR-Site")] int? siteId,
        [FromRoute(Name = "orderId")] int orderId,
        [FromRoute(Name = "actionId")] int actionId,
        [FromRoute(Name = "templateId")] int templateId,
        [FromBody] Dictionary<string, string> templateResponses
)
        {
            if (!MediaTypes.IsValidMediaType(mediaType))
            {
                return BadRequest("Unsupported media type header provided.");
            }

            if (siteId == null)
            {
                return BadRequest("Site id is missing.");
            }

            if (userId == null)
            {
                return BadRequest("User id is missing.");
            }

            if (!Enum.IsDefined(typeof(ActionEnum), actionId))
            {
                return BadRequest($"Action id '{actionId}' is invalid.");
            }

            ActionResultDto actionReturn;
            try
            {
                BaseLinkResource resource = new BaseLinkResource
                {
                    LinkExecuteOrderAction = Url.Link(nameof(ExecuteOrderAction), new {orderId = -99, actionId = "-98"})
                        .Replace("-99", "{0}").Replace("-98", "{1}"),
                    LinkExecuteAdministrationAction = Url
                        .Link(nameof(ExecuteAdministrationAction), new {administrationId = -99, actionId = "-98"})
                        .Replace("-99", "{0}").Replace("-98", "{1}"),
                    LinkGetPatientOrder = Url.Link(nameof(OrdersController.GetOrder), new {orderId = -99}),
                    LinkGetCartOrder = Url.Link(nameof(CartOrdersController.GetCartOrder), new {cartOrderId = -99}),
                    LinkGetHomeMedication = Url.Link(nameof(HomeMedicationsController.GetHomeMedication),
                        new {homeMedicationId = -99}),
                };
                actionReturn = _templateService.FireActionAgainstOrder(userId.Value, orderId, (ActionEnum) actionId,
                    siteId.Value, null, resource, templateId, templateResponses);
            }
            catch (ArgumentException e)
            {
                return BadRequest(Utilities.ExtractExceptionMessages(e));
            }
            catch (Exception e)
            {
                return Problem(Utilities.ExtractExceptionMessages(e),
                    statusCode: (int) HttpStatusCode.InternalServerError);
            }

            var uri = Url.Link(nameof(GetEvent), new { eventId = actionReturn.NewEvent.Id });

            return Created(uri, actionReturn);
        }

        /// <summary>
        /// Executes one of the standards actions against an Administration
        /// </summary>
        /// <param name="userId">User who clicked the action button</param>
        /// <param name="siteId">Unique site identifier</param>
        /// <param name="administrationId">ID of the Order Administration to fire the action against</param>
        /// <param name="actionId">The Id (from actions.id) of the Action that is being fired</param>
        /// <param name="mediaType">Media type from Accept header.</param>
        /// <param name="templateId">ID of the Template the user was filling out</param>
        /// <param name="templateResponses">Data harvested from the user's input to the template</param>
        /// <returns></returns>
        [HttpPost("administrations/{administrationId}/actions/{actionId}/templates/{templateId}", Name = nameof(FileAdministrationActionTemplateResult))]
        [ProducesResponseType(200)] // (OK) - the resource is sent in the response
        [ProducesResponseType(400)] // (bad request) - indicates a bad request (e.g. wrong parameter)
        [ProducesResponseType(404)] // (not found) - the resource does not exits
        [ProducesResponseType(406)] // (not acceptable) - the server does not support the required representation
        public ActionResult<ActionResultDto> FileAdministrationActionTemplateResult(
            [FromHeader(Name = "Accept")] string mediaType,
            [FromHeader(Name = "EMAR-User")] int? userId,
            [FromHeader(Name = "EMAR-Site")] int? siteId,
            [FromRoute(Name = "administrationId")] int administrationId,
            [FromRoute(Name = "actionId")] int actionId,
            [FromRoute(Name = "templateId")] int templateId,
            [FromBody] Dictionary<string, string> templateResponses
        )
        {
            if (!MediaTypes.IsValidMediaType(mediaType))
            {
                return BadRequest("Unsupported media type header provided.");
            }

            if (siteId == null)
            {
                return BadRequest("Site id is missing.");
            }

            if (userId == null)
            {
                return BadRequest("User id is missing.");
            }

            if (!Enum.IsDefined(typeof(ActionEnum), actionId))
            {
                return BadRequest($"Action id '{actionId}' is invalid.");
            }

            ActionResultDto actionReturn;

            try
            {
                BaseLinkResource resource = new BaseLinkResource
                {
                    LinkExecuteOrderAction = Url.Link(nameof(ExecuteOrderAction), new { orderId = -99, actionId = "-98" }).Replace("-99", "{0}").Replace("-98", "{1}"),
                    LinkExecuteAdministrationAction = Url.Link(nameof(ExecuteAdministrationAction), new { administrationId = -99, actionId = "-98" }).Replace("-99", "{0}").Replace("-98", "{1}"),
                    LinkGetPatientOrder = Url.Link(nameof(OrdersController.GetOrder), new { orderId = -99 }),
                    LinkGetCartOrder = Url.Link(nameof(CartOrdersController.GetCartOrder), new { cartOrderId = -99 }),
                    LinkGetHomeMedication = Url.Link(nameof(HomeMedicationsController.GetHomeMedication), new { homeMedicationId = -99 }),
                };

                actionReturn = _templateService.FireActionAgainstAdministration(userId.Value, administrationId,
                    (ActionEnum)actionId, siteId.Value, null, resource, templateId, templateResponses);
            }
            catch (ArgumentException e)
            {
                return BadRequest(Utilities.ExtractExceptionMessages(e));
            }
            catch (Exception e)
            {
                return Problem(Utilities.ExtractExceptionMessages(e), statusCode: (int)HttpStatusCode.InternalServerError);
            }

            var uri = Url.Link(nameof(GetEvent), new { eventId = actionReturn.NewEvent.Id });

            return Created(uri, actionReturn);
        }

        private string CreateOrdersResourceUri(PageResource pageResource, ResourceUriType type)
        {
            switch (type)
            {
                case ResourceUriType.PreviousPage:
                    {
                        pageResource.PageNumber -= 1;
                        return Url.Link(nameof(GetOrders), pageResource);
                    }
                case ResourceUriType.NextPage:
                    {
                        pageResource.PageNumber += 1;
                        return Url.Link(nameof(GetOrders), pageResource);
                    }
                default:
                    {
                        return Url.Link(nameof(GetOrders), pageResource);
                    }
            }
        }

        private IEnumerable<HateOasLinkDto> CreateHateOasLinksForOrder(long? orderId)
        {
            return new List<HateOasLinkDto>
            {
                new HateOasLinkDto(Url.Link(nameof(GetOrder), new {orderId}),
                    "get_order",
                    "GET")
            };
        }

        private IEnumerable<HateOasLinkDto> CreateHateOasLinksForOrders([FromQuery] PageResource pageResource, bool hasNext, bool hasPrevious)
        {
            var links = new List<HateOasLinkDto>
        {
                new HateOasLinkDto(CreateOrdersResourceUri(pageResource, ResourceUriType.Current),
                    "self",
                    "GET")
            };

            if (hasNext)
            {
                links.Add(
                    new HateOasLinkDto(CreateOrdersResourceUri(pageResource, ResourceUriType.NextPage),
                    "nextPage",
                    "GET"));
            }

            if (hasPrevious)
            {
                links.Add(
                    new HateOasLinkDto(CreateOrdersResourceUri(pageResource, ResourceUriType.PreviousPage),
                    "previousPage",
                    "GET"));
            }

            return links;
        }
    }
}