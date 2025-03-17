using Microsoft.AspNetCore.Mvc;
using Webshop.Data.Models;
using Webshop.Data;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Webshop.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ShoppingcartsController : ControllerBase
    {
        private readonly ShoppingcartRepositoryDict _cart;

        // Constructor with Dependency Injection for the cart repository
        public ShoppingcartsController(ShoppingcartRepositoryDict cart)
        {
            _cart = cart;
        }

        // GET: api/<ShoppingcartsController>
        [HttpGet]
        public async Task<ActionResult<Dictionary<int, (Product product, int Quantity)>>> Get()
        {
            var userEmail = HttpContext.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(userEmail))
            {
                return Unauthorized("User not logged in.");
            }

            var products = await _cart.GetAllAsync(userEmail);
            if (products == null || !products.Any())
            {
                return NotFound("Shopping cart is empty.");
            }

            return Ok(products);
        }

        // POST api/<ShoppingcartsController>
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] ProductDto product)
        {
            if (productDto == null || productDto.Quantity <= 0)
            {
                return BadRequest("Invalid product data.");
            }

            var userEmail = HttpContext.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(userEmail))
            {
                return Unauthorized("User not logged in.");
            }

            var product = new Product
            {
                Id = productDto.Id,
                Name = productDto.Name,
                Price = productDto.Price
            };

            await _cart.AddProductAsync(userEmail, product, productDto.Quantity);

            return Ok("Product added to cart.");
        }

        // DELETE api/<ShoppingcartsController>/5
        [HttpDelete("{id}/{quantity}")]
        public async Task<IActionResult> Delete(int id, int quantity)
        {
            var userEmail = HttpContext.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(userEmail))
            {
                return Unauthorized("User not logged in.");
            }

            var productToRemove = (await _cart.GetAllAsync(userEmail)).GetValueOrDefault(id).Product;
            if (productToRemove == null)
            {
                return NotFound("Product not found in cart.");
            }

            await _cart.RemoveProductAsync(userEmail, productToRemove, quantity);
            return Ok("Product removed.");
        }
    }
}
