namespace Markwardt;

public interface ISignal : IObservable<Unit>
{
    void Set();
}

public interface ISignal<T> : IObservable<T>
{
    void Set(T value);
}

public class Signal : ISignal
{
    private readonly ReplaySubject<Unit> subject = new(1);

    public void Set()
    {
        subject.OnNext(Unit.Default);
        subject.OnCompleted();
    }

    public IDisposable Subscribe(IObserver<Unit> observer)
        => subject.Subscribe(observer);
}

public class Signal<T> : ISignal<T>
{
    private readonly ReplaySubject<T> subject = new(1);

    public void Set(T value)
    {
        subject.OnNext(value);
        subject.OnCompleted();
    }

    public IDisposable Subscribe(IObserver<T> observer)
        => subject.Subscribe(observer);
}