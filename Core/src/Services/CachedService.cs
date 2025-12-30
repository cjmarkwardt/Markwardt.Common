namespace Markwardt;

public static class CachedServiceExtensions
{
    public static IService AsCached(this IService source, bool isCached = true)
        => isCached ? new CachedService(source) : source;
}

public class CachedService(IService builder) : BaseAsyncDisposable, IService
{
    private object? instance;

    public object Resolve(IServiceProvider services, IReadOnlyDictionary<ParameterInfo, object?>? parameters = null, IReadOnlyDictionary<PropertyInfo, object?>? properties = null)
        => instance ??= builder.Resolve(services, parameters, properties);

    protected override void OnSharedDispose()
    {
        base.OnSharedDispose();

        builder.DisposeWith(this);
        instance.DisposeWith(this);
    }
}