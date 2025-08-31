namespace Markwardt;

public class FactoryServiceHandler : IServiceHandler
{
    public IServiceSource? TryCreateSource(Type tag)
    {
        if (tag.IsDelegate())
        {
            return ServiceSource.FromFactory(tag);
        }

        return null;
    }
}