using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Product.API.Dto;

namespace Product.API.Infrastructure.ApiClient
{
    public class SearchApiClient : ISearchApiClient
    {
        private readonly HttpClient _client;
        public SearchApiClient(IHttpClientFactory httpClientFactory)
        {
            _client = httpClientFactory.CreateClient("SearchAPI");
        }

        public async Task<List<ProductDto>> GetSearch(string query)
        {
            var data = await _client.GetAsync($"api/search?query={query}");
            if (data.IsSuccessStatusCode)
            {
                var contentAsString = await data.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<List<ProductDto>>(contentAsString);
            }
            return new List<ProductDto>();
        }
    }

}
