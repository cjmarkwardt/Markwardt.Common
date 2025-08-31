namespace Markwardt;

public class StubTag : IServiceTag
{
    public IServiceSource GetSource()
        => throw new InvalidOperationException("Service does not exist");
}