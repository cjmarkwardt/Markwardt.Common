namespace Markwardt;

public interface IServiceInjector : IServiceResolver
{
    bool Inject(IServiceProvider services, object instance, IEnumerable<ServiceOverride> overrides);
}