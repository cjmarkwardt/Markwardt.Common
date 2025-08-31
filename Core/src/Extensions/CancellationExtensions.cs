namespace Markwardt;

public static class CancellationExtensions
{
    public static void CancelAndDispose(this CancellationTokenSource cancellation)
    {
        cancellation.Cancel();
        cancellation.Dispose();
    }

    public static CancellationTokenSource Link(this CancellationToken? cancellation, params CancellationToken?[] cancellations)
        => CancellationTokenSource.CreateLinkedTokenSource(cancellation.Yield().Concat(cancellations).Where(x => x is not null).Select(x => x!.Value).ToArray());

    public static CancellationTokenSource Link(this CancellationToken cancellation, params CancellationToken?[] cancellations)
        => ((CancellationToken?)cancellation).Link(cancellations);
}