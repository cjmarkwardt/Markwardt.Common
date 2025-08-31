namespace Markwardt;

public static class WeakSubscriptionExtensions
{
    public static IDisposable WeakSubscribe<T, TTarget>(this IObservable<T> source, TTarget target, Action<TTarget, T> onNext, Action<TTarget>? onCompleted = null, Action<TTarget, Exception>? onError = null)
        where TTarget : class
        => new WeakSubscription<T, TTarget>(source, target, onNext, onCompleted, onError);

    public static IDisposable WeakSubscribe<T, TTarget>(this IObservable<T> source, TTarget target)
        where TTarget : class, IObserver<T>
    {
        static void OnNext(TTarget target, T value)
            => target.OnNext(value);

        static void OnCompleted(TTarget target)
            => target.OnCompleted();

        static void OnError(TTarget target, Exception error)
            => target.OnError(error);

        return source.WeakSubscribe(target, OnNext, OnCompleted, OnError);
    }
}

public sealed class WeakSubscription<T, TTarget> : IDisposable, IObserver<T>
    where TTarget : class
{
    public WeakSubscription(IObservable<T> source, TTarget target, Action<TTarget, T> onNext, Action<TTarget>? onCompleted = null, Action<TTarget, Exception>? onError = null)
    {
        this.target = new(target);
        subscription = source.Subscribe(this);

        if (onNext.Target is not null)
        {
            throw new InvalidOperationException("Weak subscription next delegate must be static");
        }

        if (onCompleted?.Target is not null)
        {
            throw new InvalidOperationException("Weak subscription completed delegate must be static");
        }

        if (onError?.Target is not null)
        {
            throw new InvalidOperationException("Weak subscription error delegate must be static");
        }

        this.onNext = onNext;
        this.onCompleted = onCompleted;
        this.onError = onError;
    }

    private readonly WeakReference<TTarget> target;
    private readonly IDisposable subscription;
    private readonly Action<TTarget, T> onNext;
    private readonly Action<TTarget>? onCompleted;
    private readonly Action<TTarget, Exception>? onError;

    public void OnNext(T value)
    {
        if (TryGetTarget(out TTarget? target))
        {
            onNext(target, value);
        }
    }

    public void OnCompleted()
    {
        if (TryGetTarget(out TTarget? target))
        {
            onCompleted?.Invoke(target);
        }
    }

    public void OnError(Exception error)
    {
        if (TryGetTarget(out TTarget? target))
        {
            onError?.Invoke(target, error);
        }
    }

    public void Dispose()
        => subscription.Dispose();

    private bool TryGetTarget([NotNullWhen(true)] out TTarget? target)
    {
        if (this.target.TryGetTarget(out target))
        {
            return true;
        }
        else
        {
            Dispose();
            return false;
        }
    }
}