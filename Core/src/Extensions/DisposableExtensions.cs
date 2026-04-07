namespace Markwardt;

public static class DisposableExtensions
{
    public static void DisposeAll(this IEnumerable<IDisposable> targets)
        => targets.ForEach(x => x.Dispose());

    public static void TryDispose(this object? target)
    {
        if (target is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }
    
    public static async ValueTask TryDisposeAsync(this object? target)
    {
        if (target is IAsyncDisposable asyncDisposable)
        {
            await asyncDisposable.DisposeAsync();
        }
        else
        {
            target.TryDispose();
        }
    }

    public static BackgroundTask DisposeInBackground(this object? value)
    {
        if (value is IAsyncDisposable asyncDisposable)
        {
            return BackgroundTask.Start(new(async _ => await asyncDisposable.DisposeAsync()));
        }
        else if (value is IDisposable syncDisposable)
        {
            syncDisposable.Dispose();
        }
        
        return BackgroundTask.Completed;
    }
    
    public static TResult Using<T, TResult>(this T disposable, Func<T, TResult> func)
        where T : IDisposable
    {
        using (disposable)
        {
            return func(disposable);
        }
    }

    public static void Using<T>(this T disposable, Action<T> action)
        where T : IDisposable
    {
        using (disposable)
        {
            action(disposable);
        }
    }

    public static async ValueTask<TResult> Using<T, TResult>(this T disposable, Func<T, ValueTask<TResult>> func)
        where T : IDisposable
    {
        using (disposable)
        {
            return await func(disposable);
        }
    }

    public static async ValueTask Using<T>(this T disposable, Func<T, ValueTask> func)
        where T : IDisposable
    {
        using (disposable)
        {
            await func(disposable);
        }
    }

    public static T DisposeWith<T>(this T disposable, CompositeDisposable composite)
        where T : IDisposable
    {
        composite.Add(disposable);
        return disposable;
    }

    public static T DisposeWith<T>(this T disposable, IParentDisposable parent)
        where T : IDisposable
    {
        parent.AddChildDisposable(disposable);
        return disposable;
    }
}