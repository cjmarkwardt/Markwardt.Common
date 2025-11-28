namespace Markwardt;

public interface IServiceContainer : IAsyncServiceProvider, IServiceConfiguration, IDisposable, IAsyncDisposable;

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

    public async ValueTask<object?> GetService(Type tag, CancellationToken cancellation = default)
    {
        ObjectDisposedException.ThrowIf(isDisposed, this);

        if (tag.Equals(typeof(IAsyncServiceProvider)))
        {
            return this;
        }
        else if (services.TryGetValue(tag, out IService? service))
        {
            return await service.Resolve(this, cancellation: cancellation);
        }
        else if (source.TryGetService(tag).TryNotNull(out service))
        {
            services.Add(tag, service);
            return await service.Resolve(this, cancellation: cancellation);
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