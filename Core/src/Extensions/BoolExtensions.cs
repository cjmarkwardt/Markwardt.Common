namespace Markwardt;

public static class BoolExtensions
{
    public static void Require(this bool value, string? message = null)
    {
        if (!value)
        {
            throw new InvalidOperationException(message);
        }
    }

    public static bool On(this bool value, Action action)
    {
        if (value)
        {
            action();
        }

        return value;
    }

    public static bool Off(this bool value, Action action)
    {
        if (!value)
        {
            action();
        }

        return value;
    }
}