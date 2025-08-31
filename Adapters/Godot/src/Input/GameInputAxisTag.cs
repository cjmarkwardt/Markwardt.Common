namespace Markwardt;

[ServiceType<IGameInputAxis>]
public abstract class GameInputAxisTag : ServiceTag
{
    protected abstract string NegativeAction { get; }
    protected abstract string PositiveAction { get; }

    protected sealed override object GetService(IServiceProvider services)
        => new GameInputAxis(NegativeAction, PositiveAction);
}