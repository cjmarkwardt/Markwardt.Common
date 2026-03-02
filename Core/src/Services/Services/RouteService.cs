namespace Markwardt;

public class RouteService(Type tag, bool require = true) : BaseDisposable, IService
{
    public object? Resolve(IServiceProvider services, IEnumerable<ServiceOverride> overrides)
    {
        object? value = services.GetService(tag);
        if (value is null && require)
        {
            throw new InvalidOperationException($"Service {tag} is required but could not be resolved");
        }

        return value;
    }

    public override string ToString()
        => $"{base.ToString()} (Tag: {tag}, Require: {require})";
}