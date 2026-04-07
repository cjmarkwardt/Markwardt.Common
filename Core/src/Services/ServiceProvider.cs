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

    public static object Create(this IServiceProvider services, Type type, Func<IService, IService>? configure = null)
    {
        configure ??= service => service;
        return configure(new ConstructorService(type)).Require(services);
    }

    public static T Create<T>(this IServiceProvider services, Func<IService, IService>? configure = null)
    {
        configure ??= service => service;
        return configure(new ConstructorService(typeof(T))).Require<T>(services);
    }

    public static async ValueTask Start(this IServiceContainer services, IEnumerable<Type> starters, Action<IServiceConfiguration>? setup = null, bool setShared = true, CancellationToken cancellation = default)
    {
        if (setShared)
        {
            SetShared(services);
        }

        foreach (Type starter in starters)
        {
            if (starter.GetCustomAttribute<ConfigureServicesAttribute>() is ConfigureServicesAttribute configureAttribute)
            {
                IServicePackage package = (IServicePackage)Activator.CreateInstance(configureAttribute.ServicePackage).NotNull();
                package.Configure(services);
            }
        }

        setup?.Invoke(services);

        foreach (Type starter in starters)
        {
            if (starter.GetCustomAttribute<InitializeAttribute>() is InitializeAttribute initializeAttribute)
            {
                IAsyncInitializer initializer = (IAsyncInitializer)Activator.CreateInstance(initializeAttribute.Initializer).NotNull();
                await initializer.Initialize(cancellation);
            }
        }

        foreach (Type starter in starters)
        {
            await ((IStarter)services.Create(starter)).Start(cancellation);
        }
    }

    public static async ValueTask Start<TStarter>(this IServiceContainer services, Action<IServiceConfiguration>? setup = null, bool setShared = true, CancellationToken cancellation = default)
        where TStarter : notnull, IStarter
        => await services.Start([typeof(TStarter)], setup, setShared, cancellation);

    public static async ValueTask WaitForExit(this IServiceProvider services, CancellationToken cancellation = default)
    {
        Exception? exception = await services.GetRequiredService<ExitedTag, IObservable<Exception?>>();
        if (exception is not null)
        {
            throw exception;
        }
    }

    public static void Exit(this IServiceProvider services, Exception? exception = null)
        => services.GetRequiredService<ExitedTag, Signal<Exception?>>().Set(exception);

    public static async ValueTask Run(IEnumerable<Type> starters, Action<IServiceConfiguration>? setup = null, IServiceSource? source = null, bool setShared = true, CancellationToken cancellation = default)
    {
        using ServiceContainer services = new(source);
        await services.Start(starters, setup, setShared, cancellation);
        await services.WaitForExit(cancellation);
    }

    public static async ValueTask Run<TStarter>(Action<IServiceConfiguration>? setup = null, IServiceSource? source = null, bool setShared = true, CancellationToken cancellation = default)
        where TStarter : notnull, IStarter
        => await Run([typeof(TStarter)], setup, source, setShared, cancellation);
}