using DataAccessLayer.Contexts.Models;
using DataAccessLayer.Contexts.Repository;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Threading.Tasks;

namespace Write.API.Controllers
{
    [Produces("application/json")]
    [Route("api")]
    [ApiController]
    public class WriteController : ControllerBase
    {
        private readonly ProductRepository _productRepository;

        public WriteController(ProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        [HttpPost]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public async Task<ActionResult> Post([FromBody] Product product)
        {
            await _productRepository.Add(product);

            return Ok();
        }
    }
}
