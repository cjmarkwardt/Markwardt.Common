namespace Markwardt;

public static class DisposableExtensions
{
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
}