namespace Markwardt;

public interface IServiceConfiguration
{
    void Configure(Type tag, Func<IServiceSource>? sourceCreator);
}

public static class ServiceConfigurationExtensions
{
    public static void Reset<TTag>(this IServiceConfiguration configuration)
        => configuration.Configure(typeof(TTag), null);

    public static void ConfigureDelegate<TTag>(this IServiceConfiguration configuration, Func<IServiceProvider, object> implementation, bool isCached = false)
        => configuration.Configure(typeof(TTag), () => ServiceSource.FromDelegate(implementation, isCached));

    public static void ConfigureInstance<TTag>(this IServiceConfiguration configuration, object instance)
        => configuration.Configure(typeof(TTag), () => ServiceSource.FromInstance(instance));

    public static void ConfigureImplementation<TTag, TImplementation>(this IServiceConfiguration configuration)
        => configuration.Configure(typeof(TTag), () => ServiceSource.FromImplementation(typeof(TImplementation)));

    public static void ConfigureFactory<TFactory, TImplementation>(this IServiceConfiguration configuration)
        => configuration.Configure(typeof(TFactory), () => ServiceSource.FromFactory(typeof(TFactory), typeof(TImplementation)));

    public static void ConfigureRoute<TTag, TTargetTag>(this IServiceConfiguration configuration)
        => configuration.Configure(typeof(TTag), () => ServiceSource.FromRoute(typeof(TTargetTag)));
}