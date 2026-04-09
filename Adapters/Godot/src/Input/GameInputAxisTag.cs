namespace Markwardt;

[ServiceType<IGameInputAxis>]
public abstract class GameInputAxisTag : ServiceTag
{
    protected abstract string NegativeAction { get; }
    protected abstract string PositiveAction { get; }

    protected override object Resolve(IServiceProvider services)
        => new GameInputAxis(NegativeAction, PositiveAction);
}