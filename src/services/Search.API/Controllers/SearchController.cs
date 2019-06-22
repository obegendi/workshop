using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Search.API.Services;

namespace Search.API.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    [ApiController]
    public class SearchController : ControllerBase
    {
        private readonly SearchService _searchService;

        public SearchController(SearchService searchService)
        {
            _searchService = searchService;
        }

        // GET api/values
        [HttpGet]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<ActionResult<IEnumerable<ProductSearchType>>> Get([FromQuery] string query)
        {
            var result = await _searchService.Search(query);
            if (result.Results.Any())
                return Ok(result.Results);
            return NotFound("No result with this query");
        }

        // POST api/values
        [HttpPost]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public async Task<ActionResult> Post([FromBody] ProductSearchType value)
        {
            var result = await _searchService.AddNewRecord(value);
            return Ok(result);
        }
    }
}
