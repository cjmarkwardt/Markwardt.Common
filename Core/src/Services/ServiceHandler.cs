namespace Markwardt;

public interface IServiceSource
{
    IService? TryGetService(Type tag, out string? path);
}