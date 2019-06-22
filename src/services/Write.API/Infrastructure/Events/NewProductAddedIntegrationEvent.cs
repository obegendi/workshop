using MessageQueueProvider;

namespace Write.API.Infrastructure.Events
{
    public class NewProductAddedIntegrationEvent : IntegrationEvent
    {
        public string Name { get; set; }
        public string Brand { get; set; }
        public decimal Price { get; set; }
        public string Details { get; set; }
    }
}
