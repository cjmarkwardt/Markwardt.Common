namespace Markwardt;

public interface IServiceConfiguration
{
    void Configure(Type tag, IService? service);
}

public static class ServiceConfigurationExtensions
{
    public static void Configure<T>(this IServiceConfiguration configuration, IService? service)
        where T : notnull
        => configuration.Configure(typeof(T), service);

    public static void Configure<T, TImplementation>(this IServiceConfiguration configuration)
        where T : notnull
        where TImplementation : notnull
        => configuration.Configure<T>(Service.Constructor<TImplementation>());
}