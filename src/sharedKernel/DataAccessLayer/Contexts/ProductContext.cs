using DataAccessLayer.Contexts.Models;
using DataAccessLayer.Contexts.Repository;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace DataAccessLayer.Contexts
{
    public class ProductContext
    {
        private readonly IMongoDatabase _database;

        public ProductContext(IOptions<Settings> settings)
        {
            var client = new MongoClient(settings.Value.ConnectionString);
            if (client != null)
                _database = client.GetDatabase(settings.Value.Database);
        }

        public IMongoCollection<Product> Products => _database.GetCollection<Product>("Product");
    }
}
