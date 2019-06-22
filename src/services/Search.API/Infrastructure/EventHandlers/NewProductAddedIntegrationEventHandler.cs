using System.Threading.Tasks;
using MessageQueueProvider;
using Search.API.Infrastructure.Events;
using Search.API.Services;
using Serilog;

namespace Search.API.Infrastructure.EventHandlers
{
    public class NewProductAddedIntegrationEventHandler : IIntegrationEventHandler<NewProductAddedIntegrationEvent>
    {
        private readonly SearchService _searchService;

        public NewProductAddedIntegrationEventHandler(SearchService searchService)
        {
            _searchService = searchService;
        }
        public async Task Handle(NewProductAddedIntegrationEvent @event)
        {
            Log.ForContext<NewProductAddedIntegrationEventHandler>().Information("Handle => {@event}", @event);
            var searchRecord = new ProductSearchType
            {
                Name = @event.Name,
                Details = @event.Details,
                Brand = @event.Brand
            };
            var result = await _searchService.AddNewRecord(searchRecord);
            Log.ForContext<NewProductAddedIntegrationEventHandler>().Debug("Add new record is {result}", result);
        }
    }
}
