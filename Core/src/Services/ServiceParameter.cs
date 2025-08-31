namespace Markwardt;

public class ServiceParameter(IInvokable.Parameter parameter)
{
    private readonly Lazy<Type?> createTarget = new(() => parameter.Attributes.OfType<CreateAttribute>().FirstOrDefault()?.Implementation);

    private readonly Lazy<Type?> target = new(() =>
    {
        if (parameter.Attributes.OfType<NoInjectAttribute>().Any())
        {
            return null;
        }
        else if (parameter.Attributes.OfType<InjectAttribute>().FirstOrDefault().TryNotNull(out InjectAttribute attribute))
        {
            return attribute.Service;
        }
        else if (parameter.Type == typeof(string) || parameter.Type.IsPrimitive || parameter.Type.IsValueType)
        {
            return null;
        }
        else
        {
            return parameter.Type;
        }
    });

    public object? Resolve(IServiceProvider services, IReadOnlyDictionary<string, object?>? arguments = null)
    {
        if (arguments is not null && arguments.TryGetValue(parameter.Name, out object? argument))
        {
            return argument;
        }

        if (createTarget.Value.TryNotNull(out Type createTargetType))
        {
            return services.Create(createTargetType);
        }

        if (target.Value.TryNotNull(out Type targetType) && services.GetService(targetType).TryNotNull(out object service))
        {
            return service;
        }

        if (parameter.Default.TryGetValue(out object? defaultValue))
        {
            return defaultValue;
        }

        throw new InvalidOperationException($"Unable to resolve parameter {parameter}");
    }
}