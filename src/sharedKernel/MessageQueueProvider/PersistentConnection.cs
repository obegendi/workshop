using System;
using System.IO;
using Polly;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Serilog;

namespace MessageQueueProvider
{
    public class PersistentConnection : IPersistentConnection
    {

        private readonly IConnectionFactory _connectionFactory;
        private readonly int _retryCount;

        private readonly object sync_root = new object();
        private IConnection _connection;
        private bool _disposed;

        public PersistentConnection(IConnectionFactory connectionFactory, int retryCount = 5)
        {
            _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
            _retryCount = retryCount;
        }

        public bool IsConnected => _connection != null && _connection.IsOpen && !_disposed;

        public IModel CreateModel()
        {
            if (!IsConnected)
                throw new InvalidOperationException("RabbitMQ Not Connected!");

            return _connection.CreateModel();
        }

        public void Dispose()
        {
            if (_disposed) return;

            _disposed = true;

            try
            {
                _connection.Dispose();
            }
            catch (IOException ex)
            {
                Log.ForContext<PersistentConnection>().Warning(ex, "{@ex}");
            }
        }

        public bool TryConnect()
        {
            Log.ForContext<PersistentConnection>().Debug("RabbitMQ Client connecting");
            lock (sync_root)
            {
                var policy = Policy.Handle<Exception>()
                    .WaitAndRetry(_retryCount, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                        (ex, time) => { Log.ForContext<PersistentConnection>().Warning(ex, "{@ex}"); }
                    );

                policy.Execute(() =>
                {
                    _connection = _connectionFactory
                        .CreateConnection();
                });

                if (IsConnected)
                {
                    _connection.ConnectionShutdown += OnConnectionShutdown;
                    _connection.ConnectionBlocked += OnConnectionBlocked;

                    Log.ForContext<PersistentConnection>().Debug("RabbitMQ Client connected => {@connection}", _connection);
                    return true;
                }
                Log.ForContext<PersistentConnection>().Warning("RabbitMQ Client not connected => {@connection}", _connection);
                return false;
            }
        }

        private void OnConnectionShutdown(object sender, ShutdownEventArgs e)
        {
            Log.ForContext<PersistentConnection>().Warning("OnConnectionShutdown => {@sender}, {@e}", sender, e);
        }

        private void OnConnectionBlocked(object sender, ConnectionBlockedEventArgs e)
        {
            Log.ForContext<PersistentConnection>().Warning("OnConnectionBlocked => {@sender}, {@e}", sender, e);
        }
    }
}
