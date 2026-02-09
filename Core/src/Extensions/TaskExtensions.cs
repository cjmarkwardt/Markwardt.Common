namespace Markwardt;

public static class TaskExtensions
{
    public static async ValueTask<T> WithCancellation<T>(this TaskCompletionSource<T> task, CancellationToken cancellation)
    {
        using CancellationTokenRegistration registration = cancellation.Register(() => task.TrySetCanceled(cancellation));
        return await task.Task;
    }

    public static async ValueTask<T> WithTimeout<T>(this Task<T> task, TimeSpan? timeout, CancellationToken cancellation = default)
    {
        if (timeout is not null && await Task.WhenAny(task, Task.Delay(timeout.Value, cancellation)) != task)
        {
            throw new TimeoutException();
        }

        return await task;
    }

    public static async ValueTask<T> WithTimeout<T>(this ValueTask<T> task, TimeSpan? timeout, CancellationToken cancellation = default)
        => await task.AsTask().WithTimeout(timeout, cancellation);

    public static async ValueTask WithTimeout(this Task task, TimeSpan? timeout, CancellationToken cancellation = default)
    {
        if (timeout is not null && await Task.WhenAny(task, Task.Delay(timeout.Value, cancellation)) != task)
        {
            throw new TimeoutException();
        }
    }

    public static async ValueTask WithTimeout(this ValueTask task, TimeSpan? timeout, CancellationToken cancellation = default)
        => await task.AsTask().WithTimeout(timeout, cancellation);

    public static async void Fork(this Task task)
        => await task;

    public static async void Fork(this ValueTask task)
        => await task;
}