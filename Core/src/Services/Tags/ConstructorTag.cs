namespace Markwardt;

public class ConstructorTag<T> : IServiceTag
    where T : class
{
    public virtual bool IsCached => true;

    public IService GetService()
        => new ConstructorService(typeof(T)).Cache(IsCached);
}