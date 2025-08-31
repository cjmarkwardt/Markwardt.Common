namespace Markwardt;

public static class ServiceProvider
{
    private static IServiceProvider? shared;
    public static IServiceProvider Shared => shared ?? throw new InvalidOperationException("Shared service provider has not been set");

    public static void SetShared(IServiceProvider provider)
    {
        if (shared is not null)
        {
            throw new InvalidOperationException("Shared service provider has already been set");
        }

        shared = provider;
    }

    public static T? GetService<TTag, T>(this IServiceProvider services)
        => (T?)services.GetService(typeof(TTag));

    public static T GetRequiredService<TTag, T>(this IServiceProvider services)
        => (T)services.GetRequiredService(typeof(TTag));

    public static object Create(this IServiceProvider services, Type implementation, IReadOnlyDictionary<string, object?>? arguments = null)
        => ServiceBuilder.Build(implementation, () => implementation.GetDefaultFactory() ?? throw new InvalidOperationException($"No default factory for type {implementation}"), services, arguments);

    public static T Create<T>(this IServiceProvider services)
        => (T)services.Create(typeof(T));

    public static async ValueTask Start<T>(this IServiceContainer services, Action<IServiceConfiguration>? configure = null, bool setShared = true)
        where T : IStarter
    {
        if (setShared)
        {
            SetShared(services);
        }

        configure?.Invoke(services);
        await services.Create<T>().Start();
    }

    public static Delegate CreateFactory(this IServiceProvider services, Type factory, Type? implementation = null)
    {
        Type target = implementation ?? factory.GetDelegateResult() ?? throw new InvalidOperationException($"Type {factory} has no delegate result");
        return Delegator.CreateDelegate(factory, arguments => services.Create(target, arguments));
    }

    public static TFactory CreateFactory<TFactory>(this IServiceProvider services)
        where TFactory : Delegate
        => (TFactory)services.CreateFactory(typeof(TFactory));

    public static TFactory CreateFactory<TFactory, TImplementation>(this IServiceProvider services)
        where TFactory : Delegate
        => (TFactory)services.CreateFactory(typeof(TFactory), typeof(TImplementation));
}