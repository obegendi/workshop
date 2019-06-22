using System.Collections.Generic;
using System.Threading.Tasks;
using Product.API.Dto;

namespace Product.API.Infrastructure.ApiClient
{
    public interface ISearchApiClient
    {
        Task<List<ProductDto>> GetSearch(string query);
    }
}
