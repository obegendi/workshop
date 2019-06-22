using Nest;

namespace Search.API.Services
{
    [ElasticsearchType(Name = "product")]
    public class ProductSearchType
    {
        [Text] public string Name { get; set; }
        [Text] public string Brand { get; set; }
        [Text] public string Details { get; set; }
    }

}
