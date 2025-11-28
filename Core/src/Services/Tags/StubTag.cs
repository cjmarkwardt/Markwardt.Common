namespace Markwardt;

public class StubTag : IServiceTag
{
    public IService GetService()
        => throw new InvalidOperationException("Service does not exist");
}