namespace Markwardt;

public static class ServiceProvider
{
    private static IServiceProvider? shared;
    public static IServiceProvider Shared => shared ?? throw new InvalidOperationException("Shared service provider has not been set");

    private readonly static Dictionary<object, IService> createCache = [];

    public static void SetShared(IServiceProvider provider)
    {
        if (shared is not null)
        {
            throw new InvalidOperationException("Shared service provider has already been set");
        }

        shared = provider;
    }

    public static T? GetService<T>(this IServiceProvider services)
        where T : notnull
        => (T?)services.GetService(typeof(T?));

    public static T? GetService<TTag, T>(this IServiceProvider services)
        where T : notnull
        => (T?)services.GetService(typeof(TTag));

    public static object GetRequiredService(this IServiceProvider services, Type tag)
        => services.GetService(tag) ?? throw new InvalidOperationException($"Service {tag} is required but could not be resolved");

    public static T GetRequiredService<T>(this IServiceProvider services)
        where T : notnull
        => (T)services.GetRequiredService(typeof(T));

    public static T GetRequiredService<TTag, T>(this IServiceProvider services)
        where T : notnull
        => (T)services.GetRequiredService(typeof(TTag));

    public static async ValueTask Start(this IServiceContainer services, Type starter, bool setShared = true, CancellationToken cancellation = default)
    {
        if (setShared)
        {
            SetShared(services);
        }

        await Service.Constructor(starter).Resolve<IStarter>(services).Start(cancellation);
    }

    public static async ValueTask Start<TStarter>(this IServiceContainer services, bool setShared = true, CancellationToken cancellation = default)
        where TStarter : notnull, IStarter
        => await services.Start(typeof(TStarter), setShared, cancellation);
}