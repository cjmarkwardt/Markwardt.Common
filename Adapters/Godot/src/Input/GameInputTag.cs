namespace Markwardt;

[ServiceType<IGameInput>]
public abstract class GameInputTag : SimpleTag
{
    protected abstract string Action { get; }

    protected sealed override object Get()
        => new GameInput(Action);
}