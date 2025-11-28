namespace Markwardt;

public class ConstructorTag<T> : IServiceTag
    where T : class
{
    public IService GetService()
        => Service.Constructor<T>();
}