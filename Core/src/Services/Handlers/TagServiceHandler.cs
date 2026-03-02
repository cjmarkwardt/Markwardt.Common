namespace Markwardt;

public class TagServiceHandler : IServiceSource
{
    public IService? TryGetService(Type tag, out string? path)
    {
        path = null;

        if (tag.IsAssignableTo(typeof(IServiceTag)))
        {
            path = $"Tag {tag}";
            return ((IServiceTag)Activator.CreateInstance(tag).NotNull()).GetService();
        }

        return null;
    }
}