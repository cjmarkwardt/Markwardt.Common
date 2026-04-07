namespace Markwardt;

public interface IEvent : IObservable<Unit>
{
    void Invoke();
}

public class Event : IEvent
{
    public static Event FromSubject(ISubject<Unit> subject)
        => new(subject);

    private Event(ISubject<Unit> subject)
        => this.subject = subject;

    public Event()
        : this(new Subject<Unit>()) { }

    private readonly ISubject<Unit> subject;

    public void Invoke()
        => subject.OnNext(Unit.Default);

    public IDisposable Subscribe(IObserver<Unit> observer)
        => subject.Subscribe(observer);
}

public interface IEvent<T> : IObservable<T>
{
    void Invoke(T value);
}

public class Event<T> : IEvent<T>
{
    public static Event<T> FromSubject(ISubject<T> subject)
        => new(subject);

    private Event(ISubject<T> subject)
        => this.subject = subject;

    public Event()
        : this(new Subject<T>()) { }

    private readonly ISubject<T> subject;

    public void Invoke(T value)
        => subject.OnNext(value);

    public IDisposable Subscribe(IObserver<T> observer)
        => subject.Subscribe(observer);
}