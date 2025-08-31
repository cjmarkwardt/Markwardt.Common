namespace Markwardt;

public class AttributeServiceHandler : IServiceHandler
{
    public IServiceSource? TryCreateSource(Type tag)
    {
        ServiceResolutionAttribute? attribute = tag.GetCustomAttribute<ServiceResolutionAttribute>();
        if (attribute is not null)
        {
            return attribute.GetSource(tag);
        }

        return null;
    }
}