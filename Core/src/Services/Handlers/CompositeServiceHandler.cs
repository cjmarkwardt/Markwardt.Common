namespace Markwardt;

public class CompositeServiceHandler(IEnumerable<IServiceSource> handlers) : IServiceSource
{
    public IService? TryGetService(Type tag)
    {
        foreach (IServiceSource handler in handlers)
        {
            if (handler.TryGetService(tag).TryNotNull(out IService source))
            {
                return source;
            }
        }

        return null;
    }
}