namespace Markwardt;

public class OperationThreadQueue : BaseDisposable, IOperationQueue
{
    private readonly ConcurrentQueue<IOperation> operations = [];

    private bool started;

    public async ValueTask<TResult> Execute<TResult>(Func<CancellationToken, ValueTask<TResult>> action)
    {
        ObjectDisposedException.ThrowIf(IsDisposed, this);

        Operation<TResult> operation = new(action);
        operations.Enqueue(operation);
        TryRun();
        return await operation.Task;
    }

    private async void TryRun()
    {
        if (!started)
        {
            started = true;
            await Task.Run(Run);
        }
    }

    private async ValueTask Run()
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