namespace Markwardt;

public class GodotLogger : ILogger
{
    public void Log(object? message, bool isError = false)
    {
        if (isError)
        {
            GD.PrintErr(message);
        }
        else
        {
            GD.Print(message);
        }
    }
}