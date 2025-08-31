namespace Markwardt;

public interface IServiceBuilder
{
    object Build(IServiceProvider services, IReadOnlyDictionary<string, object?>? arguments = null);
}

public class ServiceBuilder(IServiceInjector injector, IInvokable implementation) : IServiceBuilder
{
    private static readonly Dictionary<object, ServiceBuilder> builders = [];

    public static object Build(object key, Func<IInvokable> getImplementation, IServiceProvider services, IReadOnlyDictionary<string, object?>? arguments = null)
        => builders.GetOrAdd(key, () => new(ServiceInjector.Shared, getImplementation())).Build(services, arguments);

    private readonly Lazy<IReadOnlyList<ServiceParameter>> parameters = new(() => implementation.Parameters.Select(x => new ServiceParameter(x)).ToList());
    private readonly object?[] argumentBuffer = new object?[implementation.Parameters.Count];

    public object Build(IServiceProvider services, IReadOnlyDictionary<string, object?>? arguments = null)
    {
        for (int i = 0; i < parameters.Value.Count; i++)
        {
            argumentBuffer[i] = parameters.Value[i].Resolve(services, arguments);
        }

        object service = implementation.Invoke(null, argumentBuffer).NotNull();

        if (implementation.Source is ConstructorInfo)
        {
            injector.Inject(services, service);
        }

        return service;
    }
}