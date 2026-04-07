namespace Markwardt;

public abstract class BaseBackgroundRunner : BaseDisposable
{
    private bool isStarted;
    private bool isStopped;
    private Exception? stopException;

    private readonly CancellationTokenSource cancellation = new();

    private readonly TaskCompletionSource<Exception?> completion = new();
    public Task<Exception?> Completion => completion.Task;

    protected abstract ValueTask Run(CancellationToken cancellation);

    protected virtual void OnStopped(Exception? exception) { }

    protected void Start()
    {
        if (!isStarted && !isStopped)
        {
            isStarted = true;

            this.RunInBackground(async cancellation =>
            {
                try
                {
                    await Run(cancellation);
                }
                catch (Exception exception)
                {
                    if (exception is not OperationCanceledException)
                    {
                        stopException ??= exception;
                    }
                }
                finally
                {
                    isStopped = true;
                    OnStopped(stopException);
                    completion.SetResult(stopException);
                }
            }, cancellation.Token);
        }
    }

    protected void Stop(Exception? exception = null)
    {
        if (!isStopped)
        {
            isStopped = true;
            stopException = exception;
            cancellation.Cancel();
        }
    }

    protected override void OnDispose()
    {
        base.OnDispose();

        Stop();
        cancellation.Dispose();
    }
}

public class BackgroundRunner(Func<Action<Exception>, CancellationToken, ValueTask> action) : BaseBackgroundRunner
{
    protected override async ValueTask Run(CancellationToken cancellation)
        => await action(x => Stop(x), cancellation);
}