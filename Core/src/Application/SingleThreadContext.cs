namespace Markwardt;

public class SingleThreadContext
{
    public static void Run(Action action)
        => Run(() =>
        {
            action();
            return Task.CompletedTask;
        });

    public static void Run(Func<Task> action)
    {
        SynchronizationContext? previousSyncContext = SynchronizationContext.Current;

        SingleThreadContext context = new();
        SynchronizationContext.SetSynchronizationContext(new Synchronizer(context));
        context.Run(action);

        SynchronizationContext.SetSynchronizationContext(previousSyncContext);
    }

    private readonly System.Threading.Channels.Channel<Callback> posts = Channel.CreateUnbounded<Callback>();
    private readonly System.Threading.Channels.Channel<Callback> sends = Channel.CreateUnbounded<Callback>();

    private void Run(Func<Task> action, CancellationToken cancellation = default)
    {
        ExceptionDispatchInfo? exception = null;
        bool isRunning = true;

        async void RunAction(Func<Task> action)
        {
            try
            {
                await action();
            }
            catch (Exception ex)
            {
                exception = ExceptionDispatchInfo.Capture(ex);
            }

            isRunning = false;
        }

        RunAction(action);
        
        while (isRunning && !cancellation.IsCancellationRequested)
        {
            while (sends.Reader.TryRead(out Callback send))
            {
                send.Invoke();
            }

            while (posts.Reader.TryRead(out Callback post))
            {
                post.Invoke();
            }
        }

        exception?.Throw();
    }

    private readonly record struct Callback(SendOrPostCallback Action, object? State)
    {
        public readonly void Invoke() => Action(State);
    }

    private sealed class Synchronizer(SingleThreadContext context) : SynchronizationContext
    {
        public override void Post(SendOrPostCallback d, object? state)
            => context.posts.Writer.TryWrite(new Callback(d, state));

        public override void Send(SendOrPostCallback d, object? state)
            => context.sends.Writer.TryWrite(new Callback(d, state));

        public override SynchronizationContext CreateCopy()
            => new Synchronizer(context);
    }
}