using System;
using System.Collections.Generic;
using System.Linq;
using Serilog;

namespace MessageQueueProvider
{
    public class SubscriptionManager : ISubscriptionManager
    {
        private readonly List<Type> _eventTypes;
        private readonly Dictionary<string, List<SubscriptionInfo>> _handlers;

        public SubscriptionManager()
        {
            _handlers = new Dictionary<string, List<SubscriptionInfo>>();
            _eventTypes = new List<Type>();
        }

        public event EventHandler<string> OnEventRemoved;

        public bool IsEmpty => !_handlers.Keys.Any();
        public void Clear()
        {
            _handlers.Clear();
        }

        public void AddSubscription<T, TH>()
            where T : IntegrationEvent
            where TH : IIntegrationEventHandler<T>
        {
            var eventName = GetEventKey<T>();

            if (!HasSubscriptionsForEvent(eventName))
                _handlers.Add(eventName, new List<SubscriptionInfo>());

            if (_handlers[eventName].Any(s => s.HandlerType == typeof(TH)))
                throw new ArgumentException(
                    $"Handler Type {typeof(TH).Name} already registered for '{eventName}'", nameof(TH));
            _handlers[eventName].Add(SubscriptionInfo.Typed(typeof(TH)));

            if (!_eventTypes.Contains(typeof(T)))
                _eventTypes.Add(typeof(T));
        }

        public void RemoveSubscription<T, TH>()
            where TH : IIntegrationEventHandler<T>
            where T : IntegrationEvent
        {
           
            var handlerToRemove = FindSubscriptionToRemove<T, TH>();
            var eventName = GetEventKey<T>();

            if (handlerToRemove != null)
            {
                _handlers[eventName].Remove(handlerToRemove);
                if (!_handlers[eventName].Any())
                {
                    _handlers.Remove(eventName);
                    var eventType = _eventTypes.SingleOrDefault(e => e.Name == eventName);
                    if (eventType != null)
                        _eventTypes.Remove(eventType);
                    var handler = OnEventRemoved;
                    if (handler != null)
                        OnEventRemoved(this, eventName);
                }

            }
        }

        public bool HasSubscriptionsForEvent(string eventName)
        {
            return _handlers.ContainsKey(eventName);
        }

        public string GetEventKey<T>()
        {
            return typeof(T).Name;
        }

        public Type GetEventTypeByName(string eventName)
        {
            return _eventTypes.SingleOrDefault(t => t.Name == eventName);
        }

        public IEnumerable<SubscriptionInfo> GetHandlersForEvent(string eventName)
        {
            return _handlers[eventName];
        }

        private SubscriptionInfo FindSubscriptionToRemove<T, TH>()
            where T : IntegrationEvent
            where TH : IIntegrationEventHandler<T>
        {
            var eventName = GetEventKey<T>();
            if (!HasSubscriptionsForEvent(eventName))
                return null;
            return _handlers[eventName].SingleOrDefault(s => s.HandlerType == typeof(TH));
        }
    }
}
