using LMS.Domain.Interfaces;

namespace LMS.Application.Events;

public class InProcessEventBus : IEventBus
{
    private readonly Dictionary<Type, List<object>> _subscribers = new();

    public void Subscribe<T>(ISubscriber<T> subscriber)
    {
        var eventType = typeof(T);

        if (!_subscribers.TryGetValue(eventType, out var subscribers))
        {
            subscribers = new List<object>();
            _subscribers[eventType] = subscribers;
        }

        subscribers.Add(subscriber);
    }

    public void Publish<T>(T @event)
    {
        if (!_subscribers.TryGetValue(typeof(T), out var subscribers))
        {
            return;
        }

        foreach (var subscriber in subscribers.Cast<ISubscriber<T>>())
        {
            subscriber.Handle(@event);
        }
    }
}
