namespace Markwardt;

public interface IServiceConfigurator
{
    IReadOnlyList<ParameterInfo> Parameters { get; }
    IReadOnlyList<PropertyInfo> Properties { get; }

    IServiceConfigurator Parameter(ParameterInfo parameter, IService service);
    IServiceConfigurator Property(PropertyInfo property, IService service);
}

public static class ServiceConfiguratorExtensions
{
    public static IServiceConfigurator Parameter(this IServiceConfigurator configurator, string name, IService service)
        => configurator.Parameter(configurator.Parameters.FirstOrDefault(x => x.Name == name) ?? throw new ArgumentException($"No parameter found with name '{name}'"), service);

    public static IServiceConfigurator Property(this IServiceConfigurator configurator, string name, IService service)
        => configurator.Property(configurator.Properties.FirstOrDefault(x => x.Name == name) ?? throw new ArgumentException($"No property found with name '{name}'"), service);
}

public class ServiceConfigurator(InvokableMethod constructor) : IServiceConfigurator
{
    private readonly Dictionary<ParameterInfo, IService> parameterServices = [];
    private readonly Dictionary<PropertyInfo, IService> propertieServices = [];

    public IReadOnlyList<ParameterInfo> Parameters => constructor.Parameters;
    public IReadOnlyList<PropertyInfo> Properties => constructor.ResultType.GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(x => x.SetMethod is not null).ToList();

    public IServiceConfigurator Parameter(ParameterInfo parameter, IService service)
    {
        parameterServices[parameter] = service;
        return this;
    }

    public IServiceConfigurator Property(PropertyInfo property, IService service)
    {
        propertieServices[property] = service;
        return this;
    }

    public IService Build()
        => new ConstructorService(constructor, parameterServices, propertieServices);
}