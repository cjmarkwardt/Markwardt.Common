namespace Markwardt;

public class PropertyInjector(PropertyInfo property) : ServiceResolver, IServiceInjector
{
    private readonly MethodInvoker setter = MethodInvoker.Create(property.SetMethod.NotNull());

    public override ServiceParameter Parameter { get; } = new(ServiceParameterType.Property, property.PropertyType, property.Name, property);
    
    protected override bool IsSkipped { get; } = !property.IsInit() && !property.IsRequired();
    protected override Maybe<object?> DefaultValue { get; } = property.IsNullable() ? new Maybe<object?>(null) : default;

    public bool Inject(IServiceProvider services, object instance, IEnumerable<ServiceOverride> overrides)
    {
        Maybe<object?> value = Resolve(services, overrides);
        if (value.HasValue)
        {
            setter.Invoke(instance, value.Value);
        }
        else if (!IsSkipped)
        {
            return false;
        }

        return true;
    }
}