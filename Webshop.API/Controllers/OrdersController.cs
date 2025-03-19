using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
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
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<Order>>> GetUserOrders()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return Unauthorized("User not logged in.");
            }

            var orders = await _orderRepository.GetOrdersByUserIdAsync(userId.Value);
            if (orders == null || orders.Count == 0)
            {
                return NotFound("No orders found.");
            }

            return Ok(orders);
        }
    }
}
