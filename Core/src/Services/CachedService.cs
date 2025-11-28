namespace Markwardt;

public static class CachedServiceExtensions
{
    public static IService AsCached(this IService source, bool isCached = true)
        => isCached ? new CachedService(source) : source;
}

public class CachedService(IService builder) : BaseAsyncDisposable, IService
{
    private object? instance;

    public async ValueTask<object> Resolve(IAsyncServiceProvider services, IReadOnlyDictionary<ParameterInfo, object?>? parameters = null, IReadOnlyDictionary<PropertyInfo, object?>? properties = null, CancellationToken cancellation = default)
        => instance ??= await builder.Resolve(services, parameters, properties, cancellation);

    protected override void OnSharedDispose()
    {
        base.OnSharedDispose();

        builder.DisposeWith(this);
        instance.DisposeWith(this);
    }
}