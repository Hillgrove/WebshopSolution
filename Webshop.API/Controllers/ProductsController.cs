using Microsoft.AspNetCore.Mvc;
using Webshop.Data;
using Webshop.Data.Models;


namespace Webshop.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly IProductRepository _productRepository;

        public ProductsController(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        // GET: api/<UsersController>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<Product>>> Get()
        {
            IEnumerable<Product> products = await _productRepository.GetAllAsync();
            return Ok(products);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Product?>> Get(int id)
        {
            Product? foundProduct = await _productRepository.GetByIdAsync(id);
            if (foundProduct is null)
            {
                return NotFound();
            }

            return Ok(foundProduct);
        }
    }
}
