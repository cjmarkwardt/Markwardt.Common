namespace Markwardt;

public interface IServiceTag
{
    IService GetService();
}

public abstract class ServiceTag : IServiceTag
{
    public virtual bool IsCached => true;

    public IService GetService()
        => new Service(x => Resolve(x)).Cache(IsCached);

    protected abstract object Resolve(IServiceProvider services);
}