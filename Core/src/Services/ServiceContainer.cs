namespace Markwardt;

public interface IServiceContainer : IServiceProvider, IServiceConfiguration, IDisposable, IAsyncDisposable;

public sealed class ServiceContainer(IServiceHandler handler) : IServiceContainer
{
    public ServiceContainer()
        : this(new AutoHandler()) { }

    private readonly Dictionary<Type, Func<IServiceSource>> sourceCreators = [];
    private readonly Dictionary<Type, IServiceSource> sources = [];

    private bool isDisposed;

    public void Configure(Type tag, Func<IServiceSource>? sourceCreator)
    {
        if (sourceCreator is null)
        {
            sourceCreators.Remove(tag);
        }
        else
        {
            sourceCreators[tag] = sourceCreator;
        }
    }

    public object? GetService(Type tag)
    {
        ObjectDisposedException.ThrowIf(isDisposed, this);

        if (typeof(IServiceProvider).Equals(tag))
        {
            return this;
        }
        else if (sources.TryGetValue(tag, out IServiceSource? source))
        {
            return source.GetService(this);
        }
        else if (sourceCreators.TryGetValue(tag, out Func<IServiceSource>? sourceCreator))
        {
            source = sourceCreator();
            sources.Add(tag, source);
            return source.GetService(this);
        }
        else if (handler.TryCreateSource(tag).TryNotNull(out IServiceSource createdSource))
        {
            sources.Add(tag, createdSource);
            return createdSource.GetService(this);
        }
        else
        {
            return null;
        }
    }

    public void Dispose()
    {
        if (!isDisposed)
        {
            isDisposed = true;
            sources.Values.ForEach(x => x.Dispose());
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (!isDisposed)
        {
            isDisposed = true;
            await Task.WhenAll(sources.Values.Select(x => x.DisposeAsync().AsTask()));
        }
    }

    object? IServiceProvider.GetService(Type serviceType)
        => GetService(serviceType);
}