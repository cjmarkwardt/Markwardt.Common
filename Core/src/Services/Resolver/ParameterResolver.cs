namespace Markwardt;

public class ParameterResolver(ParameterInfo parameter) : ServiceResolver
{
    private readonly Lazy<Maybe<object?>> defaultValue = new(() =>
    {
        if (parameter.HasDefaultValue)
        {
            return parameter.DefaultValue.Maybe();
        }
        else if (parameter.IsNullable())
        {
            return new Maybe<object?>(null);
        }
        else
        {
            return default;
        }
    });

    public override ServiceParameter Parameter { get; } = new(ServiceParameterType.Parameter, parameter.ParameterType, parameter.Name.NotNull(), parameter);
    
    protected override bool IsSkipped => false;
    protected override Maybe<object?> DefaultValue => defaultValue.Value;
}