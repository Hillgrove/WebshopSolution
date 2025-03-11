using Microsoft.AspNetCore.Mvc;
using Webshop.API.Extensions;
using Webshop.Data.Models;



namespace Webshop.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CartsController : ControllerBase
    {
        private const string SessionkeyCart = "ShoppingCart";

        [HttpPost("add")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult AddToCart([FromBody] CartItem newItem)
        {
            var cart = GetCart();

            var existingItem = cart.FirstOrDefault(p => p.ProductId == newItem.ProductId);
            if (existingItem != null)
            {
                existingItem.Quantity = newItem.Quantity;
            }
            else
            {
                cart.Add(newItem);
            }

            SaveCart(cart);
            return Ok(cart);
        }


        [HttpPost("update")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult UpdateCartItem([FromBody] CartItem updatedItem)
        {
            var cart = GetCart();
            var item = cart.FirstOrDefault(i => i.ProductId == updatedItem.ProductId);

            if (item == null)
            {
                return NotFound();
            }

            if (updatedItem.Quantity > 0)
            {
                item.Quantity = updatedItem.Quantity;
            }

            else
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
