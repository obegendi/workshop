using System.Threading.Tasks;
using Product.API.Dto;

namespace Product.API.Infrastructure.ApiClient
{
    public interface IReadApiClient
    {
        Task<ProductDto> GetProduct(int id);
    }
}
