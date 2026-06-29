namespace LMS.Domain.Interfaces;

public interface IEventBus
{
    void Subscribe<T>(ISubscriber<T> subscriber);
    void Publish<T>(T @event);
}
