namespace Markwardt;

public class ConstructorService(InvokableMethod constructor, IReadOnlyDictionary<ParameterInfo, IService>? parameterBuilders = null, IReadOnlyDictionary<PropertyInfo, IService>? propertyBuilders = null) : BaseAsyncDisposable, IService
{
    private readonly IReadOnlyList<ParameterResolver> parameterResolvers = constructor.Parameters.Select(x => new ParameterResolver(x)).ToList();
    private readonly IReadOnlyList<PropertyInjector> propertyInjectors = constructor.ResultType.GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(x => x.SetMethod is not null).Select(x => new PropertyInjector(x)).ToList();

    public object Resolve(IServiceProvider services, IReadOnlyDictionary<ParameterInfo, object?>? parameters = null, IReadOnlyDictionary<PropertyInfo, object?>? properties = null)
    {
        object?[] arguments = new object?[parameterResolvers.Count];
        for (int i = 0; i < parameterResolvers.Count; i++)
        {
            arguments[i] = (parameterResolvers[i].Resolve(services, parameterBuilders, parameters)).Value;
        }

        object instance = constructor.Invoke(null, arguments).NotNull();

        foreach (PropertyInjector property in propertyInjectors)
        {
            property.Inject(services, instance, propertyBuilders, properties);
        }

        if (instance is IAsyncInitializable asyncInitializable)
        {
            asyncInitializable.Initialize();
        }

        return instance;
    }

    private enum ResolveMode
    {
        Skip,
        Optional,
        Required
    }

    private abstract class Resolver<T>
        where T : ICustomAttributeProvider
    {
        public Resolver(T target)
        {
            Target = target;
            Service = Target.GetCustomAttribute<ServiceAttribute>()?.GetService(Type);
        }

        public T Target { get; }
        public IService? Service { get; }

        public abstract Type Type { get; }
        public abstract Maybe<object?> DefaultValue { get; }
        public abstract ResolveMode Mode { get; }

        public Maybe<object?> Resolve(IServiceProvider services, IReadOnlyDictionary<T, IService>? serviceOverrides, IReadOnlyDictionary<T, object?>? values)
        {
            if (values is not null && values.TryGetValue(Target, out object? argument))
            {
                return argument.Maybe();
            }
            else if (serviceOverrides is not null && serviceOverrides.TryGetValue(Target, out IService? service))
            {
                return service.Resolve(services, null, null).Maybe<object?>();
            }
            else if (Service is not null)
            {
                return Service.Resolve(services, null, null).Maybe<object?>();
            }
            else if (Mode is ResolveMode.Skip)
            {
                return DefaultValue;
            }
            else
            {
                return GetService(services, Type.GetDefaultImplementation());
            }
        }

        private Maybe<object?> GetService(IServiceProvider services, Type type)
        {
            object? service = services.GetService(type);
            if (service is null && Mode is ResolveMode.Required)
            {
                throw new InvalidOperationException($"Service {type} is required but could not be resolved");
            }

            return service.Maybe();
        }
    }

    private sealed class ParameterResolver(ParameterInfo target) : Resolver<ParameterInfo>(target)
    {
        public override Type Type => Target.ParameterType;
        public override Maybe<object?> DefaultValue => Target.HasDefaultValue ? Target.DefaultValue.Maybe() : default;

        public override ResolveMode Mode { get; } = !target.IsNullable() || !target.HasDefaultValue ? ResolveMode.Required : ResolveMode.Optional;
    }

    private sealed class PropertyInjector(PropertyInfo target) : Resolver<PropertyInfo>(target)
    {
        private readonly MethodInvoker setter = MethodInvoker.Create(target.SetMethod.NotNull());

        public override Type Type => Target.PropertyType;
        public override Maybe<object?> DefaultValue => default;

        public override ResolveMode Mode { get; } = !target.IsInit() && !target.IsRequired() ? ResolveMode.Skip : target.IsNullable() ? ResolveMode.Optional : ResolveMode.Required;

        public void Inject(IServiceProvider services, object instance, IReadOnlyDictionary<PropertyInfo, IService>? builders, IReadOnlyDictionary<PropertyInfo, object?>? values)
        {
            Maybe<object?> value = Resolve(services, builders, values);
            if (value.HasValue)
            {
                setter.Invoke(instance, value.Value);
            }
        }
    }
}