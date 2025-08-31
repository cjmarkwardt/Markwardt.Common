namespace Markwardt;

public interface IInvokable
{
    object? Source { get; }
    IReadOnlyList<Parameter> Parameters { get; }

    object? Invoke(object? instance, Span<object?> arguments);

    public record Parameter(object? Source, string Name, Type Type, Maybe<object?> Default, IEnumerable<Attribute> Attributes)
    {
        public Parameter(ParameterInfo parameter)
            : this(parameter, parameter.Name ?? string.Empty, parameter.ParameterType, parameter.HasDefaultValue ? parameter.DefaultValue.Maybe() : default, parameter.GetCustomAttributes()) { }
    }
}