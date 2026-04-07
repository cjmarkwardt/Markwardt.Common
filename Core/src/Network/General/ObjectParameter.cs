namespace Markwardt;

/*public interface IObjectParameter
{
    Type Type { get; }
    object? Default { get; }
}

public static class ObjectParameterExtensions
{
    public static bool IsDefault(this IObjectParameter parameter, object? value)
        => value.ValueEquals(parameter.Default);
}

public class ObjectParameter<T>(T defaultValue) : IObjectParameter
{
    public Type Type => typeof(T);
    public T Default => defaultValue;

    object? IObjectParameter.Default => Default;
}*/