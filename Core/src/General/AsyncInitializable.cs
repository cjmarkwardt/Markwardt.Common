namespace Markwardt;

public static class AsyncInitializableExtensions
{
    public static async ValueTask<T> WithInitialize<T>(this T initializable, CancellationToken cancellation = default)
        where T : IAsyncInitializable
    {
        await initializable.Initialize(cancellation);
        return initializable;
    }
}

public interface IAsyncInitializable
{
    ValueTask Initialize(CancellationToken cancellation = default);
}