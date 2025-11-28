namespace Markwardt;

public class DefaultServiceHandler : IServiceSource
{
    public IService? TryGetService(Type tag)
        => Service.Constructor(tag);
}