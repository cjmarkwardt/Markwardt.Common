namespace Markwardt;

public class ConstructorService(InvokableMethod constructor) : BaseAsyncDisposable, IService
{
    public ConstructorService(MethodBase constructor)
        : this(new InvokableMethod(constructor)) { }

    public ConstructorService(Type type, string? constructorName = null)
        : this(type.FindConstructor(constructorName).NotNull($"Cannot find constructor for {type} with name {constructorName}")) { }

    private readonly List<IServiceResolver> parameterResolvers = constructor.Parameters.Select(x => new ParameterResolver(x)).Cast<IServiceResolver>().ToList();
    private readonly List<IServiceInjector> propertyInjectors = constructor.ResultType.GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(x => x.SetMethod is not null).Select(x => new PropertyInjector(x)).Cast<IServiceInjector>().ToList();

    public object? Resolve(IServiceProvider services, IEnumerable<ServiceOverride> overrides)
    {
        object?[] arguments = new object?[parameterResolvers.Count];
        int i = 0;
        foreach (IServiceResolver resolver in parameterResolvers)
        {
            Maybe<object?> argument = resolver.Resolve(services, overrides);
            if (!argument.HasValue)
            {
                throw new InvalidOperationException($"Cannot resolve parameter {resolver.Parameter.Name} of type {resolver.Parameter.Type} for constructor {constructor}");
            }

            arguments[i] = argument.Value;
            i++;
        }

        object instance = constructor.Invoke(null, arguments).NotNull();
        foreach (IServiceInjector injector in propertyInjectors)
        {
            if (!injector.Inject(services, instance, overrides))
            {
                throw new InvalidOperationException($"Cannot resolve property {injector.Parameter.Name} of type {injector.Parameter.Type} in instance of {instance.GetType()} for constructor {constructor}");
            }
        }

        return instance;
    }

    public override string ToString()
        => $"{base.ToString()} (Constructor: {constructor})";
}