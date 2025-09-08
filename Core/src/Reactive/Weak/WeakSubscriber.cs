namespace Markwardt;

public interface IWeakSubscriber<T> : IDisposable
{
    void Subscribe(IObservable<T> source, IObserver<T> observer);
    void Unsubscribe();
}

public static class WeakSubscriberExtensions
{
    public static void Subscribe<T>(this IWeakSubscriber<T> subscriber, IObservable<T> source, Action<T>? onNext = null, Action? onCompleted = null, Action<Exception>? onError = null)
        => subscriber.Subscribe(source, new Observer<T>(onNext, onCompleted, onError));
}

public sealed class WeakSubscriber<T> : IWeakSubscriber<T>
{
    private IObserver<T>? observer;
    private IDisposable? subscription;

    public void Subscribe(IObservable<T> source, IObserver<T> observer)
    {
        Unsubscribe();

        this.observer = observer;
        subscription = source.WeakSubscribe(observer);
    }

    public void Unsubscribe()
    {
        observer = null;
        subscription?.Dispose();
    }

    public void Dispose()
        => subscription?.Dispose();
}