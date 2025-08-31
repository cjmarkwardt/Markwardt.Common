namespace Markwardt;

public interface IGameInput
{
    bool IsPressed { get; }
    bool IsJustPressed { get; }
    bool IsJustReleased { get; }
}

public class GameInput(string action) : IGameInput
{
    public bool IsPressed => Input.IsActionPressed(action);
    public bool IsJustPressed => Input.IsActionJustPressed(action);
    public bool IsJustReleased => Input.IsActionJustReleased(action);
}