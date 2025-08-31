namespace Markwardt;

public interface IGameInputAxis
{
    float Value { get; }
}

public class GameInputAxis(string negativeAction, string positiveAction) : IGameInputAxis
{
    public float Value => Input.GetAxis(negativeAction, positiveAction);
}