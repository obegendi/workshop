using System.Net;
using DataAccessLayer.Contexts.Repository;
using Microsoft.AspNetCore.Mvc;

namespace Read.API.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    [ApiController]
    public class ReadController : ControllerBase
    {
        private readonly ProductRepository _productRepository;

        public ReadController(ProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        // GET api/values/5
        [HttpGet("{id}")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public ActionResult<string> Get(int id)
        {
            var result = _productRepository.Get(id);
            if (result == null)
                return NotFound("");
            return Ok(result);
        }
    }
}
