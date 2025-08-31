namespace Markwardt;

public abstract class ServiceResolutionAttribute : Attribute
{
    public virtual bool IsTransient => false;

    public abstract IServiceSource GetSource(Type type);
}