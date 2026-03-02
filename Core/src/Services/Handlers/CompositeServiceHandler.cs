namespace Markwardt;

public class CompositeServiceHandler(IEnumerable<IServiceSource> handlers) : IServiceSource
{
    public static Action<Type>? Action { get; set; }

    public IService? TryGetService(Type tag, out string? path)
    {
        path = null;

        foreach (IServiceSource handler in handlers)
        {
            if (handler.TryGetService(tag, out path).TryNotNull(out IService source))
            {
                return source;
            }
        }

        return null;
    }
}