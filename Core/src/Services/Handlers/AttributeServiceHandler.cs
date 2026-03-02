namespace Markwardt;

public class AttributeServiceHandler : IServiceSource
{
    public IService? TryGetService(Type tag, out string? path)
    {
        path = null;

        ServiceAttribute? attribute = tag.GetCustomAttribute<ServiceAttribute>();
        if (attribute is not null)
        {
            path = $"Attribute {attribute}";
            return attribute.GetService(tag);
        }

        return null;
    }
}