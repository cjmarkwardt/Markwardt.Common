namespace Markwardt;

[ServiceType<Node>]
public class RootNodeTag : SourceTag<Window>
{
    protected override object GetService(IServiceProvider services, Window source)
        => source;
}