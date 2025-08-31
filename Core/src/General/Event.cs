namespace Markwardt;

public interface IEvent : IObservable
{
    void Invoke();
}

public class Event(ISubject<bool>? subject) : IEvent
{
    public Event()
        : this(null) { }

    private readonly ISubject<bool> subject = subject ?? new Subject<bool>();

    public void Invoke()
        => subject.OnNext(true);

    public IDisposable Subscribe(IObserver<bool> observer)
        => subject.Subscribe(observer);
}

public interface IEvent<T> : IObservable<T>
{
    void Invoke(T value);
}

public class Event<T>(ISubject<T>? subject) : IEvent<T>
{
    public Event()
        : this(null) { }

    private readonly ISubject<T> subject = subject ?? new Subject<T>();

    public void Invoke(T value)
        => subject.OnNext(value);

    public IDisposable Subscribe(IObserver<T> observer)
        => subject.Subscribe(observer);
}