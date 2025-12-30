namespace Markwardt;

public static class NullExtensions
{
    public static T NotNull<T>(this T? obj, Func<string>? message = null)
        where T : class
    {
        try
        {
            return obj ?? throw new InvalidOperationException(message?.Invoke() ?? obj?.GetType().FullName ?? typeof(T).FullName);
        }
        catch (InvalidOperationException)
        {
            throw;
        }
    }

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

    public static bool IsNullable(this PropertyInfo property)
        => new NullabilityInfoContext().Create(property).ReadState is NullabilityState.Nullable;

    public static bool IsNullable(this EventInfo @event)
        => new NullabilityInfoContext().Create(@event).ReadState is NullabilityState.Nullable;

    public static bool IsNullable(this FieldInfo field)
        => new NullabilityInfoContext().Create(field).ReadState is NullabilityState.Nullable;

    public static bool IsNullable(this ParameterInfo parameter)
        => new NullabilityInfoContext().Create(parameter).ReadState is NullabilityState.Nullable;
}