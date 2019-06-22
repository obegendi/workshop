using System;
using RabbitMQ.Client;

namespace MessageQueueProvider
{
    public interface IPersistentConnection
        : IDisposable
    {
        bool IsConnected { get; }

        bool TryConnect();

        IModel CreateModel();
    }
}
