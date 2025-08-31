namespace Markwardt;

public interface IVisibleDisposable : IDisposable
{
    bool IsDisposed { get; }
}

public static class VisibleDisposableExtensions
{
    public static void VerifyUndisposed(this IVisibleDisposable disposable)
        => ObjectDisposedException.ThrowIf(disposable.IsDisposed, disposable);
}