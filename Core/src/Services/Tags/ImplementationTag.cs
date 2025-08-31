namespace Markwardt;

public class ImplementationTag<T> : IServiceTag
    where T : class
{
    public IServiceSource GetSource()
        => ServiceSource.FromImplementation(typeof(T));
}