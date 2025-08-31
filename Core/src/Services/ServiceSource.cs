namespace Markwardt;

public interface IServiceSource : IDisposable, IAsyncDisposable
{
    object GetService(IServiceProvider services);
}

public class ServiceSource(Func<IServiceProvider, object> implementation, bool isCached) : BaseAsyncDisposable, IServiceSource
{
    public static IServiceSource FromDelegate(Func<IServiceProvider, object> implementation, bool isCached)
        => new ServiceSource(implementation, isCached);

    public static IServiceSource FromInstance(object instance)
        => FromDelegate(_ => instance, true);

    public static IServiceSource FromImplementation(Type implementation)
        => FromDelegate(services => services.Create(implementation), true);

    public static IServiceSource FromFactory(Type factory, Type? implementation = null)
        => FromDelegate(services => services.CreateFactory(factory, implementation), true);

    public static IServiceSource FromRoute(Type tag)
        => FromDelegate(services => services.GetRequiredService(tag), false);

    private object? instance;

    public object GetService(IServiceProvider services)
    {
        ObjectDisposedException.ThrowIf(IsDisposed, this);

        if (isCached)
        {
            return instance ??= implementation(services);
        }
        else
        {
            return implementation(services);
        }
    }

    protected override void OnPrepareDispose()
    {
        base.OnPrepareDispose();

        if (isCached && instance is not null)
        {
            instance.DisposeWith(this);
        }
    }
}