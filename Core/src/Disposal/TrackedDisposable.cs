namespace Markwardt;

public interface ITrackedDisposable : IVisibleDisposable
{
    CancellationToken Disposal { get; }
}

public static class TrackedDisposableExtensions
{
    public static async void RunInBackground(this ITrackedDisposable disposable, Func<CancellationToken, ValueTask> action, CancellationToken cancellation = default)
    {
        using CancellationTokenSource linkedCancellation = disposable.Disposal.Link(cancellation);
        await action(linkedCancellation.Token);
    }

    public static void LoopInBackground(this ITrackedDisposable disposable, Func<CancellationToken, Action, ValueTask> action, CancellationToken cancellation = default)
        => disposable.RunInBackground(async cancellation =>
        {
            bool run = true;

            void Stop()
                => run = false;

            while (run && !disposable.IsDisposed)
            {
                await action(cancellation, Stop);
            }
        }, cancellation);
}