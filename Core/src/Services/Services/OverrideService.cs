namespace Markwardt;

public class OverrideService(IService source, IEnumerable<ServiceOverride> overrides) : BaseDisposable, IService
{
    private readonly IEnumerable<ServiceOverride> overrides = overrides;

    public object? Resolve(IServiceProvider services, IEnumerable<ServiceOverride> overrides)
        => source.Resolve(services, overrides.Concat(this.overrides));

    protected override void OnDispose()
    {
        base.OnDispose();

        source.Dispose();
    }

    public override string ToString()
        => $"{base.ToString()} (Source: {source}, Overrides: {overrides.Count()})";
}