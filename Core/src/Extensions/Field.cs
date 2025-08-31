namespace Markwardt;

public static class Field
{
    public static T? Set<T>(ref T? field, T? newValue)
    {
        T? value = field;
        field = newValue;
        return value;
    }

    public static void TrySet<T>(ref T field, T newValue, Action<T>? onSet = null)
    {
        if (!field.ValueEquals(newValue))
        {
            T oldValue = field;
            field = newValue;
            onSet?.Invoke(oldValue);
        }
    }

    public static T? Clear<T>(ref T? field)
        => Set(ref field, default);
}