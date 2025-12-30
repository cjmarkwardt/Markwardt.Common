namespace Markwardt;

public interface IServiceContainer : IServiceProvider, IServiceConfiguration, IDisposable, IAsyncDisposable;

public sealed class ServiceContainer(IServiceSource source) : BaseAsyncDisposable, IServiceContainer
{
    public ServiceContainer()
        : this(new AutoHandler()) { }

    private readonly Dictionary<Type, IService> services = [];

    private bool isDisposed;

    public void Configure(Type tag, IService? service)
    {
        if (service is null)
        {
            services.Remove(tag);
        }
        else
        {
            services[tag] = service;
        }
    }

    public object? GetService(Type tag)
    {
        ObjectDisposedException.ThrowIf(isDisposed, this);

        if (tag.Equals(typeof(IServiceProvider)))
        {
            return this;
        }
        else if (services.TryGetValue(tag, out IService? service))
        {
            return service.Resolve(this);
        }
        else if (source.TryGetService(tag).TryNotNull(out service))
        {
            services.Add(tag, service);
            return service.Resolve(this);
        }
        else
        {
            return null;
        }
    }

    protected override void OnDispose()
    {
        if (!isDisposed)
        {
            isDisposed = true;
            services.Values.ForEach(x => x.Dispose());
        }
    }

    protected override async ValueTask OnAsyncDispose()
    {
        if (!isDisposed)
        {
            isDisposed = true;
            await Task.WhenAll(services.Values.Select(x => x.DisposeAsync().AsTask()));
        }
    }
}