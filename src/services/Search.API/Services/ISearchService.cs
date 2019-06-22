using System.Threading.Tasks;

namespace Search.API.Services
{
    public interface ISearchService<T>
    {
        Task<SearchResult<T>> Search(string query, int page = 1, int pageSize = 50);
        Task<bool> AddNewRecord(T record);
    }
}
