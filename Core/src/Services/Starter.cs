namespace Markwardt;

public interface IStarter
{
    ValueTask Start(CancellationToken cancellation = default);
}