using Microsoft.AspNetCore.Mvc;
using Webshop.API.Extensions;
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

        public CartController(IProductRepository productRepository)
        {
            _productRepository = productRepository;
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

            cart.Add(new CartItem
            {
                ProductId = product.Id,
                ProductName = product.Name,
                Quantity = 1,
                PriceInOere = (int)(product.Price * 100)
            });

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
        public IActionResult RemoveFromCart([FromBody] int productId)
        {
            var cart = GetCart();
            cart.RemoveAll(p => p.ProductId == productId);
            SaveCart(cart);
            return Ok(cart);
        }


        [HttpGet]
        public IActionResult GetCartItems()
        {
            return Ok(GetCart());
        }


        //[HttpPost("checkout")]
        //public async Task<IActionResult> Checkout()
        //{
        //    var cart = GetCart();
        //    if (!cart.Any()) return BadRequest("Cart is empty.");

        //    decimal totalPrice = 0;
        //    var validatedItems = new List<OrderItem>();

        //    foreach (var cartItem in cart)
        //    {
        //        var product = await _productRepository.GetByIdAsync(cartItem.ProductId);
        //        if (product == null) continue; // Skip missing products

        //        int finalPrice = (int)(product.Price * 100); // Backend-verified price
        //        totalPrice += finalPrice * cartItem.Quantity;

        //        validatedItems.Add(new OrderItem
        //        {
        //            ProductId = product.Id,
        //            ProductName = product.Name,
        //            Quantity = cartItem.Quantity,
        //            PriceInOere = finalPrice
        //        });
        //    }

        //    var order = new Order
        //    {
        //        Items = validatedItems,
        //        TotalPrice = totalPrice,
        //        CreatedAt = DateTime.UtcNow
        //    };

        //    await _orderRepository.SaveAsync(order);

        //    HttpContext.Session.Remove("ShoppingCart"); // Clear cart after purchase

        //    return Ok(new { message = "Order placed successfully", total = totalPrice / 100.0m });
        //}



        #region Private Methods
        private List<CartItem> GetCart()
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>(SessionkeyCart);
            return cart ?? new List<CartItem>();
        }

        private void SaveCart(List<CartItem> cart)
        {
            HttpContext.Session.SetObjectAsJson(SessionkeyCart, cart);
        }
        #endregion
    }
}
