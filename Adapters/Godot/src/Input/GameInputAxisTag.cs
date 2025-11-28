namespace Markwardt;

[ServiceType<IGameInputAxis>]
public abstract class GameInputAxisTag : SimpleTag
{
    protected abstract string NegativeAction { get; }
    protected abstract string PositiveAction { get; }

    protected sealed override object Get()
        => new GameInputAxis(NegativeAction, PositiveAction);
}