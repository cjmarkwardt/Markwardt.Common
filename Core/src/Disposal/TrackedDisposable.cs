namespace Markwardt;

public interface ITrackedDisposable : IVisibleDisposable
{
    CancellationToken Disposal { get; }
}

public static class TrackedDisposableExtensions
{
    public static IDisposable RunInBackground(this ITrackedDisposable disposable, Func<CancellationToken, ValueTask> action, CancellationToken cancellation = default)
    {
        CancellationTokenSource directCancellation = new();

        async void Run()
        {
            try
            {
                using CancellationTokenSource linkedCancellation = disposable.Disposal.Link(cancellation, directCancellation.Token);
                await action(linkedCancellation.Token);
            }
            catch (OperationCanceledException) { }
            finally
            {
                directCancellation.Dispose();
            }
        }

        Run();
        return directCancellation.ToDisposable();
    }
}