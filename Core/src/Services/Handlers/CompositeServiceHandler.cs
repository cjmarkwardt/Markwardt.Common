namespace Markwardt;

public class CompositeServiceHandler(IEnumerable<IServiceHandler> handlers) : IServiceHandler
{
    public IServiceSource? TryCreateSource(Type tag)
    {
        foreach (IServiceHandler handler in handlers)
        {
            if (handler.TryCreateSource(tag).TryNotNull(out IServiceSource source))
            {
                return source;
            }
        }

        return null;
    }
}