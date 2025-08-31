namespace Markwardt;

public interface ITaskQueue
{
    ValueTask<T> Enqueue<T>(Func<ValueTask<T>> action, CancellationToken cancellation = default);
}

public static class TaskQueueExtensions
{
    public static async ValueTask Enqueue(this ITaskQueue tasks, Func<ValueTask> action, CancellationToken cancellation = default)
        => await tasks.Enqueue(async () => { await action(); return false; }, cancellation);
}

public class TaskQueue : ITaskQueue
{
    private readonly LinkedList<TaskCompletionSource> completions = [];

    public async ValueTask<T> Enqueue<T>(Func<ValueTask<T>> action, CancellationToken cancellation = default)
    {
        LinkedListNode<TaskCompletionSource>? dependency = completions.Last;
        LinkedListNode<TaskCompletionSource> completion = completions.AddLast(new TaskCompletionSource());

        using IDisposable entry = Disposable.Create(() =>
        {
            completions.Remove(completion);
            completion.Value.SetResult();
        });

        if (dependency is not null)
        {
            await dependency.Value.Task.WaitAsync(cancellation);
        }

        return await action();
    }
}