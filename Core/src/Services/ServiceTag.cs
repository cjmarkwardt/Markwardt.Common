namespace Markwardt;

public interface IServiceTag
{
    IService GetService();
}

public abstract class ServiceTag : IServiceTag
{
    public virtual bool IsCached => true;

    public IService GetService()
        => Service.Delegate(x => GetService(x), IsCached);

    protected abstract object GetService(IServiceProvider services);
}