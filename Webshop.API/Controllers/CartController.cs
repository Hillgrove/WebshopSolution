using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using Webshop.API.Attributes;
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

        public CartController(
            IProductRepository productRepository, 
            OrderRepositorySQLite orderRepository)
        {
            _productRepository = productRepository;
            _orderRepository = orderRepository;
        }

        
        [HttpPost("add")]
        [SessionAuthorize(Roles = new[] { "Guest", "Customer" })]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AddToCart([FromBody] AddToCartDto dto)
        {
            if (dto.Quantity < 1)
            {
                return BadRequest("Quantity must be at least 1.");
            }

            var cart = GetCart();

            // Fetch product details from DB
            var product = await _productRepository.GetByIdAsync(dto.ProductId);
            if (product == null)
            {
                return NotFound("Product not found.");
            }

            var existingItem = cart.FirstOrDefault(i => i.ProductId == dto.ProductId);
            if (existingItem != null)
            {
                existingItem.Quantity += dto.Quantity;
            }

            else
            {
                cart.Add(new CartItem
                {
                    ProductId = product.Id,
                    ProductName = product.Name,
                    Quantity = dto.Quantity,
                    PriceInOere = product.PriceInOere
                });
            }

            SaveCart(cart);
            return Ok(cart);
        }


        [HttpPut("{productId}")]
        [SessionAuthorize(Roles = new[] { "Guest", "Customer" })]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult UpdateCartItem(int productId, [FromBody] CartUpdateDto updateDto)
        {
            var cart = GetCart();
            var item = cart.FirstOrDefault(i => i.ProductId == productId);

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


        [HttpDelete("{productId}")]
        [SessionAuthorize(Roles = new[] { "Guest", "Customer" })]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult RemoveFromCart(int productId)
        {
            var cart = GetCart();
            cart.RemoveAll(p => p.ProductId == productId);
            SaveCart(cart);
            return Ok(cart);
        }


        [HttpGet]
        [SessionAuthorize(Roles = new[] { "Guest", "Customer" })]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetCartItems()
        {
            return Ok(GetCart());
        }


        [HttpPost("checkout")]
        [SessionAuthorize(Roles = new[] { "Guest", "Customer" })]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Checkout()
        {
            var cart = GetCart();
            if (cart.Count == 0)
            {
                return BadRequest("Cart is empty.");
            }
            

            int userId = HttpContext.Session.GetInt32("UserId") ?? -1;

            var order = new Order
            {
                UserId = userId,
                Items = cart.Select(item => new OrderItem
                {
                    ProductId = item.ProductId,
                    ProductName = item.ProductName,
                    Quantity = item.Quantity,
                    PriceAtPurchaseInOere = item.PriceInOere
                }).ToList(),
                TotalPriceInOere = cart.Sum(item => item.Quantity * item.PriceInOere),
                CreatedAt = DateTime.UtcNow
            };

            await _orderRepository.SaveAsync(order);

            HttpContext.Session.Remove("ShoppingCart"); // Clear cart after purchase

            return Ok(new { message = "Order placed successfully", total = order.TotalPriceInOere });
        }


        #region Private Methods
        private List<CartItem> GetCart()
        {
            var jsonCart = HttpContext.Session.GetString(SessionkeyCart);

            if (string.IsNullOrEmpty(jsonCart))
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
