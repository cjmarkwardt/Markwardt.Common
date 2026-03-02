namespace Markwardt;

public class DefaultServiceHandler : IServiceSource
{
    public IService? TryGetService(Type tag, out string? path)
    {
        path = $"Default Constructor";
        return new ConstructorService(tag).Cache();
    }
}