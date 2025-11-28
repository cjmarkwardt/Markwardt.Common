namespace Markwardt;

public abstract class SourceTag<TSource> : ServiceTag
    where TSource : notnull
{
    protected override sealed async ValueTask<object> GetService(IAsyncServiceProvider services, CancellationToken cancellation = default)
        => await Get(services, await services.GetRequiredService<TSource>(cancellation), cancellation);

    protected abstract ValueTask<object> Get(IAsyncServiceProvider services, TSource source, CancellationToken cancellation);
}

public abstract class SourceTag<TSourceTag, TSource> : ServiceTag
    where TSource : notnull
{
    protected override sealed async ValueTask<object> GetService(IAsyncServiceProvider services, CancellationToken cancellation = default)
        => await Get(services, await services.GetRequiredService<TSourceTag, TSource>(cancellation), cancellation);

    protected abstract ValueTask<object> Get(IAsyncServiceProvider services, TSource source, CancellationToken cancellation);
}