namespace Markwardt;

public interface IServiceSource
{
    IService? TryGetService(Type tag);
}