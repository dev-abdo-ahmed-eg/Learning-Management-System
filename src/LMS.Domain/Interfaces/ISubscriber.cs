namespace LMS.Domain.Interfaces;

public interface ISubscriber<T>
{
    void Handle(T @event);
}
