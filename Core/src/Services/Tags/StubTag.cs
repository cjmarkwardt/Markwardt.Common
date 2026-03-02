namespace Markwardt;

public class StubTag : IServiceTag
{
    public IService GetService()
        => EmptyService.Instance;
}