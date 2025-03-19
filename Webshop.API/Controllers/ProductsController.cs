using Microsoft.AspNetCore.Mvc;

using Webshop.Data.Models;
using Webshop.Data;
using Webshop.Services;


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

        // GET: api/<ProductsController>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<Product>>> Get()
        {
            IEnumerable<Product> products = await _productRepository.GetAllAsync();
            return Ok(products);
        }


        // GET api/<ProductsController>/5
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Product>> Get(int id)
        {
            Product? foundProduct = await _productRepository.GetByIdAsync(id);
            if (foundProduct == null)
            {
                return NotFound();
            }

            return Ok(foundProduct);
        }

        //// POST api/<ProductsController>
        //[ProducesResponseType(StatusCodes.Status201Created)]
        //[ProducesResponseType(StatusCodes.Status400BadRequest)]
        //[HttpPost]
        //public IActionResult Post([FromBody] Product product)
        //{
        //    if (product.Id == null)
        //    {
        //        return BadRequest("Value cannot be null or empty.");
        //    }
        //    _productRepository.AddAsync(product);
        //    return Created("/" + product.Id, product);
        //}

        //// PUT api/<ProductsController>/5
        //[ProducesResponseType(StatusCodes.Status204NoContent)]
        //[ProducesResponseType(StatusCodes.Status400BadRequest)]
        //[ProducesResponseType(StatusCodes.Status404NotFound)]
        //[HttpPut("{id}")]
        //public async Task<IActionResult> Put(int id, [FromBody] Product product)
        //{
        //    if (product == null || product.Id != id)
        //    {
        //        return BadRequest("Object is null or ID mismatch.");
        //    }

        //    await _productRepository.UpdateAsync(product);

        //    if (product == null)
        //    {
        //        return NotFound($"Product with ID {id} not found.");
        //    }

        //    //Return NoContent(204) when the update is successful
        //    return NoContent();
        //}

        //// DELETE api/<ProductsController>/5
        //[ProducesResponseType(StatusCodes.Status200OK)]
        //[ProducesResponseType(StatusCodes.Status400BadRequest)]
        //[ProducesResponseType(StatusCodes.Status404NotFound)]
        //[HttpDelete("{id}")]
        //public async Task<IActionResult> Delete(int id)
        //{
        //    if (id != null)
        //    {
        //        Product? product = await _productRepository.GetByIdAsync(id);
        //        if (product.Id == 0)
        //        {
        //            return NotFound($"Product with ID {id} not found.");
        //        }
        //        _productRepository.Delete(product);
        //        return Ok(product);
        //    }
        //    return BadRequest("Id cannot be null.");
        //}
    }
}
