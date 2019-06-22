using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using CacheLayer;
using MessageQueueProvider;
using Microsoft.AspNetCore.Mvc;
using Product.API.Dto;
using Product.API.Infrastructure.ApiClient;
using Product.API.Infrastructure.Events;

namespace Product.API.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly IEventBus _eventBus;
        private readonly IReadApiClient _readApiClient;
        private readonly ISearchApiClient _searchApiClient;

        public ProductsController(IEventBus eventBus, ISearchApiClient searchApiClient, IReadApiClient readApiClient)
        {
            _eventBus = eventBus;
            _searchApiClient = searchApiClient;
            _readApiClient = readApiClient;
        }

        // GET api/values
        [HttpGet("")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<ActionResult<IEnumerable<object>>> Get([FromQuery] string query)
        {
            if (query.Length < 3)
                return BadRequest("Length must be greater than 3 chars");
            var result = await _searchApiClient.GetSearch(query);
            if (result.Any())
                return Ok(result);
            return NoContent();

        }

        [HttpGet("{id}")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<ActionResult<ProductDto>> Get([FromRoute] int id)
        {
            if (id == 0)
            {
                return Conflict();
            }
            var product = LazyRedis.Get<ProductDto>(id.ToString());
            if (product == null)
            {
                var result = await _readApiClient.GetProduct(id);
                if (result.ProductId == 0)
                    return NotFound("Product not found");
                LazyRedis.Add($"Product_{id}", result, TimeSpan.FromMinutes(30));
                return Ok(result);
            }
            else
            {
                return Ok(product);
            }
        }

        [HttpPost]
        [ProducesResponseType((int)HttpStatusCode.Accepted)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<ActionResult> Post([FromBody] ProductDto value)
        {
            if (value is null)
                return BadRequest();
            var @event = new NewProductAwaitingIntegrationEvent
            {
                Brand = value.Brand,
                Details = value.Details,
                Name = value.Name,
                Price = value.Price,
                ProductId = value.ProductId
            };
            _eventBus.Publish(@event);
            return Accepted();
        }
    }
}
