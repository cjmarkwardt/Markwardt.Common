namespace Markwardt;

public class RouteTag<TService> : IServiceTag
    where TService : class
{
    public IService GetService()
        => Service.Route<TService>();
}