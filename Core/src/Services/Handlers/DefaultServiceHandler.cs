namespace Markwardt;

public class DefaultServiceHandler : IServiceHandler
{
    public IServiceSource? TryCreateSource(Type tag)
        => ServiceSource.FromImplementation(tag);
}