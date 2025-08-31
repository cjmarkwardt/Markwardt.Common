namespace Markwardt;

public interface IOperationQueue : IDisposable
{
    ValueTask<TResult> Execute<TResult>(Func<CancellationToken, ValueTask<TResult>> action);
}

public static class OperationQueueExtensions
{
    public static async ValueTask Execute(this IOperationQueue operations, Func<CancellationToken, ValueTask> action)
        => await operations.Execute(async cancellation => { await action(cancellation); return false; });

    public static async ValueTask<TResult> Execute<TResult>(this IOperationQueue operations, Func<CancellationToken, TResult> action)
        => await operations.Execute(cancellation => ValueTask.FromResult(action(cancellation)));

    public static async ValueTask Execute(this IOperationQueue operations, Action<CancellationToken> action)
        => await operations.Execute(cancellation => { action(cancellation); return ValueTask.FromResult(false); });
}

public class OperationQueue : BaseDisposable, IOperationQueue
{
    private readonly Queue<IOperation> operations = [];

    private bool isRunning;

    public async ValueTask<TResult> Execute<TResult>(Func<CancellationToken, ValueTask<TResult>> action)
    {
        ObjectDisposedException.ThrowIf(IsDisposed, this);

        Operation<TResult> operation = new(action);
        operations.Enqueue(operation);
        TryRun();
        return await operation.Task;
    }

    private void TryRun()
    {
        if (!isRunning)
        {
            isRunning = true;
            using (Disposable.Create(() => isRunning = false))
            {
                Run();
            }
        }
    }

    private async void Run()
    {
        while (!Disposal.IsCancellationRequested && operations.TryDequeue(out IOperation? operation))
        {
            await operation.Execute(Disposal);
        }
    }

    private interface IOperation
    {
        ValueTask Execute(CancellationToken cancellation);
    }

    private sealed class Operation<TResult>(Func<CancellationToken, ValueTask<TResult>> action) : IOperation
    {
        private readonly TaskCompletionSource<TResult> completion = new();
        public Task<TResult> Task => completion.Task;

        public async ValueTask Execute(CancellationToken cancellation)
            => completion.SetResult(await action(cancellation));
    }
}