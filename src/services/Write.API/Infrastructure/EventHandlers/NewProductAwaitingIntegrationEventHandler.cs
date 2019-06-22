using System.Threading.Tasks;
using DataAccessLayer.Contexts.Models;
using DataAccessLayer.Contexts.Repository;
using MessageQueueProvider;
using Serilog;
using Write.API.Infrastructure.Events;

namespace Write.API.Infrastructure.EventHandlers
{
    public class NewProductAwaitingIntegrationEventHandler : IIntegrationEventHandler<NewProductAwaitingIntegrationEvent>
    {
        private readonly IEventBus _eventBus;
        private readonly ProductRepository _productRepository;

        public NewProductAwaitingIntegrationEventHandler(ProductRepository productRepository, IEventBus eventBus)
        {
            _productRepository = productRepository;
            _eventBus = eventBus;
        }

        public async Task Handle(NewProductAwaitingIntegrationEvent @event)
        {
            Log.ForContext<NewProductAwaitingIntegrationEventHandler>().Information("Handle => {@event}", @event);
            var product = new Product
            {
                Price = @event.Price,
                Brand = @event.Brand,
                Details = @event.Details,
                Id = @event.ProductId,
                Name = @event.Name
            };
            await _productRepository.Add(product);

            //add to text search after db insert
            var newProductAdded = new NewProductAddedIntegrationEvent
            {
                Price = @event.Price,
                Brand = @event.Brand,
                Details = @event.Details,
                Name = @event.Name
            };
            _eventBus.Publish(newProductAdded);
        }
    }
}
