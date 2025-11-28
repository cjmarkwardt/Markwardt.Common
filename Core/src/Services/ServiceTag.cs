namespace Markwardt;

public interface IServiceTag
{
    IService GetService();
}

public abstract class ServiceTag : IServiceTag
{
    public virtual bool IsCached => true;

    public IService GetService()
        => Service.Delegate(GetService, IsCached);

    protected abstract ValueTask<object> GetService(IAsyncServiceProvider services, CancellationToken cancellation = default);
}