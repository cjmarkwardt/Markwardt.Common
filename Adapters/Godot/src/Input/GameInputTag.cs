namespace Markwardt;

[ServiceType<IGameInput>]
public abstract class GameInputTag : ServiceTag
{
    protected abstract string Action { get; }

    protected sealed override object GetService(IServiceProvider services)
        => new GameInput(Action);
}