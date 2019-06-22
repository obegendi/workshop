using Autofac;
using MessageQueueProvider;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;

namespace Extensions
{
    public static class MessageQueueExtensions
    {
        public static IServiceCollection AddMQ(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<IPersistentConnection>(sp =>
            {
                var factory = new ConnectionFactory
                {
                    HostName = configuration["EventBusConnection"]
                };

                if (!string.IsNullOrEmpty(configuration["EventBusUserName"]))
                    factory.UserName = configuration["EventBusUserName"];

                if (!string.IsNullOrEmpty(configuration["EventBusPassword"]))
                    factory.Password = configuration["EventBusPassword"];

                var retryCount = int.Parse(configuration["EventBusRetryCount"]);

                return new PersistentConnection(factory, retryCount);
            });

            services.AddSingleton<IEventBus, QueueProvider>(sp =>
            {
                var connection = sp.GetRequiredService<IPersistentConnection>();
                var subsManager = sp.GetRequiredService<ISubscriptionManager>();
                var scope = sp.GetRequiredService<ILifetimeScope>();
                var retryCount = int.Parse(configuration["EventBusRetryCount"]);
                var clientName = configuration["SubscriptionClientName"];

                return new QueueProvider(connection, subsManager, scope, clientName, retryCount);
            });
            services.AddSingleton<ISubscriptionManager, SubscriptionManager>();

            return services;
        }
    }
}
