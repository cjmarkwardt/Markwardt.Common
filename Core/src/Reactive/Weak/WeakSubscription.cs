namespace Markwardt;

public static class WeakSubscriptionExtensions
{
    public static IDisposable WeakSubscribe<T>(this IObservable<T> source, IObserver<T> observer)
        => new WeakSubscription<T>(source, observer);

    public static IDisposable WeakSubscribe<T>(this IObservable<T> source, Action<T>? onNext = null, Action? onCompleted = null, Action<Exception>? onError = null)
        => source.WeakSubscribe(new Observer<T>(onNext, onCompleted, onError));
}

public sealed class WeakSubscription<T> : IObserver<T>, IDisposable
{
    public WeakSubscription(IObservable<T> source, IObserver<T> observer)
    {
        weakObserver = new(observer);
        subscription = source.Subscribe(this);
    }

    private readonly WeakReference<IObserver<T>> weakObserver;
    private readonly IDisposable subscription;

    public void Dispose()
        => subscription.Dispose();

    void IObserver<T>.OnNext(T value)
        => Execute(x => x.OnNext(value));

    void IObserver<T>.OnError(Exception error)
        => Execute(x => x.OnError(error));

    void IObserver<T>.OnCompleted()
        => Execute(x => x.OnCompleted());

    private void Execute(Action<IObserver<T>> action)
    {
        if (weakObserver.TryGetTarget(out IObserver<T>? observer))
        {
            action(observer);
        }
        else
        {
            subscription.Dispose();
        }
    }
}