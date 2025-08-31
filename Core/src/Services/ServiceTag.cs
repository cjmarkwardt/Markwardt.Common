namespace Markwardt;

public interface IServiceTag
{
    IServiceSource GetSource();
}

public abstract class ServiceTag : IServiceTag
{
    public virtual bool IsCached => true;

    public IServiceSource GetSource()
        => ServiceSource.FromDelegate(GetService, IsCached);

    protected abstract object GetService(IServiceProvider services);
}