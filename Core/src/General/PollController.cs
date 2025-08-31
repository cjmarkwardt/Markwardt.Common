namespace Markwardt;

public interface IPollController : IDisposable
{
    IDisposable Add(TimeSpan interval, Action action);
    IDisposable Add(TimeSpan interval, Func<CancellationToken, ValueTask> action);
}

public class PollController : BaseDisposable, IPollController
{
    private readonly Dictionary<TimeSpan, IntervalManager> polls = [];

    public IDisposable Add(TimeSpan interval, Action action)
    {
        ObjectDisposedException.ThrowIf(IsDisposed, this);

        return Add(interval, new Poll(action));
    }

    public IDisposable Add(TimeSpan interval, Func<CancellationToken, ValueTask> action)
    {
        ObjectDisposedException.ThrowIf(IsDisposed, this);

        return Add(interval, new AsyncPoll(action));
    }

    private IDisposable Add(TimeSpan interval, IPoll poll)
    {
        if (!polls.TryGetValue(interval, out IntervalManager? manager))
        {
            manager = new(interval);
            polls[interval] = manager;
        }

        manager.TryRun(poll);
        return Disposable.Create(() => manager.Remove(poll));
    }

    protected override void OnDispose()
    {
        base.OnDispose();

        polls.Values.ForEach(x => x.Dispose());
        polls.Clear();
    }

    private sealed class IntervalManager(TimeSpan interval) : BaseDisposable
    {
        private readonly HashSet<IPoll> polls = [];

        private bool isRunning;

        public void TryRun(IPoll poll)
        {
            ObjectDisposedException.ThrowIf(IsDisposed, this);

            polls.Add(poll);

            if (!isRunning)
            {
                isRunning = true;

                this.RunInBackground(new(async cancellation =>
                {
                    try
                    {
                        while (polls.Count > 0 && !cancellation.IsCancellationRequested)
                        {
                            polls.ForEach(x => x.TryExecute(cancellation));
                            await interval.Delay(cancellation);
                        }
                    }
                    finally
                    {
                        isRunning = false;
                    }
                }));
            }
        }

        public void Remove(IPoll poll)
        {
            poll.Dispose();
            polls.Remove(poll);
        }

        protected override void OnDispose()
        {
            base.OnDispose();

            polls.ForEach(x => x.Dispose());
            polls.Clear();
        }
    }

    private interface IPoll : IDisposable
    {
        void TryExecute(CancellationToken cancellation);
    }

    private sealed record Poll(Action Action) : IPoll
    {
        public void TryExecute(CancellationToken cancellation)
            => Action();

        public void Dispose() { }
    }

    private sealed class AsyncPoll(Func<CancellationToken, ValueTask> action) : BaseDisposable, IPoll
    {
        private TaskCompletionSource? completion;

        public void TryExecute(CancellationToken cancellation)
        {
            if (completion is null || completion.Task.IsCompleted)
            {
                completion = new();

                this.RunInBackground(new(async cancellation =>
                {
                    await action(cancellation);
                    completion.SetResult();
                }));
            }
        }
    }
}