namespace Markwardt;

public class CacheService(IService source) : BaseDisposable, IService
{
    private object? instance;

    public object? Resolve(IServiceProvider services, IEnumerable<ServiceOverride> overrides)
        => instance ??= source.Resolve(services, overrides);

    protected override void OnDispose()
    {
        base.OnDispose();

        source.Dispose();
        instance.TryDispose();
    }

    public override string ToString()
        => $"{base.ToString()} (Source: {source})";
}