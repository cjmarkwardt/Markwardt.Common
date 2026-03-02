namespace Markwardt;

public class FactoryService(Type factory, IService source, Func<ParameterInfo, ServiceParameter, bool?>? isMatch = null) : BaseDisposable, IService
{
    private static IService GetDefaultSource(Type factory, Func<IService, IService>? configure = null)
    {
        Type result;
        string? constructorName;
        if (factory.GetCustomAttribute<FactoryConstructorAttribute>().TryNotNull(out FactoryConstructorAttribute? attribute))
        {
            result = attribute.Result;
            constructorName = attribute.ConstructorName;
        }
        else
        {
            result = factory.GetDelegateResult() ?? throw new InvalidOperationException($"Factory type {factory} must be a delegate type");
            constructorName = null;
        }

        ConstructorService source = new(result.FindConstructor(constructorName) ?? throw new InvalidOperationException($"Factory result {result} must have a default constructor"));
        return configure?.Invoke(source) ?? source;
    }

    public FactoryService(Type factory, Func<IService, IService>? configureSource = null, Func<ParameterInfo, ServiceParameter, bool?>? isMatch = null)
        : this(factory, GetDefaultSource(factory, configureSource), isMatch) { }

    public object? Resolve(IServiceProvider services, IEnumerable<ServiceOverride> overrides)
        => Delegator.CreateDelegate(factory, arguments => source.Resolve(services, arguments.Select(x => new ServiceOverride(parameter => Override(x.Key, x.Value, parameter)))));

    private IService? Override(ParameterInfo argument, object? value, ServiceParameter parameter)
    {
        if (IsMatch(argument, parameter))
        {
            return value.AsService(false);
        }
        else
        {
            return null;
        }
    }

    private bool IsMatch(ParameterInfo parameter, ServiceParameter serviceParameter)
    {
        bool? match = isMatch?.Invoke(parameter, serviceParameter);
        if (match is null)
        {
            return parameter.Name!.Equals(serviceParameter.Name, StringComparison.OrdinalIgnoreCase);
        }

        return match.Value;
    }

    public override string ToString()
        => $"{base.ToString()} (Factory: {factory}, Source: {source})";
}