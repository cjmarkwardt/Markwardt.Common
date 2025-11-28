namespace Markwardt;

public interface IWorldGenerator<TWorld, TWorldConfiguration>
{
    ValueTask<TWorld> Generate(TWorldConfiguration configuration, CancellationToken cancellation = default);
}