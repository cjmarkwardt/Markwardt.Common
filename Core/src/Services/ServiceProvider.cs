namespace Markwardt;

public interface IAsyncServiceProvider
{
    ValueTask<object?> GetService(Type tag, CancellationToken cancellation = default);
}

public static class AsyncServiceProvider
{
    private static IAsyncServiceProvider? shared;
    public static IAsyncServiceProvider Shared => shared ?? throw new InvalidOperationException("Shared service provider has not been set");

    private readonly static Dictionary<object, IService> createCache = [];

    public static void SetShared(IAsyncServiceProvider provider)
    {
        if (shared is not null)
        {
            throw new InvalidOperationException("Shared service provider has already been set");
        }

        shared = provider;
    }

    public static async ValueTask<T?> GetService<T>(this IAsyncServiceProvider services, CancellationToken cancellation = default)
        where T : notnull
        => (T?)await services.GetService(typeof(T?), cancellation);

    public static async ValueTask<T?> GetService<TTag, T>(this IAsyncServiceProvider services, CancellationToken cancellation = default)
        where T : notnull
        => (T?)await services.GetService(typeof(TTag), cancellation);

    public static async ValueTask<object> GetRequiredService(this IAsyncServiceProvider services, Type tag, CancellationToken cancellation = default)
        => await services.GetService(tag, cancellation) ?? throw new InvalidOperationException($"Service {tag} is required but could not be resolved");

    public static async ValueTask<T> GetRequiredService<T>(this IAsyncServiceProvider services, CancellationToken cancellation = default)
        where T : notnull
        => (T)await services.GetRequiredService(typeof(T), cancellation);

    public static async ValueTask<T> GetRequiredService<TTag, T>(this IAsyncServiceProvider services, CancellationToken cancellation = default)
        where T : notnull
        => (T)await services.GetRequiredService(typeof(TTag), cancellation);

    public static async ValueTask Start(this IServiceContainer services, IService service, Action<IServiceConfiguration>? configureServices = null, IReadOnlyDictionary<ParameterInfo, object?>? parameters = null, IReadOnlyDictionary<PropertyInfo, object?>? properties = null, bool setShared = true, CancellationToken cancellation = default)
    {
        if (setShared)
        {
            SetShared(services);
        }

        configureServices?.Invoke(services);
        
        await (await service.Resolve<IStarter>(services, parameters, properties, cancellation)).Start(cancellation);
    }

    public static async ValueTask Start<T>(this IServiceContainer services, Action<IServiceConfiguration>? configureServices = null, IReadOnlyDictionary<ParameterInfo, object?>? parameters = null, IReadOnlyDictionary<PropertyInfo, object?>? properties = null, bool setShared = true, CancellationToken cancellation = default)
        where T : notnull, IStarter
        => await services.Start(Service.Constructor<T>(), configureServices, parameters, properties, setShared, cancellation);
}