namespace Markwardt;

[ServiceType<IGameInput>]
public abstract class GameInputTag : ServiceTag
{
    protected abstract string Action { get; }

    protected override object Resolve(IServiceProvider services)
        => new GameInput(Action);
}