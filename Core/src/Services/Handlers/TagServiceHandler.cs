namespace Markwardt;

public class TagServiceHandler : IServiceHandler
{
    public IServiceSource? TryCreateSource(Type tag)
    {
        if (tag.IsAssignableTo(typeof(IServiceTag)))
        {
            return ((IServiceTag)Activator.CreateInstance(tag).NotNull()).GetSource();
        }

        return null;
    }
}