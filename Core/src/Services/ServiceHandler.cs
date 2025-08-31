namespace Markwardt;

public interface IServiceHandler
{
    IServiceSource? TryCreateSource(Type tag);
}