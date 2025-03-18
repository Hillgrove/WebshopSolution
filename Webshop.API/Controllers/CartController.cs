using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using Webshop.Data;
using Webshop.Data.Models;
using Webshop.Shared.DTOs;



namespace Webshop.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CartController : ControllerBase
    {
        private const string SessionkeyCart = "ShoppingCart";
        private readonly IProductRepository _productRepository;
        private readonly OrderRepositorySQLite _orderRepository;
        private readonly IUserRepository _userRepository;

        public CartController(
            IProductRepository productRepository, 
            OrderRepositorySQLite orderRepository, 
            IUserRepository userRepository)
        {
            _productRepository = productRepository;
            _orderRepository = orderRepository;
            _userRepository = userRepository;
        }


        [HttpPost("add")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AddToCart([FromBody] int productId)
        {
            var cart = GetCart();

            // Fetch product details from DB
            var product = await _productRepository.GetByIdAsync(productId);
            if (product == null)
            {
                return NotFound("Product not found.");
            }

            var existingItem = cart.FirstOrDefault(i => i.ProductId == productId);
            if (existingItem != null)
            {
                existingItem.Quantity++;
            }

            else
            {
                cart.Add(new CartItem
                {
                    ProductId = product.Id,
                    ProductName = product.Name,
                    Quantity = 1,
                    PriceInOere = product.PriceInOere
                });
            }

            SaveCart(cart);
            return Ok(cart);
        }


        [HttpPost("update")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult UpdateCartItem([FromBody] CartUpdateDto updateDto)
        {
            var cart = GetCart();
            var item = cart.FirstOrDefault(i => i.ProductId == updateDto.ProductId);

            if (item == null)
            {
                return NotFound();
            }

            item.Quantity += updateDto.Delta;
            if (item.Quantity <= 0)
            {
                cart.Remove(item);
            }

            SaveCart(cart);
            return Ok(cart);
        }


        [HttpPost("remove")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult RemoveFromCart([FromBody] int productId)
        {
            var cart = GetCart();
            cart.RemoveAll(p => p.ProductId == productId);
            SaveCart(cart);
            return Ok(cart);
        }


        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetCartItems()
        {
            return Ok(GetCart());
        }


        [HttpPost("checkout")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Checkout()
        {
            var cart = GetCart();
            if (cart.Count == 0) return BadRequest("Cart is empty.");

            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return Unauthorized("User not logged in.");
            }

            var user = await _userRepository.GetByIdAsync(userId.Value);
            if (user == null)
            {
                return Unauthorized("User not found");
            }

            int totalPriceInOere = cart.Sum(item => item.Quantity * item.PriceInOere);

            var order = new Order
            {
                UserId = user.Id,
                Items = cart.Select(item => new OrderItem
                {
                    ProductId = item.ProductId,
                    ProductName = item.ProductName,
                    Quantity = item.Quantity,
                    PriceInOere = item.PriceInOere
                }).ToList(),
                TotalPriceInOere = totalPriceInOere
            };

            await _orderRepository.SaveAsync(order);

            HttpContext.Session.Remove("ShoppingCart"); // Clear cart after purchase

            return Ok(new { message = "Order placed successfully", total = totalPriceInOere });
        }


        #region Private Methods
        private List<CartItem> GetCart()
        {
            var jsonCart = HttpContext.Session.GetString(SessionkeyCart);

            if (jsonCart == null)
            {
                return new List<CartItem>();
            }

            var cart = JsonSerializer.Deserialize<List<CartItem>>(jsonCart);

            return cart ?? new List<CartItem>();
        }

        private void SaveCart(List<CartItem> cart)
        {
            HttpContext.Session.SetString(SessionkeyCart, JsonSerializer.Serialize(cart));
        }
        #endregion
    }
}
