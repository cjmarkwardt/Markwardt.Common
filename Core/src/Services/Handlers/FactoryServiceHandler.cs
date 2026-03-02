namespace Markwardt;

public class FactoryServiceHandler : IServiceSource
{
    public IService? TryGetService(Type tag, out string? path)
    {
        path = null;

        if (tag.IsDelegate())
        {
            path = $"Auto Factory";
            return new FactoryService(tag);
        }

        return null;
    }
}