namespace Markwardt;

public class AttributeServiceHandler : IServiceSource
{
    public IService? TryGetService(Type tag)
    {
        ServiceAttribute? attribute = tag.GetCustomAttribute<ServiceAttribute>();
        if (attribute is not null)
        {
            return attribute.GetService(tag);
        }

        return null;
    }
}