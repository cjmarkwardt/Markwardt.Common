namespace Markwardt;

public interface IServiceResolver
{
    ServiceParameter Parameter { get; }

    Maybe<object?> Resolve(IServiceProvider services, IEnumerable<ServiceOverride> overrides);
}

public abstract class ServiceResolver : IServiceResolver
{
    public ServiceResolver()
        => Service = Parameter.Attributes.GetCustomAttribute<ServiceAttribute>()?.GetService(Parameter.Type) ?? new RouteService(Parameter.Type, false);

    public IService Service { get; }

    public abstract ServiceParameter Parameter { get; }

    protected abstract bool IsSkipped { get; }
    protected abstract Maybe<object?> DefaultValue { get; }
    
    public Maybe<object?> Resolve(IServiceProvider services, IEnumerable<ServiceOverride> overrides)
    {
        object? value;

        if (overrides.SelectWhere(x => x(Parameter)).Select(x => x.Item).MaybeFirst().TryGetValue(out IService? overrideService))
        {
            value = overrideService.Resolve(services);
            if (value is SuppressService.Signal)
            {
                return new Maybe<object?>(null);
            }
            else if (value is not null)
            {
                return value.Maybe<object?>();
            }
        }

        if (IsSkipped)
        {
            return default;
        }

        value = Service.Resolve(services);
        if (value is SuppressService.Signal)
        {
            return new Maybe<object?>(null);
        }
        else if (value is not null)
        {
            return value.Maybe<object?>();
        }
        else
        {
            return DefaultValue;
        }
    }
}