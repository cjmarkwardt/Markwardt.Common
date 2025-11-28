namespace Markwardt;

public static class ObjectExtensions
{
    public static bool ValueEquals(this object? x, object? y)
        => (x is null && y is null) || (x is not null && y is not null && x.Equals(y));

    public static Maybe<T> Filter<T>(this T target, Func<T, bool> filter)
        => filter(target) ? target.Maybe() : default;

    public static T CastTo<T>(this object target)
        => (T)target;

    public static T Do<T>(this T target, Action<T> action)
    {
        action(target);
        return target;
    }

    public static T? DoIfNotNull<T>(this T? target, Action<T> action)
        where T : class
    {
        if (target is not null)
        {
            action(target);
        }

        return target;
    }

    public static async ValueTask<T> Do<T>(this T target, Func<T, ValueTask> action)
    {
        await action(target);
        return target;
    }

    public static async ValueTask<T?> DoIfNotNull<T>(this T? target, Func<T, ValueTask> action)
        where T : class
    {
        if (target is not null)
        {
            await action(target);
        }

        return target;
    }
}