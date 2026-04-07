namespace Markwardt;

public interface IAsyncInitializer
{
    ValueTask Initialize(CancellationToken cancellation = default);
}

public static class AsyncInitializerExtensions
{
    public static async ValueTask<T> WithInitialize<T>(this T initializable, CancellationToken cancellation = default)
        where T : IAsyncInitializer
    {
        await initializable.Initialize(cancellation);
        return initializable;
    }
}

public class AsyncInitializer : IAsyncInitializer
{
    public ValueTask Initialize(CancellationToken cancellation = default)
        => ValueTask.CompletedTask;
}