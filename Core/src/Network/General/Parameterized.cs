namespace Markwardt;

/*public interface IParameterized
{
    IDictionary<IObjectParameter, object?> Parameters { get; }
}

public static class ParameterizedExtensions
{
    private static TParameterized SetParameter<TParameterized>(this TParameterized parameterized, IObjectParameter parameter, object? value)
        where TParameterized : IParameterized
    {
        if (parameter.IsDefault(value))
        {
            parameterized.Parameters.Remove(parameter);
        }
        else
        {
            parameterized.Parameters[parameter] = value;
        }

        return parameterized;
    }

    public static TParameterized SetParameter<TParameterized, T>(this TParameterized parameterized, ObjectParameter<T> key, T value)
        where TParameterized : IParameterized
        => parameterized.SetParameter((IObjectParameter)key, value);

    public static TParameterized SetDefaultParameter<TParameterized, T>(this TParameterized parameterized, ObjectParameter<T> key)
        where TParameterized : IParameterized
        where T : notnull
        => parameterized.SetParameter(key, key.Default);

    public static TParameterized SetParameters<TParameterized>(this TParameterized parameterized, IEnumerable<KeyValuePair<IObjectParameter, object?>> parameters)
        where TParameterized : IParameterized
    {
        parameters.ForEach(x => parameterized.SetParameter(x.Key, x.Value));
        return parameterized;
    }

    public static TParameterized SetParameters<TParameterized>(this TParameterized parameterized, Message message)
        where TParameterized : IParameterized
        => parameterized.SetParameters(message.Parameters);

    public static TParameterized ClearParameters<TParameterized>(this TParameterized parameterized)
        where TParameterized : IParameterized
    {
        parameterized.Parameters.Clear();
        return parameterized;
    }

    public static T GetParameter<T>(this IParameterized parameterized, ObjectParameter<T> key)
        => parameterized.Parameters.TryGetValue(key, out object? value) && value is T typedValue ? typedValue : key.Default;
}*/