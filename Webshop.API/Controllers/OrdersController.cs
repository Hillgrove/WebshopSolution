using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using Webshop.API.Attributes;
using Webshop.Data;
using Webshop.Data.Models;

namespace Webshop.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly OrderRepositorySQLite _orderRepository;

        public OrdersController(OrderRepositorySQLite orderRepository)
        {
            _orderRepository = orderRepository;
        }

        [HttpGet("my-orders")]
        [SessionAuthorize(Roles = new[] { "Customer" })]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<IEnumerable<Order>>> GetUserOrders()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return Unauthorized("User not logged in.");
            }

            var orders = await _orderRepository.GetOrdersByUserIdAsync(userId.Value);
            return Ok(orders);
        }
    }
}
