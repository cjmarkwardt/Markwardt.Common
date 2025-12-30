namespace Markwardt;

public abstract class SimpleTag : ServiceTag
{
    protected abstract object Get();

    protected override object GetService(IServiceProvider services)
        => Get();
}