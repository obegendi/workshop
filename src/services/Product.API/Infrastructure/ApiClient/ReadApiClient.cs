using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Product.API.Dto;

namespace Product.API.Infrastructure.ApiClient
{
    public class ReadApiClient : IReadApiClient
    {
        private readonly HttpClient _client;
        public ReadApiClient(IHttpClientFactory httpClientFactory)
        {
            _client = httpClientFactory.CreateClient("ReadAPI");

        }

        /// <inheritdoc />
        public async Task<ProductDto> GetProduct(int id)
        {
            var data = await _client.GetAsync($"api/read/{id}");
            if (data.IsSuccessStatusCode)
            {
                var contentAsString = await data.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<ProductDto>(contentAsString);
            }
            return new ProductDto();
        }
    }
}
