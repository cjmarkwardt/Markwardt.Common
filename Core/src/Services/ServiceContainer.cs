namespace Markwardt;

public interface IServiceContainer : IServiceProvider, IServiceConfiguration, IDisposable;

public sealed class ServiceContainer(IServiceSource? source = null) : BaseDisposable, IServiceContainer
{
    private readonly Subject<ServiceResolution> resolved = new();
    public IObservable<ServiceResolution> Resolved => resolved;

    private readonly IServiceSource source = source ?? new AutoHandler();

    private readonly Dictionary<Type, IService> services = [];

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
        this.VerifyUndisposed();

        object? value;
        string? sourceDescription = null;
        IService? service = null;

        if (tag.Equals(typeof(IServiceProvider)))
        {
            value = this;
        }
        else if (services.TryGetValue(tag, out service))
        {
            value = service.Resolve(this);
        }
        else if (source.TryGetService(tag, out sourceDescription).TryNotNull(out service))
        {
            services.Add(tag, service);
            value = service.Resolve(this);
        }
        else
        {
            value = null;
        }

        resolved.OnNext(new ServiceResolution(tag, value, sourceDescription, service));
        return value;
    }

    protected override void OnDispose()
    {
        base.OnDispose();

        services.Values.ForEach(x => x.Dispose());
    }
}