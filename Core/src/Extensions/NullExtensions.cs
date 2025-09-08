namespace Markwardt;

public static class NullExtensions
{
    public static T NotNull<T>(this T? obj, Func<string>? message = null)
        where T : class
        => obj ?? throw new InvalidOperationException(message?.Invoke() ?? obj?.GetType().FullName ?? typeof(T).FullName);

    public static T NotNull<T>(this T? obj, string message)
        where T : class
        => obj.NotNull(() => message);

    public static T ValueNotNull<T>(this T? obj, Func<string>? message = null)
        where T : struct
        => obj ?? throw new InvalidOperationException(message?.Invoke() ?? obj?.GetType().FullName ?? typeof(T).FullName);

    public static T ValueNotNull<T>(this T? obj, string message)
        where T : struct
        => obj.ValueNotNull(() => message);

    public static bool TryNotNull<T>(this T? obj, [NotNullWhen(true)] out T value)
    {
        if (obj is null)
        {
            value = default!;
            return false;
        }
        else
        {
            value = obj;
            return true;
        }
    }
}