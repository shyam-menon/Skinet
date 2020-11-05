using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using API.Dtos;
using API.Errors;
using API.Extensions;
using AutoMapper;
using Core.Entities.OrderAggregate;
using Core.Interfaces;
using Infrastructure.Events;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace API.Controllers
{
    //Orders controller. Course item 214
    [Authorize]
    public class OrdersController : BaseApiController
    {
        private readonly IOrderService _orderService;
        private readonly IMapper _mapper;
        private readonly ILogger _logger;

        public OrdersController(IOrderService orderService, IMapper mapper, ILoggerFactory loggerFactory)
        {
            _mapper = mapper;
            _orderService = orderService;
            _logger = loggerFactory.CreateLogger("Orders");
        }

        //Create the order. Course item 214
        [HttpPost]
        public async Task<ActionResult<Order>> CreateOrder(OrderDto orderDto)
        {
            using (_logger.BeginScope("Constructing a new order for {First Name} : {LastName})"
                   , orderDto.ShipToAddress.FirstName, orderDto.ShipToAddress.LastName))
            {
                _logger.LogInformation("API ENTRY: Creating a new order");

                var email = HttpContext.User.RetrieveEmailFromPrincipal();

                var address = _mapper.Map<AddressDto, Address>(orderDto.ShipToAddress);

                var order = await _orderService.CreateOrderAsync(email, orderDto.DeliveryMethodId,
                orderDto.BasketId, address);

                if (order == null) return BadRequest(new ApiResponse(400, "Problem creating order"));

                return Ok(order);
            }           
        }

        //Get the order. Course item 222
        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<OrderDto>>> GetOrdersForUser()
        {
            _logger.LogInformation("API ENTRY: Inside get orders for user.");

            var email = HttpContext.User.RetrieveEmailFromPrincipal();

            var orders = await _orderService.GetOrdersForUserAsync(email);

            return Ok(_mapper.Map<IReadOnlyList<Order>,IReadOnlyList<OrderToReturnDto>>(orders));
        }

        //Get an individual order
        [HttpGet("{id}")]
        public async Task<ActionResult<OrderToReturnDto>> GetOrderByIdForUser(int id)
        {
            var email = HttpContext.User.RetrieveEmailFromPrincipal();

            var order = await _orderService.GetOrderByIdAsync(id, email);

            if (order == null) return NotFound(new ApiResponse(404));

            return _mapper.Map<Order, OrderToReturnDto>(order);
        }

        [HttpGet("deliveryMethods")]
        public async Task<ActionResult<IReadOnlyList<DeliveryMethod>>> GetDeliveryMethods()
        {
            return Ok(await _orderService.GetDeliveryMethodsAsync());
        }
    }
}