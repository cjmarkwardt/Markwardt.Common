namespace Markwardt;

public interface IBackgroundTask : ITrackedDisposable
{
    Task Task { get; }
    bool IsCompleted { get; }
    Exception? Exception { get; }

    IObservable<Exception?> Completed { get; }

    void Start();
}

public class BackgroundTaskk(Func<CancellationToken, Task> action) : BaseDisposable, IBackgroundTask
{
    public static BackgroundTaskk Start(Func<CancellationToken, Task> action, Action<Exception?>? completed = null)
    {
        BackgroundTaskk task = new(action);
        
        if (completed is not null)
        {
            task.Completed.Subscribe(completed);
        }

        task.Start();
        return task;
    }

    private readonly TaskCompletionSource completion = new();

    private bool isStarted;

    public bool IsThreaded { get; init; } = false;

    public Exception? Exception { get; private set; }

    public Task Task => completion.Task;
    public bool IsCompleted => Task.IsCompleted;

    private readonly Subject<Exception?> completed = new();
    public IObservable<Exception?> Completed => completed;

    public async void Start()
    {
        this.VerifyUndisposed();

        if (isStarted)
        {
            throw new InvalidOperationException("Already started");
        }

        isStarted = true;

        try
        {
            if (IsThreaded)
            {
                await Task.Run(async () => await action(Disposal), Disposal);
            }
            else
            {
                await action(Disposal);
            }

            completed.OnNext(null);
        }
        catch (Exception exception)
        {
            completed.OnNext(exception);
        }
    }
}

public sealed class BackgroundTask(AsyncOperation operation) : IDisposable
{
    public static BackgroundTask Completed { get; } = Start(new(_ => { }));

    public static BackgroundTask Start(AsyncOperation operation)
    {
        BackgroundTask task = new(operation);
        task.Start();
        return task;
    }

    private readonly CancellationTokenSource cancellation = new();
    private readonly TaskCompletionSource completion = new();

    private bool isDisposed;

    public BackgroundTaskState State { get; private set; }
    public Failable? Result { get; private set; }

    public async ValueTask WhenComplete()
        => await completion.Task;

    public async void Start()
    {
        ObjectDisposedException.ThrowIf(isDisposed, this);

        if (State is BackgroundTaskState.Unstarted)
        {
            State = BackgroundTaskState.Running;
            Result = await operation.Execute(cancellation.Token);
            cancellation.Dispose();
            completion.SetResult();
        }
    }

    public void Cancel()
    {
        if (State is BackgroundTaskState.Unstarted)
        {
            State = BackgroundTaskState.Completed;
            Result = new OperationCanceledException("Background task was cancelled");
            cancellation.Dispose();
            completion.SetResult();
        }
        else if (State is BackgroundTaskState.Running)
        {
            State = BackgroundTaskState.Cancelling;
            cancellation.Cancel();
        }
    }

    public void Dispose()
    {
        if (!isDisposed)
        {
            isDisposed = true;
            Cancel();
        }
    }
}