namespace Markwardt;

public class SkipDisposeService(IService source) : BaseDisposable, IService
{
    public object? Resolve(IServiceProvider services, IEnumerable<ServiceOverride> overrides)
        => source.Resolve(services, overrides);

    public override string ToString()
        => $"{base.ToString()} (Source: {source})";
}