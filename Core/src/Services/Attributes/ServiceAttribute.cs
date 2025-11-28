namespace Markwardt;

public abstract class ServiceAttribute : Attribute
{
    public abstract IService GetService(Type type);
}