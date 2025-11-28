namespace Markwardt;

public abstract class SimpleTag : ServiceTag
{
    protected abstract object Get();

    protected override ValueTask<object> GetService(IAsyncServiceProvider services, CancellationToken cancellation = default)
        => ValueTask.FromResult(Get());
}