using MessageQueueProvider;

namespace Product.API.Infrastructure.Events
{
    public class NewProductAwaitingIntegrationEvent : IntegrationEvent
    {
        public int ProductId { get; set; }
        public string Name { get; set; }
        public string Brand { get; set; }
        public decimal Price { get; set; }
        public string Details { get; set; }
    }
}
