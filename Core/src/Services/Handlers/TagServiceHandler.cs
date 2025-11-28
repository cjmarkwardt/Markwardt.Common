namespace Markwardt;

public class TagServiceHandler : IServiceSource
{
    public IService? TryGetService(Type tag)
    {
        if (tag.IsAssignableTo(typeof(IServiceTag)))
        {
            return ((IServiceTag)Activator.CreateInstance(tag).NotNull()).GetService();
        }

        return null;
    }
}