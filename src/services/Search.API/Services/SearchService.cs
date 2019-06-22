using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Nest;
using Serilog;

namespace Search.API.Services
{
    public class SearchService : ISearchService<ProductSearchType>
    {
        private readonly ElasticClient _client;

        public SearchService(IConfiguration configuration)
        {
            var elasticSearchEndpoint = configuration.GetValue<string>("ElasticsearchEndpoint");
            var node = new Uri(elasticSearchEndpoint);
            var settings = new ConnectionSettings(node);
            settings.DefaultIndex("fulltextsearch");
            _client = new ElasticClient(settings);
        }

        public async Task<SearchResult<ProductSearchType>> Search(string query, int page = 1, int pageSize = 50)
        {
            Log.ForContext<SearchService>().Information("Search => {query}", query);
            var result = await _client.SearchAsync<ProductSearchType>(x => x
                .Query(q => q
                    .MultiMatch(mp => mp
                        .Query(query)
                        .Fields(f => f
                            .Fields(f1 => f1.Brand, f2 => f2.Details, f3 => f3.Name))))
                .From(page - 1)
                .Size(pageSize));

            return new SearchResult<ProductSearchType>
            {
                Total = (int)result.Total,
                Page = page,
                Results = result.Documents,
                ElapsedMilliseconds = (int)result.Took
            };
        }

        public async Task<bool> AddNewRecord(ProductSearchType record)
        {
            Log.ForContext<SearchService>().Information("AddNewRecord => {@record}", record);
            var asyncIndexResponse = await _client.IndexDocumentAsync(record);
            return asyncIndexResponse.IsValid;
        }
    }

}
