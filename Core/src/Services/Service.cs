namespace Markwardt;

public interface IService : IDisposable
{
    object? Resolve(IServiceProvider services, IEnumerable<ServiceOverride> overrides);
}

public static class ServiceExtensions
{
    public static IService AsService(this object? instance, bool dispose = true)
        => new InstanceService(instance, dispose);

    public static Maybe<T> Resolve<T>(this IService service, IServiceProvider services, IEnumerable<ServiceOverride> overrides)
        => service.Resolve(services, overrides).NullToMaybe().Cast<T>();

    public static object? Resolve(this IService service, IServiceProvider services)
        => service.Resolve(services, []);

    public static Maybe<T> Resolve<T>(this IService service, IServiceProvider services)
        => service.Resolve<T>(services, []);

    public static object Require(this IService service, IServiceProvider services, IEnumerable<ServiceOverride> overrides)
        => service.Resolve(services, overrides) ?? throw new InvalidOperationException("Service resolution failed");

    public static T Require<T>(this IService service, IServiceProvider services, IEnumerable<ServiceOverride> overrides)
        => (T) service.Require(services, overrides);

    public static object Require(this IService service, IServiceProvider services)
        => service.Require(services, []);

    public static T Require<T>(this IService service, IServiceProvider services)
        => service.Require<T>(services, []);

    public static IService Override(this IService service, params IEnumerable<ServiceOverride> overrides)
        => new OverrideService(service, overrides);

    public static IService Cache(this IService service, bool isCached = true)
        => isCached ? new CacheService(service) : service;

    public static IService SkipDispose(this IService service)
        => new SkipDisposeService(service);

    public static IService Factory(this IService service, Type factory, Func<ParameterInfo, ServiceParameter, bool?>? isMatch = null)
        => new FactoryService(factory, service, isMatch);
}

public class Service(Func<IServiceProvider, IEnumerable<ServiceOverride>, object?> resolve) : BaseAsyncDisposable, IService
{
    public static IService Empty => EmptyService.Instance;
    public static IService Suppress => SuppressService.Instance;

    public Service(Func<IServiceProvider, object?> resolve)
        : this((services, _) => resolve(services)) { }

    public Service(Func<object?> resolve)
        : this(_ => resolve()) { }
        
    public object? Resolve(IServiceProvider services, IEnumerable<ServiceOverride> overrides)
        => resolve(services, overrides);

    public override string ToString()
        => "Service";
}