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
}