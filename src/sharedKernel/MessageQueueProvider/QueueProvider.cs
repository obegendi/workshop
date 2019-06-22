using System;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using Newtonsoft.Json;
using Polly;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Serilog;

namespace MessageQueueProvider
{
    public class QueueProvider : IEventBus, IDisposable
    {
        private const string BROKER_NAME = "event_bus";
        private readonly ILifetimeScope _autofac;
        private readonly IPersistentConnection _persistentConnection;
        private readonly string _queueName;
        private readonly int _retryCount;
        private readonly ISubscriptionManager _subsManager;

        private IModel _consumerChannel;

        public QueueProvider(
            IPersistentConnection persistentConnection,
            ISubscriptionManager subsManager,
            ILifetimeScope autofac,
            string queueName = null,
            int retryCount = 5)
        {
            _persistentConnection = persistentConnection ?? throw new ArgumentNullException(nameof(persistentConnection));
            _subsManager = subsManager;
            _autofac = autofac;
            _queueName = queueName;
            _consumerChannel = CreateConsumerChannel();
            _retryCount = retryCount;
            //_subsManager.OnEventRemoved += SubsManager_OnEventRemoved;
        }

        public void Dispose()
        {
            if (_consumerChannel != null)
                _consumerChannel.Dispose();

            _subsManager.Clear();
        }

        public void Publish(IntegrationEvent @event)
        {
            if (!_persistentConnection.IsConnected)
                _persistentConnection.TryConnect();

            var policy = Policy.Handle<Exception>()
                .WaitAndRetry(_retryCount, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                    (ex, time) => { Log.ForContext<QueueProvider>().Warning(ex, "{@ex}"); });

            using (var channel = _persistentConnection.CreateModel())
            {
                var eventName = @event.GetType()
                    .Name;

                channel.ExchangeDeclare(BROKER_NAME,
                    "direct");

                var message = JsonConvert.SerializeObject(@event);
                var body = Encoding.UTF8.GetBytes(message);

                policy.Execute(() =>
                {
                    var properties = channel.CreateBasicProperties();
                    properties.DeliveryMode = 2;
                    channel.BasicPublish(BROKER_NAME,
                        eventName,
                        true,
                        properties,
                        body);
                });
            }
        }

        public void Subscribe<T, TH>()
            where T : IntegrationEvent
            where TH : IIntegrationEventHandler<T>
        {
            var eventName = _subsManager.GetEventKey<T>();
            Subscription(eventName);
            _subsManager.AddSubscription<T, TH>();
        }

        private void Subscription(string eventName)
        {
            var containsKey = _subsManager.HasSubscriptionsForEvent(eventName);
            if (!containsKey)
            {
                if (!_persistentConnection.IsConnected)
                    _persistentConnection.TryConnect();

                using (var channel = _persistentConnection.CreateModel())
                {
                    channel.QueueBind(_queueName,
                        BROKER_NAME,
                        eventName);
                }
            }
        }

        private IModel CreateConsumerChannel()
        {
            if (!_persistentConnection.IsConnected)
                _persistentConnection.TryConnect();

            var channel = _persistentConnection.CreateModel();

            channel.ExchangeDeclare(BROKER_NAME,
                "direct");

            channel.QueueDeclare(_queueName,
                true,
                false,
                false,
                null);


            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += async (model, ea) =>
            {
                var eventName = ea.RoutingKey;
                var message = Encoding.UTF8.GetString(ea.Body);

                await ProcessEvent(eventName, message);

                channel.BasicAck(ea.DeliveryTag, false);
            };

            channel.BasicConsume(_queueName,
                false,
                consumer);

            channel.CallbackException += (sender, ea) =>
            {
                _consumerChannel.Dispose();
                _consumerChannel = CreateConsumerChannel();
            };

            return channel;
        }

        private async Task ProcessEvent(string eventName, string message)
        {
            Log.ForContext<QueueProvider>().Information("ProcessEvent => {eventName} , {message}", eventName, message);
            if (_subsManager.HasSubscriptionsForEvent(eventName))
                using (var scope = _autofac.BeginLifetimeScope(BROKER_NAME))
                {
                    var subscriptions = _subsManager.GetHandlersForEvent(eventName);
                    foreach (var subscription in subscriptions)
                    {
                        var handler = scope.ResolveOptional(subscription.HandlerType);
                        if (handler == null) continue;
                        var eventType = _subsManager.GetEventTypeByName(eventName);
                        var integrationEvent = JsonConvert.DeserializeObject(message, eventType);
                        var concreteType = typeof(IIntegrationEventHandler<>).MakeGenericType(eventType);
                        await (Task)concreteType.GetMethod("Handle").Invoke(handler, new[] { integrationEvent });
                    }
                }
        }
    }
}
