namespace Markwardt;

public class FactoryServiceHandler : IServiceSource
{
    public IService? TryGetService(Type tag)
    {
        if (tag.IsDelegate())
        {
            return Service.Factory(tag);
        }

        return null;
    }
}