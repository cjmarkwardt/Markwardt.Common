namespace Markwardt;

public class EmptyService : BaseDisposable, IService
{
    public static EmptyService Instance { get; } = new();

    private EmptyService() { }

    public object? Resolve(IServiceProvider services, IEnumerable<ServiceOverride> overrides)
        => null;
}