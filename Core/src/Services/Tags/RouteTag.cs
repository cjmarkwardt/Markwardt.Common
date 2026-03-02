namespace Markwardt;

public class RouteTag<TService> : IServiceTag
    where TService : class
{
    public virtual bool IsRequired => true; 

    public IService GetService()
        => new RouteService(typeof(TService), IsRequired);
}